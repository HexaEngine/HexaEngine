namespace HexaEngine.ShadingLang.LexicalAnalysis
{
    public struct LogMessage
    {
        public LogMessageType Type;
        public string Text;

        public LogMessage(LogMessageType type, string text)
        {
            Type = type;
            Text = text;
        }
    }
}