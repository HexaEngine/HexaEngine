namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using ImGuiNET;

    public class PostProcessWindow : ImGuiWindow
    {
        protected override string Name => "Post Process";

        public override void DrawContent(IGraphicsContext context)
        {
            var renderer = Application.MainWindow.Renderer;
            if (renderer == null)
            {
                return;
            }

            var manager = renderer.PostProcess;

            var effects = manager.Effects;

            ImGui.BeginListBox("Effects");
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];

                ImGui.Text($"{effect.Name}:{effect.Priority},{i}");
            }
            ImGui.EndListBox();
        }
    }
}