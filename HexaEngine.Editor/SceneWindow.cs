namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGuizmo;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
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
                return;

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
                            SceneManager.AsyncLoad(e.NewScene);
                        }

                        if (messageBox.Result == MessageBoxResult.No)
                        {
                            UnsavedChanges = false;
                            SceneManager.AsyncLoad(e.NewScene);
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
            DebugDraw.SetViewport(RenderViewport);
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

            if (!ImGui.Begin("Scene", ref isShown, flags))
            {
                if (focus)
                {
                    ImGui.FocusWindow(ImGui.GetCurrentWindow(), ImGuiFocusRequestFlags.UnlessBelowModal);
                }
                isVisible = false;
                ImGui.End();
                return;
            }

            isFocused = ImGui.IsWindowFocused();
            isHovered = ImGui.IsWindowHovered();

            if (isHovered || EditorCameraController.CapturedMouse || EditorCameraController.FirstFrame)
            {
                EditorCameraController.UpdateEditorCamera(isHovered);
            }

            if (isFocused)
            {
                HandleHotkeys();
            }

            ImGuizmo.SetDrawlist();
            isVisible = true;

            var scene = SceneManager.Current;
            if (scene != null)
            {
                if (ImGui.IsWindowHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && CameraManager.Current != null)
                {
                    var ca = CameraManager.Current;
                    Vector3 rayDir = Mouse.ScreenToWorld(ca.Transform.Projection, ca.Transform.ViewInv, RenderViewport);
                    Vector3 rayPos = ca.Transform.GlobalPosition;

                    Ray ray = new(rayPos, rayDir);

                    GameObject? gameObject = scene.SelectObject(ray);
                    if (gameObject != null)
                    {
                        if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
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
                        if (ImGui.Button("3D"))
                        {
                            EditorCameraController.Dimension = EditorCameraDimension.Dim2D;
                        }
                    }
                    else
                    {
                        if (ImGui.Button("2D"))
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

                        if (ImGui.Button("\xE768")) // Play "\xE768"
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

                        if (ImGui.Button("\xE769")) // Pause "\xE769"
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

                        if (ImGui.Button("\xE71A")) // Stop "xE71A"
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
                    if (ImGui.Combo("##ActiveCamCombo", ref cameraIndex, (byte**)scene.Cameras.Names.Data, scene.Cameras.Count))
                    {
                        scene.Cameras.ActiveCameraIndex = cameraIndex;
                    }

                    ImGui.Separator();

                    if (ImGui.BeginMenu("\xE713"))
                    {
                        ImGui.Checkbox("Fullscreen", ref Fullframe);

                        ImGui.SeparatorText("Camera Settings");

                        float fov = EditorCameraController.Fov;
                        if (ImGui.SliderAngle("Fov", ref fov, 0, 180))
                        {
                            EditorCameraController.Fov = fov;
                        }

                        float far = EditorCameraController.Far;
                        if (ImGui.InputFloat("Far", ref far))
                        {
                            EditorCameraController.Far = far;
                        }

                        float speed = EditorCameraController.Speed;
                        if (ImGui.InputFloat("Speed", ref speed))
                        {
                            EditorCameraController.Speed = speed;
                        }

                        float sensitivity = EditorCameraController.MouseSensitivity;
                        if (ImGui.InputFloat("Sensitivity", ref sensitivity))
                        {
                            EditorCameraController.MouseSensitivity = sensitivity;
                        }

                        ImGui.SeparatorText("Shading Mode");

                        SceneRenderer? renderer = SceneRenderer.Current;

                        if (renderer != null)
                        {
                            if (ImGui.RadioButton("Wireframe", renderer.Shading == ViewportShading.Wireframe))
                            {
                                renderer.Shading = ViewportShading.Wireframe;
                            }
                            if (ImGui.RadioButton("Solid", renderer.Shading == ViewportShading.Solid))
                            {
                                renderer.Shading = ViewportShading.Solid;
                            }
                            if (ImGui.RadioButton("Rendered", renderer.Shading == ViewportShading.Rendered))
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
                Vector2 vMin = ImGui.GetWindowContentRegionMin();
                Vector2 vMax = ImGui.GetWindowContentRegionMax();
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

            var mode = Inspector.Mode;
            if (ComboEnumHelper<ImGuizmoMode>.Combo("##Mode", ref mode))
            {
                Inspector.Mode = mode;
            }

            ImGui.PopItemWidth();

            if (ImGui.Button("\xECE9", new(32, 32)))
            {
                Inspector.Operation = ImGuizmoOperation.Translate;
            }
            TooltipHelper.Tooltip("Translate");

            if (ImGui.Button("\xE7AD", new(32, 32)))
            {
                Inspector.Operation = ImGuizmoOperation.Rotate;
            }
            TooltipHelper.Tooltip("Rotate");

            if (ImGui.Button("\xE740", new(32, 32)))
            {
                Inspector.Operation = ImGuizmoOperation.Scale;
            }
            TooltipHelper.Tooltip("Scale");

            if (ImGui.Button("\xE759", new(32, 32)))
            {
                Inspector.Operation = ImGuizmoOperation.Universal;
            }
            TooltipHelper.Tooltip("Translate & Rotate & Scale");

            Inspector.Draw();

            ImGui.End();
        }

        private static void HandleHotkeys()
        {
            if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                if (ImGui.IsKeyPressed(ImGuiKey.S))
                {
                    SceneManager.Save();
                }
            }

            if (ImGui.IsKeyDown(ImGuiKey.Escape) && !Application.InEditMode)
            {
                TransitionToState(EditorPlayState.Edit);
            }
        }

        public static void TransitionToState(EditorPlayState state)
        {
            var scene = SceneManager.Current;

            if (scene == null)
                return;

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
                scene.IsSimulating = false;
                SceneManager.BeginReload();
                scene.RestoreState();
                Application.EditorPlayState = EditorPlayState.Edit;
                SceneManager.EndReload();
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
                SceneManager.Save();
                scene.IsSimulating = false;
                SceneManager.BeginReload();
                scene.SaveState();
                Application.EditorPlayState = EditorPlayState.Play;
                scene.IsSimulating = true;
                SceneManager.EndReload();
            }
            else if (Application.InPauseState)
            {
                bool shouldCancel = Application.NotifyEditorPlayStateTransition(EditorPlayState.Play);
                if (shouldCancel)
                {
                    return;
                }
                scene.IsSimulating = true;
                Application.EditorPlayState = EditorPlayState.Play;
            }
        }
    }
}