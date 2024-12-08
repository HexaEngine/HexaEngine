namespace HexaEngine.Editor.Extensions
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public static class ImGuiExtensions
    {
        public static bool Contains(this ImRect rect, Vector2 pos)
        {
            return rect.Min.X <= pos.X && rect.Max.X >= pos.X && rect.Min.Y <= pos.Y && rect.Max.Y >= pos.Y;
        }

        public static bool Contains(this ImRect rect, Point2 pos)
        {
            return (int)rect.Min.X < pos.X && (int)rect.Max.X >= pos.X && (int)rect.Min.Y < pos.Y && (int)rect.Max.Y >= pos.Y;
        }

        public static Vector2 Size(this ImRect rect)
        {
            return rect.Max - rect.Min;
        }

        public static Vector2 Midpoint(this ImRect rect)
        {
            var size = rect.Max - rect.Min;
            return rect.Min + size / 2f;
        }
    }
}