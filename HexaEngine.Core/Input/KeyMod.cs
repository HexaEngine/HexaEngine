namespace HexaEngine.Core.Input
{
    /// <summary>
    /// Enumeration for key modifiers.
    /// </summary>
    [Flags]
    public enum KeyMod
    {
        /// <summary>
        /// No modifier.
        /// </summary>
        None = 0,

        /// <summary>
        /// Left Shift key modifier.
        /// </summary>
        LShift = 1,

        /// <summary>
        /// Right Shift key modifier.
        /// </summary>
        RShift = 2,

        /// <summary>
        /// Left Control key modifier.
        /// </summary>
        LCtrl = 0x40,

        /// <summary>
        /// Right Control key modifier.
        /// </summary>
        RCtrl = 0x80,

        /// <summary>
        /// Left Alt key modifier.
        /// </summary>
        LAlt = 0x100,

        /// <summary>
        /// Right Alt key modifier.
        /// </summary>
        RAlt = 0x200,

        /// <summary>
        /// Left GUI (Windows key) modifier.
        /// </summary>
        LGui = 0x400,

        /// <summary>
        /// Right GUI (Windows key) modifier.
        /// </summary>
        RGui = 0x800,

        /// <summary>
        /// Numeric keypad modifier.
        /// </summary>
        Num = 0x1000,

        /// <summary>
        /// Caps Lock modifier.
        /// </summary>
        Caps = 0x2000,

        /// <summary>
        /// Mode modifier.
        /// </summary>
        Mode = 0x4000,

        /// <summary>
        /// Scroll Lock modifier.
        /// </summary>
        Scroll = 0x8000,

        /// <summary>
        /// Control key modifier (combining Left and Right Control keys).
        /// </summary>
        Ctrl = 0xC0,

        /// <summary>
        /// Shift key modifier (combining Left and Right Shift keys).
        /// </summary>
        Shift = 3,

        /// <summary>
        /// Alt key modifier (combining Left and Right Alt keys).
        /// </summary>
        Alt = 0x300,

        /// <summary>
        /// GUI (Windows key) modifier (combining Left and Right GUI keys).
        /// </summary>
        Gui = 0xC00,

        /// <summary>
        /// Reserved modifier.
        /// </summary>
        Reserved = 0x8000
    }
}