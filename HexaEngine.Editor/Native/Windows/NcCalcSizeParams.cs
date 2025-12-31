namespace HexaEngine.Editor.Native.Windows
{
    using Hexa.NET.Mathematics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NcCalcSizeParams
    {
        public Rectangle RgRc0;
        public Rectangle RgRc1;
        public Rectangle RgRc2;
        public WindowPos* LpPos;

        public NcCalcSizeParams(Rectangle rgRc0, Rectangle rgRc1, Rectangle rgRc2, WindowPos* lpPos)
        {
            RgRc0 = rgRc0;
            RgRc1 = rgRc1;
            RgRc2 = rgRc2;
            LpPos = lpPos;
        }
    }

    public struct WindowPos
    {
        public nint Hwnd;
        public nint HwndInsertAfter;
        public int X;
        public int Y;
        public int Cx;
        public int Cy;
        public uint Flags;
    }
}