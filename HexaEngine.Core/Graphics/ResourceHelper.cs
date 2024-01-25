namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// A helper class for calculating resource sizes.
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>
        /// Computes the size of a <see cref="IResource"/>
        /// </summary>
        /// <param name="resource">The <see cref="IResource"/> resource to compute the size for.</param>
        /// <returns>The size in bytes.</returns>
        public static long ComputeSize(IResource resource)
        {
            if (resource is IBuffer buffer)
            {
                return (long)ComputeSize(buffer);
            }

            if (resource is ITexture1D texture1d)
            {
                return (long)ComputeSize(texture1d);
            }

            if (resource is ITexture2D texture2d)
            {
                return (long)ComputeSize(texture2d);
            }

            if (resource is ITexture3D texture3d)
            {
                return (long)ComputeSize(texture3d);
            }

            return -1;
        }

        /// <summary>
        /// Computes the size of an <see cref="IBuffer"/>
        /// </summary>
        /// <param name="buffer">The <see cref="IBuffer"/> resource to compute the size for.</param>
        /// <returns>The size in bytes.</returns>
        public static ulong ComputeSize(IBuffer buffer)
        {
            BufferDescription description = buffer.Description;
            ulong size = (ulong)description.ByteWidth;

            return size;
        }

        /// <summary>
        /// Computes the size of an <see cref="ITexture1D"/>
        /// </summary>
        /// <param name="texture">The <see cref="ITexture1D"/> resource to compute the size for.</param>
        /// <returns>The size in bytes.</returns>
        public static ulong ComputeSize(ITexture1D texture)
        {
            Texture1DDescription description = texture.Description;
            ulong stride = (ulong)(FormatHelper.BitsPerPixel(description.Format) / 8);
            ulong size = 0;
            ulong width = (ulong)description.Width;
            ulong arraySize = (ulong)description.ArraySize;
            for (int i = 0; i < description.MipLevels; i++)
            {
                size += stride * width * arraySize;
                width /= 2;
            }

            return size;
        }

        /// <summary>
        /// Computes the size of an <see cref="ITexture2D"/>
        /// </summary>
        /// <param name="texture">The <see cref="ITexture2D"/> resource to compute the size for.</param>
        /// <returns>The size in bytes.</returns>
        public static ulong ComputeSize(ITexture2D texture)
        {
            Texture2DDescription description = texture.Description;
            ulong stride = (ulong)(FormatHelper.BitsPerPixel(description.Format) / 8);
            ulong size = 0;
            ulong width = (ulong)description.Width;
            ulong height = (ulong)description.Height;
            ulong arraySize = (ulong)description.ArraySize;
            for (int i = 0; i < description.MipLevels; i++)
            {
                size += stride * width * height * arraySize;
                width /= 2;
                height /= 2;
            }

            return size;
        }

        /// <summary>
        /// Computes the size of an <see cref="ITexture3D"/>
        /// </summary>
        /// <param name="texture">The <see cref="ITexture3D"/> resource to compute the size for.</param>
        /// <returns>The size in bytes.</returns>
        public static ulong ComputeSize(ITexture3D texture)
        {
            Texture3DDescription description = texture.Description;
            ulong stride = (ulong)(FormatHelper.BitsPerPixel(description.Format) / 8);
            ulong size = 0;
            ulong width = (ulong)description.Width;
            ulong height = (ulong)description.Height;
            ulong depth = (ulong)description.Depth;
            for (int i = 0; i < description.MipLevels; i++)
            {
                size += stride * width * height * depth;
                width /= 2;
                height /= 2;
                depth /= 2;
            }

            return size;
        }
    }
}