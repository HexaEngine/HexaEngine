﻿namespace HexaEngine.Editor
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
        private static Vector3 center = default;
        private static EditorCameraMode editorCameraMode = EditorCameraMode.Orbit;
        private static Vector3 orbitPosition = new(10, 0, 0);
        private static bool first = true;
        private static EditorCameraDimension dimension;
        public const float Speed = 10F;
        public const float AngularSpeed = 1F;

        static EditorCameraController()
        {
            editorCamera = CameraManager.EditorCamera;

            Keyboard.KeyUp += (s, e) =>
            {
                if (e.KeyCode == Key.F6)
                {
                    EditorCameraMode = editorCameraMode == EditorCameraMode.Orbit ? EditorCameraMode.Free : EditorCameraMode.Orbit;
                }
            };

            SceneManager.SceneChanged += SceneChanged;
            Application.OnApplicationClose += OnApplicationClose;
        }

        private static void OnApplicationClose()
        {
            config?.Save();
        }

        private static void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            config?.Save();

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
            EditorCameraMode = config.GetOrAddValue("Mode", EditorCameraMode.Orbit);
            Dimension = config.GetOrAddValue("Dimension", EditorCameraDimension.Dim3D);
            editorCamera.Transform.Position = config.GetOrAddValue("Position", Vector3.Zero);
            editorCamera.Transform.Rotation = config.GetOrAddValue("Rotation", Vector3.Zero);
            orbitPosition = config.GetOrAddValue("OrbitPosition", new Vector3(10, 0, 0));
            first = true;
        }

        public static Vector3 Center { get => center; set => center = value; }

        public static EditorCameraMode EditorCameraMode
        {
            get => editorCameraMode;
            set
            {
                editorCameraMode = value;
                config?.SetValue("Mode", value);
                first = true;
            }
        }

        public static EditorCameraDimension Dimension
        {
            get => dimension; set
            {
                dimension = value;
                editorCamera.ProjectionType = value == EditorCameraDimension.Dim3D ? ProjectionType.Perspective : ProjectionType.Orthographic;
                editorCamera.Transform.Position = Vector3.Zero;
                editorCamera.Transform.Rotation = Vector3.Zero;
                config?.SetValue("Dimension", value);
                config?.SetValue("Position", Vector3.Zero);
                config?.SetValue("Rotation", Vector3.Zero);
                first = true;
            }
        }

        public static void UpdateEditorCamera()
        {
            var leftCtrl = ImGui.IsKeyDown(ImGuiKey.LeftCtrl);
            if (dimension == EditorCameraDimension.Dim3D)
            {
                if (editorCameraMode == EditorCameraMode.Orbit)
                {
                    Vector2 delta = Vector2.Zero;
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Right) && !leftCtrl)
                    {
                        delta = Mouse.Delta;
                    }

                    float wheel = 0;
                    if (leftCtrl)
                    {
                        wheel = Mouse.DeltaWheel.Y;
                    }

                    // Only update the camera's position if the mouse got moved in either direction
                    if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
                    {
                        orbitPosition.X = Math.Clamp(orbitPosition.X + orbitPosition.X / 2 * -wheel, 0.001f, float.MaxValue);

                        // Rotate the camera left and right
                        orbitPosition.Y += -delta.X * Time.Delta * 2;

                        // Rotate the camera up and down
                        // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                        orbitPosition.Z = Math.Clamp(orbitPosition.Z + delta.Y * Time.Delta * 2, -float.Pi / 2, float.Pi / 2);

                        first = false;

                        // Calculate the cartesian coordinates
                        Vector3 position = SphereHelper.GetCartesianCoordinates(orbitPosition) + center;
                        Quaternion orientation = Quaternion.CreateFromYawPitchRoll(-orbitPosition.Y, orbitPosition.Z, 0);

                        editorCamera.Transform.Position = position;
                        editorCamera.Transform.Orientation = orientation;
                        config?.SetValue("OrbitPosition", orbitPosition);
                        config?.SetValue("Position", position);
                        config?.SetValue("Rotation", editorCamera.Transform.Rotation);
                    }
                }
                if (editorCameraMode == EditorCameraMode.Free && !leftCtrl)
                {
                    Vector2 delta = Vector2.Zero;
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
                    {
                        delta = Mouse.Delta;
                    }

                    var position = editorCamera.Transform.Position;
                    var rotation = editorCamera.Transform.Orientation.ToYawPitchRoll();

                    if (delta.X != 0 | delta.Y != 0 || first)
                    {
                        var deltaT = new Vector3(delta.X, delta.Y, 0) * Time.Delta * AngularSpeed;
                        rotation += deltaT;
                        if (rotation.Y < 4.7123889804f & rotation.Y > MathUtil.PI)
                        {
                            rotation = new Vector3(rotation.X, 4.7123889804f, rotation.Z);
                        }
                        if (rotation.Y > MathUtil.PIDIV2 & rotation.Y < 4.7123889804f)
                        {
                            rotation = new Vector3(rotation.X, MathUtil.PIDIV2, rotation.Z);
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
                    config?.SetValue("Position", position);
                    config?.SetValue("Rotation", rotation);
                }
            }
            else if (dimension == EditorCameraDimension.Dim2D && !leftCtrl)
            {
                Vector2 delta = Vector2.Zero;
                if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
                {
                    delta = Mouse.Delta;
                }

                var position = editorCamera.Transform.Position;

                if (delta.X != 0 | delta.Y != 0 || first)
                {
                    var re = new Vector3(-delta.X, delta.Y, 0) * Time.Delta;
                    position += re;
                }

                position.Z = -editorCamera.Near;
                editorCamera.Transform.Position = position;
                editorCamera.Transform.Rotation = Vector3.Zero;
                config?.SetValue("Position", position);
                config?.SetValue("Rotation", Vector3.Zero);
            }
            editorCamera.Transform.Recalculate();
        }
    }
}