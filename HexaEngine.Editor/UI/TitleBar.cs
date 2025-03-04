namespace HexaEngine.Editor.UI
{
    using Hexa.NET.ImGui;
    using Hexa.NET.KittyUI.Native.Windows;
    using Hexa.NET.KittyUI.Native.X11;
    using Hexa.NET.Mathematics;
    using Hexa.NET.SDL3;
    using HexaEngine;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.UI;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using MaterialIcons = Hexa.NET.ImGui.Widgets.MaterialIcons;

    public delegate void TitleBarButtonCallback(object? userdata);

    public unsafe class TitleBar : ITitleBar
    {
        public event EventHandler<CloseWindowRequest>? CloseWindowRequest;

        public event EventHandler<MaximizeWindowRequest>? MaximizeWindowRequest;

        public event EventHandler<MinimizeWindowRequest>? MinimizeWindowRequest;

        public event EventHandler<RestoreWindowRequest>? RestoreWindowRequest;

        public CoreWindow Window { get; set; } = null!;

        private ImDrawListPtr draw;
        private Vector2 titleBarPos;
        private Vector2 titleBarSize;
        private int titleBarHeight = 30;
        private Point2 mousePos;
        private Vector2 cursorPos;
        private float buttonSize = 50;

        private uint hoveredId;

        private bool wasActive = false;
        private float timer;
        private float duration = 0;

        private uint colorFg;

        private readonly TitleBarBuilder builder = new();
        private readonly TitleBarContext context = new();

        public TitleBar()
        {
            OnBuild(builder);
        }

        public int Height { get => titleBarHeight; set => titleBarHeight = value; }

        public float ButtonSize { get => buttonSize; set => buttonSize = value; }

        public virtual void OnBuild(TitleBarBuilder builder)
        {
            builder.AddElement(new TitleBarMainMenuBar(MainMenuBar.Draw));

            builder.AddTitle();

            builder.RightAlign();

            builder.AddButton($"{MaterialIcons.Remove}", 0x1CCCCCCC, 0x1CCCCCCC, null, _ => RequestMinimize());
            builder.AddButton($"{MaterialIcons.SelectWindow2}", 0x1CCCCCCC, 0x1CCCCCCC, null, _ => ToggleState());

            builder.AddButton($"{MaterialIcons.Close}", 0xFF3333C6, 0xFF3333C6, null, _ => RequestClose());
        }

        public void ToggleState()
        {
            if (Window.State == WindowState.Maximized)
            {
                RequestRestore();
            }
            else
            {
                RequestMaximize();
            }
        }

        public virtual void Draw()
        {
            var viewport = ImGui.GetMainViewport();
            draw = ImGuiP.GetForegroundDrawList(viewport);

            // Draw the custom title bar
            titleBarPos = viewport.Pos; // Start at the top of the viewport
            titleBarSize = new Vector2(viewport.Size.X, titleBarHeight); // Full width of the viewport
            mousePos = Mouse.Global;
            cursorPos = titleBarPos;

            ImRect rect = new(titleBarPos, titleBarPos + titleBarSize);

            context.NewFrame(rect);
            context.DrawList = draw;
            context.Window = Window;

            if (timer > 0)
            {
                timer -= Time.Delta;
            }
            else
            {
                timer = 0;
            }

            bool windowFocused = Window.Focused;
            if (windowFocused != wasActive)
            {
                timer = 0.1f;
                duration = 0.1f;
            }
            wasActive = windowFocused;

            uint colorBg;

            if (timer > 0)
            {
                float s = timer / duration;
                Vector4 colA = *ImGui.GetStyleColorVec4(ImGuiCol.TitleBgActive);
                Vector4 colB = *ImGui.GetStyleColorVec4(ImGuiCol.TitleBg);
                Vector4 colAF = *ImGui.GetStyleColorVec4(ImGuiCol.Text);
                Vector4 colBF = *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled);
                if (!windowFocused)
                {
                    (colA, colB) = (colB, colA);
                    (colAF, colBF) = (colBF, colAF);
                }
                colorBg = RGBALerp(colA, colB, s);
                colorFg = RGBALerp(colAF, colBF, s);
            }
            else
            {
                colorBg = windowFocused ? ImGui.GetColorU32(ImGuiCol.TitleBgActive) : ImGui.GetColorU32(ImGuiCol.TitleBg);
                colorFg = windowFocused ? ImGui.GetColorU32(ImGuiCol.Text) : ImGui.GetColorU32(ImGuiCol.TextDisabled);
            }

            context.ForegroundColor = colorFg;
            context.BackgroundColor = colorBg;

            builder.Draw(context);

            // Adjust the cursor position to avoid drawing ImGui elements under the custom title bar
            viewport.WorkPos.Y += titleBarHeight;
            viewport.WorkSize.Y -= titleBarHeight;
        }

        private static uint RGBALerp(Vector4 colorA, Vector4 colorB, float s)
        {
            return RGBAV4ToABGRU32(Vector4.Lerp(colorA, colorB, s));
        }

        private static uint RGBAV4ToABGRU32(Vector4 color)
        {
            byte r = (byte)(Math.Clamp(color.X, 0, 1) * byte.MaxValue);
            byte g = (byte)(Math.Clamp(color.Y, 0, 1) * byte.MaxValue);
            byte b = (byte)(Math.Clamp(color.Z, 0, 1) * byte.MaxValue);
            byte a = (byte)(Math.Clamp(color.W, 0, 1) * byte.MaxValue);
            return (uint)(a << 24 | b << 16 | g << 8 | r);
        }

        public void RequestClose()
        {
            CloseWindowRequest request = new(Window);
            OnWindowCloseRequest(request);
            if (!request.Handled)
            {
                CloseWindowRequest?.Invoke(this, request);
            }
        }

        public void RequestMinimize()
        {
            MinimizeWindowRequest request = new(Window);
            OnWindowMinimizeRequest(request);
            if (!request.Handled)
            {
                MinimizeWindowRequest?.Invoke(this, request);
            }
        }

        public void RequestMaximize()
        {
            MaximizeWindowRequest request = new(Window);
            OnWindowMaximizeRequest(request);
            if (!request.Handled)
            {
                MaximizeWindowRequest?.Invoke(this, request);
            }
        }

        public void RequestRestore()
        {
            RestoreWindowRequest request = new(Window);
            OnWindowRestoreRequest(request);
            if (!request.Handled)
            {
                RestoreWindowRequest?.Invoke(this, request);
            }
        }

        protected virtual void OnWindowCloseRequest(CloseWindowRequest args)
        {
        }

        protected virtual void OnWindowMinimizeRequest(MinimizeWindowRequest args)
        {
        }

        protected virtual void OnWindowMaximizeRequest(MaximizeWindowRequest args)
        {
        }

        protected virtual void OnWindowRestoreRequest(RestoreWindowRequest args)
        {
        }

        public virtual SDLHitTestResult HitTest(SDLWindow* win, SDLPoint* area, void* data)
        {
            int w, h;
            SDL.GetWindowSize(win, &w, &h);
            builder.ComputePadding(out var left, out var right);
            if (area->X > left && area->X < w - right)
            {
                return SDLHitTestResult.Draggable;
            }

            return SDLHitTestResult.Normal;
        }

        public virtual void OnAttach(CoreWindow window)
        {
            Window = window;
            if (OperatingSystem.IsWindows())
            {
                InjectInterceptor(window.GetHWND());
            }
            else
            {
                var x11 = window.X11!.Value;
                var atom = X11Api.XInternAtom(x11.Display, "_MOTIF_WM_HINTS", false);
                nint actualType;
                int actualFormat;
                uint nitems;
                int bytesAfter;
                MotifWmHints hints;
                byte* prop;
                var result = (X11ResultCode)X11Api.XGetWindowProperty(x11.Display, x11.Window, atom, 0, sizeof(MotifWmHints) / 4, false, X11Api.AnyPropertyType, &actualType, &actualFormat, &nitems, &bytesAfter, &prop);

                MotifWmHints* pHints = &hints;

                pHints->Decorations = Decor.Border;
                pHints->Flags = MotifWmFlags.Decorations;

                result = (X11ResultCode)X11Api.XChangeProperty(x11.Display, x11.Window, atom, atom, 32, PropMode.Replace, (byte*)pHints, sizeof(MotifWmHints) / 4);
            }
        }

        public virtual void OnDetach(CoreWindow window)
        {
            if (OperatingSystem.IsWindows())
            {
                RemoveInterceptor(window.GetHWND());
            }
            Window = null!;
        }

        #region WIN32

        private void* originalWndProc;
        private WndProc? wndProc;

        [SupportedOSPlatform("windows")]
        private void InjectInterceptor(nint hwnd)
        {
            wndProc = TitleBarWndProc;
            void* injector = (void*)Marshal.GetFunctionPointerForDelegate(wndProc);
            originalWndProc = WinApi.SetWindowLongPtr(hwnd, WinApi.GWLP_WNDPROC, injector);
            WinApi.SetWindowPos(hwnd, 0, 0, 0, 0, 0, WinApi.SWP_FRAMECHANGED | WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE | WinApi.SWP_NOZORDER | WinApi.SWP_NOACTIVATE);
        }

        [SupportedOSPlatform("windows")]
        private void RemoveInterceptor(nint hwnd)
        {
            WinApi.SetWindowLongPtr(hwnd, WinApi.GWLP_WNDPROC, originalWndProc);
        }

        [SupportedOSPlatform("windows")]
        private nint TitleBarWndProc(nint hwnd, uint message, nint wParam, nint lParam)
        {
            if (message == WinApi.WM_NCCALCSIZE && wParam != 0)
            {
                // Cast the pointer to NCCALCSIZE_PARAMS
                NcCalcSizeParams* nccsp = (NcCalcSizeParams*)lParam;

                WindowPos* winPos = nccsp->LpPos;

                // see https://stackoverflow.com/questions/28524463/how-to-get-the-default-caption-bar-height-of-a-window-in-windows/28524464#28524464
                uint dpi = WinApi.GetDpiForWindow(hwnd);
                float dpiScale = dpi / 96.0f;
                int titleBarHeight = (int)MathF.Ceiling((WinApi.GetSystemMetrics(SystemMetrics.CyCaption) + WinApi.GetSystemMetrics(SystemMetrics.CyFrame)) * dpiScale + WinApi.GetSystemMetrics(SystemMetrics.CxPaddedBorder));

                if (WinApi.IsZoomed(hwnd)) // fix for maximized windows.
                {
                    nccsp->RgRc0.Top = Math.Max(nccsp->RgRc0.Top - titleBarHeight, -titleBarHeight - 1);
                }
                else
                {
                    nccsp->RgRc0.Top = nccsp->RgRc0.Top - titleBarHeight;
                }
            }

            return WinApi.CallWindowProc(originalWndProc, hwnd, message, wParam, lParam);
        }

        #endregion WIN32
    }
}