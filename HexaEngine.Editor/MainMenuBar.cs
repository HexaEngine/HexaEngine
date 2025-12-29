namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using Hexa.NET.Logging;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Extensions;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Editor.UI;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Profiling;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using HexaEngine.Scenes;
    using HexaEngine.Windows;
    using System;
    using System.Diagnostics;
    using IPopup = Dialogs.IPopup;
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
        internal static unsafe void Draw(TitleBarContext context)
        {
            if (ImGui.IsAnyMouseDown())
            {
                //popup?.Close();
            }

            if (!isShown)
            {
                return;
            }

            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);

            if (ImGui.BeginMenu("File"u8))
            {
                if (ImGui.MenuItem("Save Scene"u8, "Ctrl+S"u8))
                {
                    SceneManager.Save();
                }

                if (ImGui.MenuItem("Unload Scene"u8))
                {
                    SceneManager.Unload();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"u8))
            {
                EditSubMenu();
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("View"u8))
            {
                WindowManager.DrawMenu();

                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Project"u8))
            {
                if (ImGui.MenuItem("Load Project"u8))
                {
                    OpenFileDialog dialog = new();
                    dialog.AllowedExtensions.Add(".hexproj");
                    dialog.OnlyAllowFilteredExtensions = true;
                    dialog.Show(OpenProjectCallback);
                }

                if (ImGui.MenuItem("New Project"u8))
                {
                    SaveFileDialog dialog = new();
                    dialog.Show(NewProjectCallback);
                }

                if (ImGui.MenuItem("Init Git"u8))
                {
                }

                if (ImGui.BeginMenu("Open Recent"u8))
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

                ImGui.SeparatorText("Scripts"u8);

                if (ImGui.MenuItem("Build Project"u8))
                {
                    ProjectManager.BuildScriptsAsync(true);
                }

                if (ImGui.MenuItem("Clean Project"u8))
                {
                    ProjectManager.CleanScriptsAsync();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Open Text Editor"u8))
                {
                    ProjectManager.OpenVisualStudio();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Inspector"u8))
            {
                var enabled = Inspector.Enabled;
                if (ImGui.Checkbox("Enabled"u8, ref enabled))
                {
                    Inspector.Enabled = enabled;
                }

                var drawGimbal = Inspector.DrawGimbal;
                if (ImGui.Checkbox("Draw Gimbal"u8, ref drawGimbal))
                {
                    Inspector.DrawGimbal = drawGimbal;
                }

                var drawGrid = Inspector.DrawGrid;
                if (ImGui.Checkbox("Draw Grid"u8, ref drawGrid))
                {
                    Inspector.DrawGrid = drawGrid;
                }

                if (ImGui.BeginPopupContextItem())
                {
                    int gridSize = Inspector.GridSize;
                    if (ImGui.SliderInt("Grid Size"u8, ref gridSize, 1, 500))
                    {
                        Inspector.GridSize = gridSize;
                    }

                    ImGui.EndPopup();
                }

                var drawLights = (int)Inspector.DrawLights;
                if (ImGui.CheckboxFlags("Draw Lights"u8, ref drawLights, (int)EditorDrawLightsFlags.DrawLights))
                {
                    Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.CheckboxFlags("No Directional Lights"u8, ref drawLights, (int)EditorDrawLightsFlags.NoDirectionalLights))
                    {
                        Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                    }

                    if (ImGui.CheckboxFlags("No Point Lights"u8, ref drawLights, (int)EditorDrawLightsFlags.NoPointLights))
                    {
                        Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                    }

                    if (ImGui.CheckboxFlags("No Spot Lights"u8, ref drawLights, (int)EditorDrawLightsFlags.NoSpotLights))
                    {
                        Inspector.DrawLights = (EditorDrawLightsFlags)drawLights;
                    }

                    ImGui.EndPopup();
                }

                var drawCameras = Inspector.DrawCameras;
                if (ImGui.Checkbox("Draw Cameras"u8, ref drawCameras))
                {
                    Inspector.DrawCameras = drawCameras;
                }

                var drawLightBounds = Inspector.DrawLightBounds;
                if (ImGui.Checkbox("Draw Light Bounds"u8, ref drawLightBounds))
                {
                    Inspector.DrawLightBounds = drawLightBounds;
                }

                var drawSkeletons = Inspector.DrawSkeletons;
                if (ImGui.Checkbox("Draw Skeletons"u8, ref drawSkeletons))
                {
                    Inspector.DrawSkeletons = drawSkeletons;
                }

                var drawColliders = Inspector.DrawColliders;
                if (ImGui.Checkbox("Draw Colliders"u8, ref drawColliders))
                {
                    Inspector.DrawColliders = drawColliders;
                }

                var drawBoundingBoxes = Inspector.DrawBoundingBoxes;
                if (ImGui.Checkbox("Draw Bounding Boxes"u8, ref drawBoundingBoxes))
                {
                    Inspector.DrawBoundingBoxes = drawBoundingBoxes;
                }

                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Debug"u8))
            {
                ImGui.TextDisabled("Shaders"u8);
                if (ImGui.MenuItem("Reload All Shaders"u8, (byte*)null, false, recompileShadersTaskIsComplete))
                {
                    recompileShadersTaskIsComplete = false;
                    var recompileShadersTask = Task.Run(() =>
                       {
                           ShaderCache.Clear();
                           PipelineManager.Recompile();
                       }).ContinueWith(x => { recompileShadersTaskIsComplete = true; });
                }
                if (ImGui.MenuItem("Reload Material Shaders"u8, (byte*)null, false, recompileShadersTaskIsComplete))
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
                if (ImGui.MenuItem("Take Screenshot"u8))
                {
                    var win = Application.MainWindow as Window;
                    win?.Dispatcher.Invoke(() =>
                    {
                        string fileName = $"screenshot-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.png";
                        win.Renderer.TakeScreenshot(win.GraphicsContext, fileName);
                        LoggerFactory.General.Info($"Saved screenshot: {Path.GetFullPath(fileName)}");
                    });
                }
                if (ImGui.MenuItem("Clear Shader Cache"u8))
                {
                    ShaderCache.Clear();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Console"u8))
                {
                    ImGuiConsole.IsDisplayed = !ImGuiConsole.IsDisplayed;
                }

                ImGui.Separator();

                if (ImGui.MenuItem("ImGui Demo"u8))
                {
                    showImGuiDemo = !showImGuiDemo;
                }

                if (ImGui.MenuItem("Progress Modal Test (Spinner)"u8))
                {
                    popup = PopupManager.Show(new ProgressModal("Test progress", "Test progress"));
                }

                if (ImGui.MenuItem("Progress Modal Test (Bar)"u8))
                {
                    popup = PopupManager.Show(new ProgressModal("Test progress", "Test progress", ProgressType.Bar));
                    ((ProgressModal)popup).Report(0.8f);
                }

                if (ImGui.MenuItem("Progress Import Test"u8))
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

            if (ImGui.BeginMenu("Layout"u8))
            {
                if (ImGui.MenuItem("Save Layout"u8))
                {
                    CreateFileDialog dialog = new(LayoutManager.BasePath);
                    dialog.Extension = ".ini";
                    dialog.Show(CreateNewLayoutCallback);
                }
                if (ImGui.BeginMenu("Apply Layout"u8))
                {
                    foreach (var layout in LayoutManager.Layouts)
                    {
                        if (ImGui.MenuItem(layout.Name, LayoutManager.SelectedLayout == layout.Path))
                        {
                            LayoutManager.SelectedLayout = layout.Path;
                        }
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.MenuItem("Manage Layouts"u8))
                {
                    WindowManager.ShowWindow<LayoutsWidget>();
                }
                if (ImGui.MenuItem("Reset Layout"u8))
                {
                    LayoutManager.ResetLayout();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help"u8))
            {
                if (ImGui.MenuItem(builder.BuildLabel(UwU.Github, " HexaEngine on GitHub"u8)))
                {
                    Designer.OpenLink("https://github.com/HexaEngine/HexaEngine");
                }

                if (ImGui.MenuItem(builder.BuildLabel(UwU.Book, " HexaEngine Documentation"u8)))
                {
                    Designer.OpenLink("https://hexaengine.github.io/HexaEngine/");
                }

                if (ImGui.MenuItem(builder.BuildLabel(UwU.Bug, " Report a bug"u8)))
                {
                    Designer.OpenLink("https://github.com/HexaEngine/HexaEngine/issues");
                }

                if (ImGui.MenuItem(builder.BuildLabel(UwU.Discord, " Join our Discord"u8)))
                {
                    Designer.OpenLink("https://discord.gg/VawN5d8HMh");
                }

                ImGui.Separator();
                if (ImGui.MenuItem("About"u8))
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
        }

        private static void CreateNewLayoutCallback(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || sender is not CreateFileDialog dialog) return;
            LayoutManager.CreateNewLayout(dialog.FileName);
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