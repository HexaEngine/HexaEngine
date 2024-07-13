namespace HexaEngine.Core
{
    using Hexa.NET.SDL2;

    /// <summary>
    /// A static class for retrieving information about displays and display modes.
    /// </summary>
    public static unsafe class Display
    {
        /// <summary>
        /// Gets the number of video displays available.
        /// </summary>
        public static int NumVideoDisplays => SDL.SDLGetNumVideoDisplays();

        /// <summary>
        /// Gets the closest display mode to the specified mode for a given display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <param name="mode">The desired display mode.</param>
        /// <returns>The closest display mode available.</returns>
        public static DisplayMode GetClosestDisplayMode(int displayIndex, DisplayMode mode)
        {
            DisplayMode closest;
            SDL.SDLGetClosestDisplayMode(displayIndex, (SDLDisplayMode*)&mode, (SDLDisplayMode*)&closest);
            return closest;
        }

        /// <summary>
        /// Gets the current display mode for a given display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <returns>The current display mode.</returns>
        public static DisplayMode GetCurrentDisplayMode(int displayIndex)
        {
            DisplayMode mode;
            SDL.SDLGetCurrentDisplayMode(displayIndex, (SDLDisplayMode*)&mode);
            return mode;
        }

        /// <summary>
        /// Gets the desktop display mode for a given display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <returns>The desktop display mode.</returns>
        public static DisplayMode GetDesktopDisplayMode(int displayIndex)
        {
            DisplayMode mode;
            SDL.SDLGetDesktopDisplayMode(displayIndex, (SDLDisplayMode*)&mode);
            return mode;
        }

        /// <summary>
        /// Gets the name of a display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <returns>The name of the display.</returns>
        public static string GetDisplayName(int displayIndex)
        {
            return SDL.SDLGetDisplayNameS(displayIndex);
        }

        /// <summary>
        /// Gets the orientation of a display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <returns>The orientation of the display.</returns>
        public static DisplayOrientation GetDisplayOrientation(int displayIndex)
        {
            return (DisplayOrientation)SDL.SDLGetDisplayOrientation(displayIndex);
        }

        /// <summary>
        /// Gets the DPI (dots per inch) of a display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <param name="ddpi">The horizontal DPI.</param>
        /// <param name="hdpi">The diagonal DPI.</param>
        /// <param name="vdpi">The vertical DPI.</param>
        public static void GetDisplayDPI(int displayIndex, ref float ddpi, ref float hdpi, ref float vdpi)
        {
            SDL.SDLGetDisplayDPI(displayIndex, ref ddpi, ref hdpi, ref vdpi);
        }

        /// <summary>
        /// Gets the bounds of a display in screen coordinates.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <param name="x">The x-coordinate of the display's top-left corner.</param>
        /// <param name="y">The y-coordinate of the display's top-left corner.</param>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        public static void GetDisplayBounds(int displayIndex, ref int x, ref int y, ref int width, ref int height)
        {
            SDLRect rectangle;
            SDL.SDLGetDisplayBounds(displayIndex, &rectangle);
            x = rectangle.X;
            y = rectangle.Y;
            width = rectangle.W;
            height = rectangle.H;
        }

        /// <summary>
        /// Gets the usable bounds of a display in screen coordinates.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <param name="x">The x-coordinate of the display's top-left corner.</param>
        /// <param name="y">The y-coordinate of the display's top-left corner.</param>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        public static void GetDisplayUsableBounds(int displayIndex, ref int x, ref int y, ref int width, ref int height)
        {
            SDLRect rectangle;
            SDL.SDLGetDisplayUsableBounds(displayIndex, &rectangle);
            x = rectangle.X;
            y = rectangle.Y;
            width = rectangle.W;
            height = rectangle.H;
        }

        /// <summary>
        /// Gets a specific display mode for a given display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <param name="modeIndex">The index of the display mode.</param>
        /// <returns>The display mode at the specified index.</returns>
        public static DisplayMode GetDisplayMode(int displayIndex, int modeIndex)
        {
            DisplayMode mode;
            SDL.SDLGetDisplayMode(displayIndex, modeIndex, (SDLDisplayMode*)&mode);
            return mode;
        }

        /// <summary>
        /// Gets the number of display modes available for a given display.
        /// </summary>
        /// <param name="displayIndex">The index of the display.</param>
        /// <returns>The number of display modes.</returns>
        public static int GetNumDisplayModes(int displayIndex)
        {
            return SDL.SDLGetNumDisplayModes(displayIndex);
        }

        /// <summary>
        /// Gets the index of the display containing a specific point on the screen.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns>The index of the display containing the point.</returns>
        public static int GetPointDisplayIndex(int x, int y)
        {
            var point = new SDLPoint(x, y);
            return SDL.SDLGetPointDisplayIndex(&point);
        }

        /// <summary>
        /// Gets the index of the display containing a specific rectangle on the screen.
        /// </summary>
        /// <param name="x">The x-coordinate of the rectangle's top-left corner.</param>
        /// <param name="y">The y-coordinate of the rectangle's top-left corner.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>The index of the display containing the rectangle.</returns>
        public static int GetRectDisplayIndex(int x, int y, int width, int height)
        {
            var rect = new SDLRect(x, y, width, height);
            return SDL.SDLGetRectDisplayIndex(&rect);
        }

        /// <summary>
        /// Gets the index of the display on which a window resides.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>The index of the display containing the window.</returns>
        public static int GetWindowDisplayIndex(Windows.SdlWindow window)
        {
            return SDL.SDLGetWindowDisplayIndex(window.GetWindow());
        }

        /// <summary>
        /// Gets the current display mode of a window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>The current display mode of the window.</returns>
        public static DisplayMode GetWindowDisplayMode(Windows.SdlWindow window)
        {
            DisplayMode mode;
            SDL.SDLGetWindowDisplayMode(window.GetWindow(), (SDLDisplayMode*)&mode);
            return mode;
        }
    }
}