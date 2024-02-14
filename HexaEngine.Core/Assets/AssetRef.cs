﻿namespace HexaEngine.Core.Assets
{
    using HexaEngine.Core.IO;
    using System;

    public struct AssetRef : IEquatable<AssetRef>
    {
        public Guid Guid;

        public static readonly AssetRef Empty = new(Guid.Empty);

        public AssetRef(Guid guid)
        {
            Guid = guid;
        }

        public readonly Stream? OpenRead()
        {
            var metadata = GetMetadata();
            if (metadata == null)
            {
                return null;
            }

            var path = metadata.Path;
            if (path == null)
            {
                return null;
            }
            return FileSystem.OpenRead(path);
        }

        public readonly byte[]? GetBytes()
        {
            var path = GetPath();
            if (path == null)
            {
                return null;
            }
            return FileSystem.ReadAllBytes(path);
        }

        public readonly string? GetPath()
        {
            return GetMetadata()?.Path;
        }

        public readonly Artifact? GetMetadata()
        {
            return ArtifactDatabase.GetArtifact(Guid);
        }

        public readonly SourceAssetMetadata? GetSourceMetadata()
        {
            var artifactMetadata = GetMetadata();
            if (artifactMetadata == null)
            {
                return null;
            }

            return SourceAssetsDatabase.GetMetadata(artifactMetadata.SourceGuid);
        }

        public override readonly string ToString()
        {
            var path = GetPath();

            if (path == null)
            {
                return $"{Guid}";
            }
            else
            {
                return $"{path} ({Guid})";
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is AssetRef @ref && Equals(@ref);
        }

        public bool Equals(AssetRef other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Guid);
        }

        public static implicit operator Guid(AssetRef assetRef)
        {
            return assetRef.Guid;
        }

        public static implicit operator AssetRef(Guid guid)
        {
            return new(guid);
        }

        public static bool operator ==(AssetRef left, AssetRef right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssetRef left, AssetRef right)
        {
            return !(left == right);
        }
    }
}