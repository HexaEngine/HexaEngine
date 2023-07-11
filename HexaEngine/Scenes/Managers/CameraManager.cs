﻿namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;
    using System.Numerics;

    public static class CameraManager
    {
        internal static readonly Camera camera = new() { Far = 1000 };
        private static Camera? culling;
        private static Vector3 center = default;
        private static CameraEditorMode mode = CameraEditorMode.Orbit;
        private static Vector3 sc = new(10, 0, 0);
        private static bool first = true;
        private static CameraEditorDimension dimension;
        public const float DegToRadFactor = 0.0174532925f;
        public const float Speed = 10F;
        public const float AngluarSpeed = 20F;

        public static Vector3 Center { get => center; set => center = value; }

        public static CameraEditorMode Mode
        { get => mode; set { mode = value; first = true; } }

        public static CameraEditorDimension Dimension
        {
            get => dimension; set
            {
                dimension = value;
                camera.ProjectionType = value == CameraEditorDimension.Dim3D ? ProjectionType.Perspective : ProjectionType.Othro;
                camera.Transform.Position = Vector3.Zero;
                camera.Transform.Rotation = Vector3.Zero;
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
                    Mode = mode == CameraEditorMode.Orbit ? CameraEditorMode.Free : CameraEditorMode.Orbit;
                }
            };
        }

        public static void Update()
        {
            if (!Application.InDesignMode)
            {
                return;
            }

            if (dimension == CameraEditorDimension.Dim3D)
            {
                if (mode == CameraEditorMode.Orbit)
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
                        sc.X += sc.X / 2 * -wheel;

                        // Rotate the camera left and right
                        sc.Y += -delta.X * Time.Delta * 2;

                        // Rotate the camera up and down
                        // Prevent the camera from turning upside down (1.5f = approx. Pi / 2)
                        sc.Z = Math.Clamp(sc.Z + delta.Y * Time.Delta * 2, -MathF.PI / 2, MathF.PI / 2);

                        first = false;

                        // Calculate the cartesian coordinates
                        Vector3 pos = SphereHelper.GetCartesianCoordinates(sc) + center;
                        var orientation = Quaternion.CreateFromYawPitchRoll(-sc.Y, sc.Z, 0);
                        camera.Transform.PositionRotation = (pos, orientation);
                    }
                }
                if (mode == CameraEditorMode.Free)
                {
                    Vector2 delta = Vector2.Zero;
                    if (Mouse.IsDown(MouseButton.Middle))
                    {
                        delta = Mouse.Delta;
                    }

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

                    if (Keyboard.IsDown(Key.W))
                    {
                        var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, camera.Transform.Rotation.Y * DegToRadFactor, 0f);
                        if (Keyboard.IsDown(Key.LShift))
                        {
                            camera.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                        }
                        else
                        {
                            camera.Transform.Position += Vector3.Transform(Vector3.UnitZ, rotation) * Speed * Time.Delta;
                        }
                    }
                    if (Keyboard.IsDown(Key.S))
                    {
                        var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                        if (Keyboard.IsDown(Key.LShift))
                        {
                            camera.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * 2 * Time.Delta;
                        }
                        else
                        {
                            camera.Transform.Position += Vector3.Transform(-Vector3.UnitZ, rotation) * Speed * Time.Delta;
                        }
                    }
                    if (Keyboard.IsDown(Key.A))
                    {
                        var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                        if (Keyboard.IsDown(Key.LShift))
                        {
                            camera.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                        }
                        else
                        {
                            camera.Transform.Position += Vector3.Transform(-Vector3.UnitX, rotation) * Speed * Time.Delta;
                        }
                    }
                    if (Keyboard.IsDown(Key.D))
                    {
                        var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                        if (Keyboard.IsDown(Key.LShift))
                        {
                            camera.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * 2 * Time.Delta;
                        }
                        else
                        {
                            camera.Transform.Position += Vector3.Transform(Vector3.UnitX, rotation) * Speed * Time.Delta;
                        }
                    }
                    if (Keyboard.IsDown(Key.Space))
                    {
                        var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                        if (Keyboard.IsDown(Key.LShift))
                        {
                            camera.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                        }
                        else
                        {
                            camera.Transform.Position += Vector3.Transform(Vector3.UnitY, rotation) * Speed * Time.Delta;
                        }
                    }
                    if (Keyboard.IsDown(Key.C))
                    {
                        var rotation = Matrix4x4.CreateFromYawPitchRoll(camera.Transform.Rotation.X * DegToRadFactor, 0, 0f);
                        if (Keyboard.IsDown(Key.LShift))
                        {
                            camera.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * 2 * Time.Delta;
                        }
                        else
                        {
                            camera.Transform.Position += Vector3.Transform(-Vector3.UnitY, rotation) * Speed * Time.Delta;
                        }
                    }
                }
            }
            else if (dimension == CameraEditorDimension.Dim2D)
            {
                Vector2 delta = Vector2.Zero;
                if (Mouse.IsDown(MouseButton.Middle))
                {
                    delta = Mouse.Delta;
                }

                var newPos = camera.Transform.Position;

                if (delta.X != 0 | delta.Y != 0 || first)
                {
                    var re = new Vector3(-delta.X, delta.Y, 0) * Time.Delta;
                    newPos += re;
                }

                newPos.Z = -camera.Near;
                camera.Transform.Position = newPos;
                camera.Transform.Rotation = Vector3.Zero;
            }
            camera.Transform.Recalculate();
        }
    }
}