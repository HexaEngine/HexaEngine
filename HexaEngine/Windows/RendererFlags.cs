namespace HexaEngine.Windows
{
    public enum RendererFlags
    {
        None = 0,
        ImGui = 1,
        ImGuiWidgets = 2,
        DebugDraw = 4,
        All = ImGui | ImGuiWidgets | DebugDraw,
        ForceForward = 8,
    }
}