namespace HexaEngine.Editor.UI
{
    using Hexa.NET.ImGui;
    using HexaEngine.Editor.Extensions;
    using System.Numerics;

    public unsafe class TitleBarTitle : TitleBarElement
    {
        public override Vector2 Size { get; } // null size title should not take space in layout atleast for now.

        public override string Label { get; } = "Title";

        public override bool IsVisible { get; } = true;

        public override void Draw(TitleBarContext context)
        {
            var area = context.Area;
            var size = area.Size();
            // Draw the title text centered in the title bar
            string title = context.Window.Title;
            var textSize = ImGui.CalcTextSize(title);
            var textPos = new Vector2(
                area.Min.X + (size.X - textSize.X) * 0.5f,
                area.Min.Y + (size.Y - textSize.Y) * 0.5f
            );
            context.DrawList.AddText(textPos, context.ForegroundColor, title);
        }
    }
}