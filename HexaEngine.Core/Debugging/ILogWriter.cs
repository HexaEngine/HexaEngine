namespace HexaEngine.Core.Debugging
{
    using System;

    /// <summary>
    /// Defines the contract for a log writer responsible for writing log messages.
    /// </summary>
    public interface ILogWriter : IDisposable
    {
        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        public void Write(string message);

        /// <summary>
        /// Writes a log message asynchronously.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task WriteAsync(string message);

        /// <summary>
        /// Writes a log message followed by a newline character.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        public void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        /// <summary>
        /// Writes a log message followed by a newline character asynchronously.
        /// </summary>
        /// <param name="message">The log message to write.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task WriteLineAsync(string message)
        {
            await WriteAsync(message + Environment.NewLine);
        }

        /// <summary>
        /// Logs a formatted log message.
        /// </summary>
        /// <param name="message">The log message to log.</param>
        public void Log(LogMessage message)
        {
            WriteLine($"{message.Timestamp} [{message.Severity}] {message.Message}");
        }

        /// <summary>
        /// Logs a formatted log message asynchronously.
        /// </summary>
        /// <param name="message">The log message to log.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LogAsync(LogMessage message)
        {
            await WriteLineAsync($"{message.Timestamp} [{message.Severity}] {message.Message}");
        }

        /// <summary>
        /// Flushes any buffered log data.
        /// </summary>
        public void Flush();

        /// <summary>
        /// Clears the log, removing all log entries.
        /// </summary>
        public void Clear();
    }
}