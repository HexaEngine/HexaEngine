namespace HexaEngine.Scenes
{
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using Silk.NET.SDL;
    using System;
    using System.Numerics;

    public static class CameraManager
    {
        internal static readonly Camera camera = new();
        private static Vector3 center = default;
        private static CameraEditorMode mode = CameraEditorMode.Orbit;
        private static Vector3 sc = new(10, 0, 0);
        private static bool first = true;

        private static bool inEditMode;

        public const float DegToRadFactor = 0.0174532925f;
        public const float Speed = 10F;
        public const float AngluarSpeed = 20F;
        public const float AngluarGrain = 0.004f;

        public static Vector3 Center { get => center; set => center = value; }

        public static CameraEditorMode Mode
        { get => mode; set { mode = value; first = true; } }

        public static bool InEditMode { get => inEditMode; set => inEditMode = value; }

        public static Camera Current => Designer.InDesignMode ? inEditMode ? SceneManager.Current.CurrentCamera : camera : SceneManager.Current.CurrentCamera;

        static CameraManager()
        {
            Keyboard.OnKeyUp += (s, e) =>
            {
                if (e.KeyCode == KeyCode.KF6)
                {
                    Mode = mode == CameraEditorMode.Orbit ? CameraEditorMode.Free : CameraEditorMode.Orbit;
                }
            };
        }

        private static Vector3 GetSphericalCoordinates(Vector3 cartesian)
        {
            float r = MathF.Sqrt(
                MathF.Pow(cartesian.X, 2) +
                MathF.Pow(cartesian.Y, 2) +
                MathF.Pow(cartesian.Z, 2)
            );

            // use atan2 for built-in checks
            float phi = MathF.Atan2(cartesian.Z / cartesian.X, cartesian.X);
            float theta = MathF.Acos(cartesian.Y / r);

            return new Vector3(r, phi, theta);
        }

        private static Vector3 GetCartesianCoordinates(Vector3 spherical)
        {
            Vector3 ret = new();

            ret.Z = -(spherical.X * MathF.Cos(spherical.Z) * MathF.Cos(spherical.Y));
            ret.Y = spherical.X * MathF.Sin(spherical.Z);
            ret.X = spherical.X * MathF.Cos(spherical.Z) * MathF.Sin(spherical.Y);

            return ret;
        }

        public static void Update()
        {
            if (!Designer.InDesignMode) return;
            Camera camera = Current;
            if (mode == CameraEditorMode.Orbit)
            {
                Vector2 delta = Vector2.Zero;
                if (Mouse.IsDown(MouseButton.Middle))
                    delta = Mouse.GetDelta();

                float wheel = 0;
                if (Keyboard.IsDown(KeyCode.KLshift))
                    wheel = Mouse.WheelDelta.Y;

                // Only update the camera's position if the mouse got moved in either direction
                if (delta.X != 0f || delta.Y != 0f || wheel != 0f || first)
                {
                    sc.X += sc.X / 2 * -wheel;

                    // Rotate the camera left and right
                    sc.Y += -delta.X * Time.Delta;

                    // Rotate the camera up and down
                    // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                    sc.Z = Math.Clamp(sc.Z + delta.Y * Time.Delta, -1.5f, 1.5f);

                    // Calculate the cartesian coordinates for unity
                    Vector3 pos = GetCartesianCoordinates(sc) + center;
                    var orientation = Quaternion.CreateFromYawPitchRoll(-sc.Y, sc.Z, 0);
                    camera.Transform.PositionRotation = (pos, orientation);
                    first = false;
                }
            }
            if (mode == CameraEditorMode.Free)
            {
                Vector2 delta = Vector2.Zero;
                if (Mouse.IsDown(MouseButton.Middle))
                    delta = Mouse.GetDelta();

                if (delta.X != 0 | delta.Y != 0 || first)
                {
                    var re = new Vector3(delta.X, delta.Y, 0) * Time.Delta * AngluarSpeed;
                    camera.Transform.Rotation += re;
                    if (camera.Transform.Rotation.Y < 270 & camera.Transform.Rotation.Y > 180)
                    {
                        camera.Transform.Rotation = new Vector3(camera.Transform.Rotation.X, 270f, camera.Transform.Rotation.Z);
                    }
                    if (camera.Transform.Rotation.Y > 90 & camera.Transform.Rotation.Y < 270)
                    {
                        camera.Transform.Rotation = new Vector3(camera.Transform.Rotation.X, 90f, camera.Transform.Rotation.Z);
                    }
                }

                if (Keyboard.IsDown(KeyCode.KW))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, camera.Transform.Rotation.Y * DegToRadFactor, 0f);
                    if (Keyboard.IsDown(KeyCode.KLshift))
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(KeyCode.KS))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(KeyCode.KLshift))
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(KeyCode.KA))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(KeyCode.KLshift))
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(KeyCode.KD))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(KeyCode.KLshift))
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(KeyCode.KSpace))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(KeyCode.KLshift))
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(KeyCode.KC))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(KeyCode.KLshift))
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * Time.Delta;
                }
            }
        }
    }
}