namespace HexaEngine.Projects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.IO;
    using System.Collections.Generic;

    public class PublishSettings
    {
        public string? StartupScene { get; set; }

        public List<string> Scenes { get; set; } = new();

        public string? RuntimeIdentifier { get; set; }

        public string Profile { get; set; } = "Release";

        public bool StripDebugInfo { get; set; } = false;

        public bool SingleFile { get; set; } = true;

        public bool ReadyToRun { get; set; } = true;

        public Compression AssetsCompression { get; set; } = Compression.LZ4;

        public TextureSettings TextureSettings { get; } = new();
    }

    public class TextureSettings
    {
        public Format TargetFormat { get; set; } = Format.Unknown;

        public bool GenerateMipMaps { get; set; } = true;

        public TexFileFormat FileFormat { get; set; } = TexFileFormat.DDS;
    }
}