namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials.Nodes;

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
}