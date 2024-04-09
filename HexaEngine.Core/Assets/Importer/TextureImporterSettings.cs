namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;

    public class TextureImporterSettings
    {
        [EditorProperty<Format>]
        public Format Format { get; set; } = Format.BC7UNorm;

        [EditorProperty("Max Width")]
        public int MaxWidth { get; set; } = 2048;

        [EditorProperty("Max Height")]
        public int MaxHeight { get; set; } = 2048;

        [EditorProperty("sRGB")]
        public bool SRGB { get; set; }

        [EditorProperty("BC7 Quick")]
        public bool BC7Quick { get; set; } = true;

        [EditorProperty("Generate Mip Maps")]
        public bool GenerateMipMaps { get; set; } = true;
    }
}