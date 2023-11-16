namespace HexaEngine.Editor.Attributes
{
    /// <summary>
    /// Specifies modes for editor property conditions, determining their effect on property visibility, enabling, or read-only state.
    /// </summary>
    public enum EditorPropertyConditionMode
    {
        /// <summary>
        /// The property condition affects visibility.
        /// </summary>
        Visible,

        /// <summary>
        /// The property condition affects enabling.
        /// </summary>
        Enable,

        /// <summary>
        /// The property condition affects read-only state.
        /// </summary>
        ReadOnly,
    }
}