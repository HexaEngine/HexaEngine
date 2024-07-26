namespace HexaEngine.Core.IO.Textures
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.Mathematics;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents a class for building texture atlases.
    /// </summary>
    public unsafe class AtlasBuilder : IDisposable
    {
        private byte* data;
        private int baseHeight;
        private Format format;
        private int byteStride;
        private int atlasWidth;
        private int atlasHeight;
        private int penX;
        private int penY;
        private int maxHeightInRow;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtlasBuilder"/> class.
        /// </summary>
        /// <param name="baseWidth">The base width of the atlas.</param>
        /// <param name="baseHeight">The base height of the atlas.</param>
        /// <param name="format">The pixel format of the atlas.</param>
        public AtlasBuilder(int baseWidth, int baseHeight, Format format)
        {
            this.baseHeight = baseHeight;
            atlasWidth = baseWidth;
            atlasHeight = baseHeight;
            this.format = format;
            byteStride = (int)MathF.Ceiling(FormatHelper.BitsPerPixel(format) / 8f);
            data = AllocT<byte>(baseWidth * baseHeight * byteStride);
        }

        /// <summary>
        /// Appends image data to the atlas.
        /// </summary>
        /// <param name="data">Pointer to the image data.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <returns>The position where the image is appended.</returns>
        public Point2 Append(byte* data, int width, int height)
        {
            int nextPenX = width + penX;
            while (penY + height > atlasHeight)
            {
                IncreaseCapacity();
            }
            if (nextPenX > atlasWidth)
            {
                int nextPenY = maxHeightInRow + penY;
                while (nextPenY >= atlasHeight)
                {
                    IncreaseCapacity();
                }
                maxHeightInRow = 0;
                penY = nextPenY;
                penX = 0;
                nextPenX = width;
            }

            maxHeightInRow = Math.Max(maxHeightInRow, height);

            CopyToBuffer(data, width, height);
            Point2 position = new(penX, penY);
            penX = nextPenX;

            return position;
        }

        /// <summary>
        /// Appends image data from an <see cref="IScratchImage"/> to the atlas.
        /// </summary>
        /// <param name="image">The image to append.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <returns>The position where the image is appended.</returns>
        public Point2 Append(IScratchImage image, int width, int height)
        {
            bool owns = false;
            var metadata = image.Metadata;
            if (metadata.Format != format)
            {
                if (FormatHelper.IsCompressed(metadata.Format))
                {
                    Swap(ref owns, ref image, image.Decompress(format));
                }
                else
                {
                    Swap(ref owns, ref image, image.Convert(format, 0));
                }
            }
            if (metadata.Width != width || metadata.Height != height)
            {
                Swap(ref owns, ref image, image.Resize(width, height, 0));
            }

            var img = image.GetImages()[0];
            var pos = Append(img.Pixels, width, height);
            if (owns)
            {
                image.Dispose();
            }
            return pos;
        }

        /// <summary>
        /// Appends image data to the atlas.
        /// </summary>
        /// <param name="data">Image data as byte array.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <returns>The position where the image is appended.</returns>
        public Point2 Append(byte[] data, int width, int height)
        {
            fixed (byte* pData = data)
            {
                return Append(pData, width, height);
            }
        }

        /// <summary>
        /// Appends image data to the atlas.
        /// </summary>
        /// <param name="data">Image data as <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <returns>The position where the image is appended.</returns>
        public Point2 Append(ReadOnlySpan<byte> data, int width, int height)
        {
            fixed (byte* pData = data)
            {
                return Append(pData, width, height);
            }
        }

        /// <summary>
        /// Builds the atlas and retrieves its properties.
        /// </summary>
        /// <param name="ppData">Pointer to the atlas data.</param>
        /// <param name="pWidth">Width of the atlas.</param>
        /// <param name="pHeight">Height of the atlas.</param>
        /// <param name="pRowPitch">Row pitch of the atlas.</param>
        public void Build(byte** ppData, int* pWidth, int* pHeight, int* pRowPitch)
        {
            Trim();
            *ppData = data;
            *pWidth = atlasWidth;
            *pHeight = atlasHeight;
            *pRowPitch = atlasWidth * byteStride;
        }

        private void IncreaseCapacity()
        {
            atlasHeight += baseHeight;
            data = ReAllocT(data, atlasWidth * atlasHeight * byteStride);
        }

        private void Trim()
        {
            atlasHeight = penY + maxHeightInRow;
            data = ReAllocT(data, atlasWidth * atlasHeight * byteStride);
        }

        /// <summary>
        /// Resets the pen position to the top-left corner of the atlas.
        /// </summary>
        public void Reset()
        {
            penX = 0;
            penY = 0;
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

        private void CopyToBuffer(byte* image, int width, int height)
        {
            int upperBound = atlasWidth * atlasHeight * byteStride;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xPos = x + penX;
                    int yPos = y + penY;
                    int pixelIndexAtlas = (yPos * atlasWidth + xPos) * byteStride;
                    int pixelTexIndex = (y * width + x) * byteStride;

                    for (int c = 0; c < byteStride; c++)
                    {
                        int bufferIndex = pixelIndexAtlas + c;
                        int texIndex = pixelTexIndex + c;
                        if (bufferIndex >= upperBound)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        data[bufferIndex] = image[texIndex];
                    }
                }
            }
        }

        public void SetBuffer(Format format, int width, int height, byte* data, Point2 pen, int maxHeightInRow)
        {
            baseHeight = height;
            atlasWidth = width;
            atlasHeight = height;
            this.format = format;
            byteStride = (int)MathF.Ceiling(FormatHelper.BitsPerPixel(format) / 8f);
            if (this.data != null)
            {
                Free(this.data);
            }

            this.data = AllocT<byte>(width * baseHeight * byteStride);
            new Span<byte>(data, width * baseHeight * byteStride).CopyTo(new Span<byte>(this.data, width * baseHeight * byteStride));
            penX = pen.X;
            penY = pen.Y;
            this.maxHeightInRow = maxHeightInRow;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free(data);
                data = null;
                atlasHeight = 0;
                penX = 0; penY = 0;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="AtlasBuilder"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}