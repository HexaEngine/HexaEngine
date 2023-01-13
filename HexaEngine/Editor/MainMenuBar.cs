namespace HexaEngine.Editor
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Scenes;
    using ImGuiNET;

    public static class MainMenuBar
    {
        private static float height;
        private static bool isShown = true;
        private static readonly FilePicker filePicker = new(Environment.CurrentDirectory);
        private static readonly FileSaver fileSaver = new(Environment.CurrentDirectory);
        private static bool filePickerIsOpen = false;
        private static bool fileSaverIsOpen = false;
        private static Action<FilePickerResult, string>? filePickerCallback;
        private static Action<FilePickerResult, FileSaver>? fileSaverCallback;
        private static Task recompileShadersTask;
        private static bool recompileShadersTaskIsComplete = true;

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

            if (fileSaverIsOpen)
            {
                if (fileSaver.Draw())
                {
                    fileSaverCallback?.Invoke(fileSaver.Result, fileSaver);
                }
            }

            if (!isShown) return;

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Project"))
                    {
                        fileSaverIsOpen = true;
                        fileSaverCallback = (e, r) =>
                        {
                            if (e == FilePickerResult.Ok)
                            {
                                Directory.CreateDirectory(Path.Combine(r.CurrentFolder, r.SelectedFile));
                                Designer.CurrentProject = HexaProject.Create(Path.Combine(r.CurrentFolder, r.SelectedFile, r.SelectedFile + ".hexproj"));
                            }
                            fileSaverIsOpen = false;
                        };
                    }

                    if (ImGui.MenuItem("Load Project"))
                    {
                        filePickerIsOpen = true;
                        filePicker.AllowedExtensions.Add(".hexproj");
                        filePicker.OnlyAllowFilteredExtensions = true;
                        filePickerCallback = (e, r) =>
                        {
                            if (e == FilePickerResult.Ok)
                            {
                                Designer.CurrentProject = HexaProject.Load(r);
                            }
                            filePickerIsOpen = false;
                            filePicker.AllowedExtensions.Clear();
                            filePicker.OnlyAllowFilteredExtensions = false;
                        };
                    }

                    if (ImGui.MenuItem("New Scene"))
                    {
                        SceneManager.Load(new());
                    }

                    if (ImGui.MenuItem("Export"))
                    {
                        if (SceneManager.Current != null)
                        {
                            SceneSerializer.Serialize(SceneManager.Current);
                        }
                    }

                    if (ImGui.MenuItem("Import"))
                    {
                        filePickerIsOpen = true;
                        filePickerCallback = (r, path) =>
                        {
                            if (r == FilePickerResult.Ok)
                            {
                                Designer.OpenFile(filePicker.SelectedFile);
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

                    var drawBoundingBoxes = Inspector.DrawBoundingBoxes;
                    if (ImGui.Checkbox("Draw Bounding Boxes", ref drawBoundingBoxes))
                        Inspector.DrawBoundingBoxes = drawBoundingBoxes;

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Debug"))
                {
                    ImGui.TextDisabled("Shaders");
                    if (ImGui.MenuItem("Recompile Shaders", recompileShadersTaskIsComplete))
                    {/*
                        ShaderCache.Clear();
                        Pipeline.ReloadShaders();
                        ComputePipeline.ReloadShaders();*/
                        //TODO: Fix async recompile
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