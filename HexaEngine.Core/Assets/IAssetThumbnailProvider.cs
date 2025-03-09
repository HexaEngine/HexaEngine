using Hexa.NET.Logging;
using HexaEngine.Core.Graphics;
using System.Diagnostics.CodeAnalysis;

namespace HexaEngine.Core.Assets
{
    public enum ThumbnailResultCode
    {
        /// <summary>
        /// Thumbnail creation failed due to an error.
        /// </summary>
        Fail = -1,

        /// <summary>
        /// Thumbnail creation succeeded.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Thumbnail creation was skipped because it could not be created for the provided asset.
        /// </summary>
        Skip = 1,
    }

    public interface IAssetThumbnailProvider
    {
        bool CanCreate(ReadOnlySpan<char> fileExtension, SourceAssetMetadata metadata);

        IScratchImage? CreateThumbnail(SourceAssetMetadata metadata, ILogger? logger = null);

        public ThumbnailResultCode TryCreateThumbnail(ReadOnlySpan<char> fileExtension, SourceAssetMetadata metadata, [NotNullWhen(true)] out IScratchImage? thumbnail, ILogger? logger = null)
        {
            thumbnail = null;
            if (CanCreate(fileExtension, metadata))
            {
                try
                {
                    thumbnail = CreateThumbnail(metadata);
                    return thumbnail != null ? ThumbnailResultCode.Success : ThumbnailResultCode.Fail;
                }
                catch (Exception ex)
                {
                    logger?.Error($"Failed to create thumbnail for '{metadata.Guid}, {metadata.FilePath}'");
                    logger?.Log(ex);
                    thumbnail?.Dispose();
                    thumbnail = null;
                    return ThumbnailResultCode.Fail;
                }
            }

            return ThumbnailResultCode.Skip;
        }
    }
}