namespace HexaEngine.Core.Windows
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Mathematics;
    using Silk.NET.Core.Contexts;
    using Silk.NET.Core.Native;
    using Silk.NET.SDL;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using Key = Input.Key;

    /// <summary>
    /// The main class responsible for managing SDL windows.
    /// </summary>
    public unsafe class SdlWindow : IWindow, INativeWindow
    {
        protected readonly Sdl sdl = Silk.NET.SDL.Sdl.GetApi();
        private readonly ShownEventArgs shownEventArgs = new();
        private readonly HiddenEventArgs hiddenEventArgs = new();
        private readonly ExposedEventArgs exposedEventArgs = new();
        private readonly MovedEventArgs movedEventArgs = new();
        private readonly ResizedEventArgs resizedEventArgs = new();
        private readonly SizeChangedEventArgs sizeChangedEventArgs = new();
        private readonly MinimizedEventArgs minimizedEventArgs = new();
        private readonly MaximizedEventArgs maximizedEventArgs = new();
        private readonly RestoredEventArgs restoredEventArgs = new();
        private readonly EnterEventArgs enterEventArgs = new();
        private readonly LeaveEventArgs leaveEventArgs = new();
        private readonly FocusGainedEventArgs focusGainedEventArgs = new();
        private readonly FocusLostEventArgs focusLostEventArgs = new();
        private readonly CloseEventArgs closeEventArgs = new();
        private readonly TakeFocusEventArgs takeFocusEventArgs = new();
        private readonly HitTestEventArgs hitTestEventArgs = new();
        private readonly KeyboardEventArgs keyboardEventArgs = new();
        private readonly KeyboardCharEventArgs keyboardCharEventArgs = new();
        private readonly MouseButtonEventArgs mouseButtonEventArgs = new();
        private readonly MouseMotionEventArgs mouseMotionEventArgs = new();
        private readonly MouseWheelEventArgs mouseWheelEventArgs = new();

        private Window* window;
        private bool created;
        private int width = 1280;
        private int height = 720;
        private int y = 100;
        private int x = 100;
        private bool hovering;
        private bool focused;
        private WindowState state;
        private string title = "Window";
        private bool lockCursor;
        private bool resizable = true;
        private bool bordered = true;
        private bool destroyed = false;

        private Cursor** cursors;

        /// <summary>
        /// The position of the window centered on the screen.
        /// </summary>
        public const int WindowPosCentered = Silk.NET.SDL.Sdl.WindowposCentered;

        /// <summary>
        /// Creates a new instance of the <see cref="SdlWindow"/> class.
        /// </summary>
        public SdlWindow(WindowFlags flags = WindowFlags.Resizable)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Kind = NativeWindowFlags.Win32;
            }
            else
            {
                Kind = NativeWindowFlags.Sdl;
            }

            PlatformConstruct(flags);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SdlWindow"/> class.
        /// </summary>
        public SdlWindow(int x, int y, int width, int height, WindowFlags flags = WindowFlags.Resizable)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Kind = NativeWindowFlags.Win32;
            }
            else
            {
                Kind = NativeWindowFlags.Sdl;
            }

            PlatformConstruct(flags);
        }

        /// <summary>
        /// Method called to construct the platform-specific window.
        /// </summary>
        internal void PlatformConstruct(WindowFlags flags)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(title);
            byte* ptr = (byte*)Unsafe.AsPointer(ref bytes[0]);

            flags |= WindowFlags.Hidden;

            switch (Application.GraphicsBackend)
            {
                case GraphicsBackend.Vulkan:
                    flags |= WindowFlags.Vulkan;
                    break;

                case GraphicsBackend.OpenGL:
                    flags |= WindowFlags.Opengl;
                    break;

                case GraphicsBackend.Metal:
                    flags |= WindowFlags.Metal;
                    break;
            }

            window = SdlCheckError(sdl.CreateWindow(ptr, x, y, width, height, (uint)flags));

            WindowID = sdl.GetWindowID(window).SdlThrowIf();

            int w;
            int h;
            sdl.GetWindowSize(window, &w, &h);

            cursors = (Cursor**)AllocArray((uint)SystemCursor.NumSystemCursors);
            cursors[(int)CursorType.Arrow] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorArrow));
            cursors[(int)CursorType.IBeam] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorIbeam));
            cursors[(int)CursorType.Wait] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorWait));
            cursors[(int)CursorType.Crosshair] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorCrosshair));
            cursors[(int)CursorType.WaitArrow] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorWaitarrow));
            cursors[(int)CursorType.SizeNWSE] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorSizens));
            cursors[(int)CursorType.SizeNESW] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorSizewe));
            cursors[(int)CursorType.SizeWE] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorSizenesw));
            cursors[(int)CursorType.SizeNS] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorSizenwse));
            cursors[(int)CursorType.SizeAll] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorSizeall));
            cursors[(int)CursorType.No] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorNo));
            cursors[(int)CursorType.Hand] = SdlCheckError(sdl.CreateSystemCursor(SystemCursor.SystemCursorHand));

            Width = w;
            Height = h;
            Viewport = new(0, 0, w, h, 0, 1);
            created = true;
            destroyed = false;
        }

        ///<summary>
        /// Shows the window.
        ///</summary>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public void Show()
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            Application.RegisterWindow((IRenderWindow)this);
            sdl.ShowWindow(window);
        }

        ///<summary>
        /// Hides the window.
        ///</summary>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public void Hide()
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            sdl.HideWindow(window);
            OnHidden(hiddenEventArgs);
        }

        ///<summary>
        /// Closes the window.
        ///</summary>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public void Close()
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            closeEventArgs.Handled = false;
            OnClose(closeEventArgs);
        }

        ///<summary>
        /// Releases the mouse capture from the window.
        ///</summary>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public void ReleaseCapture()
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            sdl.CaptureMouse(SdlBool.False);
        }

        ///<summary>
        /// Captures the mouse within the window.
        ///</summary>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public void Capture()
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            sdl.CaptureMouse(SdlBool.True);
        }

        ///<summary>
        /// Sets the window to fullscreen mode.
        ///</summary>
        ///<param name="mode">The fullscreen mode to set.</param>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public void Fullscreen(FullscreenMode mode)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            sdl.SetWindowFullscreen(window, (uint)mode);
        }

        ///<summary>
        /// Creates a Vulkan surface for the window.
        ///</summary>
        ///<param name="vkHandle">The Vulkan handle.</param>
        ///<param name="vkNonDispatchableHandle">The Vulkan non-dispatchable handle.</param>
        ///<returns><c>true</c> if the Vulkan surface is created successfully; otherwise, <c>false</c>.</returns>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public bool VulkanCreateSurface(VkHandle vkHandle, VkNonDispatchableHandle* vkNonDispatchableHandle)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            return sdl.VulkanCreateSurface(window, vkHandle, vkNonDispatchableHandle) == SdlBool.True;
        }

        ///<summary>
        /// Creates an OpenGL context for the window.
        ///</summary>
        ///<returns>The created OpenGL context.</returns>
        ///<exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public IGLContext OpenGLCreateContext()
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            return new SdlContext(sdl, window, null, (GLattr.ContextMajorVersion, 4), (GLattr.ContextMinorVersion, 5));
        }

        /// <summary>
        /// Gets a pointer to the window.
        /// </summary>
        /// <returns>A pointer to the window.</returns>
        public Window* GetWindow() => window;

        /// <summary>
        /// Gets the window ID.
        /// </summary>
        public uint WindowID { get; private set; }

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public string Title
        {
            get => title;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                title = value;
                sdl.SetWindowTitle(window, value);
            }
        }

        /// <summary>
        /// Gets or sets the X position of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public int X
        {
            get => x;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                x = value;
                sdl.SetWindowPosition(window, value, y);
            }
        }

        /// <summary>
        /// Gets or sets the Y position of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public int Y
        {
            get => y;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                y = value;
                sdl.SetWindowPosition(window, x, value);
            }
        }

        /// <summary>
        /// Gets or sets the width of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public int Width
        {
            get => width;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                resizedEventArgs.OldWidth = width;
                resizedEventArgs.NewWidth = value;
                width = value;
                sdl.SetWindowSize(window, value, height);
                Viewport = new(width, height);
                OnResized(resizedEventArgs);
            }
        }

        /// <summary>
        /// Gets or sets the height of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public int Height
        {
            get => height;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                resizedEventArgs.OldHeight = height;
                resizedEventArgs.NewHeight = value;
                height = value;
                sdl.SetWindowSize(window, width, value);
                Viewport = new(width, height);
                OnResized(resizedEventArgs);
            }
        }

        /// <summary>
        /// Gets a _value indicating whether the mouse is hovering over the window.
        /// </summary>
        public bool Hovering => hovering;

        /// <summary>
        /// Gets a _value indicating whether the window has input focus.
        /// </summary>
        public bool Focused => focused;

        /// <summary>
        /// Gets or sets the state of the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public WindowState State
        {
            get => state;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                state = value;
                switch (value)
                {
                    case WindowState.Hidden:
                        sdl.HideWindow(window);
                        break;

                    case WindowState.Normal:
                        sdl.ShowWindow(window);
                        break;

                    case WindowState.Minimized:
                        sdl.MinimizeWindow(window);
                        break;

                    case WindowState.Maximized:
                        sdl.MaximizeWindow(window);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets a _value indicating whether the cursor is locked to the window.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public bool LockCursor
        {
            get => lockCursor;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                lockCursor = value;
                sdl.SetRelativeMouseMode(value ? SdlBool.True : SdlBool.False);
            }
        }

        /// <summary>
        /// Gets or sets a _value indicating whether the window is resizable.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public bool Resizable
        {
            get => resizable;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                resizable = value;
                sdl.SetWindowResizable(window, value ? SdlBool.True : SdlBool.False);
            }
        }

        /// <summary>
        /// Gets or sets a _value indicating whether the window has a border.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the window is already destroyed.</exception>
        public bool Bordered
        {
            get => bordered;
            set
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                bordered = value;
                sdl.SetWindowBordered(window, value ? SdlBool.True : SdlBool.False);
            }
        }

        /// <summary>
        /// Gets or sets the viewport of the window.
        /// </summary>
        public Viewport Viewport { get; private set; }

        /// <summary>
        /// Gets the native window flags.
        /// </summary>
        public NativeWindowFlags Kind { get; }

        /// <summary>
        /// Gets the X11 information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when X11 information is not supported.</exception>
        public (nint Display, nuint Window)? X11 => throw new NotSupportedException();

        /// <summary>
        /// Gets the Cocoa information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when Cocoa information is not supported.</exception>
        public nint? Cocoa => throw new NotSupportedException();

        /// <summary>
        /// Gets the Wayland information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when Wayland information is not supported.</exception>
        public (nint Display, nint Surface)? Wayland => throw new NotSupportedException();

        /// <summary>
        /// Gets the WinRT information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when WinRT information is not supported.</exception>
        public nint? WinRT => throw new NotSupportedException();

        /// <summary>
        /// Gets the UIKit information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when UIKit information is not supported.</exception>
        public (nint Window, uint Framebuffer, uint Colorbuffer, uint ResolveFramebuffer)? UIKit => throw new NotSupportedException();

        /// <summary>
        /// Gets the Win32 information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when Win32 information is not supported.</exception>
        public (nint Hwnd, nint HDC, nint HInstance)? Win32
        {
            get
            {
                Logger.ThrowIf(destroyed, "The window is already destroyed");
                SysWMInfo wmInfo;
                sdl.GetVersion(&wmInfo.Version);
                sdl.GetWindowWMInfo(window, &wmInfo);

                return (wmInfo.Info.Win.Hwnd, wmInfo.Info.Win.HDC, wmInfo.Info.Win.HInstance);
            }
        }

        /// <summary>
        /// Gets the Vivante information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when Vivante information is not supported.</exception>
        public (nint Display, nint Window)? Vivante => throw new NotSupportedException();

        /// <summary>
        /// Gets the Android information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when Android information is not supported.</exception>
        public (nint Window, nint Surface)? Android => throw new NotSupportedException();

        /// <summary>
        /// Gets the GLFW information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when GLFW information is not supported.</exception>
        public nint? Glfw => throw new NotSupportedException();

        /// <summary>
        /// Gets the SDL information of the window.
        /// </summary>
        public nint? Sdl => (nint)window;

        /// <summary>
        /// Gets the DirectX handle of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when DirectX handle is not supported.</exception>
        public nint? DXHandle => throw new NotSupportedException();

        /// <summary>
        /// Gets the EGL information of the window.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when EGL information is not supported.</exception>
        public (nint? Display, nint? Surface)? EGL => throw new NotSupportedException();

        /// <summary>
        /// Gets the native window interface.
        /// </summary>
        public INativeWindow? Native => this;

        #region Events

        /// <summary>
        /// Event triggered when the window is shown.
        /// </summary>
        public event EventHandler<ShownEventArgs>? Shown;

        /// <summary>
        /// Event triggered when the window is hidden.
        /// </summary>
        public event EventHandler<HiddenEventArgs>? Hidden;

        /// <summary>
        /// Event triggered when the window is exposed.
        /// </summary>
        public event EventHandler<ExposedEventArgs>? Exposed;

        /// <summary>
        /// Event triggered when the window is moved.
        /// </summary>
        public event EventHandler<MovedEventArgs>? Moved;

        /// <summary>
        /// Event triggered when the window is resized.
        /// </summary>
        public event EventHandler<ResizedEventArgs>? Resized;

        /// <summary>
        /// Event triggered when the window size is changed.
        /// </summary>
        public event EventHandler<SizeChangedEventArgs>? SizeChanged;

        /// <summary>
        /// Event triggered when the window is minimized.
        /// </summary>
        public event EventHandler<MinimizedEventArgs>? Minimized;

        /// <summary>
        /// Event triggered when the window is maximized.
        /// </summary>
        public event EventHandler<MaximizedEventArgs>? Maximized;

        /// <summary>
        /// Event triggered when the window is restored.
        /// </summary>
        public event EventHandler<RestoredEventArgs>? Restored;

        /// <summary>
        /// Event triggered when the mouse enters the window.
        /// </summary>
        public event EventHandler<EnterEventArgs>? Enter;

        /// <summary>
        /// Event triggered when the mouse leaves the window.
        /// </summary>
        public event EventHandler<LeaveEventArgs>? Leave;

        /// <summary>
        /// Event triggered when the window gains focus.
        /// </summary>
        public event EventHandler<FocusGainedEventArgs>? FocusGained;

        /// <summary>
        /// Event triggered when the window loses focus.
        /// </summary>
        public event EventHandler<FocusLostEventArgs>? FocusLost;

        /// <summary>
        /// Event triggered when the window is closing.
        /// </summary>
        public event EventHandler<CloseEventArgs>? Closing;

        /// <summary>
        /// Event triggered when the window requests to take focus.
        /// </summary>
        public event EventHandler<TakeFocusEventArgs>? TakeFocus;

        /// <summary>
        /// Event triggered when a hit test is performed on the window.
        /// </summary>
        public event EventHandler<HitTestEventArgs>? HitTest;

        /// <summary>
        /// Event triggered when a keyboard input is received.
        /// </summary>
        public event EventHandler<KeyboardEventArgs>? KeyboardInput;

        /// <summary>
        /// Event triggered when a character input is received from the keyboard.
        /// </summary>
        public event EventHandler<KeyboardCharEventArgs>? KeyboardCharInput;

        /// <summary>
        /// Event triggered when a mouse button input is received.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs>? MouseButtonInput;

        /// <summary>
        /// Event triggered when a mouse motion input is received.
        /// </summary>
        public event EventHandler<MouseMotionEventArgs>? MouseMotionInput;

        /// <summary>
        /// Event triggered when a mouse wheel input is received.
        /// </summary>
        public event EventHandler<MouseWheelEventArgs>? MouseWheelInput;

        #endregion Events

        #region EventCallMethods

        /// <summary>
        /// Raises the <see cref="Shown"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnShown(ShownEventArgs args)
        {
            Shown?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Hidden"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnHidden(HiddenEventArgs args)
        {
            Hidden?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Exposed"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnExposed(ExposedEventArgs args)
        {
            Exposed?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Moved"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMoved(MovedEventArgs args)
        {
            Moved?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Resized"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnResized(ResizedEventArgs args)
        {
            Resized?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="SizeChanged"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnSizeChanged(SizeChangedEventArgs args)
        {
            SizeChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Minimized"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMinimized(MinimizedEventArgs args)
        {
            Minimized?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Maximized"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMaximized(MaximizedEventArgs args)
        {
            Maximized?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Restored"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnRestored(RestoredEventArgs args)
        {
            Restored?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Enter"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnEnter(EnterEventArgs args)
        {
            Enter?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Leave"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnLeave(LeaveEventArgs args)
        {
            Leave?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FocusGained"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnFocusGained(FocusGainedEventArgs args)
        {
            FocusGained?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FocusLost"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnFocusLost(FocusLostEventArgs args)
        {
            FocusLost?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Closing"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnClose(CloseEventArgs args)
        {
            Closing?.Invoke(this, args);
            if (!args.Handled && !destroyed)
            {
                if (Application.MainWindow == this)
                {
                    return;
                }

                for (SystemCursor i = 0; i < SystemCursor.NumSystemCursors; i++)
                {
                    sdl.FreeCursor(cursors[(int)i]);
                }

                sdl.DestroyWindow(window);
                SdlCheckError();

                destroyed = true;
                created = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="TakeFocus"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnTakeFocus(TakeFocusEventArgs args)
        {
            TakeFocus?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="HitTest"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnHitTest(HitTestEventArgs args)
        {
            HitTest?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="KeyboardInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnKeyboardInput(KeyboardEventArgs args)
        {
            KeyboardInput?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="KeyboardCharInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnKeyboardCharInput(KeyboardCharEventArgs args)
        {
            KeyboardCharInput?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="MouseButtonInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMouseButtonInput(MouseButtonEventArgs args)
        {
            MouseButtonInput?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="MouseMotionInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMouseMotionInput(MouseMotionEventArgs args)
        {
            MouseMotionInput?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="MouseWheelInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMouseWheelInput(MouseWheelEventArgs args)
        {
            MouseWheelInput?.Invoke(this, args);
        }

        #endregion EventCallMethods

        /// <summary>
        /// Processes a window event received from the message loop.
        /// </summary>
        /// <param name="evnt">The window event to process.</param>
        internal void ProcessEvent(WindowEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            WindowEventID type = (WindowEventID)evnt.Event;
            switch (type)
            {
                case WindowEventID.None:
                    return;

                case WindowEventID.Shown:
                    {
                        shownEventArgs.Handled = false;
                        OnShown(shownEventArgs);
                        if (shownEventArgs.Handled)
                        {
                            sdl.HideWindow(window);
                        }
                    }
                    break;

                case WindowEventID.Hidden:
                    {
                        WindowState oldState = state;
                        state = WindowState.Hidden;
                        hiddenEventArgs.OldState = oldState;
                        hiddenEventArgs.NewState = WindowState.Hidden;
                        hiddenEventArgs.Handled = false;
                        OnHidden(hiddenEventArgs);
                        if (hiddenEventArgs.Handled)
                        {
                            sdl.ShowWindow(window);
                        }
                    }
                    break;

                case WindowEventID.Exposed:
                    {
                        OnExposed(exposedEventArgs);
                    }
                    break;

                case WindowEventID.Moved:
                    {
                        int xold = x;
                        int yold = y;
                        x = evnt.Data1;
                        y = evnt.Data2;
                        movedEventArgs.OldX = xold;
                        movedEventArgs.OldY = yold;
                        movedEventArgs.NewX = x;
                        movedEventArgs.NewY = y;
                        movedEventArgs.Handled = false;
                        OnMoved(movedEventArgs);
                        if (movedEventArgs.Handled)
                        {
                            sdl.SetWindowPosition(window, xold, yold);
                        }
                    }
                    break;

                case WindowEventID.Resized:
                    {
                        int widthOld = width;
                        int heightOld = height;
                        width = evnt.Data1;
                        height = evnt.Data2;
                        Viewport = new(width, height);
                        resizedEventArgs.OldWidth = widthOld;
                        resizedEventArgs.OldWidth = heightOld;
                        resizedEventArgs.NewWidth = width;
                        resizedEventArgs.NewHeight = height;
                        resizedEventArgs.Handled = false;
                        OnResized(resizedEventArgs);
                        if (resizedEventArgs.Handled)
                        {
                            sdl.SetWindowSize(window, widthOld, heightOld);
                        }
                    }
                    break;

                case WindowEventID.SizeChanged:
                    {
                        int widthOld = width;
                        int heightOld = height;
                        width = evnt.Data1;
                        height = evnt.Data2;
                        Viewport = new(width, height);
                        sizeChangedEventArgs.OldWidth = widthOld;
                        sizeChangedEventArgs.OldHeight = heightOld;
                        sizeChangedEventArgs.Width = evnt.Data1;
                        sizeChangedEventArgs.Height = evnt.Data2;
                        sizeChangedEventArgs.Handled = false;
                        OnSizeChanged(sizeChangedEventArgs);
                    }
                    break;

                case WindowEventID.Minimized:
                    {
                        WindowState oldState = state;
                        state = WindowState.Minimized;
                        minimizedEventArgs.OldState = oldState;
                        minimizedEventArgs.NewState = WindowState.Minimized;
                        minimizedEventArgs.Handled = false;
                        OnMinimized(minimizedEventArgs);
                        if (minimizedEventArgs.Handled)
                        {
                            State = oldState;
                        }
                    }
                    break;

                case WindowEventID.Maximized:
                    {
                        WindowState oldState = state;
                        state = WindowState.Maximized;
                        maximizedEventArgs.OldState = oldState;
                        maximizedEventArgs.NewState = WindowState.Maximized;
                        maximizedEventArgs.Handled = false;
                        OnMaximized(maximizedEventArgs);
                        if (maximizedEventArgs.Handled)
                        {
                            State = oldState;
                        }
                    }
                    break;

                case WindowEventID.Restored:
                    {
                        WindowState oldState = state;
                        state = WindowState.Normal;
                        restoredEventArgs.OldState = oldState;
                        restoredEventArgs.NewState = WindowState.Normal;
                        restoredEventArgs.Handled = false;
                        OnRestored(restoredEventArgs);
                        if (restoredEventArgs.Handled)
                        {
                            State = oldState;
                        }
                    }
                    break;

                case WindowEventID.Enter:
                    {
                        hovering = true;
                        OnEnter(enterEventArgs);
                    }
                    break;

                case WindowEventID.Leave:
                    {
                        hovering = false;
                        OnLeave(leaveEventArgs);
                    }
                    break;

                case WindowEventID.FocusGained:
                    {
                        focused = true;
                        OnFocusGained(focusGainedEventArgs);
                    }
                    break;

                case WindowEventID.FocusLost:
                    {
                        focused = false;
                        OnFocusLost(focusLostEventArgs);
                    }
                    break;

                case WindowEventID.Close:
                    {
                        closeEventArgs.Handled = false;
                        OnClose(closeEventArgs);
                        if (!closeEventArgs.Handled)
                        {
                            Close();
                        }
                    }
                    break;

                case WindowEventID.TakeFocus:
                    {
                        takeFocusEventArgs.Handled = false;
                        OnTakeFocus(takeFocusEventArgs);
                        if (!takeFocusEventArgs.Handled)
                        {
                            sdl.SetWindowInputFocus(window).SdlThrowIf();
                        }
                    }
                    break;

                case WindowEventID.HitTest:
                    {
                        OnHitTest(hitTestEventArgs);
                    }
                    break;
            }
        }

        /// <summary>
        /// Processes a keyboard input event received from the message loop.
        /// </summary>
        /// <param name="evnt">The keyboard event to process.</param>
        internal void ProcessInputKeyboard(KeyboardEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            KeyState state = (KeyState)evnt.State;
            Key keyCode = (Key)sdl.GetKeyFromScancode(evnt.Keysym.Scancode);
            keyboardEventArgs.KeyState = state;
            keyboardEventArgs.KeyCode = keyCode;
            keyboardEventArgs.Scancode = evnt.Keysym.Scancode;
            OnKeyboardInput(keyboardEventArgs);
        }

        /// <summary>
        /// Processes a text input event received from the message loop.
        /// </summary>
        /// <param name="evnt">The text input event to process.</param>
        internal void ProcessInputText(TextInputEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            keyboardCharEventArgs.Char = (char)evnt.Text[0];
            OnKeyboardCharInput(keyboardCharEventArgs);
        }

        /// <summary>
        /// Processes a mouse button event received from the message loop.
        /// </summary>
        /// <param name="evnt">The mouse button event to process.</param>
        internal void ProcessInputMouse(MouseButtonEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            MouseButtonState state = (MouseButtonState)evnt.State;
            MouseButton button = (MouseButton)evnt.Button;
            mouseButtonEventArgs.Button = button;
            mouseButtonEventArgs.State = state;
            mouseButtonEventArgs.Clicks = evnt.Clicks;
            OnMouseButtonInput(mouseButtonEventArgs);
        }

        /// <summary>
        /// Processes a mouse motion event received from the message loop.
        /// </summary>
        /// <param name="evnt">The mouse motion event to process.</param>
        internal void ProcessInputMouse(MouseMotionEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            if (lockCursor)
            {
                sdl.WarpMouseInWindow(window, 0, 0);
            }

            mouseMotionEventArgs.X = evnt.X;
            mouseMotionEventArgs.Y = evnt.Y;
            mouseMotionEventArgs.RelX = evnt.Xrel;
            mouseMotionEventArgs.RelY = evnt.Yrel;
            OnMouseMotionInput(mouseMotionEventArgs);
        }

        /// <summary>
        /// Processes a mouse wheel event received from the message loop.
        /// </summary>
        /// <param name="evnt">The mouse wheel event to process.</param>
        internal void ProcessInputMouse(MouseWheelEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            mouseWheelEventArgs.Wheel = new(evnt.X, evnt.Y);
            mouseWheelEventArgs.Direction = (Input.MouseWheelDirection)evnt.Direction;
            OnMouseWheelInput(mouseWheelEventArgs);
        }

        /// <summary>
        /// Destroys the window.
        /// </summary>
        internal void DestroyWindow()
        {
            if (!destroyed)
            {
                for (SystemCursor i = 0; i < SystemCursor.NumSystemCursors; i++)
                {
                    sdl.FreeCursor(cursors[(int)i]);
                }

                sdl.DestroyWindow(window);
                SdlCheckError();

                destroyed = true;
                created = false;
            }
        }

        /// <summary>
        /// Clears the input state for the window.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearState()
        {
            Keyboard.Flush();
            Mouse.Flush();
        }
    }
}