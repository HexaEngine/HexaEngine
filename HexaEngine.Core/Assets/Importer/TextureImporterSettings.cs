namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.Graphics;

    public class TextureImporterSettings
    {
        public Format Format { get; set; } = Format.BC7UNorm;

        public int MaxWidth { get; set; } = 2048;

        public int MaxHeight { get; set; } = 2048;

        public bool SRGB { get; set; }

        public bool BC7Quick { get; set; }

        public bool GenerateMipMaps { get; set; } = true;
    }
}