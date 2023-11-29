namespace HexaEngine.Core.Windows
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Threading;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Mathematics;
    using Silk.NET.Core.Contexts;
    using Silk.NET.Core.Native;
    using Silk.NET.SDL;

    /// <summary>
    /// Represents a render window.
    /// </summary>
    public interface IRenderWindow : IWindow
    {
        /// <summary>
        /// Gets the render dispatcher associated with the render window.
        /// </summary>
        ThreadDispatcher Dispatcher { get; }

        /// <summary>
        /// Gets the graphics device associated with the render window.
        /// </summary>
        IGraphicsDevice Device { get; }

        /// <summary>
        /// Gets the graphics context associated with the render window.
        /// </summary>
        IGraphicsContext Context { get; }

        /// <summary>
        /// Gets the swap chain associated with the render window.
        /// </summary>
        ISwapChain SwapChain { get; }

        /// <summary>
        /// Gets the viewport for rendering operations.
        /// </summary>
        Viewport RenderViewport { get; }

        /// <summary>
        /// Gets the viewport for the window.
        /// </summary>
        Viewport WindowViewport { get; }

        /// <summary>
        /// Initializes the render window with the specified audio and graphics devices.
        /// </summary>
        /// <param name="audioDevice">The audio device to use.</param>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        void Initialize(IAudioDevice audioDevice, IGraphicsDevice graphicsDevice);

        /// <summary>
        /// Renders the graphics using the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context to use for rendering.</param>
        void Render(IGraphicsContext context);

        /// <summary>
        /// Uninitializes the render window and releases associated resources.
        /// </summary>
        void Uninitialize();
    }

    /// <summary>
    /// Represents a window.
    /// </summary>
    public interface IWindow : INativeWindow, INativeWindowSource
    {
        /// <summary>
        /// Gets or sets a _value indicating whether the window has a border.
        /// </summary>
        bool Bordered { get; set; }

        /// <summary>
        /// Gets a _value indicating whether the window is focused.
        /// </summary>
        bool Focused { get; }

        /// <summary>
        /// Gets or sets the height of the window.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Gets a _value indicating whether the mouse is hovering over the window.
        /// </summary>
        bool Hovering { get; }

        /// <summary>
        /// Gets or sets a _value indicating whether the cursor is locked within the window.
        /// </summary>
        bool LockCursor { get; set; }

        /// <summary>
        /// Gets or sets a _value indicating whether the window is resizable.
        /// </summary>
        bool Resizable { get; set; }

        /// <summary>
        /// Gets or sets the state of the window.
        /// </summary>
        WindowState State { get; set; }

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets the viewport of the window.
        /// </summary>
        Viewport Viewport { get; }

        /// <summary>
        /// Gets or sets the width of the window.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Gets the ID of the window.
        /// </summary>
        uint WindowID { get; }

        /// <summary>
        /// Gets or sets the X position of the window.
        /// </summary>
        int X { get; set; }

        /// <summary>
        /// Gets or sets the Y position of the window.
        /// </summary>
        int Y { get; set; }

        /// <summary>
        /// Gets the windows border size.
        /// </summary>
        Rect32 BorderSize { get; }

        /// <summary>
        /// Event triggered when the window is closing.
        /// </summary>
        event EventHandler<CloseEventArgs>? Closing;

        /// <summary>
        /// Event triggered when the mouse enters the window.
        /// </summary>
        event EventHandler<EnterEventArgs>? Enter;

        /// <summary>
        /// Event triggered when the window is exposed.
        /// </summary>
        event EventHandler<ExposedEventArgs>? Exposed;

        /// <summary>
        /// Event triggered when the window gains focus.
        /// </summary>
        event EventHandler<FocusGainedEventArgs>? FocusGained;

        /// <summary>
        /// Event triggered when the window loses focus.
        /// </summary>
        event EventHandler<FocusLostEventArgs>? FocusLost;

        /// <summary>
        /// Event triggered when the window is hidden.
        /// </summary>
        event EventHandler<HiddenEventArgs>? Hidden;

        /// <summary>
        /// Event triggered when a hit test is performed on the window.
        /// </summary>
        event EventHandler<HitTestEventArgs>? HitTest;

        /// <summary>
        /// Event triggered when a character input is received from the keyboard.
        /// </summary>
        event EventHandler<KeyboardCharEventArgs>? KeyboardCharInput;

        /// <summary>
        /// Event triggered when a keyboard input is received.
        /// </summary>
        event EventHandler<KeyboardEventArgs>? KeyboardInput;

        /// <summary>
        /// Event triggered when the mouse leaves the window.
        /// </summary>
        event EventHandler<LeaveEventArgs>? Leave;

        /// <summary>
        /// Event triggered when the window is maximized.
        /// </summary>
        event EventHandler<MaximizedEventArgs>? Maximized;

        /// <summary>
        /// Event triggered when the window is minimized.
        /// </summary>
        event EventHandler<MinimizedEventArgs>? Minimized;

        /// <summary>
        /// Event triggered when a mouse button input is received.
        /// </summary>
        event EventHandler<MouseButtonEventArgs>? MouseButtonInput;

        /// <summary>
        /// Event triggered when a mouse motion input is received.
        /// </summary>
        event EventHandler<MouseMotionEventArgs>? MouseMotionInput;

        /// <summary>
        /// Event triggered when a mouse wheel input is received.
        /// </summary>
        event EventHandler<MouseWheelEventArgs>? MouseWheelInput;

        /// <summary>
        /// Event triggered when a touch input is received.
        /// </summary>
        event EventHandler<TouchEventArgs>? TouchInput;

        /// <summary>
        /// Event triggered when a touch motion input is received.
        /// </summary>
        event EventHandler<TouchMotionEventArgs>? TouchMotionInput;

        /// <summary>
        /// Event triggered when the window is moved.
        /// </summary>
        event EventHandler<MovedEventArgs>? Moved;

        /// <summary>
        /// Event triggered when the window is resized.
        /// </summary>
        event EventHandler<ResizedEventArgs>? Resized;

        /// <summary>
        /// Event triggered when the window is restored.
        /// </summary>
        event EventHandler<RestoredEventArgs>? Restored;

        /// <summary>
        /// Event triggered when the window is shown.
        /// </summary>
        event EventHandler<ShownEventArgs>? Shown;

        /// <summary>
        /// Event triggered when the size of the window is changed.
        /// </summary>
        event EventHandler<SizeChangedEventArgs>? SizeChanged;

        /// <summary>
        /// Event triggered when the window requests to take focus.
        /// </summary>
        event EventHandler<TakeFocusEventArgs>? TakeFocus;

        /// <summary>
        /// Shows the window.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the window.
        /// </summary>
        void Hide();

        /// <summary>
        /// Closes the window.
        /// </summary>
        void Close();

        /// <summary>
        /// Captures the input to the window.
        /// </summary>
        void Capture();

        /// <summary>
        /// Releases the captured input from the window.
        /// </summary>
        void ReleaseCapture();

        /// <summary>
        /// Creates a Vulkan surface for the window.
        /// </summary>
        /// <param name="vkHandle">The Vulkan handle.</param>
        /// <param name="vkNonDispatchableHandle">The Vulkan non-dispatchable handle.</param>
        /// <returns><c>true</c> if the surface creation was successful; otherwise, <c>false</c>.</returns>
        unsafe bool VulkanCreateSurface(VkHandle vkHandle, VkNonDispatchableHandle* vkNonDispatchableHandle);

        /// <summary>
        /// Creates an OpenGL context for the window.
        /// </summary>
        /// <returns>The created OpenGL context.</returns>
        IGLContext OpenGLCreateContext();

        /// <summary>
        /// Gets the underlying native window pointer.
        /// </summary>
        /// <returns>A pointer to the native window.</returns>
        unsafe Window* GetWindow();

        /// <summary>
        /// Clears the input state for the window.
        /// </summary>
        internal void ClearState();
    }
}