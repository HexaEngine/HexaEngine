namespace HexaEngine.Editor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Graphics;
    using HexaEngine.Scenes;
    using ImGuiNET;

    public static class MainMenuBar
    {
        private static float height;
        private static bool isShown = true;
        private static FilePicker filePicker = new();
        private static bool filePickerIsOpen = false;
        private static Action<FilePickerResult, string>? filePickerCallback;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static float Height => height;

        internal static void Draw()
        {
            if (filePickerIsOpen)
            {
                if (filePicker.Draw())
                {
                    filePickerCallback?.Invoke(filePicker.Result, filePicker.SelectedFile);
                }
            }
            if (!isShown) return;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Import"))
                    {
                        filePickerIsOpen = true;
                        filePickerCallback = (r, path) =>
                        {
                            if (r == FilePickerResult.Ok)
                            {
                                AssimpSceneLoader.ImportAsync(filePicker.SelectedFile);
                            }
                            filePickerIsOpen = false;
                        };
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo (CTRL+Z)", Designer.History.CanUndo))
                    {
                        Designer.History.Undo();
                    }
                    if (ImGui.MenuItem("Redo (CTRL+Y)", Designer.History.CanRedo))
                    {
                        Designer.History.Redo();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("View"))
                {
                    WidgetManager.DrawMenu();

                    ImGui.Separator();

                    if (ImGui.MenuItem("Fullframe"))
                    {
                        Framebuffer.Fullframe = !Framebuffer.Fullframe;
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

                    var drawSkeletons = Inspector.DrawSkeletons;
                    if (ImGui.Checkbox("Draw Skeletons", ref drawSkeletons))
                        Inspector.DrawSkeletons = drawSkeletons;

                    var drawColliders = Inspector.DrawColliders;
                    if (ImGui.Checkbox("Draw Colliders", ref drawColliders))
                        Inspector.DrawColliders = drawColliders;

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Debug"))
                {
                    ImGui.TextDisabled("Shaders");
                    if (ImGui.MenuItem("Recompile Shaders (F5)"))
                    {
                        Pipeline.ReloadShaders();
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