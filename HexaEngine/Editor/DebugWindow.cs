namespace HexaEngine.Editor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;

    public class DebugWindow : EditorWindow
    {
        protected override string Name => "Debug";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            var lightManager = scene.LightManager;

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