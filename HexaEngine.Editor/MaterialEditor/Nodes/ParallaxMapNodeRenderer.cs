namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using HexaEngine.Materials.Nodes.Functions;

    public class ParallaxMapNodeRenderer : BaseNodeRenderer<ParallaxMapNode>
    {
        protected override void DrawContentBeforePins(ParallaxMapNode node)
        {
            ImGui.PushItemWidth(100);

            var mode = node.ParallaxMode;
            if (ComboEnumHelper<ParallaxMode>.Combo("Mode", ref mode))
            {
                node.ParallaxMode = mode;
            }

            var maxLayer = node.MaxLayers;
            if (ImGui.InputInt("Max Layers", ref maxLayer))
            {
                node.MaxLayers = maxLayer;
            }

            var minLayer = node.MinLayers;
            if (ImGui.InputInt("Min Layers", ref minLayer))
            {
                node.MinLayers = minLayer;
            }

            ImGui.PopItemWidth();
        }
    }
}