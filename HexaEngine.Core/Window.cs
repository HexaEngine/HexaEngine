namespace HexaEngine.Core
{
    using HexaEngine.Core.Events;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Mathematics;
    using Silk.NET.SDL;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Text;

    public enum WindowState
    {
        Hidden,
        Normal,
        Minimized,
        Maximized,
    }

    public unsafe class SdlWindow
    {
        private readonly Sdl Sdl = Sdl.GetApi();

        private Window* window;
        private bool created;
        private int width = 1280;
        private int height = 720;
        private int y = 100;
        private int x = 100;
        private bool hovering;
        private bool focused;
        private WindowState state;
        private string title = "MainWindow";
        private bool lockCursor;

        public RenderBackend Backend { get; private set; }

        private void PlatformConstruct()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(title);
            byte* ptr = (byte*)Unsafe.AsPointer(ref bytes[0]);

            window = Sdl.CreateWindow(ptr, x, y, width, height, (uint)(WindowFlags.Resizable | WindowFlags.Hidden));
            WindowID = Sdl.GetWindowID(window);

            int w;
            int h;
            Sdl.GetWindowSize(window, &w, &h);

            Width = w;
            Height = h;
            Viewport = new(0, 0, w, h, 0, 1);
            created = true;
        }

        public void Show()
        {
            if (!created)
                PlatformConstruct();
            Application.RegisterWindow(this);
            Sdl.ShowWindow(window);
        }

        public void ShowHidden()
        {
            if (!created)
                PlatformConstruct();
        }

        public void Close()
        {
            CloseEventArgs args = new();
            OnClose(args);
            if (!args.Handled)
                Sdl.DestroyWindow(window);
        }

        public void ReleaseCapture()
        {
            Sdl.CaptureMouse(SdlBool.False);
        }

        public void Capture()
        {
            Sdl.CaptureMouse(SdlBool.True);
        }

        [SupportedOSPlatform("windows")]
        public IntPtr GetHWND()
        {
            SysWMInfo wmInfo;
            Sdl.GetVersion(&wmInfo.Version);
            Sdl.GetWindowWMInfo(window, &wmInfo);
            return wmInfo.Info.Win.Hwnd;
        }

        public Window* GetWindow() => window;

        public uint WindowID { get; private set; }

        public string Title
        {
            get => title;
            set
            {
                title = value;
                Sdl.SetWindowTitle(window, value);
            }
        }

        public int X
        {
            get => x;
            set
            {
                x = value;
                Sdl.SetWindowPosition(window, value, y);
            }
        }

        public int Y
        {
            get => y;
            set
            {
                y = value;
                Sdl.SetWindowPosition(window, x, value);
            }
        }

        public int Width
        {
            get => width;
            set
            {
                width = value;
                Sdl.SetWindowSize(window, value, height);
            }
        }

        public int Height
        {
            get => height;
            set
            {
                height = value;
                Sdl.SetWindowSize(window, width, value);
            }
        }

        public bool Hovering => hovering;

        public bool Focused => focused;

        public WindowState State
        {
            get => state;
            set
            {
                state = value;
                switch (value)
                {
                    case WindowState.Hidden:
                        Sdl.HideWindow(window);
                        break;

                    case WindowState.Normal:
                        Sdl.ShowWindow(window);
                        break;

                    case WindowState.Minimized:
                        Sdl.MinimizeWindow(window);
                        break;

                    case WindowState.Maximized:
                        Sdl.MaximizeWindow(window);
                        break;
                }
            }
        }

        public bool LockCursor
        {
            get => lockCursor;
            set
            {
                lockCursor = value;
                Sdl.SetRelativeMouseMode(value ? SdlBool.True : SdlBool.False);
            }
        }

        public Viewport Viewport { get; private set; }

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
            WindowEventID type = (WindowEventID)evnt.Event;
            switch (type)
            {
                case WindowEventID.None:
                    return;

                case WindowEventID.Shown:
                    {
                        ShownEventArgs args = new();
                        OnShown(args);
                        if (args.Handled)
                            Sdl.HideWindow(window);
                    }
                    break;

                case WindowEventID.Hidden:
                    {
                        WindowState oldState = state;
                        state = WindowState.Hidden;
                        HiddenEventArgs args = new(oldState, WindowState.Hidden);
                        OnHidden(args);
                        if (args.Handled)
                            Sdl.ShowWindow(window);
                    }
                    break;

                case WindowEventID.Exposed:
                    {
                        ExposedEventArgs args = new();
                        OnExposed(args);
                    }
                    break;

                case WindowEventID.Moved:
                    {
                        int xold = x;
                        int yold = y;
                        x = evnt.Data1;
                        y = evnt.Data2;
                        MovedEventArgs args = new(xold, yold, x, y);
                        OnMoved(args);
                        if (args.Handled)
                            Sdl.SetWindowPosition(window, xold, yold);
                    }
                    break;

                case WindowEventID.Resized:
                    {
                        int widthOld = width;
                        int heightOld = height;
                        width = evnt.Data1;
                        height = evnt.Data2;
                        Viewport = new(width, height);
                        ResizedEventArgs args = new(widthOld, heightOld, width, height);
                        OnResized(args);
                        if (args.Handled)
                            Sdl.SetWindowSize(window, widthOld, heightOld);
                    }
                    break;

                case WindowEventID.SizeChanged:
                    {
                        SizeChangedEventArgs args = new();
                        OnSizeChanged(args);
                    }
                    break;

                case WindowEventID.Minimized:
                    {
                        WindowState oldState = state;
                        state = WindowState.Minimized;
                        MinimizedEventArgs args = new(oldState, WindowState.Minimized);
                        OnMinimized(args);
                        if (args.Handled)
                            State = oldState;
                    }
                    break;

                case WindowEventID.Maximized:
                    {
                        WindowState oldState = state;
                        state = WindowState.Maximized;
                        MaximizedEventArgs args = new(oldState, WindowState.Maximized);
                        OnMaximized(args);
                        if (args.Handled)
                            State = oldState;
                    }
                    break;

                case WindowEventID.Restored:
                    {
                        WindowState oldState = state;
                        state = WindowState.Normal;
                        RestoredEventArgs args = new(oldState, WindowState.Normal);
                        OnRestored(args);
                        if (args.Handled)
                            State = oldState;
                    }
                    break;

                case WindowEventID.Enter:
                    {
                        hovering = true;
                        EnterEventArgs args = new();
                        OnEnter(args);
                    }
                    break;

                case WindowEventID.Leave:
                    {
                        hovering = false;
                        LeaveEventArgs args = new();
                        OnLeave(args);
                    }
                    break;

                case WindowEventID.FocusGained:
                    {
                        focused = true;
                        FocusGainedEventArgs args = new();
                        OnFocusGained(args);
                    }
                    break;

                case WindowEventID.FocusLost:
                    {
                        focused = false;
                        FocusLostEventArgs args = new();
                        OnFocusLost(args);
                    }
                    break;

                case WindowEventID.Close:
                    {
                        CloseEventArgs args = new();
                        OnClose(args);
                        if (!args.Handled)
                            Close();
                    }
                    break;

                case WindowEventID.TakeFocus:
                    {
                        TakeFocusEventArgs args = new();
                        OnTakeFocus(args);
                        if (!args.Handled)
                            Sdl.SetWindowInputFocus(window);
                    }
                    break;

                case WindowEventID.HitTest:
                    {
                        HitTestEventArgs args = new();
                        OnHitTest(args);
                    }
                    break;
            }
        }

        internal void ProcessInputKeyboard(KeyboardEvent evnt)
        {
            KeyState state = (KeyState)evnt.State;
            KeyCode keyCode = (KeyCode)Sdl.GetKeyFromScancode(evnt.Keysym.Scancode);
            KeyboardEventArgs args = new(keyCode, state);
            Keyboard.Update(args);
            OnKeyboardInput(args);
        }

        internal void ProcessInputText(TextInputEvent evnt)
        {
            KeyboardCharEventArgs args = new(Encoding.UTF8.GetString(evnt.Text, 1)[0]);
            Keyboard.Update(args);
            OnKeyboardCharInput(args);
        }

        internal void ProcessInputMouse(MouseButtonEvent evnt)
        {
            KeyState state = (KeyState)evnt.State;
            MouseButton button = (MouseButton)evnt.Button;
            MouseButtonEventArgs args = new(button, state, evnt.Clicks);
            Mouse.Update(args);
            OnMouseButtonInput(args);
        }

        internal void ProcessInputMouse(MouseMotionEvent evnt)
        {
            if (lockCursor)
                Sdl.WarpMouseInWindow(window, 0, 0);
            MouseMotionEventArgs args = new(evnt.X, evnt.Y, evnt.Xrel, evnt.Yrel);
            Mouse.Update(args);
            OnMouseMotionInput(args);
        }

        internal void ProcessInputMouse(MouseWheelEvent evnt)
        {
            MouseWheelEventArgs args = new(evnt.X, evnt.Y, (MouseWheelDirection)evnt.Direction);
            Mouse.Update(args);
            OnMouseWheelInput(args);
        }
    }
}