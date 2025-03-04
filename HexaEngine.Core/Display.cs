namespace HexaEngine.Core
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.SDL3;
    using System.Collections.Concurrent;

    public static unsafe class Displays
    {
        private static readonly ConcurrentDictionary<uint, Display> idToDisplay = new();
        private static readonly List<Display> displays = [];

        internal static void Init()
        {
            int displayCount;
            uint* sdlDisplays = SDL.GetDisplays(&displayCount);
            for (int i = 0; i < displayCount; i++)
            {
                uint id = sdlDisplays[i];
                AddDisplay(id);
            }
        }

        private static void AddDisplay(uint id)
        {
            Display display = new(id);
            displays.Add(display);
            idToDisplay[id] = display;
        }

        private static void RemoveDisplay(uint id)
        {
            if (idToDisplay.TryRemove(id, out var display))
            {
                displays.Remove(display);
            }
        }

        /// <summary>
        /// Gets the index of the display containing a specific point on the screen.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <returns>The index of the display containing the point.</returns>
        public static Display GetDisplayForPoint(int x, int y)
        {
            var point = new SDLPoint(x, y);
            return idToDisplay[SDL.GetDisplayForPoint(&point)];
        }

        /// <summary>
        /// Gets the index of the display containing a specific rectangle on the screen.
        /// </summary>
        /// <param name="x">The x-coordinate of the rectangle's top-left corner.</param>
        /// <param name="y">The y-coordinate of the rectangle's top-left corner.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>The index of the display containing the rectangle.</returns>
        public static Display GetDisplayForRect(int x, int y, int width, int height)
        {
            var rect = new SDLRect(x, y, width, height);
            return idToDisplay[SDL.GetDisplayForRect(&rect)];
        }

        /// <summary>
        /// Gets the index of the display on which a window resides.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>The index of the display containing the window.</returns>
        public static Display GetDisplayForWindow(Windows.SdlWindow window)
        {
            return idToDisplay[SDL.GetDisplayForWindow(window.GetWindow())];
        }

        internal static void ProcessEvent(SDLDisplayEvent evnt)
        {
            switch (evnt.Type)
            {
                case SDLEventType.DisplayOrientation:
                    break;

                case SDLEventType.DisplayAdded:
                    AddDisplay(evnt.DisplayID);
                    break;

                case SDLEventType.DisplayRemoved:
                    RemoveDisplay(evnt.DisplayID);
                    break;

                case SDLEventType.DisplayMoved:
                    break;

                case SDLEventType.DisplayDesktopModeChanged:
                    break;

                case SDLEventType.DisplayCurrentModeChanged:
                    break;

                case SDLEventType.DisplayContentScaleChanged:
                    break;
            }
        }
    }

    public unsafe class Display
    {
        private readonly uint id;

        public Display(uint id)
        {
            this.id = id;
        }

        protected uint Props => SDL.GetDisplayProperties(id);

        public uint Id => id;

        /// <summary>
        /// Gets the name of a display.
        /// </summary>
        public string Name => SDL.GetDisplayNameS(id);

        /// <summary>
        /// Gets the current display mode.
        /// </summary>
        public DisplayMode CurrentDisplayMode => *SDL.GetCurrentDisplayMode(id);

        /// <summary>
        /// Gets the desktop display mode for a given display.
        /// </summary>
        public DisplayMode DesktopDisplayMode => *SDL.GetDesktopDisplayMode(id);

        /// <summary>
        /// Gets the current orientation of a display.
        /// </summary>
        public DisplayOrientation CurrentOrientation => (DisplayOrientation)SDL.GetCurrentDisplayOrientation(id);

        /// <summary>
        /// Gets the natural orientation of a display.
        /// </summary>
        public DisplayOrientation NaturalOrientation => (DisplayOrientation)SDL.GetNaturalDisplayOrientation(id);

        public bool IsHDREnabled => SDL.HasProperty(Props, SDL.SDL_PROP_DISPLAY_HDR_ENABLED_BOOLEAN);

        /// <summary>
        /// Gets the bounds of a display in screen coordinates.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                SDLRect rectangle;
                SDL.GetDisplayBounds(id, &rectangle);
                Rectangle rect;
                rect.Left = rectangle.X;
                rect.Top = rectangle.Y;
                rect.Right = rectangle.X + rectangle.W;
                rect.Bottom = rectangle.Y + rectangle.H;
                return rect;
            }
        }

        /// <summary>
        /// Gets the bounds of a display in screen coordinates.
        /// </summary>
        public Rectangle UsableBounds
        {
            get
            {
                SDLRect rectangle;
                SDL.GetDisplayUsableBounds(id, &rectangle);
                Rectangle rect;
                rect.Left = rectangle.X;
                rect.Top = rectangle.Y;
                rect.Right = rectangle.X + rectangle.W;
                rect.Bottom = rectangle.Y + rectangle.H;
                return rect;
            }
        }

        /// <summary>
        /// Gets the closest display mode to the specified mode for a given display.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="refreshRate"></param>
        /// <param name="includeHighDensityModes"></param>
        /// <returns>The closest display mode available.</returns>
        public DisplayMode GetClosestDisplayMode(int w, int h, float refreshRate, bool includeHighDensityModes)
        {
            SDLDisplayMode closest;
            SDL.GetClosestFullscreenDisplayMode(id, w, h, refreshRate, includeHighDensityModes, &closest);
            return closest;
        }

        public DisplayMode[] GetFullscreenDisplayModes()
        {
            int modeCount;
            SDLDisplayMode** modes = SDL.GetFullscreenDisplayModes(id, &modeCount);
            DisplayMode[] result = new DisplayMode[modeCount];
            for (int i = 0; i < modeCount; i++)
            {
                result[i] = *modes[i];
            }
            return result;
        }
    }
}