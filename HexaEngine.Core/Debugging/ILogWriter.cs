namespace HexaEngine.Core.Debugging
{
    using System;

    public interface ILogWriter : IDisposable
    {
        public void Write(string message);

        public Task WriteAsync(string message);

        public void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        public async Task WriteLineAsync(string message)
        {
            await WriteAsync(message + Environment.NewLine);
        }

        public void Log(LogMessage message)
        {
            WriteLine($"{message.Timestamp} [{message.Severity}] {message.Message}");
        }

        public async Task LogAsync(LogMessage message)
        {
            await WriteLineAsync($"{message.Timestamp} [{message.Severity}] {message.Message}");
        }

        public void Flush();

        public void Clear();
    }
}