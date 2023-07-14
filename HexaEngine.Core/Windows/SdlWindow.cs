namespace HexaEngine.Core.Windows
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Mathematics;
    using Silk.NET.Core.Attributes;
    using Silk.NET.Core.Contexts;
    using Silk.NET.Core.Native;
    using Silk.NET.SDL;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;
    using Key = Input.Key;

    public enum FullscreenMode
    {
        Windowed,
        Fullscreen,
        WindowedFullscreen,
    }

    public enum CursorType
    {
        Arrow,
        IBeam,
        Wait,
        Crosshair,
        WaitArrow,
        SizeNWSE,
        SizeNESW,
        SizeWE,
        SizeNS,
        SizeAll,
        No,
        Hand,
    }

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

        public const int WindowPosCentered = Silk.NET.SDL.Sdl.WindowposCentered;

        public SdlWindow()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Kind = NativeWindowFlags.Win32;
            }
            else
            {
                Kind = NativeWindowFlags.Sdl;
            }

            PlatformConstruct();
        }

        internal void PlatformConstruct()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(title);
            byte* ptr = (byte*)Unsafe.AsPointer(ref bytes[0]);

            WindowFlags flags = WindowFlags.Resizable | WindowFlags.Hidden;

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

        public void Show()
        {
            ThrowIf(destroyed, "The window is already destroyed");
            Application.RegisterWindow((IRenderWindow)this);
            sdl.ShowWindow(window);
        }

        public void Hide()
        {
            ThrowIf(destroyed, "The window is already destroyed");
            sdl.HideWindow(window);
            OnHidden(hiddenEventArgs);
        }

        public void Close()
        {
            ThrowIf(destroyed, "The window is already destroyed");
            closeEventArgs.Handled = false;
            OnClose(closeEventArgs);
        }

        public void ReleaseCapture()
        {
            ThrowIf(destroyed, "The window is already destroyed");
            sdl.CaptureMouse(SdlBool.False);
        }

        public void Capture()
        {
            ThrowIf(destroyed, "The window is already destroyed");
            sdl.CaptureMouse(SdlBool.True);
        }

        public void Fullscreen(FullscreenMode mode)
        {
            ThrowIf(destroyed, "The window is already destroyed");
            sdl.SetWindowFullscreen(window, (uint)mode);
        }

        public bool VulkanCreateSurface(VkHandle vkHandle, VkNonDispatchableHandle* vkNonDispatchableHandle)
        {
            ThrowIf(destroyed, "The window is already destroyed");
            return sdl.VulkanCreateSurface(window, vkHandle, vkNonDispatchableHandle) == SdlBool.True;
        }

        public IGLContext OpenGLCreateContext()
        {
            ThrowIf(destroyed, "The window is already destroyed");
            return new SdlContext(sdl, window, null, (GLattr.ContextMajorVersion, 4), (GLattr.ContextMinorVersion, 5));
        }

        public Window* GetWindow() => window;

        public uint WindowID { get; private set; }

        public string Title
        {
            get => title;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                title = value;
                sdl.SetWindowTitle(window, value);
            }
        }

        public int X
        {
            get => x;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                x = value;
                sdl.SetWindowPosition(window, value, y);
            }
        }

        public int Y
        {
            get => y;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                y = value;
                sdl.SetWindowPosition(window, x, value);
            }
        }

        public int Width
        {
            get => width;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                resizedEventArgs.OldWidth = width;
                resizedEventArgs.NewWidth = value;
                width = value;
                sdl.SetWindowSize(window, value, height);
                Viewport = new(width, height);
                OnResized(resizedEventArgs);
            }
        }

        public int Height
        {
            get => height;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                resizedEventArgs.OldHeight = height;
                resizedEventArgs.NewHeight = value;
                height = value;
                sdl.SetWindowSize(window, width, value);
                Viewport = new(width, height);
                OnResized(resizedEventArgs);
            }
        }

        public bool Hovering => hovering;

        public bool Focused => focused;

        public WindowState State
        {
            get => state;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
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

        public bool LockCursor
        {
            get => lockCursor;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                lockCursor = value;
                sdl.SetRelativeMouseMode(value ? SdlBool.True : SdlBool.False);
            }
        }

        public bool Resizeable
        {
            get => resizable;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                resizable = value;
                sdl.SetWindowResizable(window, value ? SdlBool.True : SdlBool.False);
            }
        }

        public bool Bordered
        {
            get => bordered;
            set
            {
                ThrowIf(destroyed, "The window is already destroyed");
                bordered = value;
                sdl.SetWindowBordered(window, value ? SdlBool.True : SdlBool.False);
            }
        }

        public Viewport Viewport { get; private set; }

        public NativeWindowFlags Kind { get; }

        public (nint Display, nuint Window)? X11 => throw new NotSupportedException();

        public nint? Cocoa => throw new NotSupportedException();

        public (nint Display, nint Surface)? Wayland => throw new NotSupportedException();

        public nint? WinRT => throw new NotSupportedException();

        public (nint Window, uint Framebuffer, uint Colorbuffer, uint ResolveFramebuffer)? UIKit => throw new NotSupportedException();

        public (nint Hwnd, nint HDC, nint HInstance)? Win32
        {
            get
            {
                ThrowIf(destroyed, "The window is already destroyed");
                SysWMInfo wmInfo;
                sdl.GetVersion(&wmInfo.Version);
                sdl.GetWindowWMInfo(window, &wmInfo);
                return (wmInfo.Info.Win.Hwnd, wmInfo.Info.Win.HDC, wmInfo.Info.Win.HInstance);
            }
        }

        public (nint Display, nint Window)? Vivante => throw new NotSupportedException();

        public (nint Window, nint Surface)? Android => throw new NotSupportedException();

        public nint? Glfw => throw new NotSupportedException();

        public nint? Sdl => (nint)window;

        public nint? DXHandle => throw new NotSupportedException();

        public (nint? Display, nint? Surface)? EGL => throw new NotSupportedException();

        public INativeWindow? Native => this;

        #region Events

        public event EventHandler<ShownEventArgs>? Shown;

        public event EventHandler<HiddenEventArgs>? Hidden;

        public event EventHandler<ExposedEventArgs>? Exposed;

        public event EventHandler<MovedEventArgs>? Moved;

        public event EventHandler<ResizedEventArgs>? Resized;

        public event EventHandler<SizeChangedEventArgs>? SizeChanged;

        public event EventHandler<MinimizedEventArgs>? Minimized;

        public event EventHandler<MaximizedEventArgs>? Maximized;

        public event EventHandler<RestoredEventArgs>? Restored;

        public event EventHandler<EnterEventArgs>? Enter;

        public event EventHandler<LeaveEventArgs>? Leave;

        public event EventHandler<FocusGainedEventArgs>? FocusGained;

        public event EventHandler<FocusLostEventArgs>? FocusLost;

        public event EventHandler<CloseEventArgs>? Closing;

        public event EventHandler<TakeFocusEventArgs>? TakeFocus;

        public event EventHandler<HitTestEventArgs>? HitTest;

        public event EventHandler<KeyboardEventArgs>? KeyboardInput;

        public event EventHandler<KeyboardCharEventArgs>? KeyboardCharInput;

        public event EventHandler<MouseButtonEventArgs>? MouseButtonInput;

        public event EventHandler<MouseMotionEventArgs>? MouseMotionInput;

        public event EventHandler<MouseWheelEventArgs>? MouseWheelInput;

        #endregion Events

        #region EventCallMethods

        protected virtual void OnShown(ShownEventArgs args)
        {
            Shown?.Invoke(this, args);
        }

        protected virtual void OnHidden(HiddenEventArgs args)
        {
            Hidden?.Invoke(this, args);
        }

        protected virtual void OnExposed(ExposedEventArgs args)
        {
            Exposed?.Invoke(this, args);
        }

        protected virtual void OnMoved(MovedEventArgs args)
        {
            Moved?.Invoke(this, args);
        }

        protected virtual void OnResized(ResizedEventArgs args)
        {
            Resized?.Invoke(this, args);
        }

        protected virtual void OnSizeChanged(SizeChangedEventArgs args)
        {
            SizeChanged?.Invoke(this, args);
        }

        protected virtual void OnMinimized(MinimizedEventArgs args)
        {
            Minimized?.Invoke(this, args);
        }

        protected virtual void OnMaximized(MaximizedEventArgs args)
        {
            Maximized?.Invoke(this, args);
        }

        protected virtual void OnRestored(RestoredEventArgs args)
        {
            Restored?.Invoke(this, args);
        }

        protected virtual void OnEnter(EnterEventArgs args)
        {
            Enter?.Invoke(this, args);
        }

        protected virtual void OnLeave(LeaveEventArgs args)
        {
            Leave?.Invoke(this, args);
        }

        protected virtual void OnFocusGained(FocusGainedEventArgs args)
        {
            FocusGained?.Invoke(this, args);
        }

        protected virtual void OnFocusLost(FocusLostEventArgs args)
        {
            FocusLost?.Invoke(this, args);
        }

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

        protected virtual void OnTakeFocus(TakeFocusEventArgs args)
        {
            TakeFocus?.Invoke(this, args);
        }

        protected virtual void OnHitTest(HitTestEventArgs args)
        {
            HitTest?.Invoke(this, args);
        }

        protected virtual void OnKeyboardInput(KeyboardEventArgs args)
        {
            KeyboardInput?.Invoke(this, args);
        }

        protected virtual void OnKeyboardCharInput(KeyboardCharEventArgs args)
        {
            KeyboardCharInput?.Invoke(this, args);
        }

        protected virtual void OnMouseButtonInput(MouseButtonEventArgs args)
        {
            MouseButtonInput?.Invoke(this, args);
        }

        protected virtual void OnMouseMotionInput(MouseMotionEventArgs args)
        {
            MouseMotionInput?.Invoke(this, args);
        }

        protected virtual void OnMouseWheelInput(MouseWheelEventArgs args)
        {
            MouseWheelInput?.Invoke(this, args);
        }

        #endregion EventCallMethods

        internal void ProcessEvent(WindowEvent evnt)
        {
            ThrowIf(destroyed, "The window is already destroyed");
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

        internal void ProcessInputKeyboard(KeyboardEvent evnt)
        {
            ThrowIf(destroyed, "The window is already destroyed");
            KeyState state = (KeyState)evnt.State;
            Key keyCode = (Key)sdl.GetKeyFromScancode(evnt.Keysym.Scancode);
            keyboardEventArgs.KeyState = state;
            keyboardEventArgs.KeyCode = keyCode;
            keyboardEventArgs.Scancode = evnt.Keysym.Scancode;
            OnKeyboardInput(keyboardEventArgs);
        }

        internal void ProcessInputText(TextInputEvent evnt)
        {
            ThrowIf(destroyed, "The window is already destroyed");
            keyboardCharEventArgs.Char = (char)evnt.Text[0];
            OnKeyboardCharInput(keyboardCharEventArgs);
        }

        internal void ProcessInputMouse(MouseButtonEvent evnt)
        {
            ThrowIf(destroyed, "The window is already destroyed");
            MouseButtonState state = (MouseButtonState)evnt.State;
            MouseButton button = (MouseButton)evnt.Button;
            mouseButtonEventArgs.Button = button;
            mouseButtonEventArgs.State = state;
            mouseButtonEventArgs.Clicks = evnt.Clicks;
            OnMouseButtonInput(mouseButtonEventArgs);
        }

        internal void ProcessInputMouse(MouseMotionEvent evnt)
        {
            ThrowIf(destroyed, "The window is already destroyed");
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

        internal void ProcessInputMouse(MouseWheelEvent evnt)
        {
            ThrowIf(destroyed, "The window is already destroyed");
            mouseWheelEventArgs.Wheel = new(evnt.X, evnt.Y);
            mouseWheelEventArgs.Direction = (Input.MouseWheelDirection)evnt.Direction;
            OnMouseWheelInput(mouseWheelEventArgs);
        }

        internal void ProcessInputController()
        {
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearState()
        {
            Keyboard.Flush();
            Mouse.Flush();
        }
    }
}