namespace HexaEngine.Core.Assets.Importer
{
    using Hexa.NET.Logging;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;

    public class TextureThumbnailProvider : IAssetThumbnailProvider
    {
        private readonly IGraphicsDevice device = Application.GraphicsDevice;

        public bool CanCreate(ReadOnlySpan<char> fileExtension, SourceAssetMetadata metadata)
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

        public IScratchImage? CreateThumbnail(SourceAssetMetadata metadata, ILogger? logger = null)
        {
            IScratchImage? image = null;
            try
            {
                image = device.TextureLoader.LoadFormFile(metadata.GetFullPath());
            }
            catch (Exception ex)
            {
                image?.Dispose();
                logger?.Log(ex);
                logger?.Error($"Failed to generate thumbnail for texture {metadata}");
                return null;
            }

            var texMetadata = image.Metadata;
            bool owns = false;
            if (FormatHelper.IsCompressed(texMetadata.Format))
            {
                Swap(ref owns, ref image, image.Decompress(Format.R8G8B8A8UNorm));
            }
            else if (texMetadata.Format != Format.R8G8B8A8UNorm)
            {
                Swap(ref owns, ref image, image.Convert(Format.R8G8B8A8UNorm, 0));
            }

            Swap(ref owns, ref image, image.Resize(256, 256, 0));

            return image;
        }

        private static void Swap(ref bool owns, ref IScratchImage image, IScratchImage newImage)
        {
            if (owns)
            {
                image.Dispose();
            }
            owns = true;
            image = newImage;
        }
    }

    public class TextureImporter : IAssetImporter
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(TextureImporter));
        private readonly IGraphicsDevice device = Application.GraphicsDevice;

        public Type? SettingsType { get; } = typeof(TextureImporterSettings);

        public string SettingsKey { get; } = "TextureImportSettings";

        public string? SettingsDisplayName { get; } = "Texture Import Settings";

        public bool CanImport(ReadOnlySpan<char> fileExtension)
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
            if (Application.MainWindow != null)
            {
                Application.MainWindow.Dispatcher.InvokeBlocking(() =>
                {
                    Import(device, targetPlatform, context);
                });
            }
            else
            {
                Import(device, targetPlatform, context);
            }
        }

        public static void Import(IGraphicsDevice device, TargetPlatform targetPlatform, ImportContext context)
        {
            var name = Path.GetFileName(context.SourcePath);
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

            SourceAssetsDatabase.ThumbnailCache.GenerateAndSetThumbnail(context.AssetMetadata.Guid, image);

            ExportImage(device, targetPlatform, context, name, settings, image);
        }

        internal static void ExportImage(IGraphicsDevice device, TargetPlatform targetPlatform, ImportContext context, string name, TextureImporterSettings settings, IScratchImage image)
        {
            Logger.Info($"Importing texture '{Path.GetFileName(context.SourcePath)}'");

            var metadata = image.Metadata;

            var width = Math.Min(metadata.Width, settings.MaxWidth);
            var height = Math.Min(metadata.Height, settings.MaxHeight);

            if (width != metadata.Width || height != metadata.Height)
            {
                try
                {
                    Logger.Info($"Resizing texture ({image.Metadata.Width}, {image.Metadata.Height}) -> ({width}, {height})");
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

            if ((generateMipMaps || changeFormat) && FormatHelper.IsCompressed(metadata.Format))
            {
                try
                {
                    Logger.Info($"Decompressing texture ({metadata.Format}) -> ({Format.R32G32B32A32Float})");
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
                    Logger.Info($"Generating mip-maps");
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
                            Logger.Info($"Compressing texture ({image.Metadata.Format}) -> ({settings.Format})");
                            Application.MainWindow.Dispatcher.InvokeBlocking(() =>
                            {
                                SwapImage(ref image, image.Compress(device, settings.Format, settings.BC7Quick ? TexCompressFlags.BC7Quick | TexCompressFlags.Parallel : TexCompressFlags.Parallel));
                            });
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
                        Logger.Info($"Converting texture ({image.Metadata.Format}) -> ({settings.Format})");
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

            Logger.Info($"Imported texture '{Path.GetFileName(context.SourcePath)}'");
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