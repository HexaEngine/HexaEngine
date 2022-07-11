namespace HexaEngine.Scenes
{
    using HexaEngine.Cameras;
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Editor;
    using System.Numerics;

    public static class CameraManager
    {
        internal static readonly Camera camera = new();
        private static Vector3 center = default;
        private static CameraEditorMode mode = CameraEditorMode.Free;

        private static bool inEditMode;

        public const float DegToRadFactor = 0.0174532925f;
        public const float Speed = 10F;
        public const float AngluarSpeed = 20F;
        public const float AngluarGrain = 0.004f;

        public static Vector3 Center { get => center; set => center = value; }

        public static CameraEditorMode Mode { get => mode; set => mode = value; }

        public static bool InEditMode { get => inEditMode; set => inEditMode = value; }

        public static Camera Current => Designer.InDesignMode ? inEditMode ? SceneManager.Current.CurrentCamera : camera : SceneManager.Current.CurrentCamera;

        public static void Update()
        {
            if (!Designer.InDesignMode) return;
            Camera camera = Current;
            if (mode == CameraEditorMode.Orbit)
            {
            }
            if (mode == CameraEditorMode.Free)
            {
                if (!Mouse.IsDown(MouseButton.Middle))
                    return;

                var delta = Mouse.GetDelta();

                if (delta.X != 0 | delta.Y != 0)
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

                if (Keyboard.IsDown(Keys.W))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, camera.Transform.Rotation.Y * DegToRadFactor, 0f);
                    if (Keyboard.IsDown(Keys.Lshift))
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(Keys.S))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(Keys.Lshift))
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(Keys.A))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(Keys.Lshift))
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(Keys.D))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(Keys.Lshift))
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(Keys.Space))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(Keys.Lshift))
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * Time.Delta;
                }
                if (Keyboard.IsDown(Keys.C))
                {
                    var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                    if (Keyboard.IsDown(Keys.Lshift))
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                    else
                        camera.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * Time.Delta;
                }
            }
        }
    }
}