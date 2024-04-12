namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents a message to be displayed in a terminal with a specified text, text color, and timestamp.
    /// </summary>
    public struct TerminalMessage : IEquatable<TerminalMessage>
    {
        /// <summary>
        /// Gets or sets the text content of the message.
        /// </summary>
        public string Message;

        /// <summary>
        /// Gets or sets the color of the text for the message.
        /// </summary>
        public TerminalColor Color;

        /// <summary>
        /// Gets or sets the timestamp indicating when the message was created.
        /// </summary>
        public long Timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalMessage"/> struct with the specified message and color.
        /// </summary>
        /// <param name="message">The text content of the message.</param>
        /// <param name="color">The color of the text for the message.</param>
        public TerminalMessage(string message, TerminalColor color)
        {
            Message = message;
            Color = color;
            Timestamp = Stopwatch.GetTimestamp();
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TerminalMessage message && Equals(message);
        }

        public readonly bool Equals(TerminalMessage other)
        {
            return Message == other.Message &&
                   Color == other.Color &&
                   Timestamp == other.Timestamp;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Message, Color, Timestamp);
        }

        public static bool operator ==(TerminalMessage left, TerminalMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TerminalMessage left, TerminalMessage right)
        {
            return !(left == right);
        }
    }
}