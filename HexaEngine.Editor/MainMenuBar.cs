namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using Hexa.NET.Logging;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Profiling;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using HexaEngine.Scenes;
    using HexaEngine.Windows;
    using System;
    using System.Diagnostics;
    using OpenFileDialog = Hexa.NET.ImGui.Widgets.Dialogs.OpenFileDialog;
    using SaveFileDialog = Hexa.NET.ImGui.Widgets.Dialogs.SaveFileDialog;

    public static class MainMenuBar
    {
        private static float height;
        private static bool isShown = true;

        private static bool recompileShadersTaskIsComplete = true;
        private static float progress = -1;
        private static string? progressOverlay;
        private static long progressOverlayTime;
        private static bool showImGuiDemo;
        private static IPopup? popup;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static float Height => height;

        static MainMenuBar()
        {
            HotkeyManager.Register("Undo-Action", () => History.Default.TryUndo(), Key.LCtrl, Key.Z);
            HotkeyManager.Register("Redo-Action", () => History.Default.TryRedo(), Key.LCtrl, Key.Y);
            HotkeyManager.Register("Save Scene", SceneManager.Save, Key.LCtrl, Key.S);
        }

        internal static void Init(IGraphicsDevice device)
        {
        }

        [Profile]
        internal static unsafe void Draw()
        {
            if (ImGui.IsAnyMouseDown())
            {
                //popup?.Close();
            }

            if (!isShown)
            {
                return;
            }

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Save Scene (CTRL+S)"))
                    {
                        SceneManager.Save();
                    }

                    if (ImGui.MenuItem("Unload Scene"))
                    {
                        SceneManager.Unload();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    EditSubMenu();
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("View"))
                {
                    WindowManager.DrawMenu();

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Load Project"))
                    {
                        OpenFileDialog dialog = new();
                        dialog.AllowedExtensions.Add(".hexproj");
                        dialog.OnlyAllowFilteredExtensions = true;
                        dialog.Show(OpenProjectCallback);
                    }

                    if (ImGui.MenuItem("New Project"))
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.Show(NewProjectCallback);
                    }

                    if (ImGui.MenuItem("Init Git"))
                    {
                    }

                    if (ImGui.BeginMenu("Open Recent"))
                    {
                        var entries = ProjectHistory.Entries;
                        for (int i = 0; i < entries.Count; i++)
                        {
                            var entry = entries[i];
                            if (ImGui.MenuItem(entry.Name))
                            {
                                ProjectManager.Load(entry.Path);
                            }
                            ImGui.SameLine();
                            ImGui.TextDisabled(entry.Path);
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.SeparatorText("Scripts");

                    if (ImGui.MenuItem("Build Project"))
                    {
                        ProjectManager.BuildScriptsAsync(true);
                    }

                    if (ImGui.MenuItem("Clean Project"))
                    {
                        ProjectManager.CleanScriptsAsync();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Open Text Editor"))
                    {
                        ProjectManager.OpenVisualStudio();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Inspector"))
                {
                    var enabled = Inspector.Enabled;
                    if (ImGui.Checkbox("Enabled", ref enabled))
                    {
                        Inspector.Enabled = enabled;
                    }

                    var drawGimbal = Inspector.DrawGimbal;
                    if (ImGui.Checkbox("Draw Gimbal", ref drawGimbal))
                    {
                        Inspector.DrawGimbal = drawGimbal;
                    }

                    var drawGrid = Inspector.DrawGrid;
                    if (ImGui.Checkbox("Draw Grid", ref drawGrid))
                    {
                        Inspector.DrawGrid = drawGrid;
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        int gridSize = Inspector.GridSize;
                        if (ImGui.SliderInt("Grid Size", ref gridSize, 1, 500))
                        {
                            Inspector.GridSize = gridSize;
                        }

                        ImGui.EndPopup();
                    }

                    var drawLights = (int)Inspector.DrawLights;
                    if (ImGuiP.CheckboxFlags("Draw Lights", ref drawLights, (int)EditorDrawLightsFlags.DrawLights))
                    {
                        Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                    }

                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGuiP.CheckboxFlags("No Directional Lights", ref drawLights, (int)EditorDrawLightsFlags.NoDirectionalLights))
                        {
                            Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                        }

                        if (ImGuiP.CheckboxFlags("No Point Lights", ref drawLights, (int)EditorDrawLightsFlags.NoPointLights))
                        {
                            Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                        }

                        if (ImGuiP.CheckboxFlags("No Spot Lights", ref drawLights, (int)EditorDrawLightsFlags.NoSpotLights))
                        {
                            Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                        }

                        ImGui.EndPopup();
                    }

                    var drawCameras = Inspector.DrawCameras;
                    if (ImGui.Checkbox("Draw Cameras", ref drawCameras))
                    {
                        Inspector.DrawCameras = drawCameras;
                    }

                    var drawLightBounds = Inspector.DrawLightBounds;
                    if (ImGui.Checkbox("Draw Light Bounds", ref drawLightBounds))
                    {
                        Inspector.DrawLightBounds = drawLightBounds;
                    }

                    var drawSkeletons = Inspector.DrawSkeletons;
                    if (ImGui.Checkbox("Draw Skeletons", ref drawSkeletons))
                    {
                        Inspector.DrawSkeletons = drawSkeletons;
                    }

                    var drawColliders = Inspector.DrawColliders;
                    if (ImGui.Checkbox("Draw Colliders", ref drawColliders))
                    {
                        Inspector.DrawColliders = drawColliders;
                    }

                    var drawBoundingBoxes = Inspector.DrawBoundingBoxes;
                    if (ImGui.Checkbox("Draw Bounding Boxes", ref drawBoundingBoxes))
                    {
                        Inspector.DrawBoundingBoxes = drawBoundingBoxes;
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Debug"))
                {
                    ImGui.TextDisabled("Shaders");
                    if (ImGui.MenuItem("Reload All Shaders", (byte*)null, false, recompileShadersTaskIsComplete))
                    {
                        recompileShadersTaskIsComplete = false;
                        var recompileShadersTask = Task.Run(() =>
                           {
                               ShaderCache.Clear();
                               PipelineManager.Recompile();
                           }).ContinueWith(x => { recompileShadersTaskIsComplete = true; });
                    }
                    if (ImGui.MenuItem("Reload Material Shaders", (byte*)null, false, recompileShadersTaskIsComplete))
                    {
                        recompileShadersTaskIsComplete = false;
                        var recompileShadersTask = Task.Run(() =>
                            {
                                progressOverlay = "Reload Shaders";
                                progress = 0.1f;
                                ResourceManager.Shared.RecompileShaders();
                                progress = 1;
                                progressOverlay = "Reload Shaders Done";
                            }).ContinueWith(x => { recompileShadersTaskIsComplete = true; });
                    }
                    if (ImGui.MenuItem("Take Screenshot"))
                    {
                        var win = Application.MainWindow as Window;
                        win?.Dispatcher.Invoke(() =>
                        {
                            string fileName = $"screenshot-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.png";
                            win.Renderer.TakeScreenshot(win.GraphicsContext, fileName);
                            LoggerFactory.General.Info($"Saved screenshot: {Path.GetFullPath(fileName)}");
                        });
                    }
                    if (ImGui.MenuItem("Clear Shader Cache"))
                    {
                        ShaderCache.Clear();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Console"))
                    {
                        ImGuiConsole.IsDisplayed = !ImGuiConsole.IsDisplayed;
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("ImGui Demo"))
                    {
                        showImGuiDemo = !showImGuiDemo;
                    }

                    if (ImGui.MenuItem("Progress Modal Test (Spinner)"))
                    {
                        popup = PopupManager.Show(new ProgressModal("Test progress", "Test progress"));
                    }

                    if (ImGui.MenuItem("Progress Modal Test (Bar)"))
                    {
                        popup = PopupManager.Show(new ProgressModal("Test progress", "Test progress", ProgressType.Bar));
                        ((ProgressModal)popup).Report(0.8f);
                    }

                    if (ImGui.MenuItem("Progress Import Test"))
                    {
                        ImportProgressModal modal = new("Test progress", "Test progress");
                        popup = PopupManager.Show(modal);
                        modal.Report(0.8f);
                        modal.BeginStep("Test step 1");
                        modal.LogMessage(new LogMessage(null!, LogSeverity.Error, "Test message 1"));
                        modal.EndStep();
                        modal.BeginStep("Test step 2");
                        modal.LogMessage(new LogMessage(null!, LogSeverity.Warning, "Test message 2"));
                        modal.EndStep();
                        modal.BeginStep("Test step 3");
                        modal.LogMessage(new LogMessage(null!, LogSeverity.Info, "Test message 3"));
                        modal.EndStep();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Help"))
                {
                    if (ImGui.MenuItem($"{UwU.Github} HexaEngine on GitHub"))
                    {
                        Designer.OpenLink("https://github.com/HexaEngine/HexaEngine");
                    }

                    if (ImGui.MenuItem($"{UwU.Book} HexaEngine Documentation"))
                    {
                        Designer.OpenLink("https://hexaengine.github.io/HexaEngine/");
                    }

                    if (ImGui.MenuItem($"{UwU.Bug} Report a bug"))
                    {
                        Designer.OpenLink("https://github.com/HexaEngine/HexaEngine/issues");
                    }

                    if (ImGui.MenuItem($"{UwU.Discord} Join our Discord"))
                    {
                        Designer.OpenLink("https://discord.gg/VawN5d8HMh");
                    }

                    ImGui.Separator();
                    if (ImGui.MenuItem("About"))
                    {
                        PopupManager.Show<AboutWindow>();
                    }

                    ImGui.EndMenu();
                }

                height = ImGui.GetWindowHeight();

                if (progress != -1)
                {
                    ImGui.ProgressBar(progress, new(200, 0), progressOverlay);
                    if (progress == 1 && progressOverlayTime == 0)
                    {
                        progressOverlayTime = Stopwatch.GetTimestamp() + Stopwatch.Frequency;
                    }
                    else if (progressOverlayTime != 0 && progressOverlayTime < Stopwatch.GetTimestamp())
                    {
                        progress = -1;
                        progressOverlay = null;
                        progressOverlayTime = 0;
                    }
                }

                ImGui.EndMainMenuBar();

                if (showImGuiDemo)
                {
                    ImGui.ShowDemoWindow();
                }
            }
        }

        private static void NewProjectCallback(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || sender is not SaveFileDialog dialog) return;
            Directory.CreateDirectory(dialog.SelectedFile!);
            ProjectManager.Create(dialog.SelectedFile!);
        }

        private static void OpenProjectCallback(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || sender is not OpenFileDialog dialog) return;
            ProjectManager.Load(dialog.SelectedFile!);
        }

        private static unsafe void EditSubMenu()
        {
            if (ImGui.MenuItem("Undo (CTRL+Z)", (byte*)null, false, History.Default.CanUndo))
            {
                History.Default.Undo();
            }
            if (ImGui.MenuItem("Redo (CTRL+Y)", (byte*)null, false, History.Default.CanRedo))
            {
                History.Default.Redo();
            }

            var history = History.Default;

            lock (history.SyncObject)
            {
                ImGui.Text("Undo-Stack");
                for (int i = 0; i < history.UndoCount; i++)
                {
                    var item = history.UndoStack[i];
                    if (ImGui.MenuItem(item.Item2.ActionName))
                    {
                    }
                }
                ImGui.Separator();
                ImGui.Text("Redo-Stack");
                for (int i = 0; i < history.UndoCount; i++)
                {
                    var item = history.UndoStack[i];
                    if (ImGui.MenuItem(item.Item2.ActionName))
                    {
                    }
                }
            }
        }
    }
}