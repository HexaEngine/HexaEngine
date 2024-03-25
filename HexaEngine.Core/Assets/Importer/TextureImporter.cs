namespace HexaEngine.Core.Assets.Importer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;

    public class TextureImporter : IAssetImporter
    {
        private readonly IGraphicsDevice device = Application.GraphicsDevice;

        public bool CanImport(string fileExtension)
        {
            return fileExtension switch
            {
                ".png" => true,
                ".jpg" => true,
                ".jpeg" => true,
                ".tga" => true,
                ".dds" => true,
                ".hdr" => true,
                ".tiff" => true,
                ".gif" => true,
                ".wmp" => true,
                ".bmp" => true,
                ".ico" => true,
                ".raw" => true,
                _ => false,
            };
        }

        public void Import(TargetPlatform targetPlatform, ImportContext context)
        {
            var name = Path.GetFileNameWithoutExtension(context.SourcePath);
            var settings = context.GetOrCreateAdditionalMetadata<TextureImporterSettings>("TextureImportSettings");

            IScratchImage? image = null;
            try
            {
                image = device.TextureLoader.LoadFormFile(context.SourcePath);
            }
            catch (Exception ex)
            {
                image?.Dispose();
                Logger.Log(ex);
                Logger.Error($"Failed to import texture {context.SourcePath}");
                return;
            }

            ExportImage(device, targetPlatform, context, name, settings, image);
        }

        internal static void ExportImage(IGraphicsDevice device, TargetPlatform targetPlatform, ImportContext context, string name, TextureImporterSettings settings, IScratchImage image)
        {
            var metadata = image.Metadata;

            var width = Math.Min(metadata.Width, settings.MaxWidth);
            var height = Math.Min(metadata.Height, settings.MaxHeight);

            if (width != metadata.Width || height != metadata.Height)
            {
                try
                {
                    SwapImage(ref image, image.Resize(width, height, TexFilterFlags.Default));
                }
                catch (Exception ex)
                {
                    image.Dispose();
                    Logger.Log(ex);
                    Logger.Error($"Failed to resize the texture {context.SourcePath}");
                    return;
                }
            }

            bool generateMipMaps = settings.GenerateMipMaps && metadata.MipLevels == 1;
            bool changeFormat = settings.Format != Format.Unknown && settings.Format != metadata.Format;

            if (generateMipMaps && changeFormat && FormatHelper.IsCompressed(metadata.Format))
            {
                try
                {
                    SwapImage(ref image, image.Decompress(Format.R32G32B32A32Float));
                    metadata = image.Metadata;
                }
                catch (Exception ex)
                {
                    image.Dispose();
                    Logger.Log(ex);
                    Logger.Error($"Failed to decompress the texture {context.SourcePath}");
                    return;
                }
            }

            if (generateMipMaps)
            {
                try
                {
                    SwapImage(ref image, image.GenerateMipMaps(TexFilterFlags.Default));
                }
                catch (Exception ex)
                {
                    image.Dispose();
                    Logger.Log(ex);
                    Logger.Error($"Failed to generate mip maps for texture {context.SourcePath}");
                    return;
                }
            }

            if (changeFormat)
            {
                if (FormatHelper.IsCompressed(settings.Format))
                {
                    lock (device)
                    {
                        try
                        {
                            SwapImage(ref image, image.Compress(device, settings.Format, settings.BC7Quick ? TexCompressFlags.BC7Quick | TexCompressFlags.Parallel : TexCompressFlags.Parallel));
                        }
                        catch (Exception ex)
                        {
                            image.Dispose();
                            Logger.Log(ex);
                            Logger.Error($"Failed to compress the texture {context.SourcePath}");
                            return;
                        }
                    }
                }
                else
                {
                    try
                    {
                        SwapImage(ref image, image.Convert(settings.Format, TexFilterFlags.Default));
                    }
                    catch (Exception ex)
                    {
                        image.Dispose();
                        Logger.Log(ex);
                        Logger.Error($"Failed to convert the texture {context.SourcePath}");
                        return;
                    }
                }
            }

            AssetType type = AssetType.Texture2D;

            if (metadata.Height == 1)
            {
                type = AssetType.Texture1D;
            }

            if (metadata.IsVolumemap())
            {
                type = AssetType.Texture3D;
            }

            if (metadata.IsCubemap())
            {
                type = AssetType.TextureCube;
            }

            context.EmitArtifact(name, type, out string outputPath);

            FileStream? fs = null;
            try
            {
                fs = File.Create(outputPath);
                TexFileFormat fileFormat = targetPlatform switch
                {
                    TargetPlatform.Windows => TexFileFormat.DDS,
                    _ => TexFileFormat.TGA
                };

                image.SaveToMemory(fs, fileFormat, 0);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to save texture ({context.SourcePath}) to {outputPath}.");
            }
            finally
            {
                fs?.Close();
                image.Dispose();
            }
        }

        internal static void ExportImage(IGraphicsDevice device, TargetPlatform targetPlatform, ImportContext context, Guid guid, string name, TextureImporterSettings settings, IScratchImage image)
        {
            var metadata = image.Metadata;

            var width = Math.Min(metadata.Width, settings.MaxWidth);
            var height = Math.Min(metadata.Height, settings.MaxHeight);

            if (width != metadata.Width || height != metadata.Height)
            {
                try
                {
                    SwapImage(ref image, image.Resize(width, height, TexFilterFlags.Default));
                }
                catch (Exception ex)
                {
                    image.Dispose();
                    Logger.Log(ex);
                    Logger.Error($"Failed to resize the texture {context.SourcePath}");
                    return;
                }
            }

            bool generateMipMaps = settings.GenerateMipMaps && metadata.MipLevels == 1;
            bool changeFormat = settings.Format != Format.Unknown && settings.Format != metadata.Format;

            if (generateMipMaps && changeFormat && FormatHelper.IsCompressed(metadata.Format))
            {
                try
                {
                    SwapImage(ref image, image.Decompress(Format.R32G32B32A32Float));
                    metadata = image.Metadata;
                }
                catch (Exception ex)
                {
                    image.Dispose();
                    Logger.Log(ex);
                    Logger.Error($"Failed to decompress the texture {context.SourcePath}");
                    return;
                }
            }

            if (generateMipMaps)
            {
                try
                {
                    SwapImage(ref image, image.GenerateMipMaps(TexFilterFlags.Default));
                }
                catch (Exception ex)
                {
                    image.Dispose();
                    Logger.Log(ex);
                    Logger.Error($"Failed to generate mip maps for texture {context.SourcePath}");
                    return;
                }
            }

            if (changeFormat)
            {
                if (FormatHelper.IsCompressed(settings.Format))
                {
                    lock (device)
                    {
                        try
                        {
                            SwapImage(ref image, image.Compress(settings.Format, settings.BC7Quick ? TexCompressFlags.BC7Quick | TexCompressFlags.Parallel : TexCompressFlags.Parallel));
                        }
                        catch (Exception ex)
                        {
                            image.Dispose();
                            Logger.Log(ex);
                            Logger.Error($"Failed to compress the texture {context.SourcePath}");
                            return;
                        }
                    }
                }
                else
                {
                    try
                    {
                        SwapImage(ref image, image.Convert(settings.Format, TexFilterFlags.Default));
                    }
                    catch (Exception ex)
                    {
                        image.Dispose();
                        Logger.Log(ex);
                        Logger.Error($"Failed to convert the texture {context.SourcePath}");
                        return;
                    }
                }
            }

            AssetType type = AssetType.Texture2D;

            if (metadata.Height == 1)
            {
                type = AssetType.Texture1D;
            }

            if (metadata.IsVolumemap())
            {
                type = AssetType.Texture3D;
            }

            if (metadata.IsCubemap())
            {
                type = AssetType.TextureCube;
            }

            context.EmitArtifact(name, guid, type, out string outputPath);

            FileStream? fs = null;
            try
            {
                fs = File.Create(outputPath);
                TexFileFormat fileFormat = targetPlatform switch
                {
                    TargetPlatform.Windows => TexFileFormat.DDS,
                    _ => TexFileFormat.TGA
                };

                image.SaveToMemory(fs, fileFormat, 0);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Failed to save texture ({context.SourcePath}) to {outputPath}.");
            }
            finally
            {
                fs?.Close();
                image.Dispose();
            }
        }

        private static void SwapImage(ref IScratchImage before, IScratchImage after)
        {
            before.Dispose();
            before = after;
        }
    }
}