namespace HexaEngine.Core.Graphics.Textures
{
    using HexaEngine.Core.Graphics;
    using System;

    /// <summary>
    /// Describes metadata for a texture.
    /// </summary>
    public struct TexMetadata : IEquatable<TexMetadata>
    {
        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the texture. Should be 1 for 1D textures.
        /// </summary>
        public int Height;

        /// <summary>
        /// The depth of the texture. Should be 1 for 1D or 2D textures.
        /// </summary>
        public int Depth;

        /// <summary>
        /// The array size of the texture. For cubemaps, this is a multiple of 6.
        /// </summary>
        public int ArraySize;

        /// <summary>
        /// The number of mip levels in the texture.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// Miscellaneous flags for the texture.
        /// </summary>
        public TexMiscFlags MiscFlags;


        /// <summary>
        /// The alpha mode of the texture.
        /// </summary>
        public TexAlphaMode AlphaMode;

        /// <summary>
        /// The format of the texture.
        /// </summary>
        public Format Format;

        /// <summary>
        /// The dimension of the texture.
        /// </summary>
        public TexDimension Dimension;

        /// <summary>
        /// Computes the index for a specific mip, item, and slice in the texture.
        /// </summary>
        /// <param name="mip">The mip level.</param>
        /// <param name="item">The item in the array.</param>
        /// <param name="slice">The slice in the array or depth in the volume.</param>
        /// <returns>The computed index, or -1 if invalid.</returns>
        public readonly int ComputeIndex(int mip, int item, int slice)
        {
            if (mip >= MipLevels)
            {
                return -1;
            }

            switch (Dimension)
            {
                case TexDimension.Texture1D:
                case TexDimension.Texture2D:
                    if (slice > 0)
                    {
                        return -1;
                    }

                    if (item >= ArraySize)
                    {
                        return -1;
                    }

                    return item * MipLevels + mip;

                case TexDimension.Texture3D:
                    if (item > 0)
                    {
                        // No support for arrays of volumes
                        return -1;
                    }
                    else
                    {
                        int index = 0;
                        int d = Depth;

                        for (int level = 0; level < mip; ++level)
                        {
                            index += d;
                            if (d > 1)
                            {
                                d >>= 1;
                            }
                        }

                        if (slice >= d)
                        {
                            return -1;
                        }

                        index += slice;

                        return index;
                    }

                default:
                    return -1;
            }
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is TexMetadata metadata && Equals(metadata);
        }

        /// <inheritdoc/>
        public readonly bool Equals(TexMetadata other)
        {
            return Width == other.Width &&
                   Height == other.Height &&
                   Depth == other.Depth &&
                   ArraySize == other.ArraySize &&
                   MipLevels == other.MipLevels &&
                   MiscFlags == other.MiscFlags &&
                   AlphaMode == other.AlphaMode &&
                   Format == other.Format &&
                   Dimension == other.Dimension;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(Depth);
            hash.Add(ArraySize);
            hash.Add(MipLevels);
            hash.Add(MiscFlags);
            hash.Add(AlphaMode);
            hash.Add(Format);
            hash.Add(Dimension);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Checks if the texture is a cubemap.
        /// </summary>
        /// <returns><c>true</c> if the texture is a cubemap; otherwise, <c>false</c>.</returns>
        public readonly bool IsCubemap()
        {
            return (MiscFlags & TexMiscFlags.TextureCube) != 0;
        }

        /// <summary>
        /// Checks if the alpha mode is premultiplied.
        /// </summary>
        /// <returns><c>true</c> if the alpha mode is premultiplied; otherwise, <c>false</c>.</returns>
        public readonly bool IsPMAlpha()
        {
            return AlphaMode == TexAlphaMode.Premultiplied;
        }

        /// <summary>
        /// Checks if the texture is a volumemap.
        /// </summary>
        /// <returns><c>true</c> if the texture is a volumemap; otherwise, <c>false</c>.</returns>
        public readonly bool IsVolumemap()
        {
            return Dimension == TexDimension.Texture3D;
        }

        /// <summary>
        /// Equality operator for comparing two <see cref="TexMetadata"/> instances.
        /// </summary>
        public static bool operator ==(TexMetadata left, TexMetadata right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for comparing two <see cref="TexMetadata"/> instances.
        /// </summary>
        public static bool operator !=(TexMetadata left, TexMetadata right)
        {
            return !(left == right);
        }
    }
}