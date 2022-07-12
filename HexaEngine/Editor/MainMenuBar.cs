namespace HexaEngine.Editor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using Newtonsoft.Json;
    using System.IO;

    public static class MainMenuBar
    {
        private static float height;
        private static bool isShown;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static float Height => height;

        internal static void Draw()
        {
            if (!isShown) return;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save scene"))
                    {
                        File.WriteAllText("scene.json", JsonConvert.SerializeObject(SceneManager.Current, Formatting.Indented));
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Scene"))
                {
                    if (ImGui.MenuItem("Layout"))
                    {
                        SceneLayout.IsShown = !SceneLayout.IsShown;
                    }
                    if (ImGui.MenuItem("Materials"))
                    {
                        SceneMaterials.IsShown = !SceneMaterials.IsShown;
                    }
                    if (ImGui.MenuItem("Properties"))
                    {
                        SceneElementProperties.IsShown = !SceneElementProperties.IsShown;
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Inspector"))
                {
                    var enabled = Inspector.Enabled;
                    if (ImGui.Checkbox("Enabled", ref enabled))
                        Inspector.Enabled = enabled;

                    var drawGrid = Inspector.DrawGrid;
                    if (ImGui.Checkbox("Draw Grid", ref drawGrid))
                        Inspector.DrawGrid = drawGrid;

                    var drawLights = Inspector.DrawLights;
                    if (ImGui.Checkbox("Draw Lights", ref drawLights))
                        Inspector.DrawLights = drawLights;

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Debug"))
                {
                    ImGui.TextDisabled("Shaders");
                    if (ImGui.MenuItem("Framebuffers"))
                    {
                        FramebufferDebugger.IsShown = !FramebufferDebugger.IsShown;
                    }
                    ImGui.Separator();

                    if (ImGui.MenuItem("Console"))
                    {
                        ImGuiConsole.IsDisplayed = !ImGuiConsole.IsDisplayed;
                    }
                    ImGui.EndMenu();
                }

                height = ImGui.GetWindowHeight();

                ImGui.EndMainMenuBar();
            }
        }
    }
}