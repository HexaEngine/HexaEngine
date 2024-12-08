namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials.Nodes;

    public class ComponentMaskNodeRenderer : BaseNodeRenderer<ComponentMaskNode>
    {
        protected override void DrawContent(ComponentMaskNode node)
        {
            ImGui.PushItemWidth(100);
            ImGui.Text("Component Mask");
            if (ImGui.InputText("##MaskEdit", ref node.mask, 5))
            {
                node.UpdateOutput();
            }
            ImGui.PopItemWidth();
        }
    }
}