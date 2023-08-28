namespace HexaEngine.Editor.ImagePainter.Exporters
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.UI;
    using ImGuiNET;

    public class DDSExporter : BaseExporter
    {
        private TexCompressFlags compressFlags = TexCompressFlags.Parallel;
        private bool generateMipMaps;

        protected override void DrawExporterSettings()
        {
            ImGui.BeginListBox("Common");

            ImGui.Checkbox("GenerateMipMaps", ref generateMipMaps);

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Enables generation of MipMaps");
            }

            ImGui.EndListBox();

            ImGui.BeginListBox("Compression");
            int compressFlagsi = (int)compressFlags;
            if (ImGui.CheckboxFlags("Uniform", ref compressFlagsi, (int)TexCompressFlags.Uniform))
            {
                compressFlags = (TexCompressFlags)compressFlagsi;
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Uniform color weighting for BC1-3 compression; by default uses perceptual weighting");
            }

            if (ImGui.CheckboxFlags("DitherA", ref compressFlagsi, (int)TexCompressFlags.DitherA))
            {
                compressFlags = (TexCompressFlags)compressFlagsi;
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Enables dithering alpha for BC1-3 compression");
            }

            if (ImGui.CheckboxFlags("DitherRGB", ref compressFlagsi, (int)TexCompressFlags.DitherRGB))
            {
                compressFlags = (TexCompressFlags)compressFlagsi;
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Enables dithering RGB colors for BC1-3 compression");
            }

            if (ImGui.CheckboxFlags("BC7Use3Sunsets", ref compressFlagsi, (int)TexCompressFlags.BC7Use3Sunsets))
            {
                compressFlags = (TexCompressFlags)compressFlagsi;
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Enables exhaustive search for BC7 compress for mode 0 and 2; by default skips trying these modes");
            }

            if (ImGui.CheckboxFlags("BC7Quick", ref compressFlagsi, (int)TexCompressFlags.BC7Quick))
            {
                compressFlags = (TexCompressFlags)compressFlagsi;
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("Minimal modes (usually mode 6) for BC7 compression");
            }

            if (ImGui.CheckboxFlags("SRGB", ref compressFlagsi, (int)TexCompressFlags.SRGB))
            {
                compressFlags = (TexCompressFlags)compressFlagsi;
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip("if the input format type is IsSRGB(), then SRGB_IN is on by default\nif the output format type is IsSRGB(), then SRGB_OUT is on by default");
            }

            ImGui.EndListBox();
        }

        protected override void Export(IGraphicsDevice device)
        {
            if (Image == null || Path == null)
            {
                return;
            }

            var baseFormat = Image.Metadata.Format;
            var image = Image;
            bool recreated = false;

            try
            {
                if (generateMipMaps)
                {
                    IScratchImage image1 = image.GenerateMipMaps(TexFilterFlags.Default);
                    if (recreated)
                    {
                        image.Dispose();
                    }
                    image = image1;
                    recreated = true;
                }

                if (baseFormat != format)
                {
                    IScratchImage image1;
                    if (FormatHelper.IsCompressed(format))
                    {
                        image1 = image.Compress(device, format, compressFlags);
                    }
                    else
                    {
                        image1 = image.Convert(format, TexFilterFlags.Default);
                    }
                    if (recreated)
                    {
                        image.Dispose();
                    }
                    image = image1;
                    recreated = true;
                }

                image.SaveToFile(Path, TexFileFormat.DDS, (int)DDSFlags.None);
                if (recreated)
                {
                    image.Dispose();
                }
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