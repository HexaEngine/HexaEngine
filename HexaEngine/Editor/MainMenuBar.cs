namespace HexaEngine.Editor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Projects;
    using ImGuiNET;

    public static class MainMenuBar
    {
        private static float height;
        private static bool isShown = true;
        private static readonly ImportDialog importDialog = new();
        private static readonly OpenFileDialog filePicker = new(Environment.CurrentDirectory);
        private static readonly SaveFileDialog fileSaver = new(Environment.CurrentDirectory);
        private static Action<OpenFileResult, string>? filePickerCallback;
        private static Action<SaveFileResult, SaveFileDialog>? fileSaverCallback;
        private static Task? recompileShadersTask;
        private static bool recompileShadersTaskIsComplete = true;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static float Height => height;

        static MainMenuBar()
        {
            HotkeyManager.Register("Undo-Action", () => Designer.History.TryUndo(), KeyCode.LCtrl, KeyCode.Z);
            HotkeyManager.Register("Redo-Action", () => Designer.History.TryRedo(), KeyCode.LCtrl, KeyCode.Y);
        }

        internal static void Draw()
        {
            if (filePicker.Draw())
            {
                filePickerCallback?.Invoke(filePicker.Result, filePicker.SelectedFile);
            }

            if (fileSaver.Draw())
            {
                fileSaverCallback?.Invoke(fileSaver.Result, fileSaver);
            }

            importDialog.Draw();

            if (!isShown) return;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save Scene"))
                    {
                        SceneManager.Save();
                    }

                    if (ImGui.MenuItem("Import"))
                    {
                        if (!importDialog.Shown)
                        {
                            importDialog.Reset();
                            importDialog.Show();
                        }
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

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Load Project"))
                    {
                        filePicker.AllowedExtensions.Add(".hexproj");
                        filePicker.OnlyAllowFilteredExtensions = true;
                        filePickerCallback = (e, r) =>
                        {
                            if (e == OpenFileResult.Ok)
                            {
                                ProjectManager.Load(r);
                            }

                            filePicker.AllowedExtensions.Clear();
                            filePicker.OnlyAllowFilteredExtensions = false;
                        };
                        filePicker.Show();
                    }

                    if (ImGui.MenuItem("New Project"))
                    {
                        fileSaverCallback = (e, r) =>
                        {
                            if (e == SaveFileResult.Ok)
                            {
                                Directory.CreateDirectory(Path.Combine(r.CurrentFolder, r.SelectedFile));
                                ProjectManager.Create(Path.Combine(r.CurrentFolder, r.SelectedFile));
                            }
                            fileSaver.Show();
                        };
                    }

                    if (ImGui.BeginMenu("Open Recent"))
                    {
                        var entries = ProjectHistory.Entries;
                        for (int i = 0; i < entries.Count; i++)
                        {
                            var entry = entries[i];
                            if (ImGui.MenuItem(entry.Fullname))
                            {
                                ProjectManager.Load(entry.Path);
                            }
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Open Visual Studio"))
                    {
                        ProjectManager.OpenVisualStudio();
                    }

                    if (ImGui.MenuItem("Rebuild project"))
                    {
                        Task.Run(ProjectManager.UpdateScripts);
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

                    var drawLightBounds = Inspector.DrawLightBounds;
                    if (ImGui.Checkbox("Draw Light Bounds", ref drawLightBounds))
                        Inspector.DrawLightBounds = drawLightBounds;

                    var drawSkeletons = Inspector.DrawSkeletons;
                    if (ImGui.Checkbox("Draw Skeletons", ref drawSkeletons))
                        Inspector.DrawSkeletons = drawSkeletons;

                    var drawColliders = Inspector.DrawColliders;
                    if (ImGui.Checkbox("Draw Colliders", ref drawColliders))
                        Inspector.DrawColliders = drawColliders;

                    var drawBoundingBoxes = Inspector.DrawBoundingBoxes;
                    if (ImGui.Checkbox("Draw Bounding Boxes", ref drawBoundingBoxes))
                        Inspector.DrawBoundingBoxes = drawBoundingBoxes;

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Debug"))
                {
                    ImGui.TextDisabled("Shaders");
                    if (ImGui.MenuItem("Recompile Shaders", recompileShadersTaskIsComplete))
                    {
                        recompileShadersTaskIsComplete = false;
                        recompileShadersTask = Task.Run(() =>
                        {
                            ShaderCache.Clear();
                            PipelineManager.Recompile();
                        }).ContinueWith(x => { recompileShadersTask = null; recompileShadersTaskIsComplete = true; });
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