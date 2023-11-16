namespace HexaEngine.Editor.ImagePainter.Exporters
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Textures.Raw;
    using HexaEngine.Core.UI;

    public class RAWExporter : BaseExporter
    {
        protected override void DrawExporterSettings()
        {
        }

        protected override void Export(IGraphicsDevice device)
        {
            if (Image == null || Path == null)
            {
                return;
            }

            try
            {
                RawImage rawImage = new();
                rawImage.CopyFrom(Image);
                rawImage.SaveToFile(Path);
                rawImage.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export image to: {Path}", ex.Message);
                Logger.Error($"Failed to export image to: {Path}");
                Logger.Log(ex);
            }
        }
    }
}