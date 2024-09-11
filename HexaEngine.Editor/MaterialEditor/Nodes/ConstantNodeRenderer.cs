namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials.Nodes;

    public class ConstantNodeRenderer : BaseNodeRenderer<ConstantNode>
    {
        protected override void DrawContentBeforePins(ConstantNode node)
        {
            ImGui.PushItemWidth(100);
            if (ImGui.Combo("##Mode", ref node.item, node.names, node.names.Length))
            {
                node.mode = node.modes[node.item];
                node.UpdateMode();
                node.OnValueChanged();
            }
            ImGui.PopItemWidth();
        }
    }
}