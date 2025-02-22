namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Materials.Nodes;

    public class SwizzleVectorNodeRenderer : BaseNodeRenderer<SwizzleVectorNode>
    {
        protected override void DrawContent(SwizzleVectorNode node)
        {
            ImGui.PushItemWidth(100);
            ImGui.Text("Swizzle");
            var mask = node.Mask;
            if (ImGui.InputText("##MaskEdit", ref mask, 5))
            {
                node.Mask = mask;
            }
            ImGui.PopItemWidth();
        }
    }
}