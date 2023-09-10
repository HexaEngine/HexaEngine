namespace HexaEngine.Projects
{
    using System.Collections.Generic;

    public class PublishSettings
    {
        public string? StartupScene { get; set; }

        public List<string> Scenes { get; set; } = new();

        public string? RuntimeIdentifier { get; set; }

        public string Profile { get; set; } = "ReleaseResources";

        public bool StripDebugInfo { get; set; } = false;

        public bool SingleFile { get; set; } = true;

        public bool ReadyToRun { get; set; } = true;
    }
}