namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.UI;

    /// <summary>
    /// Represents an interface for a CPU flame profiler.
    /// </summary>
    public interface ICPUFlameProfiler : ICPUProfiler
    {
        /// <summary>
        /// Gets a reference to a profiling scope at the specified index.
        /// </summary>
        /// <param name="index">The index of the scope to get.</param>
        /// <returns>A reference to the profiling scope.</returns>
        ref ProfilerScope this[int index] { get; }

        /// <summary>
        /// Gets the pointer to the current entry.
        /// </summary>
        unsafe ProfilerEntry* Current { get; }

        /// <summary>
        /// Gets the values getter for an ImGui widget flame graph.
        /// </summary>
        ImGuiWidgetFlameGraph.ValuesGetter Getter { get; }

        /// <summary>
        /// Gets the number of stages in the profiler.
        /// </summary>
        int StageCount { get; }

        /// <summary>
        /// Destroys a profiling stage with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the stage to destroy.</param>
        void DestroyStage(uint id);

        /// <summary>
        /// Ends the current profiling frame.
        /// </summary>
        void EndFrame();

        /// <summary>
        /// Gets the index of the current entry.
        /// </summary>
        /// <returns>The index of the current entry.</returns>
        int GetCurrentEntryIndex();
    }
}