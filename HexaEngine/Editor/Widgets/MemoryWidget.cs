namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.ImGuiNET;
    using System.Numerics;

    public class MemoryWidget : EditorWindow
    {
        protected override string Name => "Memory usage";

        public override void DrawContent(IGraphicsContext context)
        {
            var entries = MemoryManager.Entries.OrderByDescending(x => x.Size);

            var budget = GraphicsAdapter.Current.GetMemoryBudget();
            var usage = GraphicsAdapter.Current.GetMemoryCurrentUsage();
            var fraction = (float)(usage / (double)budget);
            ImGui.Text($"Memory Budget: {Humanize(budget)}");
            ImGui.Text($"Memory Usage: {Humanize(usage)}");
            ImGui.ProgressBar(fraction, new Vector2(200, 20));

            foreach (var entry in entries)
            {
                ImGui.Text($"{entry.Name ?? "Unknown"}, Size: {Humanize(entry.Size)}, Type: {entry.Resource.Dimension}");
            }
        }

        private static string Humanize(ulong value)
        {
            if (value > 1000000000)
                return $"{value / 1000000000f}GB";
            if (value > 1000000)
                return $"{value / 1000000}MB";
            if (value > 1000)
                return $"{value / 1000f}KB";
            return $"{value}B";
        }
    }
}