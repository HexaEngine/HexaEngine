namespace HexaEngine.Core.Windows
{
    /// <summary>
    /// Specifies the type of cursor.
    /// </summary>
    public enum CursorType
    {
        /// <summary>
        /// The standard arrow cursor.
        /// </summary>
        Arrow,

        /// <summary>
        /// The I-beam cursor for text selection.
        /// </summary>
        IBeam,

        /// <summary>
        /// The cursor indicating a wait operation.
        /// </summary>
        Wait,

        /// <summary>
        /// The crosshair cursor.
        /// </summary>
        Crosshair,

        /// <summary>
        /// The cursor indicating a wait in arrow form.
        /// </summary>
        WaitArrow,

        /// <summary>
        /// The cursor for resizing in the top-left to bottom-right direction.
        /// </summary>
        SizeNWSE,

        /// <summary>
        /// The cursor for resizing in the top-right to bottom-left direction.
        /// </summary>
        SizeNESW,

        /// <summary>
        /// The cursor for resizing in the horizontal direction.
        /// </summary>
        SizeWE,

        /// <summary>
        /// The cursor for resizing in the vertical direction.
        /// </summary>
        SizeNS,

        /// <summary>
        /// The cursor indicating a resize operation in all directions.
        /// </summary>
        SizeAll,

        /// <summary>
        /// The cursor indicating "no" or "not allowed".
        /// </summary>
        No,

        /// <summary>
        /// The hand cursor, typically used for hyperlink or drag operations.
        /// </summary>
        Hand,
    }
}