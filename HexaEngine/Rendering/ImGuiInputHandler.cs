using HexaEngine.Core;
using HexaEngine.Core.Input;
using HexaEngine.Windows;
using ImGuiNET;
using ImGuizmoNET;
using Silk.NET.SDL;
using System;
using System.Numerics;

namespace HexaEngine.Rendering
{
    internal class ImGuiInputHandler
    {
        private readonly GameWindow window;
        private ImGuiMouseCursor lastCursor;

        public ImGuiInputHandler(GameWindow window)
        {
            this.window = window;
            window.MouseButtonInput += MouseButtonInput;
            window.MouseWheelInput += MouseWheelInput;
            window.KeyboardInput += KeyboardInput;
            window.KeyboardCharInput += KeyboardCharInput;
            InitKeyMap();
        }

        private void KeyboardCharInput(object sender, Core.Input.Events.KeyboardCharEventArgs e)
        {
            var io = ImGui.GetIO();
            io.AddInputCharactersUTF8(e.Char.ToString());
        }

        private void KeyboardInput(object sender, Core.Input.Events.KeyboardEventArgs e)
        {
            var io = ImGui.GetIO();
            io.KeysDown[(int)e.KeyCode] = e.KeyState == KeyState.Pressed;
        }

        private void MouseWheelInput(object sender, Core.Input.Events.MouseWheelEventArgs e)
        {
            var io = ImGui.GetIO();
            io.MouseWheel = e.Y;
            io.MouseWheelH = e.X;
        }

        private void MouseButtonInput(object sender, Core.Input.Events.MouseButtonEventArgs e)
        {
            window.RenderDispatcher.Invoke(() =>
            {
                var io = ImGui.GetIO();

                int index;
                switch (e.MouseButton)
                {
                    case MouseButton.None:
                        index = -1;
                        break;

                    case MouseButton.Left:
                        index = 0;
                        break;

                    case MouseButton.Right:
                        index = 1;
                        break;

                    case MouseButton.Middle:
                        index = 2;
                        break;

                    case MouseButton.X1:
                        index = 3;
                        break;

                    case MouseButton.X2:
                        index = 4;
                        break;

                    default:
                        index = -1;
                        break;
                }

                io.MouseDown[index] = e.KeyState == KeyState.Pressed;

                if (e.KeyState == KeyState.Pressed)
                    window.Capture();
                else
                    window.ReleaseCapture();
            });
        }

        private void InitKeyMap()
        {
            var io = ImGui.GetIO();

            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.Pageup;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.Pagedown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Insert] = (int)Keys.Insert;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Return;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.KeypadEnter] = (int)Keys.Return2;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
        }

        public void Update()
        {
            var io = ImGui.GetIO();
            io.DisplaySize = new(window.Width, window.Height);

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
            io.KeyCtrl = Keyboard.IsDown(Keys.Lctrl);
            io.KeyShift = Keyboard.IsDown(Keys.Lshift);
            io.KeyAlt = Keyboard.IsDown(Keys.Menu);
            io.KeySuper = false;
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
                        io.KeysDown[(int)wParam] = true;
                    return false;

                case WindowMessage.KeyUp:
                case WindowMessage.SysKeyUp:
                    if ((ulong)wParam < 256)
                        io.KeysDown[(int)wParam] = false;
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