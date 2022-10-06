namespace HexaEngine.Editor.Widgets
{
    using ImGuiNET;
    using System;
    using System.Numerics;

    public class Profiler
    {
        private readonly string label;
        private readonly Func<float> getCurrentValue;
        private readonly Func<float, string> overlay;
        private readonly float[] values;
        private readonly int length;
        private int index = 0;
        private float min;
        private float max;

        public Vector2 Size = new(120, 60);
        public bool Reverse;

        public Profiler(string label, Func<float> getCurrentValue, Func<float, string> overlay, int length)
        {
            this.label = label;
            this.getCurrentValue = getCurrentValue;
            this.overlay = overlay;
            this.length = length;
            values = new float[length];
        }

        public void Draw()
        {
            float current = getCurrentValue();
            values[index] = current;
            if (!Reverse)
                index++;
            else
                index--;
            if (index == length)
            {
                index = 0;
            }
            if (index < 0)
            {
                index = length - 1;
            }

            min = values.Min();
            max = values.Max();

            int offset = (index - length) % length;
            if (offset < 0)
                offset += length;

            ImGui.PlotHistogram(label, ref values[0], length, offset, overlay(current), 0, max + max / 2, Size);
        }
    }
}