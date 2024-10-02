namespace HexaEngine.Materials.Nodes.Textures
{
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static class ColorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RGBAToABGR(this uint color)
        {
            byte r = (byte)(color >> 24);
            byte g = (byte)(color >> 16);
            byte b = (byte)(color >> 8);
            byte a = (byte)color;
            return (uint)((a << 24) | (b << 16) | (g << 8) | r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RGBAToABGR(this int color)
        {
            byte r = (byte)(color >> 24);
            byte g = (byte)(color >> 16);
            byte b = (byte)(color >> 8);
            byte a = (byte)color;
            return (a << 24) | (b << 16) | (g << 8) | r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ABGRToRGBA(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte b = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte r = (byte)color;
            return (uint)((r << 24) | (g << 16) | (b << 8) | a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ABGRToRGBA(this int color)
        {
            byte a = (byte)(color >> 24);
            byte b = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte r = (byte)color;
            return (r << 24) | (g << 16) | (b << 8) | a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ABGRToVec4(this int color)
        {
            byte a = (byte)(color >> 24);
            byte b = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte r = (byte)color;
            return new Vector4(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 RGBAToVec4(this int color)
        {
            byte r = (byte)(color >> 24);
            byte g = (byte)(color >> 16);
            byte b = (byte)(color >> 8);
            byte a = (byte)color;
            return new Vector4(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 RGBAToVec4(this uint color)
        {
            byte r = (byte)(color >> 24);
            byte g = (byte)(color >> 16);
            byte b = (byte)(color >> 8);
            byte a = (byte)color;
            return new Vector4(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ABGRToVec4(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte b = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte r = (byte)color;
            return new Vector4(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Vec4ToABGR(this Vector4 color)
        {
            byte r = (byte)(color.X * byte.MaxValue);
            byte g = (byte)(color.Y * byte.MaxValue);
            byte b = (byte)(color.Z * byte.MaxValue);
            byte a = (byte)(color.W * byte.MaxValue);
            return (uint)((a << 24) | (b << 16) | (g << 8) | r);
        }
    }

    public struct TextureFileNodeReloadRequestEventArgs
    {
        public TextureFileNode Node;
        public bool SamplerOnly;

        public TextureFileNodeReloadRequestEventArgs(TextureFileNode node, bool samplerOnly)
        {
            Node = node;
            SamplerOnly = samplerOnly;
        }
    }

    public class TextureFileNode : Node, ITextureNode
    {
        [JsonIgnore] public Guid path = Guid.Empty;

        [JsonIgnore] public bool changed;

        public const int MaxMaxAnisotropy = unchecked(16);

        public TextureFileNode(int id, bool removable, bool isStatic) : base(id, "Texture", removable, isStatic)
        {
            TitleColor = 0x9E2A58FF.RGBAToVec4();
            TitleHoveredColor = 0x9E2A58FF.RGBAToVec4();
            TitleSelectedColor = 0xAA3363FF.RGBAToVec4();
        }

        public override void Initialize(NodeEditor editor)
        {
            OutColor = CreateOrGetPin(editor, "out", PinKind.Output, PinType.AnyFloat, PinShape.Quad);
            OutTex = CreateOrGetPin(editor, "out tex", PinKind.Output, PinType.Texture2D, PinShape.QuadFilled);
            InUV = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "uv", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv.xy"));
            base.Initialize(editor);
        }

        public TextureMapFilter Filter = TextureMapFilter.Anisotropic;

        public TextureMapMode U = TextureMapMode.Clamp;

        public TextureMapMode V = TextureMapMode.Clamp;

        public TextureMapMode W = TextureMapMode.Clamp;

        public float MipLODBias = 0.0f;

        public int MaxAnisotropy = MaxMaxAnisotropy;

        public Vector4 BorderColor = Vector4.Zero;

        public float MinLOD = float.MinValue;

        public float MaxLOD = float.MaxValue;

        public Guid Path
        {
            get => path;
            set
            {
                path = value;
                Reload();
                OnValueChanged();
            }
        }

        [JsonIgnore] public Pin OutColor { get; private set; }

        [JsonIgnore] public Pin OutTex { get; private set; }

        [JsonIgnore] public Pin InUV { get; private set; }

        [JsonIgnore] string ITextureNode.Name => Name;

        public event EventHandler<TextureFileNodeReloadRequestEventArgs>? ReloadRequested;

        public void Reload(bool samplerOnly = false)
        {
            ReloadRequested?.Invoke(this, new(this, samplerOnly));
        }

        public static TextureFileNode? FindTextureFileNode(NodeEditor editor, Guid path)
        {
            for (int i = 0; i < editor.Nodes.Count; i++)
            {
                var node = editor.Nodes[i];
                if (node is TextureFileNode textureFileNode && textureFileNode.path == path)
                {
                    return textureFileNode;
                }
            }

            return null;
        }
    }
}