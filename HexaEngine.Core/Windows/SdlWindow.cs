﻿namespace HexaEngine.Core.Windows
{
    using HexaEngine.Core.Assets.Importer;
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
    using System.ComponentModel;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using static Extensions.SdlErrorHandlingExtensions;
    using Key = Input.Key;

    /// <summary>
    /// The main class responsible for managing SDL windows.
    /// </summary>
    public unsafe class SdlWindow : IWindow, INativeWindow
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(Sdl));

        /// <summary>
        /// The SDL instance
        /// </summary>
        protected static readonly Sdl sdl = Application.Sdl;

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
        private readonly TextInputEventArgs keyboardCharEventArgs = new();
        private readonly MouseButtonEventArgs mouseButtonEventArgs = new();
        private readonly MouseMoveEventArgs mouseMotionEventArgs = new();
        private readonly MouseWheelEventArgs mouseWheelEventArgs = new();
        private readonly TouchEventArgs touchEventArgs = new();
        private readonly TouchMoveEventArgs touchMotionEventArgs = new();
        private readonly DropEventArgs dropEventArgs = new();
        private readonly DropFileEventArgs dropFileEventArgs = new();
        private readonly DropTextEventArgs dropTextEventArgs = new();

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
            if (created)
            {
                return;
            }

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
            Application.RegisterWindow((ICoreWindow)this);
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
            OnClosing(closeEventArgs);
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
        /// Sets the window to Fullscreen mode.
        ///</summary>
        ///<param name="mode">The Fullscreen mode to set.</param>
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
        /// Gets the windows border size
        /// </summary>
        public Rectangle BorderSize
        {
            get
            {
                Rectangle result;
                sdl.GetWindowBordersSize(window, &result.Top, &result.Left, &result.Bottom, &result.Right);
                return result;
            }
        }

        /// <summary>
        /// Gets the mouse position inside of the window client area.
        /// </summary>
        /// <remarks>Nan signals that the window is not focused.</remarks>
        public Vector2 MousePosition
        {
            get
            {
                if (!hovering)
                {
                    return new(float.NaN);
                }

                int x, y;
                sdl.GetMouseState(&x, &y);
                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the mouse is hovering over the window.
        /// </summary>
        public bool Hovering => hovering;

        /// <summary>
        /// Gets a value indicating whether the window has input focus.
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

        private readonly EventHandlers<ShownEventArgs> ShownHandlers = new();
        private readonly EventHandlers<HiddenEventArgs> HiddenHandlers = new();
        private readonly EventHandlers<ExposedEventArgs> ExposedHandlers = new();
        private readonly EventHandlers<MovedEventArgs> MovedHandlers = new();
        private readonly EventHandlers<ResizedEventArgs> ResizedHandlers = new();
        private readonly EventHandlers<SizeChangedEventArgs> SizeChangedHandlers = new();
        private readonly EventHandlers<MinimizedEventArgs> MinimizedHandlers = new();
        private readonly EventHandlers<MaximizedEventArgs> MaximizedHandlers = new();
        private readonly EventHandlers<RestoredEventArgs> RestoredHandlers = new();
        private readonly EventHandlers<EnterEventArgs> EnterHandlers = new();
        private readonly EventHandlers<LeaveEventArgs> LeaveHandlers = new();
        private readonly EventHandlers<FocusGainedEventArgs> FocusGainedHandlers = new();
        private readonly EventHandlers<FocusLostEventArgs> FocusLostHandlers = new();
        private readonly EventHandlers<CloseEventArgs> ClosingHandlers = new();
        private readonly EventHandlers<CloseEventArgs> ClosedHandlers = new();
        private readonly EventHandlers<TakeFocusEventArgs> TakeFocusHandlers = new();
        private readonly EventHandlers<HitTestEventArgs> HitTestHandlers = new();
        private readonly EventHandlers<KeyboardEventArgs> KeyboardInputHandlers = new();
        private readonly EventHandlers<TextInputEventArgs> KeyboardCharInputHandlers = new();
        private readonly EventHandlers<MouseButtonEventArgs> MouseButtonInputHandlers = new();
        private readonly EventHandlers<MouseMoveEventArgs> MouseMotionInputHandlers = new();
        private readonly EventHandlers<MouseWheelEventArgs> MouseWheelInputHandlers = new();
        private readonly EventHandlers<TouchEventArgs> TouchInputHandlers = new();
        private readonly EventHandlers<TouchMoveEventArgs> TouchMotionInputHandlers = new();
        private readonly EventHandlers<DropEventArgs> DropBeginHandlers = new();
        private readonly EventHandlers<DropFileEventArgs> DropFileHandlers = new();
        private readonly EventHandlers<DropTextEventArgs> DropTextHandlers = new();
        private readonly EventHandlers<DropEventArgs> DropCompleteHandlers = new();

        /// <summary>
        /// Event triggered when the window is shown.
        /// </summary>
        public event EventHandler<ShownEventArgs> Shown
        {
            add => ShownHandlers.AddHandler(value);
            remove => ShownHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is hidden.
        /// </summary>
        public event EventHandler<HiddenEventArgs> Hidden
        {
            add => HiddenHandlers.AddHandler(value);
            remove => HiddenHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is exposed.
        /// </summary>
        public event EventHandler<ExposedEventArgs> Exposed
        {
            add => ExposedHandlers.AddHandler(value);
            remove => ExposedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is moved.
        /// </summary>
        public event EventHandler<MovedEventArgs> Moved
        {
            add => MovedHandlers.AddHandler(value);
            remove => MovedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is resized.
        /// </summary>
        public event EventHandler<ResizedEventArgs> Resized
        {
            add => ResizedHandlers.AddHandler(value);
            remove => ResizedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window size is changed.
        /// </summary>
        public event EventHandler<SizeChangedEventArgs> SizeChanged
        {
            add => SizeChangedHandlers.AddHandler(value);
            remove => SizeChangedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is minimized.
        /// </summary>
        public event EventHandler<MinimizedEventArgs> Minimized
        {
            add => MinimizedHandlers.AddHandler(value);
            remove => MinimizedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is maximized.
        /// </summary>
        public event EventHandler<MaximizedEventArgs> Maximized
        {
            add => MaximizedHandlers.AddHandler(value);
            remove => MaximizedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is restored.
        /// </summary>
        public event EventHandler<RestoredEventArgs> Restored
        {
            add => RestoredHandlers.AddHandler(value);
            remove => RestoredHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the mouse enters the window.
        /// </summary>
        public event EventHandler<EnterEventArgs> Enter
        {
            add => EnterHandlers.AddHandler(value);
            remove => EnterHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the mouse leaves the window.
        /// </summary>
        public event EventHandler<LeaveEventArgs> Leave
        {
            add => LeaveHandlers.AddHandler(value);
            remove => LeaveHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window gains focus.
        /// </summary>
        public event EventHandler<FocusGainedEventArgs> FocusGained
        {
            add => FocusGainedHandlers.AddHandler(value);
            remove => FocusGainedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window loses focus.
        /// </summary>
        public event EventHandler<FocusLostEventArgs> FocusLost
        {
            add => FocusLostHandlers.AddHandler(value);
            remove => FocusLostHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is closing.
        /// </summary>
        public event EventHandler<CloseEventArgs> Closing
        {
            add => ClosingHandlers.AddHandler(value);
            remove => ClosingHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window is closed.
        /// </summary>
        public event EventHandler<CloseEventArgs> Closed
        {
            add => ClosedHandlers.AddHandler(value);
            remove => ClosedHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the window requests to take focus.
        /// </summary>
        public event EventHandler<TakeFocusEventArgs> TakeFocus
        {
            add => TakeFocusHandlers.AddHandler(value);
            remove => TakeFocusHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a hit test is performed on the window.
        /// </summary>
        public event EventHandler<HitTestEventArgs> HitTest
        {
            add => HitTestHandlers.AddHandler(value);
            remove => HitTestHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a keyboard input is received.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyboardInput
        {
            add => KeyboardInputHandlers.AddHandler(value);
            remove => KeyboardInputHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a character input is received from the keyboard.
        /// </summary>
        public event EventHandler<TextInputEventArgs> KeyboardCharInput
        {
            add => KeyboardCharInputHandlers.AddHandler(value);
            remove => KeyboardCharInputHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a mouse button input is received.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseButtonInput
        {
            add => MouseButtonInputHandlers.AddHandler(value);
            remove => MouseButtonInputHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a mouse motion input is received.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseMotionInput
        {
            add => MouseMotionInputHandlers.AddHandler(value);
            remove => MouseMotionInputHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a mouse wheel input is received.
        /// </summary>
        public event EventHandler<MouseWheelEventArgs> MouseWheelInput
        {
            add => MouseWheelInputHandlers.AddHandler(value);
            remove => MouseWheelInputHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a touch input is received.
        /// </summary>
        public event EventHandler<TouchEventArgs> TouchInput
        {
            add => TouchInputHandlers.AddHandler(value);
            remove => TouchInputHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when a touch motion input is received.
        /// </summary>
        public event EventHandler<TouchMoveEventArgs> TouchMotionInput
        {
            add => TouchMotionInputHandlers.AddHandler(value);
            remove => TouchMotionInputHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the user drops a file/text onto the window.
        /// </summary>
        public event EventHandler<DropEventArgs> DropBegin
        {
            add => DropBeginHandlers.AddHandler(value);
            remove => DropBeginHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the user drops a file onto the window.
        /// </summary>
        public event EventHandler<DropFileEventArgs> DropFile
        {
            add => DropFileHandlers.AddHandler(value);
            remove => DropFileHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the user drops a text onto the window.
        /// </summary>
        public event EventHandler<DropTextEventArgs> DropText
        {
            add => DropTextHandlers.AddHandler(value);
            remove => DropTextHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Event triggered when the user drops a file/text onto the window and it's completed.
        /// </summary>
        public event EventHandler<DropEventArgs> DropComplete
        {
            add => DropCompleteHandlers.AddHandler(value);
            remove => DropCompleteHandlers.RemoveHandler(value);
        }

        /// <summary>
        /// Raises the <see cref="Shown"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnShown(ShownEventArgs args)
        {
            ShownHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Hidden"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnHidden(HiddenEventArgs args)
        {
            HiddenHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Exposed"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnExposed(ExposedEventArgs args)
        {
            ExposedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Moved"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMoved(MovedEventArgs args)
        {
            MovedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Resized"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnResized(ResizedEventArgs args)
        {
            ResizedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="SizeChanged"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnSizeChanged(SizeChangedEventArgs args)
        {
            SizeChangedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Minimized"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMinimized(MinimizedEventArgs args)
        {
            MinimizedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Maximized"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMaximized(MaximizedEventArgs args)
        {
            MaximizedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Restored"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnRestored(RestoredEventArgs args)
        {
            RestoredHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Enter"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnEnter(EnterEventArgs args)
        {
            EnterHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Leave"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnLeave(LeaveEventArgs args)
        {
            LeaveHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FocusGained"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnFocusGained(FocusGainedEventArgs args)
        {
            FocusGainedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FocusLost"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnFocusLost(FocusLostEventArgs args)
        {
            FocusLostHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Closing"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnClosing(CloseEventArgs args)
        {
            ClosingHandlers.InvokeRouted(this, args);
            if (!args.Handled)
            {
                OnClosed(args);
            }
            else
            {
                Application.SuppressQuitApp();
            }
        }

        /// <summary>
        /// Raises the <see cref="Closed"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnClosed(CloseEventArgs args)
        {
            DestroyWindow();

            ClosedHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="TakeFocus"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnTakeFocus(TakeFocusEventArgs args)
        {
            TakeFocusHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="HitTest"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnHitTest(HitTestEventArgs args)
        {
            HitTestHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="KeyboardInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnKeyboardInput(KeyboardEventArgs args)
        {
            KeyboardInputHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="KeyboardCharInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnKeyboardCharInput(TextInputEventArgs args)
        {
            KeyboardCharInputHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="MouseButtonInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMouseButtonInput(MouseButtonEventArgs args)
        {
            MouseButtonInputHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="MouseMotionInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMouseMotionInput(MouseMoveEventArgs args)
        {
            MouseMotionInputHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="MouseWheelInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnMouseWheelInput(MouseWheelEventArgs args)
        {
            MouseWheelInputHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="TouchInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnTouchInput(TouchEventArgs args)
        {
            TouchInputHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="TouchMotionInput"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnTouchMotionInput(TouchMoveEventArgs args)
        {
            TouchMotionInputHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="DropBegin"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnDropBegin(DropEventArgs args)
        {
            DropBeginHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="DropFile"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnDropFile(DropFileEventArgs args)
        {
            DropFileHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="DropText"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnDropText(DropTextEventArgs args)
        {
            DropTextHandlers.InvokeRouted(this, args);
        }

        /// <summary>
        /// Raises the <see cref="DropComplete"/> event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        protected virtual void OnDropComplete(DropEventArgs args)
        {
            DropCompleteHandlers.InvokeRouted(this, args);
        }

        #endregion Events

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
                        exposedEventArgs.Handled = false;
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
                        enterEventArgs.Handled = false;
                        OnEnter(enterEventArgs);
                    }
                    break;

                case WindowEventID.Leave:
                    {
                        hovering = false;
                        leaveEventArgs.Handled = false;
                        OnLeave(leaveEventArgs);
                    }
                    break;

                case WindowEventID.FocusGained:
                    {
                        focused = true;
                        focusGainedEventArgs.Handled = false;
                        OnFocusGained(focusGainedEventArgs);
                    }
                    break;

                case WindowEventID.FocusLost:
                    {
                        focused = false;
                        focusLostEventArgs.Handled = false;
                        OnFocusLost(focusLostEventArgs);
                    }
                    break;

                case WindowEventID.Close:
                    {
                        closeEventArgs.Handled = false;
                        OnClosing(closeEventArgs);
                        if (closeEventArgs.Handled)
                        {
                            sdl.ShowWindow(window);
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
                        hitTestEventArgs.Timestamp = evnt.Timestamp;
                        hitTestEventArgs.Handled = false;
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
            keyboardEventArgs.Timestamp = evnt.Timestamp;
            keyboardEventArgs.Handled = false;
            keyboardEventArgs.State = state;
            keyboardEventArgs.KeyCode = keyCode;
            keyboardEventArgs.ScanCode = (ScanCode)evnt.Keysym.Scancode;
            OnKeyboardInput(keyboardEventArgs);
        }

        /// <summary>
        /// Processes a text input event received from the message loop.
        /// </summary>
        /// <param name="evnt">The text input event to process.</param>
        internal void ProcessInputText(TextInputEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            keyboardCharEventArgs.Timestamp = evnt.Timestamp;
            keyboardCharEventArgs.Handled = false;
            keyboardCharEventArgs.Text = evnt.Text;
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
            mouseButtonEventArgs.Timestamp = evnt.Timestamp;
            mouseButtonEventArgs.Handled = false;
            mouseButtonEventArgs.MouseId = evnt.Which;
            mouseButtonEventArgs.Button = button;
            mouseButtonEventArgs.State = state;
            mouseButtonEventArgs.Clicks = evnt.Clicks;
            mouseButtonEventArgs.Position = new(evnt.X, evnt.Y);
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

            mouseMotionEventArgs.Timestamp = evnt.Timestamp;
            mouseMotionEventArgs.Handled = false;
            mouseMotionEventArgs.MouseId = evnt.Which;
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
            mouseWheelEventArgs.Timestamp = evnt.Timestamp;
            mouseWheelEventArgs.Handled = false;
            mouseWheelEventArgs.MouseId = evnt.Which;
            mouseWheelEventArgs.Wheel = new(evnt.X, evnt.Y);
            mouseWheelEventArgs.Direction = (Input.MouseWheelDirection)evnt.Direction;
            OnMouseWheelInput(mouseWheelEventArgs);
        }

        internal void ProcessInputTouchMotion(TouchFingerEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            touchMotionEventArgs.Timestamp = evnt.Timestamp;
            touchMotionEventArgs.TouchDeviceId = evnt.TouchId;
            touchMotionEventArgs.FingerId = evnt.FingerId;
            touchMotionEventArgs.Pressure = evnt.Pressure;
            touchMotionEventArgs.X = evnt.X;
            touchMotionEventArgs.Y = evnt.Y;
            touchMotionEventArgs.Dx = evnt.Dx;
            touchMotionEventArgs.Dy = evnt.Dy;
            OnTouchMotionInput(touchMotionEventArgs);
        }

        internal void ProcessInputTouchUp(TouchFingerEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            touchEventArgs.Timestamp = evnt.Timestamp;
            touchEventArgs.TouchDeviceId = evnt.TouchId;
            touchEventArgs.FingerId = evnt.FingerId;
            touchEventArgs.Pressure = evnt.Pressure;
            touchEventArgs.X = evnt.X;
            touchEventArgs.Y = evnt.Y;
            touchEventArgs.State = FingerState.Up;
            OnTouchInput(touchEventArgs);
        }

        internal void ProcessInputTouchDown(TouchFingerEvent evnt)
        {
            Logger.ThrowIf(destroyed, "The window is already destroyed");
            touchEventArgs.Timestamp = evnt.Timestamp;
            touchEventArgs.TouchDeviceId = evnt.TouchId;
            touchEventArgs.FingerId = evnt.FingerId;
            touchEventArgs.Pressure = evnt.Pressure;
            touchEventArgs.X = evnt.X;
            touchEventArgs.Y = evnt.Y;
            touchEventArgs.State = FingerState.Down;
            OnTouchInput(touchEventArgs);
        }

        internal void ProcessDropBegin(DropEvent evnt)
        {
            dropEventArgs.Timestamp = evnt.Timestamp;
            dropEventArgs.Handled = false;
            OnDropBegin(dropEventArgs);
        }

        internal void ProcessDropFile(DropEvent evnt)
        {
            int x, y;
            sdl.GetMouseState(&x, &y);
            dropFileEventArgs.Timestamp = evnt.Timestamp;
            dropFileEventArgs.File = evnt.File;
            dropFileEventArgs.X = x;
            dropFileEventArgs.Y = y;
            dropFileEventArgs.Handled = false;
            OnDropFile(dropFileEventArgs);
        }

        internal void ProcessDropText(DropEvent evnt)
        {
            int x, y;
            sdl.GetMouseState(&x, &y);
            dropTextEventArgs.Timestamp = evnt.Timestamp;
            dropTextEventArgs.Text = evnt.File;
            dropTextEventArgs.X = x;
            dropTextEventArgs.Y = y;
            dropTextEventArgs.Handled = false;
            OnDropText(dropTextEventArgs);
        }

        internal void ProcessDropComplete(DropEvent evnt)
        {
            dropEventArgs.Timestamp = evnt.Timestamp;
            dropEventArgs.Handled = false;
            OnDropComplete(dropEventArgs);
        }

        /// <summary>
        /// Destroys the window.
        /// </summary>
        internal void DestroyWindow()
        {
            if (!destroyed && Application.MainWindow != this)
            {
                ShownHandlers.Clear();
                HiddenHandlers.Clear();
                ExposedHandlers.Clear();
                MovedHandlers.Clear();
                ResizedHandlers.Clear();
                SizeChangedHandlers.Clear();
                MinimizedHandlers.Clear();
                MaximizedHandlers.Clear();
                RestoredHandlers.Clear();
                EnterHandlers.Clear();
                LeaveHandlers.Clear();
                FocusGainedHandlers.Clear();
                FocusLostHandlers.Clear();
                ClosingHandlers.Clear();
                ClosedHandlers.Clear();
                TakeFocusHandlers.Clear();
                HitTestHandlers.Clear();
                KeyboardInputHandlers.Clear();
                KeyboardCharInputHandlers.Clear();
                MouseButtonInputHandlers.Clear();
                MouseMotionInputHandlers.Clear();
                MouseWheelInputHandlers.Clear();
                TouchInputHandlers.Clear();
                TouchMotionInputHandlers.Clear();
                DropBeginHandlers.Clear();
                DropFileHandlers.Clear();
                DropTextHandlers.Clear();
                DropCompleteHandlers.Clear();

                for (SystemCursor i = 0; i < SystemCursor.NumSystemCursors; i++)
                {
                    sdl.FreeCursor(cursors[(int)i]);
                }
                Free(cursors);
                cursors = null;

                sdl.DestroyWindow(window);
                SdlCheckError();
                window = null;

                destroyed = true;
                created = false;
            }
        }
    }
}