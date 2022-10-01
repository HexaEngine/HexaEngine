namespace HexaEngine.Editor
{
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using ImGuizmoNET;
    using imnodesNET;
    using ImPlotNET;
    using System;
    using System.Numerics;

    public static class Inspector
    {
        private static bool drawGrid = true;
        private static bool drawLights = true;
        private static bool drawCameras = true;
        private static bool enabled = true;

        public static bool Enabled { get => enabled; set => enabled = value; }

        public static bool DrawGrid { get => drawGrid; set => drawGrid = value; }

        public static bool DrawLights { get => drawLights; set => drawLights = value; }

        public static bool DrawCameras { get => drawCameras; set => drawCameras = value; }

        public static void Draw()
        {
            if (!enabled)
                return;

            var scene = SceneManager.Current;

            if (drawGrid)
            {
                //DebugDraw.DrawGrid(10, 10, Vector4.Zero);
            }

            if (drawLights)
            {
                for (int i = 0; i < scene.Lights.Count; i++)
                {
                    Light light = scene.Lights[i];
                    if (light.Type == LightType.Directional)
                    {
                        DebugDraw.DrawRay(light.Transform.GlobalPosition, light.Transform.Forward, false, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitY * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitY * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                    }
                    if (light is Spotlight spotlight)
                    {
                        DebugDraw.DrawRay(light.Transform.GlobalPosition, light.Transform.Forward * 10, false, Vector4.One);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward, spotlight.GetConeEllipse(1), Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward * 10, spotlight.GetConeEllipse(10), Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition + light.Transform.Forward * 10, spotlight.GetInnerConeEllipse(10), Vector4.Zero);
                    }
                    if (light.Type == LightType.Point)
                    {
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitX * 0.1f, Vector3.UnitY * 0.1f, Vector4.Zero);
                        DebugDraw.DrawRing(light.Transform.GlobalPosition, Vector3.UnitY * 0.1f, Vector3.UnitZ * 0.1f, Vector4.Zero);
                    }
                }
            }

            if (drawCameras)
            {
                for (int i = 0; i < scene.Cameras.Count; i++)
                {
                    var cam = scene.Cameras[i];
                    DebugDraw.Draw(new BoundingFrustum(cam.Transform.View * MathUtil.PerspectiveFovLH(cam.Transform.Fov, cam.Transform.AspectRatio, 0.1f, 10)), Vector4.Zero);
                }
            }
        }
    }
}