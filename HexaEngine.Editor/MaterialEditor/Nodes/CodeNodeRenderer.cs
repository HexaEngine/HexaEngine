namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Materials.Nodes.Functions;
    using HexaEngine.Materials.Generator.Structs;

    public class CodeNodeRenderer : BaseNodeRendererInstanced<CodeNode>
    {
        private bool isEditSignature = false;

        protected override void DrawContentBeforePins(CodeNode node)
        {
            ImGui.PushItemWidth(100);
            if (ImGui.SmallButton($"{UwU.Pen}##EditSig{node.Id}"))
            {
                isEditSignature = !isEditSignature;
            }
            if (!isEditSignature) return;

            ImGui.Text("Return Type"u8);
            bool changed = false;
            var returnType = node.ReturnType;
            if (UIHelper.EditSType("Return", ref returnType))
            {
                node.ReturnType = returnType;
                changed = true;
            }

            ImGui.SeparatorText("Parameters"u8);

            for (int i = 0; i < node.Parameters.Count; i++)
            {
                Parameter param = node.Parameters[i];
                var paramEdited = ImGui.InputText($"##{i}", ref param.Name, 1024);
                paramEdited |= UIHelper.EditSType(param.Name, ref param.Type);
                if (paramEdited)
                {
                    node.Parameters[i] = param;
                    changed = true;
                }
            }

            if (ImGui.SmallButton($"{UwU.CirclePlus}##AddParam{node.Id}"))
            {
                node.Parameters.Add(new() { Name = "param" });
                changed = true;
            }

            if (changed)
            {
                node.Rebuild();
            }
            ImGui.PopItemWidth();
        }

        protected override void DrawContent(CodeNode node)
        {
            ImGui.PushItemWidth(300);

            string code = node.CodeBody;
            if (ImGui.InputTextMultiline("##Code", ref code, 1024, ImGuiInputTextFlags.AllowTabInput))
            {
                node.CodeBody = code;
            }

            ImGui.PopItemWidth();
        }
    }
}