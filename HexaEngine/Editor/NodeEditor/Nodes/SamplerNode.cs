namespace HexaEngine.Editor.NodeEditor.Nodes
{
    using HexaEngine.Core.Graphics;
    using ImGuiNET;
    using System;

    public class SamplerNode : Node
    {
        private static readonly string[] filterNames = Enum.GetNames<Filter>();
        private static readonly Filter[] filters = Enum.GetValues<Filter>();

        private static readonly string[] textureAddressModesNames = Enum.GetNames<TextureAddressMode>();
        private static readonly TextureAddressMode[] textureAddressModes = Enum.GetValues<TextureAddressMode>();

        private static readonly string[] comparisonFunctionNames = Enum.GetNames<ComparisonFunction>();
        private static readonly ComparisonFunction[] comparisonFunctions = Enum.GetValues<ComparisonFunction>();

        private SamplerDescription description = SamplerDescription.PointClamp;

        public SamplerNode(NodeEditor graph, string name, bool removable, bool isStatic) : base(graph, name, removable, isStatic)
        {
            CreatePin("out", PinKind.Output, PinType.Sampler, ImNodesNET.PinShape.TriangleFilled);
        }

        public SamplerDescription Description => description;

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            int filterIndex = Array.IndexOf(filters, description.Filter);
            if (ImGui.Combo("Filter", ref filterIndex, filterNames, filterNames.Length))
            {
                description.Filter = filters[filterIndex];
            }

            int addressUIndex = Array.IndexOf(textureAddressModes, description.AddressU);
            if (ImGui.Combo("AddressU", ref addressUIndex, textureAddressModesNames, textureAddressModesNames.Length))
            {
                description.AddressU = textureAddressModes[addressUIndex];
            }

            int addressVIndex = Array.IndexOf(textureAddressModes, description.AddressV);
            if (ImGui.Combo("AddressV", ref addressVIndex, textureAddressModesNames, textureAddressModesNames.Length))
            {
                description.AddressV = textureAddressModes[addressVIndex];
            }

            int addressWIndex = Array.IndexOf(textureAddressModes, description.AddressW);
            if (ImGui.Combo("AddressW", ref addressWIndex, textureAddressModesNames, textureAddressModesNames.Length))
            {
                description.AddressW = textureAddressModes[addressWIndex];
            }

            ImGui.InputFloat("MipLODBias", ref description.MipLODBias);
            ImGui.SliderInt("Anisotropy", ref description.MaxAnisotropy, 1, SamplerDescription.MaxMaxAnisotropy);

            int comparisonFunctionIndex = Array.IndexOf(comparisonFunctions, description.ComparisonFunction);
            if (ImGui.Combo("Comparison", ref comparisonFunctionIndex, comparisonFunctionNames, comparisonFunctionNames.Length))
            {
                description.ComparisonFunction = comparisonFunctions[comparisonFunctionIndex];
            }

            ImGui.ColorEdit4("BorderColor", ref description.BorderColor);
            ImGui.InputFloat("MinLOD", ref description.MinLOD);
            ImGui.InputFloat("MaxLOD", ref description.MaxLOD);

            ImGui.PopItemWidth();
        }
    }
}