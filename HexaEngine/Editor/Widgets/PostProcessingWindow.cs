namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Rendering.Renderers;
    using ImGuiNET;

    public class PostProcessWindow : EditorWindow
    {
        protected override string Name => "Post Process";

        public override void DrawContent(IGraphicsContext context)
        {
            SceneRenderer? renderer = SceneRenderer.Current;
            if (renderer == null)
            {
                return;
            }

            var manager = renderer.PostProcessing;

            var effects = manager.Effects;

            ImGui.BeginListBox("Effects");
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];

                ImGui.Text($"{effect.Name},{i}");
            }
            ImGui.EndListBox();
        }
    }
}