namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials.Nodes;

    public class TypedNodeBaseRenderer : BaseNodeRenderer<TypedNodeBase>
    {
        protected override void DrawContentBeforePins(TypedNodeBase node)
        {
            if (node is InferTypedNodeBase inferTypedNode)
            {
                if (inferTypedNode.InferredType || !inferTypedNode.EnableManualSelection)
                {
                    return;
                }
            }

            if (node.LockType)
            {
                return;
            }

            ImGui.PushItemWidth(100);
            int index = node.ModeIndex;
            if (ImGui.Combo("##Mode", ref index, node.ModesComboString))
            {
                node.Mode = node.Modes[index];
            }
            ImGui.PopItemWidth();
        }
    }
}