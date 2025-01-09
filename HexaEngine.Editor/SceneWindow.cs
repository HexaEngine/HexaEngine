namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGuizmo;
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Extensions;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public static class SceneWindow
    {
        private static bool isShown;
        private static bool isVisible;
        private static bool isFocused;
        private static bool isHovered;
        private static bool focus;
        private static bool unsavedDataDialogIsOpen;

        static SceneWindow()
        {
            SceneManager.SceneChanging += SceneChanging;
            Application.MainWindow.Closing += MainWindowClosing;
        }

        private static void SceneChanging(object? sender, SceneChangingEventArgs e)
        {
            // Do not handle reloads, no data is lost, editor saves automatically.
            if (e.ChangeType == SceneChangeType.Reload)
            {
                return;
            }

            if (UnsavedChanges)
            {
                e.Handled = true;
                if (!unsavedDataDialogIsOpen)
                {
                    MessageBox.Show("Unsaved Changes Detected!", $"Do you want to save the changes in Scene?", null, (messageBox, state) =>
                    {
                        if (messageBox.Result == MessageBoxResult.Yes)
                        {
                            SceneManager.Save();
                            SceneManager.AsyncLoad(e.NewScene, SceneInitFlags.SkipOnLoadWait);
                        }

                        if (messageBox.Result == MessageBoxResult.No)
                        {
                            UnsavedChanges = false;
                            SceneManager.AsyncLoad(e.NewScene, SceneInitFlags.SkipOnLoadWait);
                        }

                        unsavedDataDialogIsOpen = false;
                    }, MessageBoxType.YesNoCancel);
                    unsavedDataDialogIsOpen = true;
                }
            }
        }

        private static void MainWindowClosing(object? sender, Core.Windows.Events.CloseEventArgs e)
        {
            if (UnsavedChanges)
            {
                e.Handled = true;
                if (!unsavedDataDialogIsOpen)
                {
                    MessageBox.Show("Unsaved Changes Detected!", $"Do you want to save the changes in Scene?", null, (messageBox, state) =>
                    {
                        if (messageBox.Result == MessageBoxResult.Yes)
                        {
                            SceneManager.Save();
                            Application.MainWindow.Close();
                        }

                        if (messageBox.Result == MessageBoxResult.No)
                        {
                            UnsavedChanges = false;
                            Application.MainWindow.Close();
                        }

                        unsavedDataDialogIsOpen = false;
                    }, MessageBoxType.YesNoCancel);
                    unsavedDataDialogIsOpen = true;
                }
            }
        }

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static bool IsVisible => isVisible;

        public static bool IsFocused => isFocused;

        public static bool IsHovered => isHovered;

        public static Viewport RenderViewport;
        public static Viewport ImGuiViewport;
        public static Viewport SourceViewport;

        public static bool Fullframe;

        public static bool UnsavedChanges
        {
            get
            {
                return SceneManager.Current?.UnsavedChanged ?? false;
            }
            set
            {
                if (SceneManager.Current != null)
                {
                    SceneManager.Current.UnsavedChanged = value;
                }
            }
        }

        public static void Update()
        {
            ImGuizmo.SetRect(ImGuiViewport.X, ImGuiViewport.Y, ImGuiViewport.Width, ImGuiViewport.Height);
            ImGuizmo.SetOrthographic(EditorCameraController.Dimension == EditorCameraDimension.Dim2D);

            //DebugDraw.SetViewport(RenderViewport);
        }

        public static void Focus()
        {
            focus = true;
        }

        public static unsafe void Draw()
        {
            ImGuiWindowFlags flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.MenuBar;

            if (UnsavedChanges)
            {
                flags |= ImGuiWindowFlags.UnsavedDocument;
            }

            if (!ImGui.Begin("Scene"u8, ref isShown, flags))
            {
                if (focus)
                {
                    ImGuiP.FocusWindow(ImGuiP.GetCurrentWindow(), ImGuiFocusRequestFlags.UnlessBelowModal);
                }
                isVisible = false;
                ImGui.End();
                return;
            }

            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);

            isFocused = ImGui.IsWindowFocused();
            isHovered = ImGui.IsWindowHovered();

            if (Application.InEditMode && (isHovered || EditorCameraController.CapturedMouse || EditorCameraController.FirstFrame))
            {
                EditorCameraController.UpdateEditorCamera(isHovered);
            }

            if (isFocused)
            {
                HandleHotkeys();
            }

            bool inEdit = Application.InEditMode;

            ImGuizmo.SetDrawlist();
            isVisible = true;

            var scene = SceneManager.Current;
            if (scene != null)
            {
                if (inEdit && ImGui.IsWindowHovered() && ImGuiP.IsMouseDoubleClicked(ImGuiMouseButton.Left) && CameraManager.Current != null)
                {
                    var ca = CameraManager.Current;
                    Vector3 rayDir = Mouse.ScreenToWorld(ca.Transform.Projection, ca.Transform.ViewInv, RenderViewport);
                    Vector3 rayPos = ca.Transform.GlobalPosition;

                    Ray ray = new(rayPos, rayDir);

                    GameObject? gameObject = scene.SelectObject(ray);
                    if (gameObject != null)
                    {
                        if (ImGuiP.IsKeyDown(ImGuiKey.LeftCtrl))
                        {
                            SelectionCollection.Global.AddSelection(gameObject);
                        }
                        else
                        {
                            SelectionCollection.Global.AddOverwriteSelection(gameObject);
                        }
                    }
                }

                if (ImGui.BeginMenuBar())
                {
                    if (EditorCameraController.Dimension == EditorCameraDimension.Dim3D)
                    {
                        if (inEdit && ImGui.Button("3D"u8))
                        {
                            EditorCameraController.Dimension = EditorCameraDimension.Dim2D;
                        }
                    }
                    else
                    {
                        if (inEdit && ImGui.Button("2D"u8))
                        {
                            EditorCameraController.Dimension = EditorCameraDimension.Dim3D;
                        }
                    }

                    {
                        bool modeSwitched = false;
                        if (!Application.InEditMode && scene.IsSimulating)
                        {
                            ImGui.BeginDisabled(true);
                        }

                        if (ImGui.Button(builder.BuildLabel(UwU.Play)))
                        {
                            Play(scene);
                            modeSwitched = true;
                        }

                        if (!Application.InEditMode && scene.IsSimulating && !modeSwitched)
                        {
                            ImGui.EndDisabled();
                        }
                    }

                    {
                        bool modeSwitched = false;
                        if (Application.InEditMode || !scene.IsSimulating)
                        {
                            ImGui.BeginDisabled(true);
                        }

                        if (ImGui.Button(builder.BuildLabel(UwU.Pause)))
                        {
                            Pause(scene);
                            modeSwitched = true;
                        }

                        if ((Application.InEditMode || !scene.IsSimulating) && !modeSwitched)
                        {
                            ImGui.EndDisabled();
                        }
                    }

                    {
                        bool modeSwitched = false;
                        if (Application.InEditMode)
                        {
                            ImGui.BeginDisabled(true);
                        }

                        if (ImGui.Button(builder.BuildLabel(UwU.Stop)))
                        {
                            Stop(scene);
                            modeSwitched = true;
                        }

                        if (Application.InEditMode && !modeSwitched)
                        {
                            ImGui.EndDisabled();
                        }
                    }

                    int cameraIndex = scene.Cameras.ActiveCameraIndex;
                    if (inEdit && ImGui.Combo("##ActiveCamCombo"u8, ref cameraIndex, (byte**)scene.Cameras.Names.Data, scene.Cameras.Count))
                    {
                        scene.Cameras.ActiveCameraIndex = cameraIndex;
                    }

                    ImGui.Separator();

                    if (inEdit && ImGui.BeginMenu(builder.BuildLabel(UwU.Gear)))
                    {
                        ImGui.Checkbox("Fullscreen"u8, ref Fullframe);

                        ImGui.SeparatorText("Camera Settings"u8);

                        float fov = EditorCameraController.Fov;
                        if (ImGui.SliderAngle("Fov"u8, ref fov, 0, 180))
                        {
                            EditorCameraController.Fov = fov;
                        }

                        float far = EditorCameraController.Far;
                        if (ImGui.InputFloat("Far"u8, ref far))
                        {
                            EditorCameraController.Far = far;
                        }

                        float speed = EditorCameraController.Speed;
                        if (ImGui.InputFloat("Speed"u8, ref speed))
                        {
                            EditorCameraController.Speed = speed;
                        }

                        float sensitivity = EditorCameraController.MouseSensitivity;
                        if (ImGui.InputFloat("Sensitivity"u8, ref sensitivity))
                        {
                            EditorCameraController.MouseSensitivity = sensitivity;
                        }

                        ImGui.SeparatorText("Shading Mode"u8);

                        SceneRenderer? renderer = SceneRenderer.Current;

                        if (renderer != null)
                        {
                            if (ImGui.RadioButton("Wireframe"u8, renderer.Shading == ViewportShading.Wireframe))
                            {
                                renderer.Shading = ViewportShading.Wireframe;
                            }
                            if (ImGui.RadioButton("Solid"u8, renderer.Shading == ViewportShading.Solid))
                            {
                                renderer.Shading = ViewportShading.Solid;
                            }
                            if (ImGui.RadioButton("Rendered"u8, renderer.Shading == ViewportShading.Rendered))
                            {
                                renderer.Shading = ViewportShading.Rendered;
                            }
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }
            }

            if (Fullframe)
            {
                ImGuiViewport = RenderViewport = SourceViewport;
            }
            else
            {
                // Get content region.
                Vector2 vMin = ImGui.GetCursorPos();
                Vector2 vMax = ImGui.GetWindowSize();
                Vector2 wPos = ImGui.GetWindowPos();

                // Viewport window.
                var viewport = ImGui.GetWindowViewport();

                // Computes the bounds on the Swap-chain aka render viewport.
                Vector2 rVMin = vMin + (wPos - viewport.Pos);
                Vector2 rVMax = vMax + (wPos - viewport.Pos);

                Vector2 rPosition = rVMin;
                Vector2 rSize = rVMax - rVMin;

                // Computes the bounds of the rect for ImGui it's different because ImGui does something internally and subtraction is done automatically with viewport.Pos.
                Vector2 iVMin = vMin + wPos;
                Vector2 iVMax = vMax + wPos;

                Vector2 iPosition = iVMin;
                Vector2 iSize = iVMax - iVMin;

                RenderViewport = Viewport.ScaleAndCenterToFit(SourceViewport, rPosition, rSize);
                ImGuiViewport = Viewport.ScaleAndCenterToFit(SourceViewport, iPosition, iSize);
            }

            ImGui.PushItemWidth(100);

            if (Application.InEditMode)
            {
                var mode = Inspector.Mode;
                if (ComboEnumHelper<ImGuizmoMode>.Combo("##Mode", ref mode))
                {
                    Inspector.Mode = mode;
                }

                // something here causes gc pressue needs investigation.
                ImGui.PopItemWidth();

                if (ImGui.Button(builder.BuildLabel(UwU.UpDownLeftRight), new(32, 32)))
                {
                    Inspector.Operation = ImGuizmoOperation.Translate;
                }
                TooltipHelper.Tooltip("Translate"u8);

                if (ImGui.Button(builder.BuildLabel(UwU.Rotate), new(32, 32)))
                {
                    Inspector.Operation = ImGuizmoOperation.Rotate;
                }
                TooltipHelper.Tooltip("Rotate"u8);

                if (ImGui.Button(builder.BuildLabel(UwU.Maximize), new(32, 32)))
                {
                    Inspector.Operation = ImGuizmoOperation.Scale;
                }
                TooltipHelper.Tooltip("Scale"u8);

                if (ImGui.Button(builder.BuildLabel(UwU.ArrowsToDot), new(32, 32)))
                {
                    Inspector.Operation = ImGuizmoOperation.Universal;
                }
                TooltipHelper.Tooltip("Translate & Rotate & Scale"u8);

                Inspector.Draw();
            }

            ImGui.End();
        }

        private static void HandleHotkeys()
        {
            if (Application.InEditMode)
            {
                if (ImGuiP.IsKeyDown(ImGuiKey.LeftCtrl))
                {
                    if (ImGuiP.IsKeyPressed(ImGuiKey.S))
                    {
                        SceneManager.Save();
                    }
                }

                if (ImGuiP.IsKeyDown(ImGuiKey.F5))
                {
                    TransitionToState(EditorPlayState.Play);
                }
            }

            if (ImGuiP.IsKeyDown(ImGuiKey.Escape) && !Application.InEditMode)
            {
                TransitionToState(EditorPlayState.Edit);
            }
        }

        public static void TransitionToState(EditorPlayState state)
        {
            var scene = SceneManager.Current;

            if (scene == null)
            {
                return;
            }

            switch (state)
            {
                case EditorPlayState.Edit:
                    Stop(scene);
                    break;

                case EditorPlayState.Play:
                    Play(scene);
                    break;

                case EditorPlayState.Pause:
                    Pause(scene);
                    break;
            }
        }

        public static unsafe void Stop(Scene scene)
        {
            if (!Application.InEditMode)
            {
                bool shouldCancel = Application.NotifyEditorPlayStateTransition(EditorPlayState.Edit);
                if (shouldCancel)
                {
                    return;
                }
                ImGuiManager.DisableNav(false);
                scene.IsSimulating = false;
                SceneManager.BeginReload();
                scene.RestoreState();
                Application.EditorPlayState = EditorPlayState.Edit;
                SceneManager.EndReload(SceneInitFlags.SkipOnLoadWait);
            }
        }

        public static unsafe void Pause(Scene scene)
        {
            if (Application.InEditMode)
            {
                bool shouldCancel = Application.NotifyEditorPlayStateTransition(EditorPlayState.Pause);
                if (shouldCancel)
                {
                    return;
                }
                ImGuiManager.DisableNav(false);
                scene.IsSimulating = false;
                Application.EditorPlayState = EditorPlayState.Pause;
            }
        }

        public static unsafe void Play(Scene scene)
        {
            if (Application.InEditMode)
            {
                bool shouldCancel = Application.NotifyEditorPlayStateTransition(EditorPlayState.Play);
                if (shouldCancel)
                {
                    return;
                }
                if (ProjectManager.ScriptProjectChanged)
                {
                    ProjectManager.BuildScripts();
                }
                if (ProjectManager.ScriptProjectBuildFailed)
                {
                    MessageBox.Show("Script Project Build Failed", "The script project build failed. Please fix any compilation errors before playing the scene.");
                    return;
                }
                ImGuiManager.DisableNav(true);
                SceneManager.Save();
                scene.IsSimulating = false;
                SceneManager.BeginReload();
                scene.SaveState();
                Application.EditorPlayState = EditorPlayState.Play;
                scene.IsSimulating = true;
                SceneManager.EndReload(SceneInitFlags.SkipOnLoadWait);
            }
            else if (Application.InPauseState)
            {
                bool shouldCancel = Application.NotifyEditorPlayStateTransition(EditorPlayState.Play);
                if (shouldCancel)
                {
                    return;
                }
                ImGuiManager.DisableNav(true);
                scene.IsSimulating = true;
                Application.EditorPlayState = EditorPlayState.Play;
            }
        }
    }
}