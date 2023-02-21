namespace HexaEngine.Editor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using ImGuiNET;

    public class DebugWindow : ImGuiWindow
    {
        protected override string Name => "Debug";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene == null)
                return;

            var lightManager = scene.Lights;

            ImGui.BeginListBox("Lights");
            for (int i = 0; i < lightManager.Count; i++)
            {
                var light = lightManager.Lights[i];

                ImGui.Text($"{light.Name}:{light.GetQueueIndex()}");
            }
            ImGui.EndListBox();
        }
    }
}