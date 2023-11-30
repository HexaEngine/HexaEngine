namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;

    public class DebugWindow : EditorWindow
    {
        protected override string Name => "Debug";

        public DebugWindow()
        {
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;
            if (scene == null)
            {
                return;
            }

            var renderers = scene.RenderManager;
            for (var i = 0; i < renderers.Renderers.Count; i++)
            {
                var r = renderers.Renderers[i];
                ImGui.Text(r.BoundingBox.ToString());
            }
        }
    }
}