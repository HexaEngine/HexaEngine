namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials.Nodes;

    public class PackNodeRenderer : BaseNodeRenderer<PackNode>
    {
        protected override void DrawContentBeforePins(PackNode node)
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