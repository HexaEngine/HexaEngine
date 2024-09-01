namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using System;

    public class GraphicsDebugger : EditorWindow
    {
        private string searchString = string.Empty;
        protected override string Name { get; } = $"{UwU.BugSlash} Graphics Debugger";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            ImGui.InputTextWithHint("##Search", "Search", ref searchString, 1024);

            foreach (var pso in PipelineStateManager.GraphicsPipelineStates)
            {
                if (Filter(pso))
                    continue;

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

            if (desc.VertexShader?.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.HullShader?.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.DomainShader?.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.GeometryShader?.Contains(searchString) ?? false)
            {
                return false;
            }
            if (desc.PixelShader?.Contains(searchString) ?? false)
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