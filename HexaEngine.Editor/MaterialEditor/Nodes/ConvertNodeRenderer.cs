namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using HexaEngine.Materials.Nodes;

    public class ConvertNodeRenderer : BaseNodeRenderer<ConvertNode>
    {
        protected override void DrawContent(ConvertNode node)
        {
            ImGui.PushItemWidth(100);
            ImGui.InputFloat("##Value", ref node.Value);
            ImGui.PopItemWidth();
        }
    }

    public class FlipUVNodeRenderer : BaseNodeRenderer<FlipUVNode>
    {
        protected override void DrawContent(FlipUVNode node)
        {
            ImGui.PushItemWidth(100);
            if (ComboEnumHelper<FlipMode>.Combo("Mode", ref node.FlipMode))
            {
                node.OnValueChanged();
            }
            ImGui.PopItemWidth();
        }
    }
}