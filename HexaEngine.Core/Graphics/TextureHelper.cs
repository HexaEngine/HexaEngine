namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class TextureHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeMipLevels(int width, int height)
        {
            return (int)MathF.Log2(MathF.Max(width, height));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex(TextureDimension dimension, int mipLevels, int arraySize, int depth, int mip, int item, int slice)
        {
            if (mip >= mipLevels)
                return (-1);

            switch (dimension)
            {
                case TextureDimension.Texture1D:
                case TextureDimension.Texture2D:
                    if (slice > 0)
                        return (-1);

                    if (item >= arraySize)
                        return (-1);

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
                                d >>= 1;
                        }

                        if (slice >= d)
                            return (-1);

                        index += slice;

                        return index;
                    }

                default:
                    return (-1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex1D(int mipLevels, int arraySize, int mip, int item)
        {
            if (mip >= mipLevels)
                return (-1);

            if (item >= arraySize)
                return (-1);

            return (item * (mipLevels) + mip);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex2D(int mipLevels, int arraySize, int mip, int item)
        {
            if (mip >= mipLevels)
                return (-1);

            if (item >= arraySize)
                return (-1);

            return (item * (mipLevels) + mip);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSubresourceIndex3D(int mipLevels, int depth, int mip, int item, int slice)
        {
            if (mip >= mipLevels)
                return (-1);

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
                        d >>= 1;
                }

                if (slice >= d)
                    return (-1);

                index += slice;

                return index;
            }
        }
    }
}