namespace HexaEngine.Core.Debugging
{
    using System;

    /// <summary>
    /// Represents a log message with severity, message content, and timestamp.
    /// </summary>
    public struct LogMessage
    {
        /// <summary>
        /// Gets or sets the severity of the log message.
        /// </summary>
        public LogSeverity Severity;

        /// <summary>
        /// Gets or sets the main content of the log message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Gets or sets the timestamp of the log message.
        /// </summary>
        public string Timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> struct with a specified severity, source, and message.
        /// </summary>
        /// <param name="severity">The severity of the log message.</param>
        /// <param name="source">The source of the log message.</param>
        /// <param name="message">The main content of the log message.</param>
        public LogMessage(LogSeverity severity, string source, string message) : this()
        {
            Severity = severity;
            Message = $"[{source}]: {message}";
            Timestamp = DateTime.Now.ToShortTimeString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> struct with a specified severity and message.
        /// </summary>
        /// <param name="severity">The severity of the log message.</param>
        /// <param name="message">The main content of the log message.</param>
        public LogMessage(LogSeverity severity, string message) : this()
        {
            Severity = severity;
            Message = message;
            Timestamp = DateTime.Now.ToShortTimeString();
        }
    }
}