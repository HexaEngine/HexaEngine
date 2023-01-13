namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using ImGuiNET;
    using System.Threading.Tasks;

    public class PipelineWidget : ImGuiWindow
    {
        private Task task;

        public PipelineWidget()
        {
        }

        protected override string Name => "Pipelines";

        private void Recompile(IGraphicsPipeline pipeline)
        {
            if (task == null || task.IsCompleted)
            {
                task = Task.Factory.StartNew(pipeline.Recompile);
            }
        }

        private void Recompile(IComputePipeline pipeline)
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
                IGraphicsPipeline pipeline = PipelineManager.GraphicsPipelines[i];
                if (ImGui.Button(pipeline.Name))
                    Recompile(pipeline);
            }

            ImGui.Text("Compute Pipelines");
            for (int i = 0; i < PipelineManager.ComputePipelines.Count; i++)
            {
                IComputePipeline pipeline = PipelineManager.ComputePipelines[i];
                if (ImGui.Button(pipeline.Name))
                    Recompile(pipeline);
            }
        }
    }
}