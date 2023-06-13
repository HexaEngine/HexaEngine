namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.ImNodesNET;
    using ImGuiNET;
    using System.Numerics;

    public class TextureFileNode : Node
    {
        private readonly IGraphicsDevice device;
        private IShaderResourceView? image;

        [JsonIgnore]
        public Vector2 Size = new(128, 128);

        private static readonly string[] filterNames = Enum.GetNames<Filter>();
        private static readonly Filter[] filters = Enum.GetValues<Filter>();

        private static readonly string[] textureAddressModesNames = Enum.GetNames<TextureAddressMode>();
        private static readonly TextureAddressMode[] textureAddressModes = Enum.GetValues<TextureAddressMode>();

        private static readonly string[] comparisonFunctionNames = Enum.GetNames<ComparisonFunction>();
        private static readonly ComparisonFunction[] comparisonFunctions = Enum.GetValues<ComparisonFunction>();

        public string Path = string.Empty;
        public SamplerDescription Description = SamplerDescription.PointClamp;

        public TextureFileNode(IGraphicsDevice device, int id, bool removable, bool isStatic) : base(id, "Texture", removable, isStatic)
        {
            this.device = device;
        }

        public override void Initialize(NodeEditor editor)
        {
            OutColor = CreateOrGetPin(editor, "out", PinKind.Output, PinType.VectorAny, ImNodesPinShape.Quad);
            InUV = CreateOrGetPin(editor, "uv", PinKind.Input, PinType.Float2, ImNodesPinShape.CircleFilled);
            base.Initialize(editor);
        }

        [JsonIgnore]
        public Pin OutColor;

        [JsonIgnore]
        public Pin InUV;

        [JsonIgnore]
        public IShaderResourceView? Image
        {
            get => image;
            set
            {
                if (value == null)
                {
                    image = value;
                }
            }
        }

        private void Reload()
        {
            image?.Dispose();
            if (FileSystem.Exists(Paths.CurrentTexturePath + Path))
            {
                try
                {
                    var tmp = device.TextureLoader.LoadTexture2D(Paths.CurrentTexturePath + Path);
                    image = device.CreateShaderResourceView(tmp);
                    tmp.Dispose();
                }
                catch
                {
                }
            }
        }

        protected override void DrawContent()
        {
            ImGui.Image(image?.NativePointer ?? 0, Size);

            ImGui.PushItemWidth(100);

            if (ImGui.InputText("Path", ref Path, 512, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                Reload();
            }

            if (ImGui.TreeNodeEx("Test"))
            {
                int filterIndex = Array.IndexOf(filters, Description.Filter);
                if (ImGui.Combo("Filter", ref filterIndex, filterNames, filterNames.Length))
                {
                    Description.Filter = filters[filterIndex];
                }

                int addressUIndex = Array.IndexOf(textureAddressModes, Description.AddressU);
                if (ImGui.Combo("AddressU", ref addressUIndex, textureAddressModesNames, textureAddressModesNames.Length))
                {
                    Description.AddressU = textureAddressModes[addressUIndex];
                }

                int addressVIndex = Array.IndexOf(textureAddressModes, Description.AddressV);
                if (ImGui.Combo("AddressV", ref addressVIndex, textureAddressModesNames, textureAddressModesNames.Length))
                {
                    Description.AddressV = textureAddressModes[addressVIndex];
                }

                int addressWIndex = Array.IndexOf(textureAddressModes, Description.AddressW);
                if (ImGui.Combo("AddressW", ref addressWIndex, textureAddressModesNames, textureAddressModesNames.Length))
                {
                    Description.AddressW = textureAddressModes[addressWIndex];
                }

                ImGui.InputFloat("MipLODBias", ref Description.MipLODBias);
                ImGui.SliderInt("Anisotropy", ref Description.MaxAnisotropy, 1, SamplerDescription.MaxMaxAnisotropy);

                int comparisonFunctionIndex = Array.IndexOf(comparisonFunctions, Description.ComparisonFunction);
                if (ImGui.Combo("Comparison", ref comparisonFunctionIndex, comparisonFunctionNames, comparisonFunctionNames.Length))
                {
                    Description.ComparisonFunction = comparisonFunctions[comparisonFunctionIndex];
                }

                ImGui.ColorEdit4("BorderColor", ref Description.BorderColor);
                ImGui.InputFloat("MinLOD", ref Description.MinLOD);
                ImGui.InputFloat("MaxLOD", ref Description.MaxLOD);

                ImGui.TreePop();
            }

            ImGui.PopItemWidth();
        }

        public override void Destroy()
        {
            image?.Dispose();
            base.Destroy();
        }
    }
}