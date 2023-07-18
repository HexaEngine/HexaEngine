namespace HexaEngine.Core.Debugging
{
    using System;

    public struct ConsoleMessage
    {
        public ConsoleColor Severity;
        public string Message;
        public string Timestamp;

        public ConsoleMessage(ConsoleColor severity, string message, string timestamp)
        {
            Severity = severity;
            Message = message;
            Timestamp = timestamp;
        }

        public ConsoleMessage(ConsoleColor severity, string message) : this()
        {
            Severity = severity;
            Message = message;
            Timestamp = DateTime.Now.ToShortTimeString();
        }

        public static implicit operator ConsoleMessage(LogMessage message)
        {
            return new((ConsoleColor)(int)message.Severity, message.Message, message.Timestamp);
        }
    }
}