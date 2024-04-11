namespace HexaEngine.Core.Debugging
{
    public interface ILogger
    {
        LogSeverity LogLevel { get; set; }

        string Name { get; internal set; }

        List<ILogWriter> Writers { get; }

        void Assert(bool condition);

        void Assert(bool condition, string? message);

        void Assert(bool condition, string? message, string? detailedMessage);

        Task AssertAsync(bool condition);

        Task AssertAsync(bool condition, string? message);

        Task AssertAsync(bool condition, string? message, string? detailedMessage);

        void Clear();

        void Close();

        void Critical(object? value, bool throwException = false);

        void Critical(string? message, bool throwException = false);

        Task CriticalAsync(object? value, bool throwException = false);

        Task CriticalAsync(string? message, bool throwException = false);

        void CriticalIf(bool condition, object? value, bool throwException = false);

        void CriticalIf(bool condition, string? message, bool throwException = false);

        Task CriticalIfAsync(bool condition, object? value, bool throwException = false);

        Task CriticalIfAsync(bool condition, string? message, bool throwException = false);

        void CriticalIfNotNull(object? value, bool throwException = false);

        void CriticalIfNotNull(string? message, bool throwException = false);

        Task CriticalIfNotNullAsync(object? value, bool throwException = false);

        Task CriticalIfNotNullAsync(string? message, bool throwException = false);

        void Error(object? value, bool throwException = false);

        void Error(string? message, bool throwException = false);

        Task ErrorAsync(object? value, bool throwException = false);

        Task ErrorAsync(string? message, bool throwException = false);

        void ErrorIf(bool condition, object? value, bool throwException = false);

        void ErrorIf(bool condition, string? message, bool throwException = false);

        Task ErrorIfAsync(bool condition, object? value, bool throwException = false);

        Task ErrorIfAsync(bool condition, string? message, bool throwException = false);

        void ErrorIfNotNull(object? value, bool throwException = false);

        void ErrorIfNotNull(string? message, bool throwException = false);

        Task ErrorIfNotNullAsync(object? value, bool throwException = false);

        Task ErrorIfNotNullAsync(string? message, bool throwException = false);

        void Fail(string? message);

        void Fail(string? message, string? detailedMessage);

        Task FailAsync(string? message);

        Task FailAsync(string? message, string? detailedMessage);

        void Flush();

        Task HandleError(Task task);

        void Info(object? value);

        void Info(string? message);

        Task InfoAsync(object? value);

        Task InfoAsync(string? message);

        void InfoIf(bool condition, object? value);

        void InfoIf(bool condition, string? message);

        Task InfoIfAsync(bool condition, object? value);

        Task InfoIfAsync(bool condition, string? message);

        void InfoIfNotNull(object? value);

        void InfoIfNotNull(string? message);

        Task InfoIfNotNullAsync(object? value);

        Task InfoIfNotNullAsync(string? message);

        void Log(Exception? e);

        void Log(LogMessage message);

        void Log(LogSeverity severity, string? message);

        void Log(object? value);

        void Log(string? message);

        Task LogAsync(Exception? e);

        Task LogAsync(LogMessage message);

        Task LogAsync(LogSeverity type, string? message);

        Task LogAsync(object? value);

        Task LogAsync(string? message);

        void LogIf(bool condition, LogSeverity severity, string? message);

        void LogIf(bool condition, object? value);

        void LogIf(bool condition, string? message);

        Task LogIfAsync(bool condition, LogSeverity severity, string? message);

        Task LogIfAsync(bool condition, object? value);

        Task LogIfAsync(bool condition, string? message);

        void LogIfNotNull(LogSeverity severity, string? message);

        void LogIfNotNull(object? value);

        void LogIfNotNull(string? message);

        Task LogIfNotNullAsync(LogSeverity severity, string? message);

        Task LogIfNotNullAsync(object? value);

        Task LogIfNotNullAsync(string? message);

        void Throw(Exception exception);

        void Throw(string? message);

        void Throw(string? message, Exception innerException);

        Task ThrowAsync(Exception exception);

        Task ThrowAsync(string? message);

        Task ThrowAsync(string? message, Exception innerException);

        void ThrowIf(bool condition, Exception exception);

        void ThrowIf(bool condition, string? message);

        void ThrowIf(bool condition, string? message, Exception innerException);

        Task ThrowIfAsync(bool condition, Exception exception);

        Task ThrowIfAsync(bool condition, string? message);

        Task ThrowIfAsync(bool condition, string? message, Exception innerException);

        void ThrowIfNotNull(Exception? exception);

        void ThrowIfNotNull(string? message);

        void ThrowIfNotNull(string? message, Exception? innerException);

        Task ThrowIfNotNullAsync(Exception? exception);

        Task ThrowIfNotNullAsync(string? message);

        Task ThrowIfNotNullAsync(string? message, Exception? innerException);

        void Trace(object? value);

        void Trace(string? message);

        Task TraceAsync(object? value);

        Task TraceAsync(string? message);

        void TraceIf(bool condition, object? value);

        void TraceIf(bool condition, string? message);

        Task TraceIfAsync(bool condition, object? value);

        Task TraceIfAsync(bool condition, string? message);

        void TraceIfNotNull(object? value);

        void TraceIfNotNull(string? message);

        Task TraceIfNotNullAsync(object? value);

        Task TraceIfNotNullAsync(string? message);

        void Warn(object? value);

        void Warn(string? message);

        Task WarnAsync(object? value);

        Task WarnAsync(string? message);

        void WarnIf(bool condition, object? value);

        void WarnIf(bool condition, string? message);

        Task WarnIfAsync(bool condition, object? value);

        Task WarnIfAsync(bool condition, string? message);

        void WarnIfNotNull(object? value);

        void WarnIfNotNull(string? message);

        Task WarnIfNotNullAsync(object? value);

        Task WarnIfNotNullAsync(string? message);
    }
}