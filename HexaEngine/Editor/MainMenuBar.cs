namespace HexaEngine.Editor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Projects;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Diagnostics;

    public static class MainMenuBar
    {
        private static float height;
        private static bool isShown = true;

        private static readonly OpenFileDialog filePicker = new(Environment.CurrentDirectory);

        private static readonly SaveFileDialog fileSaver = new(Environment.CurrentDirectory);
        private static Action<OpenFileResult, string>? filePickerCallback;
        private static Action<SaveFileResult, SaveFileDialog>? fileSaverCallback;
        private static Task? recompileShadersTask;
        private static bool recompileShadersTaskIsComplete = true;
        private static float progress = -1;
        private static string? progressOverlay;
        private static long progressOverlayTime;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static float Height => height;

        static MainMenuBar()
        {
            HotkeyManager.Register("Undo-Action", () => History.Default.TryUndo(), Key.LCtrl, Key.Z);
            HotkeyManager.Register("Redo-Action", () => History.Default.TryRedo(), Key.LCtrl, Key.Y);
        }

        internal static void Init(IGraphicsDevice device)
        {
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

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo (CTRL+Z)", (byte*)null, false, History.Default.CanUndo))
                    {
                        History.Default.Undo();
                    }
                    if (ImGui.MenuItem("Redo (CTRL+Y)", (byte*)null, false, History.Default.CanRedo))
                    {
                        History.Default.Redo();
                    }

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

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Build"))
                {
                    if (ImGui.MenuItem("Build Scripts"))
                    {
                        Task.Run(ProjectManager.BuildScripts);
                    }

                    if (ImGui.MenuItem("Rebuild Scripts"))
                    {
                        Task.Run(ProjectManager.RebuildScripts);
                    }

                    if (ImGui.MenuItem("Clean Scripts"))
                    {
                        Task.Run(ProjectManager.CleanScripts);
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Open Visual Studio"))
                    {
                        ProjectManager.OpenVisualStudio();
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
                    if (ImGui.MenuItem("Reload All Shaders", (byte*)null, false, recompileShadersTaskIsComplete))
                    {
                        recompileShadersTaskIsComplete = false;
                        recompileShadersTask = Task.Run(() =>
                        {
                            ShaderCache.Clear();
                            PipelineManager.Recompile();
                        }).ContinueWith(x => { recompileShadersTask = null; recompileShadersTaskIsComplete = true; });
                    }
                    if (ImGui.MenuItem("Reload Material Shaders", (byte*)null, false, recompileShadersTaskIsComplete))
                    {
                        recompileShadersTaskIsComplete = false;
                        recompileShadersTask = Task.Run(() =>
                        {
                            progressOverlay = "Reload Shaders";
                            progress = 0.1f;
                            ResourceManager.Shared.RecompileShaders();
                            progress = 1;
                            progressOverlay = "Reload Shaders Done";
                        }).ContinueWith(x => { recompileShadersTask = null; recompileShadersTaskIsComplete = true; });
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
            }
        }
    }
}