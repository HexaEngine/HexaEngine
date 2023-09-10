namespace HexaEngine.Core.Debugging
{
    using System;
    using System.Diagnostics;
    using System.Text;

    public static class Logger
    {
        private static readonly SemaphoreSlim semaphore = new(1);
        private static LogSeverity logLevel;

        public static List<ILogWriter> Writers { get; } = new();

        public static LogSeverity LogLevel { get => logLevel; set => logLevel = value; }

        public static void Flush()
        {
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Flush();
            }
            semaphore.Release();
        }

        public static void Clear()
        {
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Clear();
            }
            semaphore.Release();
        }

        public static void Close()
        {
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Dispose();
            }
            semaphore.Release();
        }

        public static Task HandleError(Task task)
        {
            if (!task.IsCompletedSuccessfully && task.Exception != null)
            {
                Log(task.Exception);
            }
            task.Dispose();

            return Task.CompletedTask;
        }

        public static void Log(LogMessage message)
        {
            if (message.Severity < logLevel)
                return;
            semaphore.Wait();
            for (int i = 0; i < Writers.Count; i++)
            {
                Writers[i].Log(message);
            }
            semaphore.Release();
        }

        public static async Task LogAsync(LogMessage message)
        {
            if (message.Severity < logLevel)
                return;
            await semaphore.WaitAsync();
            for (int i = 0; i < Writers.Count; i++)
            {
                await Writers[i].LogAsync(message);
            }
            semaphore.Release();
        }

        public static void Log(LogSeverity severity, string? message)
        {
            Log(new LogMessage(severity, message ?? "null"));
        }

        public static void Log(Exception? e)
        {
            var message = e?.ToString() ?? "null";

            Log(new LogMessage(LogSeverity.Error, message));
        }

        private static LogSeverity Evaluate(string message)
        {
            LogSeverity type = LogSeverity.Information;
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
                type = LogSeverity.Information;
            }
            else if (message.Contains("information", StringComparison.CurrentCultureIgnoreCase))
            {
                type = LogSeverity.Information;
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

        public static void Log(object? value)
        {
            var message = value?.ToString() ?? "null";
            LogSeverity type = Evaluate(message);
            Log(new LogMessage(type, message));
        }

        public static void Log(string? message)
        {
            var msg = message ?? "null";
            LogSeverity type = Evaluate(msg);
            Log(new LogMessage(type, msg));
        }

        public static void Fail(string? message, string? detailedMessage)
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

            Log(new LogMessage(LogSeverity.Error, msg));
        }

        public static void Fail(string? message) => Fail(message, string.Empty);

        public static void Assert(bool condition, string? message, string? detailedMessage)
        {
            if (!condition)
            {
                Fail(message, detailedMessage);
            }
        }

        public static void Assert(bool condition, string? message) => Assert(condition, message, string.Empty);

        public static void Assert(bool condition) => Assert(condition, string.Empty, string.Empty);

        public static void LogIf(bool condition, LogSeverity severity, string? message)
        {
            if (condition)
            {
                Log(severity, message);
            }
        }

        public static void LogIf(bool condition, string? message)
        {
            if (condition)
            {
                Log(message);
            }
        }

        public static void LogIf(bool condition, object? value)
        {
            if (condition)
            {
                Log(value);
            }
        }

        public static void LogIfNotNull(LogSeverity severity, string? message)
        {
            if (message != null)
            {
                Log(severity, message);
            }
        }

        public static void LogIfNotNull(string? message)
        {
            if (message != null)
            {
                Log(message);
            }
        }

        public static void LogIfNotNull(object? value)
        {
            if (value != null)
            {
                Log(value);
            }
        }

        public static async Task LogAsync(LogSeverity type, string? message)
        {
            await LogAsync(new LogMessage(type, message ?? "null"));
        }

        public static async Task LogAsync(Exception? e)
        {
            var message = e?.ToString() ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Error, message));
        }

        public static async Task LogAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            LogSeverity type = Evaluate(message);
            await LogAsync(new LogMessage(type, message));
        }

        public static async Task LogAsync(string? message)
        {
            var msg = message ?? "null";
            LogSeverity type = Evaluate(msg);
            await LogAsync(new LogMessage(type, msg));
        }

        public static async Task FailAsync(string? message, string? detailedMessage)
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

            await LogAsync(new LogMessage(LogSeverity.Error, msg));
            throw new Exception(msg);
        }

        public static async Task FailAsync(string? message) => await FailAsync(message, string.Empty);

        public static async Task AssertAsync(bool condition, string? message, string? detailedMessage)
        {
            if (!condition)
            {
                await FailAsync(message, detailedMessage);
            }
        }

        public static async Task AssertAsync(bool condition, string? message) => await AssertAsync(condition, message, string.Empty);

        public static async Task AssertAsync(bool condition) => await AssertAsync(condition, string.Empty, string.Empty);

        public static async void LogIfAsync(bool condition, LogSeverity severity, string? message)
        {
            if (condition)
            {
                await LogAsync(severity, message);
            }
        }

        public static async void LogIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await LogAsync(message);
            }
        }

        public static async void LogIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await LogAsync(value);
            }
        }

        public static void Trace(object? value)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(LogSeverity.Trace, message));
        }

        public static void Trace(string? message)
        {
            var msg = message ?? "null";
            Log(new LogMessage(LogSeverity.Trace, msg));
        }

        public static void Info(object? value)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(LogSeverity.Information, message));
        }

        public static void Info(string? message)
        {
            var msg = message ?? "null";
            Log(new LogMessage(LogSeverity.Information, msg));
        }

        public static void Warn(object? value)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(LogSeverity.Warning, message));
        }

        public static void Warn(string? message)
        {
            var msg = message ?? "null";
            Log(new LogMessage(LogSeverity.Warning, msg));
        }

        public static void Error(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(LogSeverity.Error, message));
            if (throwException)
                throw new(message);
        }

        public static void Error(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            Log(new LogMessage(LogSeverity.Error, msg));
            if (throwException)
                throw new(msg);
        }

        public static void Critical(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            Log(new LogMessage(LogSeverity.Critical, message));
            if (throwException)
                throw new(message);
        }

        public static void Critical(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            Log(new LogMessage(LogSeverity.Critical, msg));
            if (throwException)
                throw new(msg);
        }

        public static async Task TraceAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Trace, message));
        }

        public static async Task TraceAsync(string? message)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Trace, msg));
        }

        public static async Task InfoAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Information, message));
        }

        public static async Task InfoAsync(string? message)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Information, msg));
        }

        public static async Task WarnAsync(object? value)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Warning, message));
        }

        public static async Task WarnAsync(string? message)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Warning, msg));
        }

        public static async Task ErrorAsync(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Error, message));
            if (throwException)
                throw new(message);
        }

        public static async Task ErrorAsync(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Error, msg));
            if (throwException)
                throw new(message);
        }

        public static async Task CriticalAsync(object? value, bool throwException = false)
        {
            var message = value?.ToString() ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Critical, message));
            if (throwException)
                throw new(message);
        }

        public static async Task CriticalAsync(string? message, bool throwException = false)
        {
            var msg = message ?? "null";
            await LogAsync(new LogMessage(LogSeverity.Critical, msg));
            if (throwException)
                throw new(message);
        }

        public static void TraceIf(bool condition, object? value)
        {
            if (condition)
            {
                Trace(value);
            }
        }

        public static void TraceIf(bool condition, string? message)
        {
            if (condition)
            {
                Trace(message);
            }
        }

        public static void InfoIf(bool condition, object? value)
        {
            if (condition)
            {
                Info(value);
            }
        }

        public static void InfoIf(bool condition, string? message)
        {
            if (condition)
            {
                Info(message);
            }
        }

        public static void WarnIf(bool condition, object? value)
        {
            if (condition)
            {
                Warn(value);
            }
        }

        public static void WarnIf(bool condition, string? message)
        {
            if (condition)
            {
                Warn(message);
            }
        }

        public static void ErrorIf(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                Error(value, throwException);
            }
        }

        public static void ErrorIf(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                Error(message, throwException);
            }
        }

        public static void CriticalIf(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                Critical(value, throwException);
            }
        }

        public static void CriticalIf(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                Critical(message, throwException);
            }
        }

        public static async Task TraceIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await TraceAsync(value);
            }
        }

        public static async Task TraceIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await TraceAsync(message);
            }
        }

        public static async Task InfoIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await InfoAsync(value);
            }
        }

        public static async Task InfoIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await InfoAsync(message);
            }
        }

        public static async Task WarnIfAsync(bool condition, object? value)
        {
            if (condition)
            {
                await WarnAsync(value);
            }
        }

        public static async Task WarnIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await WarnAsync(message);
            }
        }

        public static async Task ErrorIfAsync(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                await ErrorAsync(value, throwException);
            }
        }

        public static async Task ErrorIfAsync(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                await ErrorAsync(message, throwException);
            }
        }

        public static async Task CriticalIfAsync(bool condition, object? value, bool throwException = false)
        {
            if (condition)
            {
                await CriticalAsync(value, throwException);
            }
        }

        public static async Task CriticalIfAsync(bool condition, string? message, bool throwException = false)
        {
            if (condition)
            {
                await CriticalAsync(message, throwException);
            }
        }

        public static void Throw(Exception exception)
        {
            Log(exception);
            throw exception;
        }

        public static void Throw(string? message)
        {
            Log(LogSeverity.Error, message);
            throw new(message);
        }

        public static void Throw(string? message, Exception innerException)
        {
            Log(LogSeverity.Error, message);
            throw new(message, innerException);
        }

        public static void ThrowIf(bool condition, Exception exception)
        {
            if (condition)
            {
                Throw(exception);
            }
        }

        public static void ThrowIf(bool condition, string? message)
        {
            if (condition)
            {
                Throw(message);
            }
        }

        public static void ThrowIf(bool condition, string? message, Exception innerException)
        {
            if (condition)
            {
                Throw(message, innerException);
            }
        }

        public static async Task ThrowAsync(Exception exception)
        {
            await LogAsync(exception);
            throw exception;
        }

        public static async Task ThrowAsync(string? message)
        {
            await LogAsync(LogSeverity.Error, message);
            throw new(message);
        }

        public static async Task ThrowAsync(string? message, Exception innerException)
        {
            await LogAsync(LogSeverity.Error, message);
            throw new(message, innerException);
        }

        public static async Task ThrowIfAsync(bool condition, Exception exception)
        {
            if (condition)
            {
                await ThrowAsync(exception);
            }
        }

        public static async Task ThrowIfAsync(bool condition, string? message)
        {
            if (condition)
            {
                await ThrowAsync(message);
            }
        }

        public static async Task ThrowIfAsync(bool condition, string? message, Exception innerException)
        {
            if (condition)
            {
                await ThrowAsync(message, innerException);
            }
        }
    }
}