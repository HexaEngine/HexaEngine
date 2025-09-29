namespace HexaEngine.ShadingLang
{
    using HexaEngine.ShadingLang.LexicalAnalysis;

    public class LogBase
    {
        protected readonly List<LogMessage> messages = [];
        protected bool hasError;

        protected void LogError(string message)
        {
            LogMessage logMessage = new(LogMessageType.Error, message);
            messages.Add(logMessage);
            hasError = true;
        }

        protected void LogWarning(string message)
        {
            LogMessage logMessage = new(LogMessageType.Warn, message);
            messages.Add(logMessage);
        }

        protected void LogInfo(string message)
        {
            LogMessage logMessage = new(LogMessageType.Info, message);
            messages.Add(logMessage);
        }
    }
}