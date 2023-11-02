namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using Hexa.NET.ImGui;
    using System.Threading.Tasks;

    [EditorWindowCategory("Debug")]
    public class PipelineWidget : EditorWindow
    {
        private Task task;

#pragma warning disable CS8618 // Non-nullable field 'task' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        public PipelineWidget()
#pragma warning restore CS8618 // Non-nullable field 'task' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        {
        }

        protected override string Name => "Pipelines";

        private void Recompile(IGraphicsPipeline pipeline)
        {
            pipeline.Recompile();
        }

        private void Recompile(IComputePipeline pipeline)
        {
            pipeline.Recompile();
        }

        public override void DrawContent(IGraphicsContext context)
        {
            ImGui.Text("Graphics Pipelines");
            for (int i = 0; i < PipelineManager.GraphicsPipelines.Count; i++)
            {
                IGraphicsPipeline pipeline = PipelineManager.GraphicsPipelines[i];
                if (ImGui.Button(pipeline.DebugName))
                {
                    Recompile(pipeline);
                }
            }

            ImGui.Text("Compute Pipelines");
            for (int i = 0; i < PipelineManager.ComputePipelines.Count; i++)
            {
                IComputePipeline pipeline = PipelineManager.ComputePipelines[i];
                if (ImGui.Button(pipeline.DebugName))
                {
                    Recompile(pipeline);
                }
            }
        }
    }
}