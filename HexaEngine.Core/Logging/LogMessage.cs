namespace HexaEngine.Core.Logging
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a log message with severity, message content, and timestamp.
    /// </summary>
    public struct LogMessage : IEquatable<LogMessage>
    {
        /// <summary>
        /// Gets or sets the logger of the log message.
        /// </summary>
        public ILogger Logger;

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
        public DateTime Timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> struct with a specified severity, source, and message.
        /// </summary>
        /// <param name="logger">The logger of the log message.</param>
        /// <param name="severity">The severity of the log message.</param>
        /// <param name="source">The source of the log message.</param>
        /// <param name="message">The main content of the log message.</param>
        public LogMessage(ILogger logger, LogSeverity severity, string source, string message) : this()
        {
            Logger = logger;
            Severity = severity;
            Message = $"[{source}]: {message}";
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> struct with a specified severity and message.
        /// </summary>
        /// <param name="logger">The logger of the log message.</param>
        /// <param name="severity">The severity of the log message.</param>
        /// <param name="message">The main content of the log message.</param>
        public LogMessage(ILogger logger, LogSeverity severity, string message) : this()
        {
            Logger = logger;
            Severity = severity;
            Message = message;
            Timestamp = DateTime.Now;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is LogMessage message && Equals(message);
        }

        public readonly bool Equals(LogMessage other)
        {
            return EqualityComparer<ILogger>.Default.Equals(Logger, other.Logger) &&
                   Severity == other.Severity &&
                   Message == other.Message &&
                   Timestamp == other.Timestamp;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Logger, Severity, Message, Timestamp);
        }

        public static bool operator ==(LogMessage left, LogMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LogMessage left, LogMessage right)
        {
            return !(left == right);
        }
    }
}