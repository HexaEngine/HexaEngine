namespace HexaEngine.Materials.Nodes.Textures
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;
    using System.ComponentModel;
    using System.Numerics;
    using System.Xml.Serialization;

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

        [JsonIgnore]
        public Vector2 Size = new(128, 128);

        [JsonIgnore] public bool showMore;

        public TextureMapFilter Filter = TextureMapFilter.Anisotropic;
        public TextureMapMode U = TextureMapMode.Clamp;
        public TextureMapMode V = TextureMapMode.Clamp;
        public TextureMapMode W = TextureMapMode.Clamp;
        public float MipLODBias = 0.0f;
        public int MaxAnisotropy = MaxMaxAnisotropy;
        public Vector4 BorderColor = Vector4.Zero;
        public float MinLOD = float.MinValue;
        public float MaxLOD = float.MaxValue;

        public const int MaxMaxAnisotropy = unchecked(16);

        public TextureFileNode(int id, bool removable, bool isStatic) : base(id, "Texture", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            OutColor = CreateOrGetPin(editor, "out", PinKind.Output, PinType.AnyFloat, PinShape.Quad);
            InUV = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "uv", PinShape.CircleFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv.xy"));
            base.Initialize(editor);
        }

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

        [JsonIgnore]
        public Pin OutColor { get; private set; }

        [JsonIgnore]
        public Pin InUV { get; private set; }

        [JsonIgnore]
        string ITextureNode.Name => Name;

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