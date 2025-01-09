namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Extensions;

    public class GraphicsDebugger : EditorWindow
    {
        private string searchString = string.Empty;
        protected override string Name { get; } = $"{UwU.BugSlash} Graphics Debugger";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);
            ImGui.InputTextWithHint("##Search"u8, "Search"u8, ref searchString, 1024);

            foreach (var pso in PipelineStateManager.GraphicsPipelineStates)
            {
                if (Filter(pso))
                    continue;

                if (ImGui.CollapsingHeader(pso.DebugName ?? "<unknown>", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Indent();

                    var desc = pso.Pipeline.Description;

                    ImGui.Text(desc.VertexShader?.Identifier ?? "none");
                    ImGui.Text(desc.HullShader?.Identifier ?? "none");
                    ImGui.Text(desc.DomainShader?.Identifier ?? "none");
                    ImGui.Text(desc.GeometryShader?.Identifier ?? "none");
                    ImGui.Text(desc.PixelShader?.Identifier ?? "none");

                    var bindings = pso.Bindings;
                    if (ImGui.CollapsingHeader(builder.BuildLabelId("SRVs"u8, pso.DebugName), ImGuiTreeNodeFlags.DefaultOpen))
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

        private bool Filter(IGraphicsPipelineState pso)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return false;
            }

            if (pso.DebugName.Contains(searchString))
            {
                return false;
            }

            var desc = pso.Pipeline.Description;

            if (desc.VertexShader?.Identifier.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.HullShader?.Identifier.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.DomainShader?.Identifier.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.GeometryShader?.Identifier.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.PixelShader?.Identifier.Contains(searchString) ?? false)
            {
                return false;
            }
            return true;
        }

        private unsafe void DisplayBindings(IEnumerable<BindingValuePair> bindings)
        {
            foreach (var binding in bindings)
            {
                if (binding.Value == null)
                {
                    ImGui.TextColored(Colors.Crimson, $"{binding.Stage}, {binding.Name}, {(nint)binding.Value:X}");
                }
                else
                {
                    ImGui.Text($"{binding.Stage}, {binding.Name}, {(nint)binding.Value:X}");
                }
            }
        }
    }
}