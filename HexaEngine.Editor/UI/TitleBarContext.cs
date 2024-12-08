namespace HexaEngine.Editor.UI
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Windows;
    using System.Numerics;

    public class TitleBarContext
    {
        private Vector2 cursor;

        public CoreWindow Window { get; set; } = null!;

        public Vector2 Cursor { get => cursor; set => cursor = value; }

        public uint HoveredId { get; set; }

        public uint ForegroundColor { get; set; }

        public uint BackgroundColor { get; set; }

        public ImRect Area { get; set; }

        public ImDrawListPtr DrawList { get; set; }

        public Vector2 Offset { get; set; }

        public void AddItem(Vector2 size)
        {
            cursor.X += size.X; // no y addition horizontal layout.
        }

        public void NewFrame(ImRect area)
        {
            Area = area;
            cursor = area.Min;
        }
    }
}