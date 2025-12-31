using System.Runtime.InteropServices;

namespace HexaEngine.Editor.Native.X11
{
    public static unsafe class X11Api
    {
        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern int XChangeProperty(nint display, nuint window, nint property, nint type, int format, PropMode mode, byte* data, int nelements);

        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern int XGetWindowProperty(nint display, nuint window, nint property, int offset, int length, bool delete, nint reqType, nint* actualType, int* actualFormatReturn, uint* nItemsReturn, int* bytesAfterReturn, byte** propReturn);

        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern nint XInternAtom(nint display, string atomName, bool onyIfExists);

        [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern string XGetAtomName(nint display, nint actualType);

        public const int AnyPropertyType = 0;
    }

    [Flags]
    public enum MotifWmFlags : uint
    {
        Functions = 1 << 0,  // Valid functions field
        Decorations = 1 << 1,  // Valid decorations field
        InputMode = 1 << 2,  // Valid input mode field
        Status = 1 << 3   // Valid status field
    }


    [Flags]
    public enum MotifWmFunctions : uint
    {
        All = 1 << 0,  // All functions
        Resize = 1 << 1,  // Allow window resizing
        Move = 1 << 2,  // Allow window moving
        Minimize = 1 << 3,  // Allow window minimizing
        Maximize = 1 << 4,  // Allow window maximizing
        Close = 1 << 5   // Allow window closing
    }
    public enum MotifWmInputMode : int
    {
        Modeless = 0,   // Modeless input mode
        ApplicationModal = 1,  // Application modal input mode
        SystemModal = 2,   // System modal input mode
        FullApplicationModal = 3   // Full application modal input mode
    }

    [Flags]
    public enum MotifWmStatus : uint
    {
        TearOffWindow = 1 << 0  // Window is a tear-off window
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MotifWmHints
    {
        public MotifWmFlags Flags;
        public MotifWmFunctions Functions;
        public Decor Decorations;
        public MotifWmInputMode InputMode;
        public MotifWmStatus Status;
    }

    [Flags]
    public enum Decor : uint
    {
        None = 0,
        All = 0x01, // All decorations
        Border = 0x02, // Border decoration
        ResizeH = 0x04, // Resize handle decoration
        Title = 0x08, // Title decoration
        Menu = 0x10, // Menu decoration
        Minimize = 0x20, // Minimize button decoration
        Maximize = 0x40,  // Maximize button decoration

    }


    public enum PropMode : int
    {
        Replace = 0,
        Prepend = 1,
        Append = 2,
    }

    public enum X11ResultCode
    {
        Success = 0,
        BadRequest = 1,
        BadValue = 2,
        BadWindow = 3,
        BadPixmap = 4,
        BadAtom = 5,
        BadCursor = 6,
        BadFont = 7,
        BadMatch = 8,
        BadDrawable = 9,
        BadAccess = 10,
        BadAlloc = 11,
        BadColor = 12,
        BadGC = 13,
        BadIDChoice = 14,
        BadName = 15,
        BadLength = 16,
        BadImplementation = 17,
        FirstExtensionError = 128,
        LastExtensionError = 255
    }

    public enum X11PropertyType : uint
    {
        None = 0,     // No property type
        String = 31,    // XA_STRING
        Atom = 4,     // XA_ATOM
        Cardinal = 6,     // XA_CARDINAL
        Integer = 19,    // XA_INTEGER (not always used, often integers are stored as CARDINAL)
        Window = 33,    // XA_WINDOW
        Pixmap = 20,    // XA_PIXMAP
        Cursor = 21,    // XA_CURSOR
        Font = 22,    // XA_FONT
        Drawable = 23,    // XA_DRAWABLE
        Colormap = 28,    // XA_COLORMAP
        Rectangle = 29,    // XA_RECTANGLE
        Point = 30,    // XA_POINT
        VisualID = 32,    // XA_VISUALID
        WmHints = 34,    // XA_WM_HINTS (used for window manager hints)
        WmName = 39,    // XA_WM_NAME
        WmIconName = 40,    // XA_WM_ICON_NAME
        WmClass = 67,    // XA_WM_CLASS
        WmCommand = 78,    // XA_WM_COMMAND
        WmClientMachine = 79,    // XA_WM_CLIENT_MACHINE
        WmSizeHints = 77,    // XA_WM_SIZE_HINTS
        WmNormalHints = 72,    // XA_WM_NORMAL_HINTS
        WmState = 73,    // XA_WM_STATE
        WmTransientFor = 68,    // XA_WM_TRANSIENT_FOR
        WmProtocols = 81     // XA_WM_PROTOCOLS
    }

}