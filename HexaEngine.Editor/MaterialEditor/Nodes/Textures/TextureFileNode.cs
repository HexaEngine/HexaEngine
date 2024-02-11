namespace HexaEngine.Editor.MaterialEditor.Nodes.Textures
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;
    using System.Numerics;

    public class TextureFileNode : Node, ITextureNode
    {
        private IGraphicsDevice device;
        private readonly OpenFileDialog dialog = new();
        private string path = string.Empty;
        private IShaderResourceView? image;
        private ISamplerState? sampler;
        private bool changed;

        [JsonIgnore]
        public Vector2 Size = new(128, 128);

        private static readonly string[] filterNames = Enum.GetNames<Filter>();
        private static readonly Filter[] filters = Enum.GetValues<Filter>();

        private static readonly string[] textureAddressModesNames = Enum.GetNames<TextureAddressMode>();
        private static readonly TextureAddressMode[] textureAddressModes = Enum.GetValues<TextureAddressMode>();

        private static readonly string[] comparisonFunctionNames = Enum.GetNames<ComparisonFunction>();
        private static readonly ComparisonFunction[] comparisonFunctions = Enum.GetValues<ComparisonFunction>();

        private bool showMore;

        private SamplerStateDescription samplerDescription = SamplerStateDescription.AnisotropicClamp;

        public TextureFileNode(int id, bool removable, bool isStatic, IGraphicsDevice device) : base(id, "Texture", removable, isStatic)
        {
            this.device = device;
        }

        public override void Initialize(NodeEditor editor)
        {
            OutColor = CreateOrGetPin(editor, "out", PinKind.Output, PinType.AnyFloat, ImNodesPinShape.Quad);
            InUV = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "uv", ImNodesPinShape.CircleFilled, PinKind.Input, PinType.Float2, 1, defaultExpression: "pixel.uv.xy"));
            base.Initialize(editor);
        }

        [JsonIgnore]
        public IGraphicsDevice Device { set => device = value; }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                Reload();
                OnValueChanged();
            }
        }

        public SamplerStateDescription SamplerDescription { get => samplerDescription; set => samplerDescription = value; }

        [JsonIgnore]
        public Pin OutColor;

        [JsonIgnore]
        public Pin InUV { get; private set; }

        [JsonIgnore]
        public IShaderResourceView? Image { get => image; set => image = value; }

        [JsonIgnore]
        public ISamplerState? Sampler { get => sampler; set => sampler = value; }

        [JsonIgnore]
        string ITextureNode.Name => Name;

        public void Reload(bool samplerOnly = false)
        {
            sampler?.Dispose();
            image?.Dispose();

            var path = Paths.CurrentTexturePath + Path;

            if (FileSystem.Exists(path) && device != null)
            {
                try
                {
                    if (samplerOnly)
                    {
                        sampler = device.CreateSamplerState(samplerDescription);
                        return;
                    }
                    var scratchImage = device.TextureLoader.LoadFormAssets(path);
                    scratchImage.Resize(128, 128, 0);
                    var tmp = scratchImage.CreateTexture2D(device, Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, ResourceMiscFlag.None);
                    scratchImage.Dispose();
                    image = device.CreateShaderResourceView(tmp);
                    tmp.Dispose();

                    sampler = device.CreateSamplerState(samplerDescription);
                }
                catch
                {
                }
            }
        }

        public override void Draw()
        {
            if (dialog.Draw())
            {
                if (dialog.Result == OpenFileResult.Ok)
                {
                    Path = System.IO.Path.GetRelativePath("textures", FileSystem.GetRelativePath(dialog.FullPath));
                    Reload();
                }
            }
            base.Draw();
        }

        protected override void DrawContent()
        {
            ImGui.Image(image?.NativePointer ?? 0, Size);

            ImGui.PushItemWidth(100);

            if (ImGui.InputText("Path", ref path, 512, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                Reload();
                OnValueChanged();
            }

            ImGui.SameLine();

            if (ImGui.Button("...##OpenFileButton"))
            {
                dialog.Show();
            }

            if (!showMore && ImGui.Button("more..."))
            {
                showMore = true;
            }

            if (showMore && ImGui.Button("less..."))
            {
                showMore = false;
            }

            if (showMore)
            {
                bool active = false;
                int filterIndex = Array.IndexOf(filters, samplerDescription.Filter);
                if (ImGui.Combo("Filter", ref filterIndex, filterNames, filterNames.Length))
                {
                    samplerDescription.Filter = filters[filterIndex];
                    changed = true;
                }
                active |= ImGui.IsItemActive();

                int addressUIndex = Array.IndexOf(textureAddressModes, samplerDescription.AddressU);
                if (ImGui.Combo("AddressU", ref addressUIndex, textureAddressModesNames, textureAddressModesNames.Length))
                {
                    samplerDescription.AddressU = textureAddressModes[addressUIndex];
                    changed = true;
                }
                active |= ImGui.IsItemActive();

                int addressVIndex = Array.IndexOf(textureAddressModes, samplerDescription.AddressV);
                if (ImGui.Combo("AddressV", ref addressVIndex, textureAddressModesNames, textureAddressModesNames.Length))
                {
                    samplerDescription.AddressV = textureAddressModes[addressVIndex];
                    changed = true;
                }
                active |= ImGui.IsItemActive();

                int addressWIndex = Array.IndexOf(textureAddressModes, samplerDescription.AddressW);
                if (ImGui.Combo("AddressW", ref addressWIndex, textureAddressModesNames, textureAddressModesNames.Length))
                {
                    samplerDescription.AddressW = textureAddressModes[addressWIndex];
                    changed = true;
                }
                active |= ImGui.IsItemActive();

                if (ImGui.InputFloat("MipLODBias", ref samplerDescription.MipLODBias))
                {
                    changed = true;
                }
                active |= ImGui.IsItemActive();
                if (ImGui.SliderInt("Anisotropy", ref samplerDescription.MaxAnisotropy, 1, SamplerStateDescription.MaxMaxAnisotropy))
                {
                    changed = true;
                }
                active |= ImGui.IsItemActive();

                int comparisonFunctionIndex = Array.IndexOf(comparisonFunctions, samplerDescription.ComparisonFunction);
                if (ImGui.Combo("Comparison", ref comparisonFunctionIndex, comparisonFunctionNames, comparisonFunctionNames.Length))
                {
                    samplerDescription.ComparisonFunction = comparisonFunctions[comparisonFunctionIndex];
                    changed = true;
                }
                active |= ImGui.IsItemActive();

                if (ImGui.ColorEdit4("BorderColor", ref samplerDescription.BorderColor))
                {
                    changed = true;
                }
                active |= ImGui.IsItemActive();
                if (ImGui.InputFloat("MinLOD", ref samplerDescription.MinLOD))
                {
                    changed = true;
                }
                active |= ImGui.IsItemActive();
                if (ImGui.InputFloat("MaxLOD", ref samplerDescription.MaxLOD))
                {
                    changed = true;
                }
                active |= ImGui.IsItemActive();

                if (!active && changed)
                {
                    changed = false;
                    Reload(true);
                }
            }

            ImGui.PopItemWidth();
        }

        public override void Destroy()
        {
            image?.Dispose();
            sampler?.Dispose();
            base.Destroy();
        }

        public static TextureFileNode? FindTextureFileNode(NodeEditor editor, string path)
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