namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using HexaEngine.Materials.Nodes;

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