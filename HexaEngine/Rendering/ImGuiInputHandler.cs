using HexaEngine.Core;
using HexaEngine.Core.Input;
using ImGuiNET;
using Silk.NET.SDL;
using System.Numerics;
using KeyCode = HexaEngine.Core.Input.KeyCode;

namespace HexaEngine.Rendering
{
    internal class ImGuiInputHandler
    {
        private readonly SdlWindow window;
        private ImGuiMouseCursor lastCursor;

        public ImGuiInputHandler(SdlWindow window)
        {
            this.window = window;
            window.MouseButtonInput += MouseButtonInput;
            window.MouseWheelInput += MouseWheelInput;
            window.KeyboardInput += KeyboardInput;
            window.KeyboardCharInput += KeyboardCharInput;
            InitKeyMap();
        }

        private void KeyboardCharInput(object? sender, Core.Input.Events.KeyboardCharEventArgs e)
        {
            var io = ImGui.GetIO();
            io.AddInputCharactersUTF8(e.Char.ToString());
        }

        private void KeyboardInput(object? sender, Core.Input.Events.KeyboardEventArgs e)
        {
            var io = ImGui.GetIO();
            io.AddKeyEvent(KeycodeToImGuiKey(e.KeyCode), e.KeyState == KeyState.Pressed);
        }

        private void MouseWheelInput(object? sender, Core.Input.Events.MouseWheelEventArgs e)
        {
            var io = ImGui.GetIO();
            io.MouseWheel = e.Y;
            io.MouseWheelH = e.X;
        }

        private void MouseButtonInput(object? sender, Core.Input.Events.MouseButtonEventArgs e)
        {
            var io = ImGui.GetIO();
            var index = e.MouseButton switch
            {
                MouseButton.Unknown => -1,
                MouseButton.Left => 0,
                MouseButton.Right => 1,
                MouseButton.Middle => 2,
                MouseButton.X1 => 3,
                MouseButton.X2 => 4,
                _ => -1,
            };
            io.MouseDown[index] = e.KeyState == KeyState.Pressed;

            if (e.KeyState == KeyState.Pressed)
                window.Capture();
            else
                window.ReleaseCapture();
        }

        private void InitKeyMap()
        {
            var io = ImGui.GetIO();
        }

        public void Update()
        {
            var io = ImGui.GetIO();
            io.DisplaySize = new(window.Width, window.Height);
            io.DeltaTime = Time.Delta;
            UpdateKeyModifiers();
            UpdateMousePosition();

            var mouseCursor = ImGui.GetIO().MouseDrawCursor ? ImGuiMouseCursor.None : ImGui.GetMouseCursor();
            if (mouseCursor != lastCursor)
            {
                lastCursor = mouseCursor;
                UpdateMouseCursor();
            }
        }

        private void UpdateKeyModifiers()
        {
            var io = ImGui.GetIO();
            io.KeyCtrl = Keyboard.IsDown(KeyCode.LCtrl);
            io.KeyShift = Keyboard.IsDown(KeyCode.LShift);
            io.KeyAlt = Keyboard.IsDown(KeyCode.Menu);
            io.KeySuper = Keyboard.IsDown(KeyCode.Application);
        }

        public bool UpdateMouseCursor()
        {
            var io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
                return false;

            var requestedcursor = ImGui.GetMouseCursor();
            if (requestedcursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
                CursorHelper.SetCursor(IntPtr.Zero);
            else
            {
                var cursor = SystemCursor.SystemCursorArrow;
                switch (requestedcursor)
                {
                    case ImGuiMouseCursor.Arrow: cursor = SystemCursor.SystemCursorArrow; break;
                    case ImGuiMouseCursor.TextInput: cursor = SystemCursor.SystemCursorIbeam; break;
                    case ImGuiMouseCursor.ResizeAll: cursor = SystemCursor.SystemCursorSizeall; break;
                    case ImGuiMouseCursor.ResizeEW: cursor = SystemCursor.SystemCursorSizewe; break;
                    case ImGuiMouseCursor.ResizeNS: cursor = SystemCursor.SystemCursorSizens; break;
                    case ImGuiMouseCursor.ResizeNESW: cursor = SystemCursor.SystemCursorSizenesw; break;
                    case ImGuiMouseCursor.ResizeNWSE: cursor = SystemCursor.SystemCursorSizenwse; break;
                    case ImGuiMouseCursor.Hand: cursor = SystemCursor.SystemCursorHand; break;
                    case ImGuiMouseCursor.NotAllowed: cursor = SystemCursor.SystemCursorNo; break;
                }
                CursorHelper.SetCursor(cursor);
            }

            return true;
        }

        private void UpdateMousePosition()
        {
            var io = ImGui.GetIO();
            io.MousePos = Mouse.Position;
        }

        private static ImGuiKey KeycodeToImGuiKey(KeyCode keycode)
        {
            return keycode switch
            {
                KeyCode.Tab => ImGuiKey.Tab,
                KeyCode.Left => ImGuiKey.LeftArrow,
                KeyCode.Right => ImGuiKey.RightArrow,
                KeyCode.Up => ImGuiKey.UpArrow,
                KeyCode.Down => ImGuiKey.DownArrow,
                KeyCode.Pageup => ImGuiKey.PageUp,
                KeyCode.Pagedown => ImGuiKey.PageDown,
                KeyCode.Home => ImGuiKey.Home,
                KeyCode.End => ImGuiKey.End,
                KeyCode.Insert => ImGuiKey.Insert,
                KeyCode.Delete => ImGuiKey.Delete,
                KeyCode.Backspace => ImGuiKey.Backspace,
                KeyCode.Space => ImGuiKey.Space,
                KeyCode.Return => ImGuiKey.Enter,
                KeyCode.Escape => ImGuiKey.Escape,
                KeyCode.Quote => ImGuiKey.Apostrophe,
                KeyCode.Comma => ImGuiKey.Comma,
                KeyCode.Minus => ImGuiKey.Minus,
                KeyCode.Period => ImGuiKey.Period,
                KeyCode.Slash => ImGuiKey.Slash,
                KeyCode.Semicolon => ImGuiKey.Semicolon,
                KeyCode.Equals => ImGuiKey.Equal,
                KeyCode.Leftbracket => ImGuiKey.LeftBracket,
                KeyCode.Backslash => ImGuiKey.Backslash,
                KeyCode.Rightbracket => ImGuiKey.RightBracket,
                KeyCode.Backquote => ImGuiKey.GraveAccent,
                KeyCode.Capslock => ImGuiKey.CapsLock,
                KeyCode.Scrolllock => ImGuiKey.ScrollLock,
                KeyCode.Numlockclear => ImGuiKey.NumLock,
                KeyCode.Printscreen => ImGuiKey.PrintScreen,
                KeyCode.Pause => ImGuiKey.Pause,
                KeyCode.Num0 => ImGuiKey.Keypad0,
                KeyCode.Num1 => ImGuiKey.Keypad1,
                KeyCode.Num2 => ImGuiKey.Keypad2,
                KeyCode.Num3 => ImGuiKey.Keypad3,
                KeyCode.Num4 => ImGuiKey.Keypad4,
                KeyCode.Num5 => ImGuiKey.Keypad5,
                KeyCode.Num6 => ImGuiKey.Keypad6,
                KeyCode.Num7 => ImGuiKey.Keypad7,
                KeyCode.Num8 => ImGuiKey.Keypad8,
                KeyCode.Num9 => ImGuiKey.Keypad9,
                KeyCode.NumPeriod => ImGuiKey.KeypadDecimal,
                KeyCode.NumDivide => ImGuiKey.KeypadDivide,
                KeyCode.NumMultiply => ImGuiKey.KeypadMultiply,
                KeyCode.NumMinus => ImGuiKey.KeypadSubtract,
                KeyCode.NumPlus => ImGuiKey.KeypadAdd,
                KeyCode.NumEnter => ImGuiKey.KeypadEnter,
                KeyCode.NumEquals => ImGuiKey.KeypadEqual,
                KeyCode.LCtrl => ImGuiKey.LeftCtrl,
                KeyCode.LShift => ImGuiKey.LeftShift,
                KeyCode.LAlt => ImGuiKey.LeftAlt,
                KeyCode.LGui => ImGuiKey.LeftSuper,
                KeyCode.RCtrl => ImGuiKey.RightCtrl,
                KeyCode.RShift => ImGuiKey.RightShift,
                KeyCode.RAlt => ImGuiKey.RightAlt,
                KeyCode.RGui => ImGuiKey.RightSuper,
                KeyCode.Application => ImGuiKey.Menu,
                KeyCode.N0 => ImGuiKey._0,
                KeyCode.N1 => ImGuiKey._1,
                KeyCode.N2 => ImGuiKey._2,
                KeyCode.N3 => ImGuiKey._3,
                KeyCode.N4 => ImGuiKey._4,
                KeyCode.N5 => ImGuiKey._5,
                KeyCode.N6 => ImGuiKey._6,
                KeyCode.N7 => ImGuiKey._7,
                KeyCode.N8 => ImGuiKey._8,
                KeyCode.N9 => ImGuiKey._9,
                KeyCode.A => ImGuiKey.A,
                KeyCode.B => ImGuiKey.B,
                KeyCode.C => ImGuiKey.C,
                KeyCode.D => ImGuiKey.D,
                KeyCode.E => ImGuiKey.E,
                KeyCode.F => ImGuiKey.F,
                KeyCode.G => ImGuiKey.G,
                KeyCode.H => ImGuiKey.H,
                KeyCode.I => ImGuiKey.I,
                KeyCode.J => ImGuiKey.J,
                KeyCode.K => ImGuiKey.K,
                KeyCode.L => ImGuiKey.L,
                KeyCode.M => ImGuiKey.M,
                KeyCode.N => ImGuiKey.N,
                KeyCode.O => ImGuiKey.O,
                KeyCode.P => ImGuiKey.P,
                KeyCode.Q => ImGuiKey.Q,
                KeyCode.R => ImGuiKey.R,
                KeyCode.S => ImGuiKey.S,
                KeyCode.T => ImGuiKey.T,
                KeyCode.U => ImGuiKey.U,
                KeyCode.V => ImGuiKey.V,
                KeyCode.W => ImGuiKey.W,
                KeyCode.X => ImGuiKey.X,
                KeyCode.Y => ImGuiKey.Y,
                KeyCode.Z => ImGuiKey.Z,
                KeyCode.F1 => ImGuiKey.F1,
                KeyCode.F2 => ImGuiKey.F2,
                KeyCode.F3 => ImGuiKey.F3,
                KeyCode.F4 => ImGuiKey.F4,
                KeyCode.F5 => ImGuiKey.F5,
                KeyCode.F6 => ImGuiKey.F6,
                KeyCode.F7 => ImGuiKey.F7,
                KeyCode.F8 => ImGuiKey.F8,
                KeyCode.F9 => ImGuiKey.F9,
                KeyCode.F10 => ImGuiKey.F10,
                KeyCode.F11 => ImGuiKey.F11,
                KeyCode.F12 => ImGuiKey.F12,
                _ => ImGuiKey.None,
            };
        }

        /*
        public bool ProcessMessage(WindowMessage msg, UIntPtr wParam, IntPtr lParam)
        {
            if (ImGui.GetCurrentContext() == IntPtr.Zero)
                return false;

            var io = ImGui.GetIO();
            switch (msg)
            {
                case WindowMessage.LButtonDown:
                case WindowMessage.LButtonDBLCLK:
                case WindowMessage.RButtonDown:
                case WindowMessage.RButtonDBLCLK:
                case WindowMessage.MButtonDown:
                case WindowMessage.MButtonDBLCLK:
                case WindowMessage.XButtonDown:
                case WindowMessage.XButtonDBLCLK:
                    {
                        int button = 0;
                        if (msg == WindowMessage.LButtonDown || msg == WindowMessage.LButtonDBLCLK) { button = 0; }
                        if (msg == WindowMessage.RButtonDown || msg == WindowMessage.RButtonDBLCLK) { button = 1; }
                        if (msg == WindowMessage.MButtonDown || msg == WindowMessage.MButtonDBLCLK) { button = 2; }
                        if (msg == WindowMessage.XButtonDown || msg == WindowMessage.XButtonDBLCLK) { button = GET_XBUTTON_WPARAM(wParam) == 1 ? 3 : 4; }
                        if (!ImGui.IsAnyMouseDown() && User32.GetCapture() == IntPtr.Zero)
                            User32.SetCapture(hwnd);
                        io.MouseDown[button] = true;
                        return false;
                    }
                case WindowMessage.LButtonUp:
                case WindowMessage.RButtonUp:
                case WindowMessage.MButtonUp:
                case WindowMessage.XButtonUp:
                    {
                        int button = 0;
                        if (msg == WindowMessage.LButtonUp) { button = 0; }
                        if (msg == WindowMessage.RButtonUp) { button = 1; }
                        if (msg == WindowMessage.MButtonUp) { button = 2; }
                        if (msg == WindowMessage.XButtonUp) { button = GET_XBUTTON_WPARAM(wParam) == 1 ? 3 : 4; }
                        io.MouseDown[button] = false;
                        if (!ImGui.IsAnyMouseDown() && User32.GetCapture() == hwnd)
                            User32.ReleaseCapture();
                        return false;
                    }
                case WindowMessage.MouseWheel:
                    io.MouseWheel += GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA;
                    return false;

                case WindowMessage.MouseHWheel:
                    io.MouseWheelH += GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA;
                    return false;

                case WindowMessage.KeyDown:
                case WindowMessage.SysKeyDown:
                    if ((ulong)wParam < 256)
                        io.KeyCodeDown[(int)wParam] = true;
                    return false;

                case WindowMessage.KeyUp:
                case WindowMessage.SysKeyUp:
                    if ((ulong)wParam < 256)
                        io.KeyCodeDown[(int)wParam] = false;
                    return false;

                case WindowMessage.Char:
                    io.AddInputCharacter((uint)wParam);
                    return false;

                case WindowMessage.SetCursor:
                    if (Helper.SignedLOWORD((int)(long)lParam) == 1 && UpdateMouseCursor())
                        return true;
                    return false;

                case WindowMessage.Size:
                    int width = Helper.SignedLOWORD(lParam);
                    int height = Helper.SignedHIWORD(lParam);
                    io.DisplaySize = new(width, height);
                    break;
            }
            return false;
        }

        private static readonly int WHEEL_DELTA = 120;

        */
    }
}