namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Nodes.Functions;

    public class TypedNodeBaseRenderer : BaseNodeRenderer<TypedNodeBase>
    {
        protected override void DrawContentBeforePins(TypedNodeBase node)
        {
            if (node.lockType)
            {
                return;
            }

            ImGui.PushItemWidth(100);
            if (ImGui.Combo("##Mode", ref node.item, node.names, node.names.Length))
            {
                node.mode = node.modes[node.item];
                node.UpdateMode();
            }
            ImGui.PopItemWidth();
        }
    }

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