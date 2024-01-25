namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Input;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public static class CameraManager
    {
        internal static readonly Camera camera = new() { Far = 1000 };
        private static ConfigKey? config;
        private static Camera? culling;
        private static Vector3 center = default;
        private static EditorCameraMode mode = EditorCameraMode.Orbit;
        private static Vector3 orbitPosition = new(10, 0, 0);
        private static bool first = true;
        private static EditorCameraDimension dimension;
        public const float Speed = 10F;
        public const float AngularSpeed = 20F;

        public static Vector3 Center { get => center; set => center = value; }

        public static EditorCameraMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                config?.SetValue("Mode", value);
                first = true;
            }
        }

        public static EditorCameraDimension Dimension
        {
            get => dimension; set
            {
                dimension = value;
                camera.ProjectionType = value == EditorCameraDimension.Dim3D ? ProjectionType.Perspective : ProjectionType.Orthographic;
                camera.Transform.Position = Vector3.Zero;
                camera.Transform.Rotation = Vector3.Zero;
                config?.SetValue("Dimension", value);
                config?.SetValue("Position", Vector3.Zero);
                config?.SetValue("Rotation", Vector3.Zero);
                first = true;
            }
        }

        public static Camera? Current => Application.InDesignMode ? camera : SceneManager.Current?.CurrentCamera;

        public static Camera? Culling { get => Application.InDesignMode ? culling ?? Current : SceneManager.Current?.CurrentCamera; set => culling = value; }

        static CameraManager()
        {
            Keyboard.KeyUp += (s, e) =>
            {
                if (e.KeyCode == Key.F6)
                {
                    Mode = mode == EditorCameraMode.Orbit ? EditorCameraMode.Free : EditorCameraMode.Orbit;
                }
            };

            SceneManager.SceneChanged += SceneChanged;
        }

        private static void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            if (e.Scene == null)
            {
                return;
            }

            config = e.Scene.EditorConfig.GetOrCreateKey("EditorCamera");
            Mode = config.GetOrAddValue("Mode", EditorCameraMode.Orbit);
            Dimension = config.GetOrAddValue("Dimension", EditorCameraDimension.Dim3D);
            camera.Transform.Position = config.GetOrAddValue("Position", Vector3.Zero);
            camera.Transform.Rotation = config.GetOrAddValue("Rotation", Vector3.Zero);
            orbitPosition = config.GetOrAddValue("OrbitPosition", new Vector3(10, 0, 0));
            first = true;
        }

        public static void Update()
        {
            if (!Application.InDesignMode)
            {
                return;
            }

            if (dimension == EditorCameraDimension.Dim3D)
            {
                if (mode == EditorCameraMode.Orbit)
                {
                    Vector2 delta = Vector2.Zero;
                    if (Mouse.IsDown(MouseButton.Middle))
                    {
                        delta = Mouse.Delta;
                    }

                    float wheel = 0;
                    if (Keyboard.IsDown(Key.LCtrl))
                    {
                        wheel = Mouse.DeltaWheel.Y;
                    }

                    // Only update the camera's position if the mouse got moved in either direction
                    if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
                    {
                        orbitPosition.X += orbitPosition.X / 2 * -wheel;

                        // Rotate the camera left and right
                        orbitPosition.Y += -delta.X * Time.Delta * 2;

                        // Rotate the camera up and down
                        // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                        orbitPosition.Z = Math.Clamp(orbitPosition.Z + delta.Y * Time.Delta * 2, -float.Pi / 2, float.Pi / 2);

                        first = false;

                        // Calculate the cartesian coordinates
                        Vector3 position = SphereHelper.GetCartesianCoordinates(orbitPosition) + center;
                        Quaternion orientation = Quaternion.CreateFromYawPitchRoll(-orbitPosition.Y, orbitPosition.Z, 0);

                        camera.Transform.Position = position;
                        camera.Transform.Orientation = orientation;
                        config?.SetValue("OrbitPosition", orbitPosition);
                        config?.SetValue("Position", position);
                        config?.SetValue("Rotation", camera.Transform.Rotation);
                    }
                }
                if (mode == EditorCameraMode.Free)
                {
                    Vector2 delta = Vector2.Zero;
                    if (Mouse.IsDown(MouseButton.Middle))
                    {
                        delta = Mouse.Delta;
                    }

                    var position = camera.Transform.Position;
                    var rotation = camera.Transform.Rotation;

                    if (delta.X != 0 | delta.Y != 0 || first)
                    {
                        var re = new Vector3(delta.X, delta.Y, 0) * Time.Delta * AngularSpeed;
                        rotation += re;
                        if (rotation.Y < 270 & rotation.Y > 180)
                        {
                            rotation = new Vector3(rotation.X, 270f, rotation.Z);
                        }
                        if (rotation.Y > 90 & rotation.Y < 270)
                        {
                            rotation = new Vector3(rotation.X, 90f, rotation.Z);
                        }
                    }

                    var speedMult = Speed;
                    if (Keyboard.IsDown(Key.LShift))
                    {
                        speedMult *= 2;
                    }
                    speedMult *= Time.Delta;

                    if (Keyboard.IsDown(Key.W))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X.ToRad(), camera.Transform.Rotation.Y.ToRad(), 0f);
                        position += Vector3.Transform(Vector3.UnitZ, rotationM) * speedMult;
                    }
                    if (Keyboard.IsDown(Key.S))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(-Vector3.UnitZ, rotationM) * speedMult;
                    }
                    if (Keyboard.IsDown(Key.A))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(-Vector3.UnitX, rotationM) * speedMult;
                    }
                    if (Keyboard.IsDown(Key.D))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(Vector3.UnitX, rotationM) * speedMult;
                    }
                    if (Keyboard.IsDown(Key.Space))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(Vector3.UnitY, rotationM) * speedMult;
                    }
                    if (Keyboard.IsDown(Key.C))
                    {
                        var rotationM = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X.ToRad(), 0, 0f);
                        position += Vector3.Transform(-Vector3.UnitY, rotationM) * speedMult;
                    }

                    camera.Transform.Position = position;
                    camera.Transform.Rotation = rotation;
                    config?.SetValue("Position", position);
                    config?.SetValue("Rotation", rotation);
                }
            }
            else if (dimension == EditorCameraDimension.Dim2D)
            {
                Vector2 delta = Vector2.Zero;
                if (Mouse.IsDown(MouseButton.Middle))
                {
                    delta = Mouse.Delta;
                }

                var position = camera.Transform.Position;

                if (delta.X != 0 | delta.Y != 0 || first)
                {
                    var re = new Vector3(-delta.X, delta.Y, 0) * Time.Delta;
                    position += re;
                }

                position.Z = -camera.Near;
                camera.Transform.Position = position;
                camera.Transform.Rotation = Vector3.Zero;
                config?.SetValue("Position", position);
                config?.SetValue("Rotation", Vector3.Zero);
            }
            camera.Transform.Recalculate();
        }
    }
}