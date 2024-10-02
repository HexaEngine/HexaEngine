namespace HexaEngine.Core.Assets
{
    using Hexa.NET.Logging;

    public interface IImportProgress
    {
        public void Report(float value);

        public void BeginStep(string title);

        public void EndStep();

        public void LogMessage(LogMessage message);
    }
}