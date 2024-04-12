namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A static class containing helper methods for working with textures.
    /// </summary>
    public static class TextureHelper
    {
        /// <summary>
        /// Computes the number of mip levels for a texture with the given width and height.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <returns>The computed number of mip levels.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeMipLevels(int width, int height)
        {
            return (int)MathF.Log2(MathF.Max(width, height));
        }

        /// <summary>
        /// Computes the subresource index for a texture based on its dimension and parameters.
        /// </summary>
        /// <param name="dimension">The dimension of the texture.</param>
        /// <param name="mipLevels">The number of mip levels.</param>
        /// <param name="arraySize">The size of the array.</param>
        /// <param name="depth">The depth of the texture (for 3D textures).</param>
        /// <param name="mip">The mip level.</param>
        /// <param name="item">The array item.</param>
        /// <param name="slice">The array slice (for 3D textures).</param>
        /// <returns>The computed subresource index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex(TextureDimension dimension, int mipLevels, int arraySize, int depth, int mip, int item, int slice)
        {
            if (mip >= mipLevels)
            {
                return (-1);
            }

            switch (dimension)
            {
                case TextureDimension.Texture1D:
                case TextureDimension.Texture2D:
                    if (slice > 0)
                    {
                        return (-1);
                    }

                    if (item >= arraySize)
                    {
                        return (-1);
                    }

                    return (item * (mipLevels) + mip);

                case TextureDimension.Texture3D:
                    if (item > 0)
                    {
                        // No support for arrays of volumes
                        return (-1);
                    }
                    else
                    {
                        int index = 0;
                        int d = depth;

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
                            return (-1);
                        }

                        index += slice;

                        return index;
                    }

                default:
                    return (-1);
            }
        }

        /// <summary>
        /// Computes the subresource index for a 1D texture based on its parameters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex1D(int mipLevels, int arraySize, int mip, int item)
        {
            if (mip >= mipLevels)
            {
                return (-1);
            }

            if (item >= arraySize)
            {
                return (-1);
            }

            return (item * (mipLevels) + mip);
        }

        /// <summary>
        /// Computes the subresource index for a 2D texture based on its parameters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex2D(int mipLevels, int arraySize, int mip, int item)
        {
            if (mip >= mipLevels)
            {
                return (-1);
            }

            if (item >= arraySize)
            {
                return (-1);
            }

            return (item * (mipLevels) + mip);
        }

        /// <summary>
        /// Computes the subresource index for a 3D texture based on its parameters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex3D(int mipLevels, int depth, int mip, int item, int slice)
        {
            if (mip >= mipLevels)
            {
                return (-1);
            }

            if (item > 0)
            {
                // No support for arrays of volumes
                return (-1);
            }
            else
            {
                int index = 0;
                int d = depth;

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
                    return (-1);
                }

                index += slice;

                return index;
            }
        }

        /// <summary>
        /// Converts CpuAccessFlags and GpuAccessFlags to Usage and BindFlags
        /// </summary>
        /// <param name="cpuAccessFlags">The CpuAccessFlags for cpu access.</param>
        /// <param name="gpuAccessFlags">The GpuAccessFlags for gpu access.</param>
        /// <param name="usage">The converted Usage.</param>
        /// <param name="bindFlags">The converted BindFlags.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Convert(CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, out Usage usage, out BindFlags bindFlags)
        {
            usage = 0;
            bindFlags = 0;

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if (cpuAccessFlags != CpuAccessFlags.None && (gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot use rw with uva at the same time");
            }

            if (gpuAccessFlags.HasFlag(GpuAccessFlags.DepthStencil) && gpuAccessFlags.HasFlag(GpuAccessFlags.Write))
            {
                throw new ArgumentException("Cannot bind depth stencil as render target at the same time.");
            }

            if ((gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.ShaderResource;
            }

            if ((gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.RenderTarget;
            }

            if ((gpuAccessFlags & GpuAccessFlags.UA) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.UnorderedAccess;
            }

            if ((gpuAccessFlags & GpuAccessFlags.DepthStencil) != 0)
            {
                usage = Usage.Default;
                bindFlags |= BindFlags.DepthStencil;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                usage = Usage.Dynamic;
                bindFlags = BindFlags.ShaderResource;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                usage = Usage.Staging;
                bindFlags = BindFlags.None;
            }
        }
    }
}