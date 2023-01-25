namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using ImGuiNET;
    using ImPlotNET;
    using System.Security.Cryptography;

    public class DebugWindow : ImGuiWindow
    {
        protected override string Name => "Debug";
        private int[] bar_data = new int[11];
        private float[] x_data = new float[1000];
        private float[] y_data = new float[1000];
        private nint ctx;

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene == null)
                return;

            var lightManager = scene.LightManager;

            ImGui.BeginListBox("Lights");
            for (int i = 0; i < lightManager.Count; i++)
            {
                var light = lightManager.Lights[i];

                ImGui.Text($"{light.Name}:{light.GetQueueIndex()}");
            }
            ImGui.EndListBox();

            for (int i = 0; i < 1000; i++)
            {
                x_data[i] = RandomNumberGenerator.GetInt32(int.MaxValue);
                y_data[i] = RandomNumberGenerator.GetInt32(int.MaxValue);
            }
        }
    }
}