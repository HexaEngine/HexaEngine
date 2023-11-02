namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using System.Numerics;

    public static class ImageHelper
    {
        public static void ImageCenteredV(ImTextureID image, Vector2 size)
        {
            var windowHeight = ImGui.GetWindowSize().Y;
            var imageHeight = size.Y;

            ImGui.SetCursorPosY((windowHeight - imageHeight) * 0.5f);
            ImGui.Image(image, size);
        }

        public static void ImageCenteredH(ImTextureID image, Vector2 size)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var imageWidth = size.X;

            ImGui.SetCursorPosX((windowWidth - imageWidth) * 0.5f);
            ImGui.Image(image, size);
        }

        public static void ImageCenteredVH(ImTextureID image, Vector2 size)
        {
            var windowSize = ImGui.GetWindowSize();

            ImGui.SetCursorPos((windowSize - size) * 0.5f);
            ImGui.Image(image, size);
        }
    }
}