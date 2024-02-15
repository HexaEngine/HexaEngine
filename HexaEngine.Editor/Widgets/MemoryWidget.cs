namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using System.Numerics;

    [EditorWindowCategory("Debug")]
    public class MemoryWidget : EditorWindow
    {
        protected override string Name => "Memory usage";

        public override void DrawContent(IGraphicsContext context)
        {
            var entries = MemoryManager.Entries.OrderByDescending(x => x.Size);

            var budget = GraphicsAdapter.Current.GetMemoryBudget();
            var usage = GraphicsAdapter.Current.GetMemoryCurrentUsage();
            var fraction = (float)(usage / (double)budget);
            ImGui.Text($"Memory Budget: {budget.FormatDataSize()}");
            ImGui.Text($"Memory Usage: {usage.FormatDataSize()}");
            ImGui.ProgressBar(fraction, new Vector2(200, 20));

            foreach (var entry in entries)
            {
                ImGui.Text($"{entry.Name ?? "Unknown"}, Size: {entry.Size.FormatDataSize()}, Type: {entry.Resource.Dimension}");
            }
        }
    }
}