namespace HexaEngine.Profiling
{
    /// <summary>
    /// Represents an interface for a CPU profiler.
    /// </summary>
    public interface ICPUProfiler
    {
        /// <summary>
        /// Gets the accumulated duration for a profiling stage with the specified name.
        /// </summary>
        /// <param name="stage">The name of the profiling stage.</param>
        /// <returns>The accumulated duration of the profiling stage.</returns>
        double this[string stage] { get; }

        /// <summary>
        /// Creates a new profiling stage with the specified name.
        /// </summary>
        /// <param name="name">The name of the profiling stage to create.</param>
        void CreateStage(string name);

        /// <summary>
        /// Destroys a profiling stage with the specified name.
        /// </summary>
        /// <param name="name">The name of the profiling stage to destroy.</param>
        void DestroyStage(string name);

        /// <summary>
        /// Begins a new profiling frame.
        /// </summary>
        void BeginFrame();

        /// <summary>
        /// Begins profiling the specified stage.
        /// </summary>
        /// <param name="stage">The name of the stage to begin profiling.</param>
        void Begin(string stage);

        /// <summary>
        /// Ends profiling of the specified stage.
        /// </summary>
        /// <param name="stage">The name of the stage to end profiling.</param>
        void End(string stage);
    }
}