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

        private static ImportDialog importDialog;
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
            HotkeyManager.Register("Undo-Action", () => Designer.History.TryUndo(), Key.LCtrl, Key.Z);
            HotkeyManager.Register("Redo-Action", () => Designer.History.TryRedo(), Key.LCtrl, Key.Y);
        }

        internal static void Init(IGraphicsDevice device)
        {
            importDialog = new(device);
        }

        internal static unsafe void Draw()
        {
            if (filePicker.Draw())
            {
                filePickerCallback?.Invoke(filePicker.Result, filePicker.FullPath);
            }

            if (fileSaver.Draw())
            {
                fileSaverCallback?.Invoke(fileSaver.Result, fileSaver);
            }

            importDialog.Draw();

            if (!isShown)
            {
                return;
            }

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save Scene"))
                    {
                        SceneManager.Save();
                    }

                    if (ImGui.MenuItem("Unload Scene"))
                    {
                        SceneManager.Unload();
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
                    if (ImGui.MenuItem("Undo (CTRL+Z)", (byte*)null, false, Designer.History.CanUndo))
                    {
                        Designer.History.Undo();
                    }
                    if (ImGui.MenuItem("Redo (CTRL+Y)", (byte*)null, false, Designer.History.CanRedo))
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
                                Directory.CreateDirectory(r.FullPath);
                                ProjectManager.Create(r.FullPath);
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
                    var enabled = Frameviewer.InspectorEnabled;
                    if (ImGui.Checkbox("Enabled", ref enabled))
                    {
                        Frameviewer.InspectorEnabled = enabled;
                    }

                    var drawGrid = Frameviewer.DrawGrid;
                    if (ImGui.Checkbox("Draw Grid", ref drawGrid))
                    {
                        Frameviewer.DrawGrid = drawGrid;
                    }

                    var drawLights = Frameviewer.DrawLights;
                    if (ImGui.Checkbox("Draw Lights", ref drawLights))
                    {
                        Frameviewer.DrawLights = drawLights;
                    }

                    var drawLightBounds = Frameviewer.DrawLightBounds;
                    if (ImGui.Checkbox("Draw Light Bounds", ref drawLightBounds))
                    {
                        Frameviewer.DrawLightBounds = drawLightBounds;
                    }

                    var drawSkeletons = Frameviewer.DrawSkeletons;
                    if (ImGui.Checkbox("Draw Skeletons", ref drawSkeletons))
                    {
                        Frameviewer.DrawSkeletons = drawSkeletons;
                    }

                    var drawColliders = Frameviewer.DrawColliders;
                    if (ImGui.Checkbox("Draw Colliders", ref drawColliders))
                    {
                        Frameviewer.DrawColliders = drawColliders;
                    }

                    var drawBoundingBoxes = Frameviewer.DrawBoundingBoxes;
                    if (ImGui.Checkbox("Draw Bounding Boxes", ref drawBoundingBoxes))
                    {
                        Frameviewer.DrawBoundingBoxes = drawBoundingBoxes;
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Debug"))
                {
                    ImGui.TextDisabled("Shaders");
                    if (ImGui.MenuItem("Recompile Shaders", (byte*)null, false, recompileShadersTaskIsComplete))
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