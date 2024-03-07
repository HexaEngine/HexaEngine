namespace HexaEngine.Core.Debugging
{
    /// <summary>
    /// Represents the severity levels for log messages.
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Trace severity, used for detailed diagnostic information.
        /// </summary>
        Trace = 0,

        /// <summary>
        /// Debug severity, used for debugging information.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Information severity, used for general information.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Warning severity, used for warning messages.
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Error severity, used for error messages.
        /// </summary>
        Error = 8,

        /// <summary>
        /// Critical severity, used for critical error messages.
        /// </summary>
        Critical = 16
    }
}