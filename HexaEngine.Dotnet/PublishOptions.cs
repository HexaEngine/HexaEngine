namespace HexaEngine.Dotnet
{
    public class PublishOptions
    {
        public string Profile { get; set; } = "Debug";

        public string? RuntimeIdentifer { get; set; }

        public string? Framework { get; set; }

        public bool SelfContained { get; set; } = false;

        public bool PublishSingleFile { get; set; } = false;

        public bool PublishReadyToRun { get; set; } = false;

        public DebugType DebugType { get; set; } = DebugType.Full;

        public bool DebugSymbols { get; set; } = true;
    }
}