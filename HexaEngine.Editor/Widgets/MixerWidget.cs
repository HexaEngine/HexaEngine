namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using System.Numerics;

    public class MixerWidget : EditorWindow
    {
        protected override string Name => $"{UwU.Music} Mixer";

        public override unsafe void DrawContent(IGraphicsContext context)
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
            ImGui.PlotLines("L", ref data[0], 2, 0, -80, 0, new Vector2(15, 200));
            ImGui.SameLine();
            ImGui.PlotLines("R", ref data[0], 2, 0, -80, 0, new Vector2(15, 200));
            ImGui.EndGroup();
            ImGui.GetForegroundDrawList().AddRect(ImGui.GetItemRectMin() - new Vector2(2, 2), ImGui.GetItemRectMax() + new Vector2(2, 2), 0xFFAFAFAF);
            ImGui.Dummy(default);
        }
    }
}