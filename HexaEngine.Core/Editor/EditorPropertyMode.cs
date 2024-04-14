namespace HexaEngine.Editor.Attributes
{
    /// <summary>
    /// Enumerates the different modes for editor properties.
    /// </summary>
    public enum EditorPropertyMode
    {
        /// <summary>
        /// Default mode for editor properties.
        /// </summary>
        Default,

        /// <summary>
        /// Enum mode for editor properties.
        /// </summary>
        Enum,

        /// <summary>
        /// Color picker mode for editor properties.
        /// </summary>
        Colorpicker,

        /// <summary>
        /// Slider mode for editor properties.
        /// </summary>
        Slider,

        /// <summary>
        /// Angle slider mode for editor properties. (degrees)
        /// </summary>
        SliderAngle,

        /// <summary>
        /// Type selector mode for editor properties.
        /// </summary>
        TypeSelector,

        /// <summary>
        /// File picker mode for editor properties.
        /// </summary>
        [Obsolete("Use AssetRefs instead.")]
        Filepicker,

        /// <summary>
        /// Reference selector mode for editor properties.
        /// </summary>
        ReferenceSelector,
    }
}