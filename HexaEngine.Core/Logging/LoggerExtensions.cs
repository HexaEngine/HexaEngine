namespace HexaEngine.Core.Logging
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.UI;

    public static class LoggerExtensions
    {
        public static void LogAndShowError(this ILogger logger, string errorMessage, Exception ex)
        {
            logger.Error(errorMessage);
            logger.Log(ex);
            MessageBox.Show(errorMessage, ex.Message);
        }
    }
}