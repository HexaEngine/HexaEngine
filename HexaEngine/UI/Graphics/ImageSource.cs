namespace HexaEngine.UI.Graphics
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Web;
    using System.Numerics;

    public class ImageSource : IDisposable
    {
        private bool disposedValue;
        private readonly Texture2D texture;
        private readonly bool hasOwnership;

        public ImageSource(Texture2D texture, bool transferOwnership = false)
        {
            this.texture = texture;
            hasOwnership = transferOwnership;
        }

        public Format Format => texture.Format;

        public int Width => texture.Width;

        public int Height => texture.Height;

        public Texture2D Texture => texture;

        public void DrawImage(UICommandList commandList, Vector2 origin, Vector2 size)
        {
            commandList.PrimReserve(6, 4);
            commandList.PrimRect(origin, origin + size, Vector2.Zero, Vector2.One, uint.MaxValue);
            commandList.RecordDraw(UICommandType.DrawTexture, null, texture.SRV.NativePointer);
        }

        public void DrawImage(UICommandList commandList, Vector2 origin, Vector2 size, Vector2 uv0, Vector2 uv1)
        {
            commandList.PrimReserve(6, 4);
            commandList.PrimRect(origin, origin + size, uv0, uv1, uint.MaxValue);
            commandList.RecordDraw(UICommandType.DrawTexture, null, texture.SRV.NativePointer);
        }

        public void DrawImage(UICommandList commandList, Vector2 origin, Vector2 size, Vector2 uv0, Vector2 uv1, Vector4 tint)
        {
            commandList.PrimReserve(6, 4);
            commandList.PrimRect(origin, origin + size, uv0, uv1, tint.Col4ToUInt());
            commandList.RecordDraw(UICommandType.DrawTexture, null, texture.SRV.NativePointer);
        }

        public static ImageSource CreateFromFile(string path)
        {
            Texture2D texture = new(new TextureFileDescription(path, mipLevels: 1, usage: Usage.Immutable, bindFlags: BindFlags.ShaderResource));
            return new(texture, true);
        }

        public static unsafe ImageSource CreateFromMemory(byte* data, int width, int height, Format format)
        {
            ulong rowPitch = 0;
            ulong slicePitch = 0;
            FormatHelper.ComputePitch(format, (ulong)width, (ulong)height, ref rowPitch, ref slicePitch, Core.Graphics.Textures.CPFlags.None);
            Texture2D texture = new(new Texture2DDescription(format, width, height, 1, 1, GpuAccessFlags.Read), new SubresourceData((nint)data, (int)rowPitch));
            return new(texture, true);
        }

        public static unsafe ImageSource CreateFromMemory(Span<byte> data, int width, int height, Format format)
        {
            Texture2D texture;
            ulong rowPitch = 0;
            ulong slicePitch = 0;
            FormatHelper.ComputePitch(format, (ulong)width, (ulong)height, ref rowPitch, ref slicePitch, Core.Graphics.Textures.CPFlags.None);
            fixed (byte* dataPtr = data)
            {
                texture = new(new Texture2DDescription(format, width, height, 1, 1, GpuAccessFlags.Read), new SubresourceData((nint)dataPtr, (int)rowPitch));
            }
            return new(texture, true);
        }

        public static ImageSource CreateFromUri(Uri uri)
        {
            using HttpClient client = new();
            using MemoryStream stream = new();

            client.DownloadAsyncCached(uri, stream).Wait();

            stream.Position = 0;

            var image = Application.GraphicsDevice.TextureLoader.LoadFromMemory(Path.GetFileName(uri.AbsolutePath), stream);
            Texture2D texture = new(new Texture2DDescription(Format.R32G32B32A32Float, 1, 1, 1, 1, GpuAccessFlags.Read), image);
            image.Dispose();

            return new ImageSource(texture, true);
        }

        public static async Task<ImageSource> CreateFromUriAsync(Uri uri)
        {
            using HttpClient client = new();
            using MemoryStream stream = new();

            await client.DownloadAsyncCached(uri, stream);

            var data = stream.ToArray();

            var image = Application.GraphicsDevice.TextureLoader.LoadFromMemory(Path.GetFileName(uri.AbsolutePath), stream);
            Texture2D texture = new(new Texture2DDescription(Format.R32G32B32A32Float, 1, 1, 1, 1, GpuAccessFlags.Read), image);
            image.Dispose();

            return new ImageSource(texture, true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (hasOwnership)
                {
                    texture.Dispose();
                }

                disposedValue = true;
            }
        }

        ~ImageSource()
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