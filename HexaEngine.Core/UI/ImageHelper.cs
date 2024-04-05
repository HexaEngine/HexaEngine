namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using System.Numerics;

    /// <summary>
    /// A helper class for working with ImGui images
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Displays an image centered vertically in the window with the specified size.
        /// </summary>
        /// <param name="image">The identifier of the image to display.</param>
        /// <param name="size">The size of the image.</param>
        public static void ImageCenteredV(ImTextureID image, Vector2 size)
        {
            var windowHeight = ImGui.GetWindowSize().Y;
            var imageHeight = size.Y;

            ImGui.SetCursorPosY((windowHeight - imageHeight) * 0.5f);
            ImGui.Image(image, size);
        }

        /// <summary>
        /// Displays an image centered horizontally in the window with the specified size.
        /// </summary>
        /// <param name="image">The identifier of the image to display.</param>
        /// <param name="size">The size of the image.</param>
        public static void ImageCenteredH(ImTextureID image, Vector2 size)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var imageWidth = size.X;

            ImGui.SetCursorPosX((windowWidth - imageWidth) * 0.5f);
            ImGui.Image(image, size);
        }

        /// <summary>
        /// Displays an image centered both vertically and horizontally in the window with the specified size.
        /// </summary>
        /// <param name="image">The identifier of the image to display.</param>
        /// <param name="size">The size of the image.</param>
        public static void ImageCenteredVH(ImTextureID image, Vector2 size)
        {
            var windowSize = ImGui.GetWindowSize();

            ImGui.SetCursorPos((windowSize - size) * 0.5f);
            ImGui.Image(image, size);
        }

        /// <summary>
        /// Displays an image centered vertically in the window with the specified size.
        /// </summary>
        /// <param name="image">The identifier of the image to display.</param>
        /// <param name="size">The size of the image.</param>
        /// <param name="uv0">The start uv coordinates.</param>
        /// <param name="uv1">The end uv coordinates.</param>
        public static void ImageCenteredV(ImTextureID image, Vector2 size, Vector2 uv0, Vector2 uv1)
        {
            var windowHeight = ImGui.GetWindowSize().Y;
            var imageHeight = size.Y;

            ImGui.SetCursorPosY((windowHeight - imageHeight) * 0.5f);
            ImGui.Image(image, size, uv0, uv1);
        }

        /// <summary>
        /// Displays an image centered horizontally in the window with the specified size.
        /// </summary>
        /// <param name="image">The identifier of the image to display.</param>
        /// <param name="size">The size of the image.</param>
        /// <param name="uv0">The start uv coordinates.</param>
        /// <param name="uv1">The end uv coordinates.</param>
        public static void ImageCenteredH(ImTextureID image, Vector2 size, Vector2 uv0, Vector2 uv1)
        {
            var windowWidth = ImGui.GetWindowSize().X;
            var imageWidth = size.X;

            ImGui.SetCursorPosX((windowWidth - imageWidth) * 0.5f);
            ImGui.Image(image, size, uv0, uv1);
        }

        /// <summary>
        /// Displays an image centered both vertically and horizontally in the window with the specified size.
        /// </summary>
        /// <param name="image">The identifier of the image to display.</param>
        /// <param name="size">The size of the image.</param>
        /// <param name="uv0">The start uv coordinates.</param>
        /// <param name="uv1">The end uv coordinates.</param>
        public static void ImageCenteredVH(ImTextureID image, Vector2 size, Vector2 uv0, Vector2 uv1)
        {
            var windowSize = ImGui.GetWindowSize();

            ImGui.SetCursorPos((windowSize - size) * 0.5f);
            ImGui.Image(image, size, uv0, uv1);
        }
    }
}