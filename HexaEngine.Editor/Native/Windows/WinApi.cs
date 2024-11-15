namespace Hexa.NET.KittyUI.Native.Windows
{
    using Hexa.NET.Mathematics;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    [SupportedOSPlatform("windows")]
    public static unsafe class WinApi
    {
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetWindowRect(nint hwnd, Rectangle* rect);

        public const uint SWP_FRAMECHANGED = 0x0020;

        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int DwmExtendFrameIntoClientArea(nint hWnd, Margins* margins);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint GetWindowLong(nint hwnd, int nIndex);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint SetWindowLong(nint hwnd, int nIndex, uint dwLong);

        public const int GWL_STYLE = -16;

        public const int GWL_EXSTYLE = -20;
        public const uint WS_CAPTION = 0x00C00000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;
        public const uint WS_MINIMIZEBOX = 0x00020000;

        public const uint WS_EX_LAYERED = 0x00080000;

        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;

        public const int WM_NCCALCSIZE = 0x0083;
        public const int WM_WINDOWPOSCHANGED = 0x0047;

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool IsZoomed(nint hwnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void* SetWindowLongPtr(nint hwnd, int index, void* ptr);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern nint CallWindowProc(void* lpPrevWndFunc, nint hwnd, uint msg, nint wParam, nint lParam);

        public const int GWLP_WNDPROC = -4;

        [DllImport("UxTheme.dll")]
        public static extern int GetThemeSysSize(nint htheme, SystemMetrics iSizeId);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetSystemMetrics(SystemMetrics nIndex);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint GetDpiForWindow(nint hwnd);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate nint WndProc(nint hwnd, uint message, nint wParam, nint lParam);
}