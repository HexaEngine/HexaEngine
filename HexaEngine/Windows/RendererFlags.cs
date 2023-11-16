namespace HexaEngine.Windows
{
    /// <summary>
    /// Flags representing various rendering options for the application.
    /// </summary>
    [Flags]
    public enum RendererFlags
    {
        /// <summary>
        /// No specific rendering flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Flag indicating the use of ImGui rendering.
        /// </summary>
        ImGui = 1,

        /// <summary>
        /// Flag indicating the use of rendering for an editor.
        /// </summary>
        ImGuiWidgets = 2,

        /// <summary>
        /// Flag indicating the use of debug drawing in rendering.
        /// </summary>
        DebugDraw = 4,

        /// <summary>
        /// Combination of ImGui, Editor, and DebugDraw flags.
        /// </summary>
        All = ImGui | ImGuiWidgets | DebugDraw,

        /// <summary>
        /// Flag indicating the forceful use of forward rendering.
        /// </summary>
        ForceForward = 8,
    }
}