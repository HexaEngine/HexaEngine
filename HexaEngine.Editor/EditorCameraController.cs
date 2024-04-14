namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Input;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public static class EditorCameraController
    {
        private static SourceAssetMetadata? config;
        private static readonly Camera editorCamera;
        private static EditorCameraState? editorCameraState;

        private static bool first = true;

        private static int ignoreMouseInputFrames;
        private static bool capturedMouse;

        static EditorCameraController()
        {
            editorCamera = CameraManager.EditorCamera;

            Keyboard.KeyUp += (s, e) =>
            {
                if (editorCameraState == null)
                {
                    return;
                }

                if (e.KeyCode == Key.F6)
                {
                    EditorCameraMode = editorCameraState.Mode == EditorCameraMode.Orbit ? EditorCameraMode.Free : EditorCameraMode.Orbit;
                }
                float modifier = 1;
                if (ImGui.GetIO().KeyCtrl)
                {
                    modifier = 0.1f;
                }
                if (ImGui.GetIO().KeyShift)
                {
                    modifier = 10f;
                }
                switch (e.KeyCode)
                {
                    case Key.NumPlus:
                    case Key.Plus:
                        Speed += modifier;
                        break;

                    case Key.NumMinus:
                    case Key.Minus:
                        Speed -= modifier;
                        break;
                }
            };

            SceneManager.SceneChanged += SceneChanged;
            Application.OnApplicationClose += OnApplicationClose;
            Application.OnEditorPlayStateTransition += OnEditorPlayStateTransition;
        }

        private static void OnEditorPlayStateTransition(EditorPlayStateTransitionEventArgs args)
        {
            if (config == null || editorCameraState == null)
            {
                return;
            }

            config.Save();

            editorCamera.Transform.Position = editorCameraState.FreePosition;
            editorCamera.Transform.Orientation = editorCameraState.FreeRotation.ToQuaternion();
            editorCamera.Transform.Far = editorCameraState.Far;
            editorCamera.Transform.Fov = editorCameraState.Fov;
        }

        private static void OnApplicationClose()
        {
            config?.Save();
            config = null;
            editorCameraState = null;
        }

        private static void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            config?.Save();
            config = null;
            editorCameraState = null;

            if (e.Scene == null)
            {
                return;
            }

            var path = e.Scene.Path;

            if (path == null)
            {
                return;
            }

            SourceAssetMetadata? metadata = SourceAssetsDatabase.GetMetadata(path);

            if (metadata == null)
            {
                return;
            }

            config = metadata;
            editorCameraState = config.GetOrCreateAdditionalMetadata<EditorCameraState>(nameof(EditorCameraState));

            editorCamera.Transform.Position = editorCameraState.FreePosition;
            editorCamera.Transform.Orientation = editorCameraState.FreeRotation.ToQuaternion();
            editorCamera.Transform.Far = editorCameraState.Far;
            editorCamera.Transform.Fov = editorCameraState.Fov;

            first = true;
        }

        public static float Fov
        {
            get => editorCameraState?.Fov ?? default;
            set
            {
                if (editorCameraState == null)
                {
                    return;
                }

                editorCameraState.Fov = value;
                editorCamera.Fov = value;
                first = true;
            }
        }

        public static float Speed
        {
            get => editorCameraState?.Speed ?? default;
            set
            {
                if (editorCameraState == null)
                {
                    return;
                }

                editorCameraState.Speed = value;
            }
        }

        public static float MouseSensitivity
        {
            get => EditorConfig.Default.MouseSensitivity;
            set
            {
                EditorConfig.Default.MouseSensitivity = value;
                EditorConfig.Default.Save();
            }
        }

        public static Vector3 Center
        {
            get => editorCameraState?.OrbitCenter ?? default;
            set
            {
                if (editorCameraState == null)
                {
                    return;
                }

                editorCameraState.OrbitCenter = value;
                first = true;
            }
        }

        public static EditorCameraMode EditorCameraMode
        {
            get => editorCameraState?.Mode ?? default;
            set
            {
                if (editorCameraState == null)
                {
                    return;
                }

                editorCameraState.Mode = value;
                first = true;
            }
        }

        public static EditorCameraDimension Dimension
        {
            get => editorCameraState?.Dimension ?? default; set
            {
                if (editorCameraState == null)
                {
                    return;
                }

                editorCameraState.Dimension = value;
                editorCameraState.FreePosition = Vector3.Zero;
                editorCameraState.FreeRotation = Vector3.Zero;
                editorCamera.ProjectionType = value == EditorCameraDimension.Dim3D ? ProjectionType.Perspective : ProjectionType.Orthographic;
                editorCamera.Transform.Position = Vector3.Zero;
                editorCamera.Transform.Rotation = Vector3.Zero;
                first = true;
            }
        }

        public static float Far
        {
            get => editorCameraState?.Far ?? default;
            set
            {
                if (editorCameraState == null)
                {
                    return;
                }

                editorCameraState.Far = value;
                editorCamera.Far = value;
                first = true;
            }
        }

        public static bool CapturedMouse => capturedMouse;

        public static bool FirstFrame => first;

        public static Vector2 UpdateMouse(bool leftCtrl, bool hovered)
        {
            if (!hovered)
            {
                return default;
            }

            Vector2 delta = Vector2.Zero;
            bool mouseDown = ImGui.IsMouseDown(ImGuiMouseButton.Right);
            if (mouseDown && !leftCtrl)
            {
                capturedMouse = true;
                ImGui.SetNextFrameWantCaptureMouse(true);
                //ImGui.SetMouseCursor(ImGuiMouseCursor.None);
                delta = Mouse.Delta;

                if (ignoreMouseInputFrames != 0 && delta != Vector2.Zero)
                {
                    delta = Vector2.Zero;
                    ignoreMouseInputFrames--;
                }
                else
                {
                    Vector2 mousePos = ImGui.GetMousePos();

                    Vector2 newMousePos = mousePos;

                    Viewport viewport = SceneWindow.ImGuiViewport;

                    const float padding = 20;

                    Vector2 viewportMin = viewport.Offset + new Vector2(padding);
                    Vector2 viewportMax = viewportMin + viewport.Size - new Vector2(padding * 2);

                    if (mousePos.X >= viewportMax.X)
                    {
                        newMousePos.X = viewportMin.X + mousePos.X % viewportMax.X;
                    }
                    else if (mousePos.X < viewportMin.X)
                    {
                        newMousePos.X = viewportMax.X;
                    }

                    if (mousePos.Y >= viewportMax.Y)
                    {
                        newMousePos.Y = viewportMin.Y + mousePos.Y % viewportMax.Y;
                    }
                    else if (mousePos.Y < viewportMin.Y)
                    {
                        newMousePos.Y = viewportMax.Y;
                    }

                    if (newMousePos != mousePos)
                    {
                        ImGui.TeleportMousePos(newMousePos);
                        ignoreMouseInputFrames = 2;
                    }
                }
            }
            else if (!mouseDown)
            {
                capturedMouse = false;
                ImGui.SetNextFrameWantCaptureMouse(false);
            }

            return delta;
        }

        public static void UpdateEditorCamera(bool hovered)
        {
            if (editorCameraState == null)
            {
                return;
            }

            var leftCtrl = ImGui.IsKeyDown(ImGuiKey.LeftCtrl);
            var delta = UpdateMouse(leftCtrl, hovered);
            if (editorCameraState.Dimension == EditorCameraDimension.Dim3D)
            {
                if (editorCameraState.Mode == EditorCameraMode.Orbit)
                {
                    float wheel = 0;
                    if (leftCtrl)
                    {
                        wheel = Mouse.DeltaWheel.Y;
                    }

                    // Only update the camera's position if the mouse got moved in either direction
                    if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
                    {
                        var orbitPosition = editorCameraState.OrbitPosition;
                        var orbitCenter = editorCameraState.OrbitCenter;
                        orbitPosition.X = Math.Clamp(orbitPosition.X + orbitPosition.X / 2 * -wheel, 0.001f, float.MaxValue);

                        // Rotate the camera left and right
                        orbitPosition.Y += -delta.X * Time.Delta * MouseSensitivity;

                        // Rotate the camera up and down
                        // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                        orbitPosition.Z = Math.Clamp(orbitPosition.Z + delta.Y * Time.Delta * MouseSensitivity, -float.Pi / 2, float.Pi / 2);

                        // Calculate the cartesian coordinates
                        Vector3 position = SphereHelper.GetCartesianCoordinates(orbitPosition) + orbitCenter;
                        Quaternion orientation = Quaternion.CreateFromYawPitchRoll(-orbitPosition.Y, orbitPosition.Z, 0);

                        editorCamera.Transform.Position = position;
                        editorCamera.Transform.Orientation = orientation;
                        editorCameraState.OrbitPosition = orbitPosition;
                        editorCameraState.FreePosition = position;
                        editorCameraState.FreeRotation = editorCamera.Transform.Rotation;
                    }
                }
                if (editorCameraState.Mode == EditorCameraMode.Free && !leftCtrl)
                {
                    var position = editorCamera.Transform.Position;
                    var rotation = editorCamera.Transform.Orientation.ToYawPitchRoll();

                    if (delta.X != 0 | delta.Y != 0 || first)
                    {
                        var deltaT = new Vector3(delta.X, delta.Y, 0) * Time.Delta * MouseSensitivity;
                        rotation += deltaT;

                        const float maxAngle = 1.5690509975f; // 89.9° (90° would cause nan values.)

                        if (rotation.Y < -maxAngle)
                        {
                            rotation = new Vector3(rotation.X, -maxAngle, rotation.Z);
                        }
                        if (rotation.Y > maxAngle)
                        {
                            rotation = new Vector3(rotation.X, maxAngle, rotation.Z);
                        }
                    }

                    var speedMult = Speed;
                    if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
                    {
                        speedMult *= 2;
                    }
                    speedMult *= Time.Delta;

                    if (ImGui.IsKeyDown(ImGuiKey.W))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(editorCamera.Transform.Rotation.X.ToRad(), editorCamera.Transform.Rotation.Y.ToRad(), 0f);
                        position += Vector3.Transform(Vector3.UnitZ, rotationM) * speedMult;
                    }
                    if (ImGui.IsKeyDown(ImGuiKey.S))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(editorCamera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(-Vector3.UnitZ, rotationM) * speedMult;
                    }
                    if (ImGui.IsKeyDown(ImGuiKey.A))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(editorCamera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(-Vector3.UnitX, rotationM) * speedMult;
                    }
                    if (ImGui.IsKeyDown(ImGuiKey.D))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(editorCamera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(Vector3.UnitX, rotationM) * speedMult;
                    }
                    if (ImGui.IsKeyDown(ImGuiKey.Space))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(editorCamera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(Vector3.UnitY, rotationM) * speedMult;
                    }
                    if (ImGui.IsKeyDown(ImGuiKey.C))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(editorCamera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(-Vector3.UnitY, rotationM) * speedMult;
                    }

                    editorCamera.Transform.Position = position;
                    editorCamera.Transform.Orientation = rotation.ToQuaternion();
                    editorCameraState.FreePosition = position;
                    editorCameraState.FreeRotation = rotation;
                }
            }
            else if (editorCameraState.Dimension == EditorCameraDimension.Dim2D && !leftCtrl)
            {
                var position = editorCamera.Transform.Position;

                if (delta.X != 0 | delta.Y != 0 || first)
                {
                    var re = new Vector3(-delta.X, delta.Y, 0) * Time.Delta;
                    position += re;
                }

                position.Z = -editorCamera.Near;
                editorCamera.Transform.Position = position;
                editorCamera.Transform.Rotation = Vector3.Zero;
                editorCameraState.FreePosition = position;
                editorCameraState.FreeRotation = Vector3.Zero;
            }
            editorCamera.Transform.Recalculate();
            first = false;
        }
    }
}