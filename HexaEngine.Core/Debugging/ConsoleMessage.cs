﻿namespace HexaEngine.Core.Debugging
{
    using System;

    public struct ConsoleMessage
    {
        public ConsoleColor ForegroundColor;
        public string Message;
        public string Timestamp;

        public ConsoleMessage(ConsoleColor foregroundColor, string message, string timestamp)
        {
            ForegroundColor = foregroundColor;
            Message = message;
            Timestamp = timestamp;
        }

        public ConsoleMessage(ConsoleColor foregroundColor, string message) : this()
        {
            ForegroundColor = foregroundColor;
            Message = message;
            Timestamp = DateTime.Now.ToShortTimeString();
        }

        public static ConsoleColor GetForegroundFromSeverity(LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Trace => ConsoleColor.DarkGray,
                LogSeverity.Debug => ConsoleColor.DarkCyan,
                LogSeverity.Information => ConsoleColor.White,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Critical => ConsoleColor.Magenta,
                _ => ConsoleColor.White, // Default color
            };
        }

        public static implicit operator ConsoleMessage(LogMessage message)
        {
            return new(GetForegroundFromSeverity(message.Severity), message.Message, message.Timestamp);
        }
    }
}