namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using ImGuiNET;
    using System.Threading.Tasks;

    public class PipelineWidget : ImGuiWindow
    {
        private Task task;

        public PipelineWidget()
        {
        }

        protected override string Name => "Pipelines";

        private void Recompile(GraphicsPipeline pipeline)
        {
            if (task == null || task.IsCompleted)
            {
                task = Task.Factory.StartNew(pipeline.Recompile);
            }
        }

        private void Recompile(ComputePipeline pipeline)
        {
            if (task == null || task.IsCompleted)
            {
                task = Task.Factory.StartNew(pipeline.Recompile);
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            ImGui.Text("Graphics Pipelines");
            for (int i = 0; i < PipelineManager.GraphicsPipelines.Count; i++)
            {
                GraphicsPipeline pipeline = PipelineManager.GraphicsPipelines[i];
                if (ImGui.Button(pipeline.Name))
                    Recompile(pipeline);
            }

            ImGui.Text("Compute Pipelines");
            for (int i = 0; i < PipelineManager.ComputePipelines.Count; i++)
            {
                ComputePipeline pipeline = PipelineManager.ComputePipelines[i];
                if (ImGui.Button(pipeline.Name))
                    Recompile(pipeline);
            }
        }
    }
}