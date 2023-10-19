namespace HexaEngine.Core.IO.Textures.Raw
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;

    public unsafe class RawImage : IDisposable
    {
        private RawHeader header;
        private byte* data;
        private int rowPitch;
        private int byteCount;
        private int pixelCount;
        private bool disposedValue;

        public int Width
        {
            get => header.Width;
            set
            {
                header.Width = value;
                Recalculate();
            }
        }

        public int Height
        {
            get => header.Height;
            set
            {
                header.Height = value;
                Recalculate();
            }
        }

        public Format Format
        {
            get => header.Format;
            set
            {
                header.Format = value;
                Recalculate();
            }
        }

        public byte* Data => data;

        public Span<byte> Span => new(data, byteCount);

        public int RowPitch => rowPitch;

        public int ByteCount => byteCount;

        public int PixelCount => pixelCount;

        private void Recalculate()
        {
            rowPitch = 0;
            int slicePitch = 0;
            FormatHelper.ComputePitch(header.Format, header.Width, header.Height, ref rowPitch, ref slicePitch, CPFlags.None);
            byteCount = rowPitch * header.Height;
            pixelCount = header.Width * header.Height;
        }

        public void Write(Stream dst)
        {
            header.Write(dst);
            Span<byte> span = new(data, byteCount);
            dst.Write(span);
        }

        public void SaveToFile(string filename)
        {
            var fs = File.Create(filename);
            Write(fs);
            fs.Close();
        }

        public static RawImage ReadFromExternal(string path)
        {
            return Read(File.OpenRead(path));
        }

        public static RawImage ReadFrom(string path)
        {
            return Read(FileSystem.Open(path));
        }

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

        public ITexture2D CreateTexture(IGraphicsDevice device, BindFlags bindFlags, Usage usage, CpuAccessFlags cpuAccessFlags, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            Texture2DDescription description = new(header.Format, header.Width, header.Height, 1, 1, bindFlags, usage, cpuAccessFlags, sampleCount, sampleQuality, miscFlag);

            SubresourceData subresourceData = new(data, rowPitch);

            ITexture2D texture = device.CreateTexture2D(description, [subresourceData]);

            return texture;
        }

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

        ~RawImage()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}