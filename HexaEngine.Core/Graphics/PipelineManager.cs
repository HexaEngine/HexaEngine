namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Logging;
    using System.Collections.Generic;

    /// <summary>
    /// A manager class responsible for managing graphics and compute pipelines in the application.
    /// </summary>
    public static class PipelineManager
    {
        private static readonly List<IGraphicsPipeline> graphicsPipelines = new();
        private static readonly List<IComputePipeline> computePipelines = new();
#nullable disable
        private static IGraphicsDevice device;
#nullable enable

        /// <summary>
        /// An event triggered when <see cref="Recompile"/> is called.
        /// </summary>
        public static event Action? OnRecompile;

        /// <summary>
        /// Gets a list of registered graphics pipelines.
        /// </summary>
        public static IReadOnlyList<IGraphicsPipeline> GraphicsPipelines => graphicsPipelines;

        /// <summary>
        /// Gets a list of registered compute pipelines.
        /// </summary>
        public static IReadOnlyList<IComputePipeline> ComputePipelines => computePipelines;

        /// <summary>
        /// Initializes the pipeline manager with the provided graphics device.
        /// </summary>
        /// <param name="device">The graphics device to associate with the pipeline manager.</param>
        public static void Initialize(IGraphicsDevice device)
        {
            PipelineManager.device = device;
        }

        /// <summary>
        /// Recompiles all registered graphics and compute pipelines.
        /// </summary>
        public static void Recompile()
        {
            lock (graphicsPipelines)
            {
                OnRecompile?.Invoke();

                LoggerFactory.General.Info("recompiling graphics pipelines ...");
                for (int i = 0; i < graphicsPipelines.Count; i++)
                {
                    graphicsPipelines[i].Recompile();
                }
                LoggerFactory.General.Info("recompiling graphics pipelines ... done!");

                LoggerFactory.General.Info("recompiling compute pipelines ...");
                for (int i = 0; i < computePipelines.Count; i++)
                {
                    computePipelines[i].Recompile();
                }
                LoggerFactory.General.Info("recompiling compute pipelines ... done!");
            }
        }

        /// <summary>
        /// Registers a graphics pipeline.
        /// </summary>
        /// <param name="pipeline">The graphics pipeline to register.</param>
        public static void Register(IGraphicsPipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                graphicsPipelines.Add(pipeline);
            }
        }

        /// <summary>
        /// Registers a compute pipeline.
        /// </summary>
        /// <param name="pipeline">The compute pipeline to register.</param>
        public static void Register(IComputePipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                computePipelines.Add(pipeline);
            }
        }

        /// <summary>
        /// Unregisters a graphics pipeline.
        /// </summary>
        /// <param name="pipeline">The graphics pipeline to unregister.</param>
        public static void Unregister(IGraphicsPipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                graphicsPipelines.Remove(pipeline);
            }
        }

        /// <summary>
        /// Unregisters a compute pipeline.
        /// </summary>
        /// <param name="pipeline">The compute pipeline to unregister.</param>
        public static void Unregister(IComputePipeline pipeline)
        {
            lock (graphicsPipelines)
            {
                computePipelines.Remove(pipeline);
            }
        }
    }
}