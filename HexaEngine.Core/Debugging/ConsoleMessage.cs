namespace HexaEngine.Core.Debugging
{
    using System;

    /// <summary>
    /// Represents a console message with formatting and timestamp information.
    /// </summary>
    public struct ConsoleMessage : IEquatable<ConsoleMessage>
    {
        /// <summary>
        /// Gets or sets the foreground color of the console message.
        /// </summary>
        public ConsoleColor ForegroundColor;

        /// <summary>
        /// Gets or sets the background color of the console message.
        /// </summary>
        public ConsoleColor BackgroundColor;

        /// <summary>
        /// Gets or sets the content of the console message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Gets or sets the timestamp associated with the console message.
        /// </summary>
        public string Timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleMessage"/> struct with specified foreground and background colors, message, and timestamp.
        /// </summary>
        /// <param name="foregroundColor">The foreground color of the message.</param>
        /// <param name="backgroundColor">The background color of the message.</param>
        /// <param name="message">The content of the message.</param>
        /// <param name="timestamp">The timestamp associated with the message.</param>
        public ConsoleMessage(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message, string timestamp)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Message = message;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleMessage"/> struct with specified foreground and background colors and message, using the current timestamp.
        /// </summary>
        /// <param name="foregroundColor">The foreground color of the message.</param>
        /// <param name="backgroundColor">The background color of the message.</param>
        /// <param name="message">The content of the message.</param>
        public ConsoleMessage(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message) : this()
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Message = message;
            Timestamp = DateTime.Now.ToShortTimeString();
        }

        /// <summary>
        /// Gets the foreground color for a given log message severity level.
        /// </summary>
        /// <param name="severity">The severity level of the log message.</param>
        /// <returns>The corresponding foreground color for the specified severity.</returns>
        public static ConsoleColor GetForegroundFromSeverity(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Trace => ConsoleColor.DarkGray,
                LogSeverity.Debug => ConsoleColor.DarkCyan,
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Critical => ConsoleColor.Magenta,
                _ => ConsoleColor.White, // Default color
            };
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ConsoleMessage message && Equals(message);
        }

        public readonly bool Equals(ConsoleMessage other)
        {
            return ForegroundColor == other.ForegroundColor &&
                   BackgroundColor == other.BackgroundColor &&
                   Message == other.Message &&
                   Timestamp == other.Timestamp;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(ForegroundColor, BackgroundColor, Message, Timestamp);
        }

        /// <summary>
        /// Implicitly converts a <see cref="LogMessage"/> to a <see cref="ConsoleMessage"/> with a foreground color based on the log message's severity.
        /// </summary>
        /// <param name="message">The log message to convert.</param>
        public static implicit operator ConsoleMessage(LogMessage message)
        {
            return new(GetForegroundFromSeverity(message.Severity), ImGuiConsole.BackgroundColor, message.Message, message.Timestamp.ToShortTimeString());
        }

        public static bool operator ==(ConsoleMessage left, ConsoleMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConsoleMessage left, ConsoleMessage right)
        {
            return !(left == right);
        }
    }
}