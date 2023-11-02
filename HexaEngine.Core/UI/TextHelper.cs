namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;

    public static class TextHelper
    {
        public static void TextCenteredV(string text)
        {
            var windowHeight = ImGui.GetWindowSize().Y;
            var textHeight = ImGui.CalcTextSize(text).Y;

            ImGui.SetCursorPosY((windowHeight - textHeight) * 0.5f);
            ImGui.Text(text);
        }

        public static void TextCenteredH(string text)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var textWidth = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);
        }

        public static void TextCenteredVH(string text)
        {
            var windowSize = ImGui.GetWindowSize();
            var textSize = ImGui.CalcTextSize(text);

            ImGui.SetCursorPos((windowSize - textSize) * 0.5f);
            ImGui.Text(text);
        }
    }
}