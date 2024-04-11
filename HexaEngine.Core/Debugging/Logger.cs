namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Provides logging capabilities with various log writers and log levels.
    /// </summary>
    public class Logger : ILogger
    {
        private readonly SemaphoreSlim semaphore = new(1);
        private LogSeverity logLevel;

        public Logger(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a list of log writers used for writing log messages.
        /// </summary>
        public List<ILogWriter> Writers { get; } = new();

        /// <summary>
        /// Gets or sets the current log level.
        /// </summary>
        public LogSeverity LogLevel { get => logLevel; set => logLevel = value; }

        /// <summary>
        /// Flushes all log writers to ensure that pending log messages are written.
        /// </summary>
        public void Flush()
        {
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Flush();
            }
            semaphore.Release();
        }

        /// <summary>
        /// Clears log messages from all log writers.
        /// </summary>
        public void Clear()
        {
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Clear();
            }
            semaphore.Release();
        }

        /// <summary>
        /// Closes and disposes of all log writers.
        /// </summary>
        public void Close()
        {
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Dispose();
            }
            semaphore.Release();
        }

        /// <summary>
        /// Handles errors that occur in asynchronous tasks by logging them.
        /// </summary>
        /// <param name="task">The task to handle.</param>
        /// <returns>A completed task.</returns>
        public Task HandleError(Task task)
        {
            if (!task.IsCompletedSuccessfully && task.Exception != null)
            {
                Log(task.Exception);
            }
            task.Dispose();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Logs a log message.
        /// </summary>
        /// <param name="message">The log message to be logged.</param>
        public void Log(LogMessage message)
        {
            if (message.Severity < logLevel)
                return;
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Log(message);
            }
            foreach (ILogWriter writer in LoggerFactory.GetGlobalWriters())
            {
                writer.Log(message);
            }
            semaphore.Release();
        }

        /// <summary>
        /// Asynchronously logs a log message.
        /// </summary>
        /// <param name="message">The log message to be logged asynchronously.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogAsync(LogMessage message)
        {
            if (message.Severity < logLevel)
                return;
            await semaphore.WaitAsync();
            for (int i = 0; i < Writers.Count; i++)
            {
                await Writers[i].LogAsync(message);
            }
            foreach (ILogWriter writer in LoggerFactory.GetGlobalWriters())
            {
                await writer.LogAsync(message);
            }
            semaphore.Release();
        }

        /// <summary>
        /// Logs a log message with a specific severity and message.
        /// </summary>
        /// <param name="severity">The severity level of the log message.</param>
        /// <param name="message">The message to be logged.</param>
        public void Log(LogSeverity severity, string? message)
        {
            Log(new LogMessage(this, severity, message ?? "null"));
        }

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="e">The exception to be logged.</param>
        public void Log(Exception? e)
        {
            var message = e?.ToString() ?? "null";

            Log(new LogMessage(this, LogSeverity.Error, message));
        }

        /// <summary>
        /// A helper method that evaluates the log severity based on a log message.
        /// </summary>
        /// <param name="message">The log message to evaluate.</param>
        /// <returns>The determined log severity.</returns>
        private LogSeverity Evaluate(string message)
        {
            LogSeverity type = LogSeverity.Info;
            if (message.Contains("critical", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Critical;
            }
            else if (message.Contains("error", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Error;
            }
            else if (message.Contains("warn", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Warning;
            }
            else if (message.Contains("warning", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Warning;
            }
            else if (message.Contains("info", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Info;
            }
            else if (message.Contains("information", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Info;
            }
            else if (message.Contains("debug", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Debug;
            }
            else if (message.Contains("dbg", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Debug;
            }
            else if (message.Contains("trace", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Trace;
            }

            return type;
        }

        /// <summary>
        /// Logs an object's string representation with an automatically determined log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void Log(object? value)
        {
            var message = value?.ToString() ?? "null";
            LogSeverity type = Evaluate(message);
            Log(new LogMessage(this, type, message));
        }

        /// <summary>
        /// Logs a message with an automatically determined log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Log(string? message)
        {
            var msg = message ?? "null";
            LogSeverity type = Evaluate(msg);
            Log(new LogMessage(this, type, msg));
        }

        /// <summary>
        /// Logs a failed assertion with an error log severity.
        /// </summary>
        /// <param name="message">The assertion message.</param>
        /// <param name="detailedMessage">The detailed message.</param>
        public void Fail(string? message, string? detailedMessage)
        {
            StringBuilder sb = new();
            sb.AppendLine("//// Assert Fail");
            sb.AppendLine();
            sb.AppendLine("Message:");
            sb.AppendLine(message);
            sb.AppendLine();
            sb.AppendLine("Stack:");
            sb.AppendLine(new StackTrace().ToString());
            sb.AppendLine();
            sb.AppendLine("Detailed Message:");
            sb.AppendLine(detailedMessage);
            sb.AppendLine();
            sb.AppendLine("//// Assert Fail End");

            var msg = sb.ToString();

            Log(new LogMessage(this, LogSeverity.Error, msg));
        }

        /// <summary>
        /// Logs a failed assertion with an error log severity, with no detailed message.
        /// </summary>
        /// <param name="message">The assertion message.</param>
        public void Fail(string? message) => Fail(message, string.Empty);

        /// <summary>
        /// Asserts a condition and logs a failure if it is false.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">The assertion message.</param>
        /// <param name="detailedMessage">The detailed message.</param>
        public void Assert(bool condition, string? message, string? detailedMessage)
        {
            if (!condition)
            {
                Fail(message, detailedMessage);
            }
        }

        /// <summary>
        /// Asserts a condition and logs a failure if it is false, with no detailed message.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">The assertion message.</param>
        public void Assert(bool condition, string? message) => Assert(condition, message, string.Empty);

        /// <summary>
        /// Asserts a condition and logs a failure if it is false, with no assertion message or detailed message.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        public void Assert(bool condition) => Assert(condition, string.Empty, string.Empty);

        /// <summary>
        /// Logs a message with the specified log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="severity">The log severity for the message.</param>
        /// <param name="message">The message to log.</param>
        public void LogIf(bool condition, LogSeverity severity, string? message)
        {
            if (condition)
            {
                Log(severity, message);
            }
        }

        /// <summary>
        /// Logs a message with automatically determined log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        public void LogIf(bool condition, string? message)
        {
            if (condition)
            {
                Log(message);
            }
        }

        /// <summary>
        /// Logs an object's string representation if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        public void LogIf(bool condition, object? value)
        {
            if (condition)
            {
                Log(value);
            }
        }

        /// <summary>
        /// Logs a message with the specified log severity if it is not null.
        /// </summary>
        /// <param name="severity">The log severity for the message.</param>
        /// <param name="message">The message to log.</param>
        public void LogIfNotNull(LogSeverity severity, string? message)
        {
            if (message != null)
            {
                Log(severity, message);
            }
        }

        /// <summary>
        /// Logs a message with automatically determined log severity if it is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void LogIfNotNull(string? message)
        {
            if (message != null)
            {
                Log(message);
            }
        }

        /// <summary>
        /// Logs an object's string representation if it is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void LogIfNotNull(object? value)
        {
            if (value != null)
            {
                Log(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a log message with a specified log severity and message.
        /// </summary>
        /// <param name="type">The log severity of the message.</param>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogAsync(LogSeverity type, string? message)
        {
            await LogAsync(new LogMessage(this, type, message ?? "null"));
        }

        /// <summary>
        /// Asynchronously logs an exception with an error log severity.
        /// </summary>
        /// <param name="e">The exception to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogAsync(Exception? e)
        {
            var message = e?.ToString() ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Error, message));
        }

        /// <summary>
        /// Asynchronously logs an object's string representation with an automatically determined log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            LogSeverity type = Evaluate(message);
            await LogAsync(new LogMessage(this, type, message));
        }

        /// <summary>
        /// Asynchronously logs a message with an automatically determined log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogAsync(string? message)
        {
            var msg = message ?? "null";
            LogSeverity type = Evaluate(msg);
            await LogAsync(new LogMessage(this, type, msg));
        }

        /// <summary>
        /// Asynchronously logs a failed assertion with an error log severity.
        /// </summary>
        /// <param name="message">The assertion message.</param>
        /// <param name="detailedMessage">The detailed message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task FailAsync(string? message, string? detailedMessage)
        {
            StringBuilder sb = new();
            sb.AppendLine("//// Assert Fail");
            sb.AppendLine();
            sb.AppendLine("Message:");
            sb.AppendLine(message);
            sb.AppendLine();
            sb.AppendLine("Stack:");
            sb.AppendLine(new StackTrace().ToString());
            sb.AppendLine();
            sb.AppendLine("Detailed Message:");
            sb.AppendLine(detailedMessage);
            sb.AppendLine();
            sb.AppendLine("//// Assert Fail End");

            var msg = sb.ToString();

            await LogAsync(new LogMessage(this, LogSeverity.Error, msg));
            throw new Exception(msg);
        }

        /// <summary>
        /// Asynchronously logs a failed assertion with an error log severity, with no detailed message.
        /// </summary>
        /// <param name="message">The assertion message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task FailAsync(string? message) => await FailAsync(message, string.Empty);

        /// <summary>
        /// Asynchronously asserts a condition and logs a failure if it is false.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">The assertion message.</param>
        /// <param name="detailedMessage">The detailed message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AssertAsync(bool condition, string? message, string? detailedMessage)
        {
            if (!condition)
            {
                await FailAsync(message, detailedMessage);
            }
        }

        /// <summary>
        /// Asynchronously asserts a condition and logs a failure if it is false, with no detailed message.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">The assertion message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AssertAsync(bool condition, string? message) => await AssertAsync(condition, message, string.Empty);

        /// <summary>
        /// Asynchronously asserts a condition and logs a failure if it is false, with no assertion message or detailed message.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AssertAsync(bool condition) => await AssertAsync(condition, string.Empty, string.Empty);

        /// <summary>
        /// Asynchronously logs a message with the specified log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="severity">The log severity for the message.</param>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogIfAsync(bool condition, LogSeverity severity, string? message)
        {
            if (condition)
            {
                await LogAsync(severity, message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with automatically determined log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await LogAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs an object's string representation if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await LogAsync(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with the specified log severity if it is not null.
        /// </summary>
        /// <param name="severity">The log severity for the message.</param>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogIfNotNullAsync(LogSeverity severity, string? message)
        {
            if (message != null)
            {
                await LogAsync(severity, message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with automatically determined log severity if it is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogIfNotNullAsync(string? message)
        {
            if (message != null)
            {
                await LogAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs an object's string representation if it is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogIfNotNullAsync(object? value)
        {
            if (value != null)
            {
                await LogAsync(value);
            }
        }

        /// <summary>
        /// Logs a message with a trace log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void Trace(object? value)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(this, LogSeverity.Trace, message));
        }

        /// <summary>
        /// Logs a message with a trace log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Trace(string? message)
        {
            var msg = message ?? "null";
            Log(new LogMessage(this, LogSeverity.Trace, msg));
        }

        /// <summary>
        /// Logs a message with an information log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void Info(object? value)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(this, LogSeverity.Info, message));
        }

        /// <summary>
        /// Logs a message with an information log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Info(string? message)
        {
            var msg = message ?? "null";
            Log(new LogMessage(this, LogSeverity.Info, msg));
        }

        /// <summary>
        /// Logs a message with a warning log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void Warn(object? value)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(this, LogSeverity.Warning, message));
        }

        /// <summary>
        /// Logs a message with a warning log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Warn(string? message)
        {
            var msg = message ?? "null";
            Log(new LogMessage(this, LogSeverity.Warning, msg));
        }

        /// <summary>
        /// Logs a message with an error log severity and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void Error(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(this, LogSeverity.Error, message));
            if (throwException)
                throw new(message);
        }

        /// <summary>
        /// Logs a message with an error log severity and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void Error(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            Log(new LogMessage(this, LogSeverity.Error, msg));
            if (throwException)
                throw new(msg);
        }

        /// <summary>
        /// Logs a message with a critical log severity and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void Critical(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(this, LogSeverity.Critical, message));
            if (throwException)
                throw new(message);
        }

        /// <summary>
        /// Logs a message with a critical log severity and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void Critical(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            Log(new LogMessage(this, LogSeverity.Critical, msg));
            if (throwException)
                throw new(msg);
        }

        /// <summary>
        /// Asynchronously logs a message with a trace log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task TraceAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Trace, message));
        }

        /// <summary>
        /// Asynchronously logs a message with a trace log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task TraceAsync(string? message)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Trace, msg));
        }

        /// <summary>
        /// Asynchronously logs a message with an information log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InfoAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Info, message));
        }

        /// <summary>
        /// Asynchronously logs a message with an information log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InfoAsync(string? message)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Info, msg));
        }

        /// <summary>
        /// Asynchronously logs a message with a warning log severity.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task WarnAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Warning, message));
        }

        /// <summary>
        /// Asynchronously logs a message with a warning log severity.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task WarnAsync(string? message)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Warning, msg));
        }

        /// <summary>
        /// Asynchronously logs a message with an error log severity and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ErrorAsync(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Error, message));
            if (throwException)
                throw new(message);
        }

        /// <summary>
        /// Asynchronously logs a message with an error log severity and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ErrorAsync(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Error, msg));
            if (throwException)
                throw new(message);
        }

        /// <summary>
        /// Asynchronously logs a message with a critical log severity and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CriticalAsync(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Critical, message));
            if (throwException)
                throw new(message);
        }

        /// <summary>
        /// Asynchronously logs a message with a critical log severity and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CriticalAsync(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(this, LogSeverity.Critical, msg));
            if (throwException)
                throw new(message);
        }

        /// <summary>
        /// Logs a message with a trace log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        public void TraceIf(bool condition, object? value)
        {
            if (condition)
            {
                Trace(value);
            }
        }

        /// <summary>
        /// Logs a message with a trace log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        public void TraceIf(bool condition, string? message)
        {
            if (condition)
            {
                Trace(message);
            }
        }

        /// <summary>
        /// Logs a message with an information log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        public void InfoIf(bool condition, object? value)
        {
            if (condition)
            {
                Info(value);
            }
        }

        /// <summary>
        /// Logs a message with an information log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        public void InfoIf(bool condition, string? message)
        {
            if (condition)
            {
                Info(message);
            }
        }

        /// <summary>
        /// Logs a message with a warning log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        public void WarnIf(bool condition, object? value)
        {
            if (condition)
            {
                Warn(value);
            }
        }

        /// <summary>
        /// Logs a message with a warning log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        public void WarnIf(bool condition, string? message)
        {
            if (condition)
            {
                Warn(message);
            }
        }

        /// <summary>
        /// Logs a message with an error log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void ErrorIf(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                Error(value, throwException);
            }
        }

        /// <summary>
        /// Logs a message with an error log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void ErrorIf(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                Error(message, throwException);
            }
        }

        /// <summary>
        /// Logs a message with a critical log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void CriticalIf(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                Critical(value, throwException);
            }
        }

        /// <summary>
        /// Logs a message with a critical log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void CriticalIf(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                Critical(message, throwException);
            }
        }

        /// <summary>
        /// Logs a message with a trace log severity if the object is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void TraceIfNotNull(object? value)
        {
            if (value != null)
            {
                Trace(value);
            }
        }

        /// <summary>
        /// Logs a message with a trace log severity if the message is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void TraceIfNotNull(string? message)
        {
            if (message != null)
            {
                Trace(message);
            }
        }

        /// <summary>
        /// Logs a message with an information log severity if the object is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void InfoIfNotNull(object? value)
        {
            if (value != null)
            {
                Info(value);
            }
        }

        /// <summary>
        /// Logs a message with an information log severity if the message is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void InfoIfNotNull(string? message)
        {
            if (message != null)
            {
                Info(message);
            }
        }

        /// <summary>
        /// Logs a message with a warning log severity if the object is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public void WarnIfNotNull(object? value)
        {
            if (value != null)
            {
                Warn(value);
            }
        }

        /// <summary>
        /// Logs a message with a warning log severity if the message is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void WarnIfNotNull(string? message)
        {
            if (message != null)
            {
                Warn(message);
            }
        }

        /// <summary>
        /// Logs a message with an error log severity if the object is not null and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void ErrorIfNotNull(object? value, bool throwException = false)
        {
            if (value != null)
            {
                Error(value, throwException);
            }
        }

        /// <summary>
        /// Logs a message with an error log severity if the message is not null and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void ErrorIfNotNull(string? message, bool throwException = false)
        {
            if (message != null)
            {
                Error(message, throwException);
            }
        }

        /// <summary>
        /// Logs a message with a critical log severity if the object is not null and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void CriticalIfNotNull(object? value, bool throwException = false)
        {
            if (value != null)
            {
                Critical(value, throwException);
            }
        }

        /// <summary>
        /// Logs a message with a critical log severity if the message is not null and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public void CriticalIfNotNull(string? message, bool throwException = false)
        {
            if (message != null)
            {
                Critical(message, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a trace log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        public async Task TraceIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await TraceAsync(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a trace log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        public async Task TraceIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await TraceAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an information log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        public async Task InfoIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await InfoAsync(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an information log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        public async Task InfoIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await InfoAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a warning log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        public async Task WarnIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await WarnAsync(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a warning log severity if a condition is met.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        public async Task WarnIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await WarnAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an error log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task ErrorIfAsync(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                await ErrorAsync(value, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an error log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task ErrorIfAsync(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                await ErrorAsync(message, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a critical log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task CriticalIfAsync(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                await CriticalAsync(value, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a critical log severity if a condition is met and optionally throws an exception.
        /// </summary>
        /// <param name="condition">The condition for logging.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task CriticalIfAsync(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                await CriticalAsync(message, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a trace log severity if the object is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public async Task TraceIfNotNullAsync(object? value)
        {
            if (value != null)
            {
                await TraceAsync(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a trace log severity if the message is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public async Task TraceIfNotNullAsync(string? message)
        {
            if (message != null)
            {
                await TraceAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an information log severity if the object is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public async Task InfoIfNotNullAsync(object? value)
        {
            if (value != null)
            {
                await InfoAsync(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an information log severity if the message is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public async Task InfoIfNotNullAsync(string? message)
        {
            if (message != null)
            {
                await InfoAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a warning log severity if the object is not null.
        /// </summary>
        /// <param name="value">The object to log.</param>
        public async Task WarnIfNotNullAsync(object? value)
        {
            if (value != null)
            {
                await WarnAsync(value);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a warning log severity if the message is not null.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public async Task WarnIfNotNullAsync(string? message)
        {
            if (message != null)
            {
                await WarnAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an error log severity if the object is not null and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task ErrorIfNotNullAsync(object? value, bool throwException = false)
        {
            if (value != null)
            {
                await ErrorAsync(value, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with an error log severity if the message is not null and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task ErrorIfNotNullAsync(string? message, bool throwException = false)
        {
            if (message != null)
            {
                await ErrorAsync(message, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a critical log severity if the object is not null and optionally throws an exception.
        /// </summary>
        /// <param name="value">The object to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task CriticalIfNotNullAsync(object? value, bool throwException = false)
        {
            if (value != null)
            {
                await CriticalAsync(value, throwException);
            }
        }

        /// <summary>
        /// Asynchronously logs a message with a critical log severity if the message is not null and optionally throws an exception.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="throwException">Whether to throw an exception.</param>
        public async Task CriticalIfNotNullAsync(string? message, bool throwException = false)
        {
            if (message != null)
            {
                await CriticalAsync(message, throwException);
            }
        }

        /// <summary>
        /// Logs an exception and then rethrows it.
        /// </summary>
        /// <param name="exception">The exception to log and throw.</param>
        public void Throw(Exception exception)
        {
            Log(exception);
            throw exception;
        }

        /// <summary>
        /// Logs an error message and then throws a new exception with the provided message.
        /// </summary>
        /// <param name="message">The error message to log and throw as an exception.</param>
        public void Throw(string? message)
        {
            Log(LogSeverity.Error, message);
            throw new(message);
        }

        /// <summary>
        /// Logs an error message, then throws a new exception with the provided message and inner exception.
        /// </summary>
        /// <param name="message">The error message to log and throw as an exception.</param>
        /// <param name="innerException">The inner exception to include in the thrown exception.</param>
        public void Throw(string? message, Exception innerException)
        {
            Log(LogSeverity.Error, message);
            throw new(message, innerException);
        }

        /// <summary>
        /// Logs an exception and then rethrows it if a condition is met.
        /// </summary>
        /// <param name="condition">The condition to determine if the exception should be thrown.</param>
        /// <param name="exception">The exception to log and potentially throw.</param>
        public void ThrowIf(bool condition, Exception exception)
        {
            if (condition)
            {
                Throw(exception);
            }
        }

        /// <summary>
        /// Logs an error message and then throws a new exception with the provided message if a condition is met.
        /// </summary>
        /// <param name="condition">The condition to determine if the exception should be thrown.</param>
        /// <param name="message">The error message to log and potentially throw as an exception.</param>
        public void ThrowIf(bool condition, string? message)
        {
            if (condition)
            {
                Throw(message);
            }
        }

        /// <summary>
        /// Logs an error message, then throws a new exception with the provided message and inner exception if a condition is met.
        /// </summary>
        /// <param name="condition">The condition to determine if the exception should be thrown.</param>
        /// <param name="message">The error message to log and potentially throw as an exception.</param>
        /// <param name="innerException">The inner exception to include in the thrown exception.</param>
        public void ThrowIf(bool condition, string? message, Exception innerException)
        {
            if (condition)
            {
                Throw(message, innerException);
            }
        }

        /// <summary>
        /// Logs an exception and then rethrows it if the provided exception is not null.
        /// </summary>
        /// <param name="exception">The exception to log and potentially throw if it is not null.</param>
        public void ThrowIfNotNull(Exception? exception)
        {
            if (exception != null)
            {
                Throw(exception);
            }
        }

        /// <summary>
        /// Logs an error message and then throws a new exception with the provided message if the message is not null.
        /// </summary>
        /// <param name="message">The error message to log and potentially throw as an exception if it is not null.</param>
        public void ThrowIfNotNull(string? message)
        {
            if (message != null)
            {
                Throw(message);
            }
        }

        /// <summary>
        /// Logs an error message, then throws a new exception with the provided message and inner exception if both are not null.
        /// </summary>
        /// <param name="message">The error message to log and potentially throw as an exception if it is not null.</param>
        /// <param name="innerException">The inner exception to include in the thrown exception if it is not null.</param>
        public void ThrowIfNotNull(string? message, Exception? innerException)
        {
            if (message != null && innerException != null)
            {
                Throw(message, innerException);
            }
        }

        /// <summary>
        /// Asynchronously logs an exception and then rethrows it.
        /// </summary>
        /// <param name="exception">The exception to log and throw.</param>
        public async Task ThrowAsync(Exception exception)
        {
            await LogAsync(exception);
            throw exception;
        }

        /// <summary>
        /// Asynchronously logs an error message and then throws a new exception with the provided message.
        /// </summary>
        /// <param name="message">The error message to log and throw as an exception.</param>
        public async Task ThrowAsync(string? message)
        {
            await LogAsync(LogSeverity.Error, message);
            throw new(message);
        }

        /// <summary>
        /// Asynchronously logs an error message, then throws a new exception with the provided message and inner exception.
        /// </summary>
        /// <param name="message">The error message to log and throw as an exception.</param>
        /// <param name="innerException">The inner exception to include in the thrown exception.</param>
        public async Task ThrowAsync(string? message, Exception innerException)
        {
            await LogAsync(LogSeverity.Error, message);
            throw new(message, innerException);
        }

        /// <summary>
        /// Asynchronously logs an exception and then rethrows it if a condition is met.
        /// </summary>
        /// <param name="condition">The condition to determine if the exception should be thrown.</param>
        /// <param name="exception">The exception to log and potentially throw.</param>
        public async Task ThrowIfAsync(bool condition, Exception exception)
        {
            if (condition)
            {
                await ThrowAsync(exception);
            }
        }

        /// <summary>
        /// Asynchronously logs an error message and then throws a new exception with the provided message if a condition is met.
        /// </summary>
        /// <param name="condition">The condition to determine if the exception should be thrown.</param>
        /// <param name="message">The error message to log and potentially throw as an exception.</param>
        public async Task ThrowIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await ThrowAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs an error message, then throws a new exception with the provided message and inner exception if a condition is met.
        /// </summary>
        /// <param name="condition">The condition to determine if the exception should be thrown.</param>
        /// <param name="message">The error message to log and potentially throw as an exception.</param>
        /// <param name="innerException">The inner exception to include in the thrown exception.</param>
        public async Task ThrowIfAsync(bool condition, string? message, Exception innerException)
        {
            if (condition)
            {
                await ThrowAsync(message, innerException);
            }
        }

        /// <summary>
        /// Asynchronously logs an exception and then rethrows it if the provided exception is not null.
        /// </summary>
        /// <param name="exception">The exception to log and potentially throw if it is not null.</param>
        public async Task ThrowIfNotNullAsync(Exception? exception)
        {
            if (exception != null)
            {
                await ThrowAsync(exception);
            }
        }

        /// <summary>
        /// Asynchronously logs an error message and then throws a new exception with the provided message if the message is not null.
        /// </summary>
        /// <param name="message">The error message to log and potentially throw as an exception if it is not null.</param>
        public async Task ThrowIfNotNullAsync(string? message)
        {
            if (message != null)
            {
                await ThrowAsync(message);
            }
        }

        /// <summary>
        /// Asynchronously logs an error message, then throws a new exception with the provided message and inner exception if both are not null.
        /// </summary>
        /// <param name="message">The error message to log and potentially throw as an exception if it is not null.</param>
        /// <param name="innerException">The inner exception to include in the thrown exception if it is not null.</param>
        public async Task ThrowIfNotNullAsync(string? message, Exception? innerException)
        {
            if (message != null && innerException != null)
            {
                await ThrowAsync(message, innerException);
            }
        }
    }
}