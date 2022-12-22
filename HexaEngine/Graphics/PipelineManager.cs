namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public static class PipelineManager
    {
        private static readonly List<GraphicsPipeline> graphicsPipelines = new();
        private static readonly List<ComputePipeline> computePipelines = new();
#nullable disable
        private static IGraphicsDevice device;
#nullable enable

        public static event Action? OnRecompile;

        public static IReadOnlyList<GraphicsPipeline> GraphicsPipelines => graphicsPipelines;

        public static IReadOnlyList<ComputePipeline> ComputePipelines => computePipelines;

        public static void Initialize(IGraphicsDevice device)
        {
            PipelineManager.device = device;
        }

        public static void Recompile()
        {
            lock (graphicsPipelines)
            {
                OnRecompile?.Invoke();

                ImGuiConsole.Log(LogSeverity.Info, "recompiling graphics pipelines ...");
                for (int i = 0; i < graphicsPipelines.Count; i++)
                {
                    graphicsPipelines[i].Recompile();
                }
                ImGuiConsole.Log(LogSeverity.Info, "recompiling graphics pipelines ... done!");

                ImGuiConsole.Log(LogSeverity.Info, "recompiling compute pipelines ...");
                for (int i = 0; i < computePipelines.Count; i++)
                {
                    computePipelines[i].Recompile();
                }
                ImGuiConsole.Log(LogSeverity.Info, "recompiling compute pipelines ... done!");
            }
        }

        internal static void Register(GraphicsPipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                graphicsPipelines.Add(pipeline);
            }
        }

        internal static void Register(ComputePipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                computePipelines.Add(pipeline);
            }
        }

        internal static void Unregister(GraphicsPipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                graphicsPipelines.Remove(pipeline);
            }
        }

        internal static void Unregister(ComputePipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                computePipelines.Remove(pipeline);
            }
        }
    }
}