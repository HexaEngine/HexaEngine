using HexaEngine.Core;
using HexaEngine.Core.Input;
using ImGuiNET;
using Silk.NET.SDL;
using System.Numerics;

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
                MouseButton.None => -1,
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
            io.KeyCtrl = Keyboard.IsDown(KeyCode.KLctrl);
            io.KeyShift = Keyboard.IsDown(KeyCode.KLshift);
            io.KeyAlt = Keyboard.IsDown(KeyCode.KMenu);
            io.KeySuper = Keyboard.IsDown(KeyCode.KApplication);
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

            Vector2 mousePosition = Mouse.PositionVector;

            io.MousePos = mousePosition;
        }

        private static ImGuiKey KeycodeToImGuiKey(KeyCode keycode)
        {
            return keycode switch
            {
                KeyCode.KTab => ImGuiKey.Tab,
                KeyCode.KLeft => ImGuiKey.LeftArrow,
                KeyCode.KRight => ImGuiKey.RightArrow,
                KeyCode.KUp => ImGuiKey.UpArrow,
                KeyCode.KDown => ImGuiKey.DownArrow,
                KeyCode.KPageup => ImGuiKey.PageUp,
                KeyCode.KPagedown => ImGuiKey.PageDown,
                KeyCode.KHome => ImGuiKey.Home,
                KeyCode.KEnd => ImGuiKey.End,
                KeyCode.KInsert => ImGuiKey.Insert,
                KeyCode.KDelete => ImGuiKey.Delete,
                KeyCode.KBackspace => ImGuiKey.Backspace,
                KeyCode.KSpace => ImGuiKey.Space,
                KeyCode.KReturn => ImGuiKey.Enter,
                KeyCode.KEscape => ImGuiKey.Escape,
                KeyCode.KQuote => ImGuiKey.Apostrophe,
                KeyCode.KComma => ImGuiKey.Comma,
                KeyCode.KMinus => ImGuiKey.Minus,
                KeyCode.KPeriod => ImGuiKey.Period,
                KeyCode.KSlash => ImGuiKey.Slash,
                KeyCode.KSemicolon => ImGuiKey.Semicolon,
                KeyCode.KEquals => ImGuiKey.Equal,
                KeyCode.KLeftbracket => ImGuiKey.LeftBracket,
                KeyCode.KBackslash => ImGuiKey.Backslash,
                KeyCode.KRightbracket => ImGuiKey.RightBracket,
                KeyCode.KBackquote => ImGuiKey.GraveAccent,
                KeyCode.KCapslock => ImGuiKey.CapsLock,
                KeyCode.KScrolllock => ImGuiKey.ScrollLock,
                KeyCode.KNumlockclear => ImGuiKey.NumLock,
                KeyCode.KPrintscreen => ImGuiKey.PrintScreen,
                KeyCode.KPause => ImGuiKey.Pause,
                KeyCode.KKP0 => ImGuiKey.Keypad0,
                KeyCode.KKP1 => ImGuiKey.Keypad1,
                KeyCode.KKP2 => ImGuiKey.Keypad2,
                KeyCode.KKP3 => ImGuiKey.Keypad3,
                KeyCode.KKP4 => ImGuiKey.Keypad4,
                KeyCode.KKP5 => ImGuiKey.Keypad5,
                KeyCode.KKP6 => ImGuiKey.Keypad6,
                KeyCode.KKP7 => ImGuiKey.Keypad7,
                KeyCode.KKP8 => ImGuiKey.Keypad8,
                KeyCode.KKP9 => ImGuiKey.Keypad9,
                KeyCode.KKPPeriod => ImGuiKey.KeypadDecimal,
                KeyCode.KKPDivide => ImGuiKey.KeypadDivide,
                KeyCode.KKPMultiply => ImGuiKey.KeypadMultiply,
                KeyCode.KKPMinus => ImGuiKey.KeypadSubtract,
                KeyCode.KKPPlus => ImGuiKey.KeypadAdd,
                KeyCode.KKPEnter => ImGuiKey.KeypadEnter,
                KeyCode.KKPEquals => ImGuiKey.KeypadEqual,
                KeyCode.KLctrl => ImGuiKey.LeftCtrl,
                KeyCode.KLshift => ImGuiKey.LeftShift,
                KeyCode.KLalt => ImGuiKey.LeftAlt,
                KeyCode.KLgui => ImGuiKey.LeftSuper,
                KeyCode.KRctrl => ImGuiKey.RightCtrl,
                KeyCode.KRshift => ImGuiKey.RightShift,
                KeyCode.KRalt => ImGuiKey.RightAlt,
                KeyCode.KRgui => ImGuiKey.RightSuper,
                KeyCode.KApplication => ImGuiKey.Menu,
                KeyCode.K0 => ImGuiKey._0,
                KeyCode.K1 => ImGuiKey._1,
                KeyCode.K2 => ImGuiKey._2,
                KeyCode.K3 => ImGuiKey._3,
                KeyCode.K4 => ImGuiKey._4,
                KeyCode.K5 => ImGuiKey._5,
                KeyCode.K6 => ImGuiKey._6,
                KeyCode.K7 => ImGuiKey._7,
                KeyCode.K8 => ImGuiKey._8,
                KeyCode.K9 => ImGuiKey._9,
                KeyCode.KA => ImGuiKey.A,
                KeyCode.KB => ImGuiKey.B,
                KeyCode.KC => ImGuiKey.C,
                KeyCode.KD => ImGuiKey.D,
                KeyCode.KE => ImGuiKey.E,
                KeyCode.KF => ImGuiKey.F,
                KeyCode.KG => ImGuiKey.G,
                KeyCode.KH => ImGuiKey.H,
                KeyCode.KI => ImGuiKey.I,
                KeyCode.KJ => ImGuiKey.J,
                KeyCode.KK => ImGuiKey.K,
                KeyCode.KL => ImGuiKey.L,
                KeyCode.KM => ImGuiKey.M,
                KeyCode.KN => ImGuiKey.N,
                KeyCode.KO => ImGuiKey.O,
                KeyCode.KP => ImGuiKey.P,
                KeyCode.KQ => ImGuiKey.Q,
                KeyCode.KR => ImGuiKey.R,
                KeyCode.KS => ImGuiKey.S,
                KeyCode.KT => ImGuiKey.T,
                KeyCode.KU => ImGuiKey.U,
                KeyCode.KV => ImGuiKey.V,
                KeyCode.KW => ImGuiKey.W,
                KeyCode.KX => ImGuiKey.X,
                KeyCode.KY => ImGuiKey.Y,
                KeyCode.KZ => ImGuiKey.Z,
                KeyCode.KF1 => ImGuiKey.F1,
                KeyCode.KF2 => ImGuiKey.F2,
                KeyCode.KF3 => ImGuiKey.F3,
                KeyCode.KF4 => ImGuiKey.F4,
                KeyCode.KF5 => ImGuiKey.F5,
                KeyCode.KF6 => ImGuiKey.F6,
                KeyCode.KF7 => ImGuiKey.F7,
                KeyCode.KF8 => ImGuiKey.F8,
                KeyCode.KF9 => ImGuiKey.F9,
                KeyCode.KF10 => ImGuiKey.F10,
                KeyCode.KF11 => ImGuiKey.F11,
                KeyCode.KF12 => ImGuiKey.F12,
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