namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;

    public class GraphicsDebugger : EditorWindow
    {
        protected override string Name { get; } = $"{UwU.BugSlash} Graphics Debugger";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            foreach (var pso in PipelineStateManager.GraphicsPipelineStates)
            {
                if (ImGui.CollapsingHeader(pso.DebugName ?? "<unknown>", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Indent();

                    var desc = pso.Pipeline.Description;

                    ImGui.Text(desc.VertexShader ?? "none");
                    ImGui.Text(desc.HullShader ?? "none");
                    ImGui.Text(desc.DomainShader ?? "none");
                    ImGui.Text(desc.GeometryShader ?? "none");
                    ImGui.Text(desc.PixelShader ?? "none");

                    var bindings = pso.Bindings;
                    if (ImGui.CollapsingHeader($"SRVs##{pso.DebugName}", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        DisplayBindings(bindings.SRVs);
                    }
                    if (ImGui.CollapsingHeader($"CBVs##{pso.DebugName}", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        DisplayBindings(bindings.CBVs);
                    }
                    if (ImGui.CollapsingHeader($"Samplers##{pso.DebugName}", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        DisplayBindings(bindings.Samplers);
                    }
                    ImGui.Unindent();
                }
            }
        }

        private unsafe void DisplayBindings(IEnumerable<BindingValuePair> bindings)
        {
            foreach (var binding in bindings)
            {
                if (binding.Value == null)
                {
                    ImGui.TextColored(Colors.Crimson, $"{binding.Stage}, {binding.Name}, {(nint)binding.Value}");
                }
                else
                {
                    ImGui.Text($"{binding.Stage}, {binding.Name}, {(nint)binding.Value}");
                }
            }
        }
    }
}