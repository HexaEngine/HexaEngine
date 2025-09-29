namespace HexaEngine.Editor.UI
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.KittyUI.Native.Windows;
    using Hexa.NET.KittyUI.Native.X11;
    using Hexa.NET.SDL3;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.UI;
    using HexaEngine.Editor.Extensions;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;

    public delegate void TitleBarButtonCallback(object? userdata);

    public class TitleBarContext
    {
        private Vector2 cursor;
        public float DpiScale;

        public CoreWindow Window = null!;
        public Vector2 Cursor;
        public uint HoveredId;
        public uint ForegroundColor;
        public uint BackgroundColor;
        public ImRect Area;
        public ImDrawListPtr DrawList;

        public Vector2 Offset { get; internal set; }

        public void AddItem(Vector2 size)
        {
            cursor.X += size.X * DpiScale; 
        }

        public void NewFrame(ImRect area)
        {
            Area = area;
            cursor = area.Min + Offset;
        }
    }

    public interface ITitleBarElement
    {
        public void Draw(TitleBarContext context);

        public Vector2 Size { get; }

        public string Label { get; }

        bool IsVisible { get; }
    }

    public abstract class TitleBarElement : ITitleBarElement
    {
        protected static uint ABGRLerp(uint colorA, uint colorB, float s)
        {
            var colA = ABGRU32ToRGBAV4(colorA);
            var colB = ABGRU32ToRGBAV4(colorB);
            var lerp = Vector4.Lerp(colA, colB, s);
            return RGBAV4ToABGRU32(lerp);
        }

        protected static uint RGBALerp(Vector4 colorA, Vector4 colorB, float s)
        {
            return RGBAV4ToABGRU32(Vector4.Lerp(colorA, colorB, s));
        }

        protected static Vector4 ABGRU32ToRGBAV4(uint color)
        {
            byte a = (byte)(color >> 24 & 0xFF);
            byte b = (byte)(color >> 16 & 0xFF);
            byte g = (byte)(color >> 8 & 0xFF);
            byte r = (byte)(color & 0xFF);
            return new Vector4(r / (float)byte.MaxValue, g / (float)byte.MaxValue, b / (float)byte.MaxValue, a / (float)byte.MaxValue);
        }

        protected static uint RGBAV4ToABGRU32(Vector4 color)
        {
            byte r = (byte)(Math.Clamp(color.X, 0, 1) * byte.MaxValue);
            byte g = (byte)(Math.Clamp(color.Y, 0, 1) * byte.MaxValue);
            byte b = (byte)(Math.Clamp(color.Z, 0, 1) * byte.MaxValue);
            byte a = (byte)(Math.Clamp(color.W, 0, 1) * byte.MaxValue);
            return (uint)(a << 24 | b << 16 | g << 8 | r);
        }

        public abstract Vector2 Size { get; }

        public abstract string Label { get; }

        public abstract bool IsVisible { get; }

        public abstract void Draw(TitleBarContext context);
    }

    public unsafe class TitleBarTitle : TitleBarElement
    {
        public override Vector2 Size { get; } // null size title should not take space in layout atleast for now.

        public override string Label { get; } = "Title";

        public override bool IsVisible { get; } = true;

        public override void Draw(TitleBarContext context)
        {
            var area = context.Area;
            var size = area.Size();
            // Draw the title text centered in the title bar
            string title = context.Window.Title;
            var textSize = ImGui.CalcTextSize(title);
            var textPos = new Vector2(
                area.Min.X + (size.X - textSize.X) * 0.5f,
                area.Min.Y + (size.Y - textSize.Y) * 0.5f
            );
            context.DrawList.AddText(textPos, context.ForegroundColor, title);
        }
    }

    public unsafe class TitleBarButton : TitleBarElement
    {
        public string label;
        public uint HoveredColor;
        public uint ActiveColor;
        public Vector2 size;
        public object? Userdata;
        public TitleBarButtonCallback Callback;
        public Func<object?, bool>? isVisible;

        public TitleBarButton(string label, uint hoveredColor, uint activeColor, Vector2 size, object? userdata, TitleBarButtonCallback callback)
        {
            this.label = label;
            HoveredColor = hoveredColor;
            ActiveColor = activeColor;
            this.size = size;
            Userdata = userdata;
            Callback = callback;
        }

        public override string Label => label;

        public override Vector2 Size => size;

        public override bool IsVisible => isVisible?.Invoke(Userdata) ?? true;

        public override void Draw(TitleBarContext context)
        {
            if (Button(context, label, HoveredColor, ActiveColor, size))
            {
                Callback(Userdata);
            }
        }

        private unsafe bool Button(TitleBarContext context, string label, uint hoveredColor, uint activeColor, Vector2 size)
        {
            int byteCount = Encoding.UTF8.GetByteCount(label);
            byte* pLabel;
            if (byteCount > StackAllocLimit)
            {
                pLabel = (byte*)Alloc(byteCount + 1);
            }
            else
            {
                byte* stackLabel = stackalloc byte[byteCount + 1];
                pLabel = stackLabel;
            }
            int offset = Encoding.UTF8.GetBytes(label, new Span<byte>(pLabel, byteCount));
            pLabel[offset] = 0;

            bool result = Button(context, pLabel, hoveredColor, activeColor, size);

            if (byteCount > StackAllocLimit)
            {
                Free(pLabel);
            }

            return result;
        }

        private bool Button(TitleBarContext context, ReadOnlySpan<byte> label, uint hoveredColor, uint activeColor, Vector2 size)
        {
            fixed (byte* pLabel = label)
            {
                return Button(context, pLabel, hoveredColor, activeColor, size);
            }
        }

        private unsafe bool Button(TitleBarContext context, byte* label, uint hoveredColor, uint activeColor, Vector2 size)
        {
            var id = ImGui.GetID(label);
            var mousePos = Mouse.Global;
            // Draw a custom close button on the right side of the title bar
            var pos = context.Cursor;

            var transitionState = ImGui.GetStateStorage().GetFloatRef(id, 0);

            size *= context.DpiScale;
            context.Cursor += new Vector2(size.X, 0);

            ImRect rect = new(pos, pos + size);


            bool isHovered = rect.Contains(mousePos);
            bool isMouseDown = ImGuiP.IsMouseDown(ImGuiMouseButton.Left) && isHovered;

            if (!isHovered && context.HoveredId == id)
            {
                context.HoveredId = 0;
                *transitionState = 0.1f;
            }

            if (isHovered && context.HoveredId != id)
            {
                context.HoveredId = id;
                *transitionState = 0.1f;
            }

            if (*transitionState > 0)
            {
                *transitionState = *transitionState - Time.Delta;
            }
            if (*transitionState <= 0)
            {
                *transitionState = 0;
            }

            uint color;
            if (*transitionState != 0)
            {
                float s = *transitionState / 0.1f;
                uint colA = hoveredColor;
                uint colB = 0xFF;
                if (!isHovered)
                {
                    s = 1 - s;
                }
                color = ABGRLerp(colA, colB, s);
            }
            else
            {
                color = isMouseDown ? activeColor : isHovered ? hoveredColor : 0;
            }

            if (color != 0)
            {
                context.DrawList.AddRectFilled(rect.Min, rect.Max, color);
            }

            bool clicked = ImGuiP.IsMouseReleased(ImGuiMouseButton.Left);
            var textSizeClose = ImGui.CalcTextSize(label);
            var midpoint = rect.Midpoint() - textSizeClose / 2;
            context.DrawList.AddText(midpoint, context.ForegroundColor, label);

            if (isHovered && clicked)
            {
                return true;
            }

            return false;
        }
    }

    public class TitleBarBuilder
    {
        private readonly List<ITitleBarElement> elements = new();
        private int rightAlignAfter;
        private float buttonSize = 50;
        private int titleBarHeight = 30;

        public int Height { get => titleBarHeight; set => titleBarHeight = value; }

        public float ButtonSize { get => buttonSize; set => buttonSize = value; }

        public int RightAlignAfter { get => rightAlignAfter; set => rightAlignAfter = value; }

        public List<ITitleBarElement> Elements => elements;

        public TitleBarBuilder AddButton(string label, uint hoveredColor, uint activeColor, object? userdata, TitleBarButtonCallback callback, Func<object?, bool>? isVisible = null)
        {
            elements.Add(new TitleBarButton(label, hoveredColor, activeColor, new Vector2(buttonSize, titleBarHeight), userdata, callback) { isVisible = isVisible });
            return this;
        }

        public TitleBarBuilder InsertButton(string before, string label, uint hoveredColor, uint activeColor, object? userdata, TitleBarButtonCallback callback)
        {
            TitleBarButton button = new(label, hoveredColor, activeColor, new Vector2(buttonSize, titleBarHeight), userdata, callback);
            int idx = elements.FindIndex(x => x.Label == before);
            if (idx == -1)
            {
                idx = elements.Count;
            }

            elements.Insert(idx, button);
            return this;
        }

        public TitleBarBuilder AddTitle()
        {
            elements.Add(new TitleBarTitle());
            return this;
        }

        public TitleBarBuilder AddElement(ITitleBarElement element)
        {
            elements.Add(element);
            return this;
        }

        public TitleBarBuilder InsertElement(string before, ITitleBarElement element)
        {
            int idx = elements.FindIndex(x => x.Label == before);
            if (idx == -1)
            {
                idx = elements.Count;
            }
            elements.Insert(idx, element);
            return this;
        }

        public TitleBarBuilder RightAlign()
        {
            rightAlignAfter = elements.Count - 1;
            return this;
        }

        public TitleBarBuilder RightAlign(string before)
        {
            int idx = elements.FindIndex(x => x.Label == before);
            if (idx == -1)
            {
                idx = elements.Count;
            }
            rightAlignAfter = idx;
            return this;
        }

        public void Draw(TitleBarContext context)
        {
            context.DrawList.AddRectFilled(context.Area.Min, context.Area.Max, context.BackgroundColor);

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                if (element.IsVisible)
                {
                    element.Draw(context);
                }

                if (i == rightAlignAfter)
                {
                    AlignContext(context);
                }
            }
        }

        public void AlignContext(TitleBarContext context)
        {
            float x = 0;
            for (int i = rightAlignAfter + 1; i < elements.Count; i++)
            {
                if (elements[i].IsVisible)
                {
                    x += elements[i].Size.X * context.DpiScale;
                }
            }
            context.Cursor = new(context.Area.Max.X - x, context.Area.Min.Y);
        }

        public float ComputeLeftContentSize(TitleBarContext context)
        {
            float x = 0;
            for (int i = 0; i < rightAlignAfter; i++)
            {
                if (elements[i].IsVisible)
                {
                    x += elements[i].Size.X * context.DpiScale;
                }
            }
            return x;
        }

        public float ComputeRightContentSize(TitleBarContext context)
        {
            float x = 0;
            for (int i = rightAlignAfter + 1; i < elements.Count; i++)
            {
                if (elements[i].IsVisible)
                {
                    x += elements[i].Size.X * context.DpiScale;
                }
            }
            return x;
        }

        public void ComputePadding(TitleBarContext context, out float left, out float right)
        {
            left = ComputeLeftContentSize(context);
            right = ComputeRightContentSize(context);
        }
    }

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
        private Vector2 mousePos;
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

        public int NativeHeight { get; }

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

            var scale = viewport.DpiScale;

            // Draw the custom title bar
            titleBarPos = viewport.Pos; // Start at the top of the viewport
            titleBarSize = new Vector2(viewport.Size.X, titleBarHeight * scale); // Full width of the viewport
            mousePos = Mouse.Global;
            cursorPos = titleBarPos;

            ImRect rect = new(titleBarPos, titleBarPos + titleBarSize);

            context.NewFrame(rect);
            context.DpiScale = scale;
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
            viewport.WorkPos.Y += titleBarHeight * scale;
            viewport.WorkSize.Y -= titleBarHeight * scale;
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
            builder.ComputePadding(context, out var left, out var right);
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
                int titleBarHeight = (int)MathF.Ceiling(WinApi.GetSystemMetrics(SystemMetrics.CyCaption) + WinApi.GetSystemMetrics(SystemMetrics.CyFrame) //* dpiScale 
                    + WinApi.GetSystemMetrics(SystemMetrics.CxPaddedBorder));

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