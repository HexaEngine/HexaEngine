namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using ImGuiNET;
    using System.Numerics;

    public class MixerWidget : ImGuiWindow
    {
        protected override string Name => "Mixer";

        public override void DrawContent(IGraphicsContext context)
        {
            ImGui.BeginGroup();
            ImGui.Text("Master");

            var gain = AudioManager.Master.Gain;
            if (ImGui.VSliderFloat("##Master", new(30, 200), ref gain, 0, 1))
            {
                AudioManager.Master.Gain = gain;
            }

            float[] data = new float[2] { -80, -80 };
            ImGui.SameLine();
            ImGui.PlotLines("L", ref data[0], 2, 0, null, -80, 0, new(15, 200));
            ImGui.SameLine();
            ImGui.PlotLines("R", ref data[0], 2, 0, null, -80, 0, new(15, 200));
            ImGui.EndGroup();
            ImGui.GetForegroundDrawList().AddRect(ImGui.GetItemRectMin() - new Vector2(2, 2), ImGui.GetItemRectMax() + new Vector2(2, 2), 0xFFAFAFAF);
            ImGui.Dummy(default);
        }
    }
}