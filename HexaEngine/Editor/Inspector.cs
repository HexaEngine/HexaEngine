namespace HexaEngine.Editor
{
    using HexaEngine.Lights;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using ImGuizmoNET;
    using imnodesNET;
    using ImPlotNET;
    using System.Numerics;

    public static class Inspector
    {
        private static bool drawGrid;
        private static bool drawLights;
        private static bool enabled;

        public static bool Enabled { get => enabled; set => enabled = value; }

        public static bool DrawGrid { get => drawGrid; set => drawGrid = value; }

        public static bool DrawLights { get => drawLights; set => drawLights = value; }

        public static void Draw()
        {
            if (!enabled)
                return;

            var scene = SceneManager.Current;
            var camera = CameraManager.Current;
            var proj = camera.Transform.Projection;
            var view = camera.Transform.View;
            var world = Matrix4x4.Identity;

            if (drawGrid)
            {
                ImGuizmo.DrawGrid(ref view, ref proj, ref world, 100);
            }

            if (drawLights)
            {
                for (int i = 0; i < scene.Lights.Count; i++)
                {
                    Light light = scene.Lights[i];
                }
            }
        }
    }
}