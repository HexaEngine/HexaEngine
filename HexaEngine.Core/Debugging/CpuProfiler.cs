namespace HexaEngine.Core.Debugging
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Represents a CPU profiler for measuring the execution time of various stages in a program.
    /// </summary>
    public class CPUProfiler : ICPUProfiler
    {
        private readonly Dictionary<string, double> stages;
        private readonly Dictionary<string, long> startTimeStamps;

        /// <summary>
        /// Initializes a new instance of the <see cref="CPUProfiler"/> class with the specified initial stage count.
        /// </summary>
        /// <param name="initialStageCount">The initial number of stages to allocate space for.</param>
        public CPUProfiler(int initialStageCount)
        {
            stages = new Dictionary<string, double>(initialStageCount);
            startTimeStamps = new Dictionary<string, long>(initialStageCount);
        }

        /// <summary>
        /// Gets the execution time in seconds for the specified stage.
        /// </summary>
        /// <param name="stage">The name of the stage to retrieve execution time for.</param>
        /// <returns>The execution time in seconds for the specified stage, or -1.0 if the stage does not exist.</returns>
        public double this[string stage]
        {
            get
            {
                if (stages.TryGetValue(stage, out var value))
                {
                    return value;
                }

                return -1.0;
            }
        }

        /// <summary>
        /// Creates a new profiling stage with the specified name.
        /// </summary>
        /// <param name="name">The name of the stage to create.</param>
        public void CreateStage(string name)
        {
        }

        /// <summary>
        /// Destroys the profiling stage with the specified name.
        /// </summary>
        /// <param name="name">The name of the stage to destroy.</param>
        public void DestroyStage(string name)
        {
        }

        /// <summary>
        /// Begins a new profiling frame, clearing all previous stage data.
        /// </summary>
        public void BeginFrame()
        {
            stages.Clear();
        }

        /// <summary>
        /// Marks the beginning of a profiling stage with the specified name.
        /// </summary>
        /// <param name="stage">The name of the stage to begin profiling.</param>
        public void Begin(string stage)
        {
            startTimeStamps.Add(stage, Stopwatch.GetTimestamp());
        }

        /// <summary>
        /// Marks the end of a profiling stage with the specified name, updating the execution time.
        /// </summary>
        /// <param name="stage">The name of the stage to end profiling.</param>
        public void End(string stage)
        {
            long timestamp = Stopwatch.GetTimestamp();
            if (!stages.TryGetValue(stage, out var value))
            {
                value = 0.0;
            }

            stages[stage] = value + (timestamp - startTimeStamps[stage]) / (double)Stopwatch.Frequency;
            startTimeStamps.Remove(stage);
        }
    }
}