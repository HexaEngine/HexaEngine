namespace HexaEngine.Core.IO.Textures.Raw
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;

    /// <summary>
    /// Represents a raw image with a header including width, height, and format information.
    /// </summary>
    public unsafe class RawImage : IDisposable
    {
        private RawHeader header;
        private byte* data;
        private int rowPitch;
        private int byteCount;
        private int pixelCount;
        private bool disposedValue;

        /// <summary>
        /// Gets or sets the width of the raw image in pixels.
        /// </summary>
        public int Width
        {
            get => header.Width;
            set
            {
                header.Width = value;
                Recalculate();
            }
        }

        /// <summary>
        /// Gets or sets the height of the raw image in pixels.
        /// </summary>
        public int Height
        {
            get => header.Height;
            set
            {
                header.Height = value;
                Recalculate();
            }
        }

        /// <summary>
        /// Gets or sets the format of the raw image data.
        /// </summary>
        public Format Format
        {
            get => header.Format;
            set
            {
                header.Format = value;
                Recalculate();
            }
        }

        /// <summary>
        /// Gets a pointer to the raw image data.
        /// </summary>
        public byte* Data => data;

        /// <summary>
        /// Gets a <see cref="Span{T}"/> representing the raw image data.
        /// </summary>
        public Span<byte> Span => new(data, byteCount);

        /// <summary>
        /// Gets the row pitch of the raw image.
        /// </summary>
        public int RowPitch => rowPitch;

        /// <summary>
        /// Gets the byte count of the raw image.
        /// </summary>
        public int ByteCount => byteCount;

        /// <summary>
        /// Gets the pixel count of the raw image.
        /// </summary>
        public int PixelCount => pixelCount;

        /// <summary>
        /// Recalculates the row pitch, byte count, and pixel count based on the current header and format.
        /// </summary>
        private void Recalculate()
        {
            rowPitch = 0;
            int slicePitch = 0;
            FormatHelper.ComputePitch(header.Format, header.Width, header.Height, ref rowPitch, ref slicePitch, CPFlags.None);
            byteCount = rowPitch * header.Height;
            pixelCount = header.Width * header.Height;
        }

        /// <summary>
        /// Writes the raw image to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="dst">The destination <see cref="Stream"/> to write to.</param>
        public void Write(Stream dst)
        {
            header.Write(dst);
            Span<byte> span = new(data, byteCount);
            dst.Write(span);
        }

        /// <summary>
        /// Saves the raw image to a file with the specified filename.
        /// </summary>
        /// <param name="filename">The filename to save the raw image to.</param>
        public void SaveToFile(string filename)
        {
            var fs = File.Create(filename);
            Write(fs);
            fs.Close();
        }

        /// <summary>
        /// Reads a raw image from the specified path using an external file.
        /// </summary>
        /// <param name="path">The path to the raw image file.</param>
        /// <returns>A <see cref="RawImage"/> object containing the raw image data.</returns>
        public static RawImage ReadFromExternal(string path)
        {
            return Read(File.OpenRead(path));
        }

        /// <summary>
        /// Reads a raw image from the specified path using the file system.
        /// </summary>
        /// <param name="path">The path to the raw image file.</param>
        /// <returns>A <see cref="RawImage"/> object containing the raw image data.</returns>
        public static RawImage ReadFrom(string path)
        {
            return Read(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Reads a raw image from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="src">The source <see cref="Stream"/> to read from.</param>
        /// <returns>A <see cref="RawImage"/> object containing the raw image data.</returns>
        public static RawImage Read(Stream src)
        {
            RawImage image = new();
            image.header = RawHeader.ReadFrom(src);

            image.Recalculate();

            image.data = AllocT<byte>(image.byteCount);
            Span<byte> span = new(image.data, image.byteCount);
            src.Read(span);
            return image;
        }

        /// <summary>
        /// Captures the raw image from a graphics device and a 2D texture.
        /// </summary>
        /// <param name="device">The graphics device to use for capturing.</param>
        /// <param name="texture">The 2D texture to capture from.</param>
        public void Capture(IGraphicsDevice device, ITexture2D texture)
        {
            var context = device.Context;
            var desc = texture.Description;
            desc.CPUAccessFlags = CpuAccessFlags.Read;
            desc.BindFlags = BindFlags.None;
            desc.Usage = Usage.Staging;

            var staging = device.CreateTexture2D(desc);

            context.CopyResource(staging, texture);

            var mapped = context.Map(staging, 0, MapMode.Read, MapFlags.None);
            header.Format = desc.Format;
            header.Width = desc.Width;
            header.Height = desc.Height;

            Recalculate();

            if (data != null)
            {
                Free(data);
            }

            data = AllocT<byte>(byteCount);
            Memcpy(mapped.PData, data, byteCount);

            context.Unmap(staging, 0);
            staging.Dispose();
        }

        /// <summary>
        /// Copies the raw image to a graphics device's 2D texture.
        /// </summary>
        /// <param name="device">The graphics device to use for copying.</param>
        /// <param name="texture">The target 2D texture to copy to.</param>
        public void CopyTo(IGraphicsDevice device, ITexture2D texture)
        {
            var context = device.Context;
            var desc = texture.Description;
            desc.CPUAccessFlags = CpuAccessFlags.Write;
            desc.BindFlags = BindFlags.None;
            desc.Usage = Usage.Staging;

            if (desc.Width != header.Width || desc.Height != header.Height || desc.Format != header.Format)
            {
                if (desc.Width != header.Width)
                    throw new ArgumentException($"Texture does not match width must be {header.Width} but is given {desc.Width}", nameof(texture));
                if (desc.Height != header.Height)
                    throw new ArgumentException($"Texture does not match height must be {header.Height} but is given {desc.Height}", nameof(texture));
                if (desc.Format != header.Format)
                    throw new ArgumentException($"Texture does not match format must be {header.Format} but is given {desc.Format}", nameof(texture));
            }

            var staging = device.CreateTexture2D(desc);
            var mapped = context.Map(staging, 0, MapMode.WriteDiscard, MapFlags.None);

            Memcpy(data, mapped.PData, byteCount);

            context.Unmap(staging, 0);

            context.CopyResource(texture, staging);

            staging.Dispose();
        }

        /// <summary>
        /// Creates a 2D texture from the raw image data with the specified parameters.
        /// </summary>
        /// <param name="device">The graphics device to create the texture with.</param>
        /// <param name="bindFlags">The bind flags for the new texture.</param>
        /// <param name="usage">The usage for the new texture.</param>
        /// <param name="cpuAccessFlags">The CPU access flags for the new texture.</param>
        /// <param name="sampleCount">The sample count for the new texture.</param>
        /// <param name="sampleQuality">The sample quality for the new texture.</param>
        /// <param name="miscFlag">The miscellaneous flags for the new texture.</param>
        /// <returns>A 2D texture created from the raw image data.</returns>
        public ITexture2D CreateTexture(IGraphicsDevice device, BindFlags bindFlags, Usage usage, CpuAccessFlags cpuAccessFlags, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            Texture2DDescription description = new(header.Format, header.Width, header.Height, 1, 1, bindFlags, usage, cpuAccessFlags, sampleCount, sampleQuality, miscFlag);

            SubresourceData subresourceData = new(data, rowPitch);

            ITexture2D texture = device.CreateTexture2D(description, [subresourceData]);

            return texture;
        }

        /// <summary>
        /// Copies the raw image data from an <see cref="IScratchImage"/>.
        /// </summary>
        /// <param name="image">The source <see cref="IScratchImage"/> containing the raw image data.</param>
        public void CopyFrom(IScratchImage image)
        {
            var metadata = image.Metadata;

            var src = image.GetImages()[0];
            header.Format = metadata.Format;
            header.Width = metadata.Width;
            header.Height = metadata.Height;

            Recalculate();

            if (data != null)
            {
                Free(data);
            }

            data = AllocT<byte>(byteCount);
            Memcpy(src.Pixels, data, byteCount);
        }

        /// <summary>
        /// Frees the allocated resources, including pixel data.
        /// </summary>
        /// <param name="disposing">A flag indicating whether the method is being called from the explicit method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free(data);
                data = null;
                header = default;
                rowPitch = 0;
                byteCount = 0;
                pixelCount = 0;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RawImage"/> class.
        /// </summary>
        ~RawImage()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="RawImage"/> instance.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}