namespace HexaEngine.Core.Debugging
{
    using System;

    public struct LogMessage
    {
        public LogSeverity Severity;
        public string Message;
        public DateTime Timestamp;

        public LogMessage(LogSeverity severity, string source, string message) : this()
        {
            Severity = severity;
            Message = $"[{source}]: {message}";
            Timestamp = DateTime.Now;
        }
    }
}