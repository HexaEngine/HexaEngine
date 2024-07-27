namespace HexaEngine.Core.IO.Binary.Terrains
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using System.IO;
    using System.Numerics;

    /// <summary>
    /// Represents a layer blend mask used for terrain rendering.
    /// </summary>
    public class LayerBlendMask
    {
        private uint width;
        private uint height;
        private ulong[] data;
        private long streamPosition;
        private Endianness endianness;
        private Compression compression;

#nullable disable

        private LayerBlendMask()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerBlendMask"/> class with the specified width, height, and data.
        /// </summary>
        /// <param name="width">The width of the layer mask.</param>
        /// <param name="height">The height of the layer mask.</param>
        /// <param name="data">The data of the layer mask.</param>
        public LayerBlendMask(uint width, uint height, ulong[] data)
        {
            this.width = width;
            this.height = height;
            this.data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerBlendMask"/> class with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the layer mask.</param>
        /// <param name="height">The height of the layer mask.</param>
        public LayerBlendMask(uint width = 1024, uint height = 1024)
        {
            this.width = width;
            this.height = height;
            data = new ulong[width * height];
        }

        public ulong[] Data => data;

        /// <summary>
        /// Gets or sets the layer mask data at the specified index.
        /// </summary>
        /// <param name="index">The index of the layer mask data.</param>
        /// <returns>The layer mask data at the specified index.</returns>
        public unsafe Vector4 this[uint index]
        {
            get
            {
                return DecodePixel(data[index]);
            }
            set
            {
                data[index] = EncodePixel(value);
            }
        }

        /// <summary>
        /// Gets or sets the layer mask data at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The layer mask data at the specified coordinates.</returns>
        public unsafe Vector4 this[uint x, uint y]
        {
            get
            {
                return DecodePixel(data[GetIndexFor(x, y)]);
            }
            set
            {
                data[GetIndexFor(x, y)] = EncodePixel(value);
            }
        }

        /// <summary>
        /// Gets the width of the height map.
        /// </summary>
        public uint Width => width;

        /// <summary>
        /// Gets the height of the height map.
        /// </summary>
        public uint Height => height;

        public void Set(uint index, ulong value)
        {
            data[index] = value;
        }

        public static ulong EncodePixel(Vector4 pixel)
        {
            ulong encodedPixel = 0;
            encodedPixel |= (ulong)(pixel.X * 65535.0f);
            encodedPixel |= (ulong)(pixel.Y * 65535.0f) << 16;
            encodedPixel |= (ulong)(pixel.Z * 65535.0f) << 32;
            encodedPixel |= (ulong)(pixel.W * 65535.0f) << 48;
            return encodedPixel;
        }

        public static Vector4 DecodePixel(ulong encodedPixel)
        {
            ushort r = (ushort)encodedPixel;
            ushort g = (ushort)(encodedPixel >> 16);
            ushort b = (ushort)(encodedPixel >> 32);
            ushort a = (ushort)(encodedPixel >> 48);
            return new Vector4(r, g, b, a) / 65535.0f;
        }

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public uint GetIndexFor(uint x, uint y)
        {
            return y * width + x;
        }

        /// <summary>
        /// Reads a <see cref="LayerBlendMask"/> from a stream.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        /// <param name="compression"></param>
        /// <param name="mode"></param>
        /// <returns>The read height map.</returns>
        public static LayerBlendMask ReadFrom(Stream src, Endianness endianness, Compression compression, TerrainLoadMode mode)
        {
            LayerBlendMask layerMask = new();
            layerMask.Read(src, endianness, compression, mode);
            return layerMask;
        }

        /// <summary>
        /// Reads the layer mask data from a stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        /// <param name="compression"></param>
        /// <param name="mode"></param>
        public void Read(Stream stream, Endianness endianness, Compression compression, TerrainLoadMode mode)
        {
            this.endianness = endianness;
            this.compression = compression;
            width = stream.ReadUInt32(endianness);
            height = stream.ReadUInt32(endianness);
            uint size = stream.ReadUInt32(endianness);
            streamPosition = stream.Position;

            if (mode == TerrainLoadMode.Immediate)
            {
                ReadMaskData(stream);
            }

            stream.Position = streamPosition + size;
        }

        public void ReadMaskData(Stream stream)
        {
            if (data != null)
            {
                return;
            }

            stream.Position = streamPosition;
            var decompressor = stream.CreateDecompressionStream(compression, out var isCompressed);

            data = new ulong[width * height];
            decompressor.ReadArrayUInt64(data, endianness);

            if (isCompressed)
            {
                decompressor.Dispose();
            }
        }

        /// <summary>
        /// Writes the layer mask data to a stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        /// <param name="endianness">The endianness used for writing.</param>
        /// <param name="compression"></param>
        public void Write(Stream stream, Endianness endianness, Compression compression)
        {
            stream.WriteUInt32(width, endianness);
            stream.WriteUInt32(height, endianness);
            long basePosition = stream.Position;
            stream.Position += 4;

            var compressor = stream.CreateCompressionStream(compression, out var isCompressed);

            compressor.WriteArrayUInt64(data, endianness);

            if (isCompressed)
            {
                compressor.Dispose();
            }

            long endPos = stream.Position;
            uint size = (uint)(endPos - (basePosition + 4));
            stream.Position = basePosition;
            stream.WriteUInt32(size, endianness);
            stream.Position = endPos;
        }

        /// <summary>
        /// Writes the layer mask data to a texture.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="gpuAccessFlags">The GPU access flags.</param>
        /// <param name="cpuAccessFlags">The CPU access flags.</param>
        public unsafe Texture2D CreateLayerMask(IGraphicsDevice device, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None)
        {
            Texture2DDescription desc = new(Format.R16G16B16A16UNorm, (int)width, (int)height, 1, 1, gpuAccessFlags, cpuAccessFlags);
            Texture2D layerMask;

            fixed (ulong* pData = data)
            {
                layerMask = new(desc, new SubresourceData(pData, (int)(width * sizeof(ulong))));
            }

            return layerMask;
        }

        /// <summary>
        /// Creates a staging <see cref="Texture2D"/> from the layer mask.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="accessFlags">The CPU access flags.</param>
        /// <returns>The created staging layer mask texture.</returns>
        public unsafe Texture2D CreateStagingLayerMask(IGraphicsDevice device, CpuAccessFlags accessFlags = CpuAccessFlags.None)
        {
            Texture2DDescription desc = new(Format.R16G16B16A16UNorm, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, Usage.Staging, accessFlags);
            Texture2D heightMap;
            fixed (ulong* pData = data)
            {
                heightMap = new(desc, new SubresourceData(pData, (int)(width * sizeof(ulong))));
            }

            return heightMap;
        }

        /// <summary>
        /// Writes the layer mask data to a texture.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="texture">The destination texture.</param>
        public unsafe void WriteLayerMask(IGraphicsContext context, Texture2D texture)
        {
            if ((texture.CpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                uint bufferSize = height * (uint)texture.RowPitch;
                byte* buffer = AllocT<byte>(bufferSize);

                var pixel = (float*)buffer;
                for (int i = 0; i < data.Length; i++)
                {
                    pixel[i] = data[i];
                }
                texture.Write(context, buffer, bufferSize);

                Free(buffer);
            }
            else
            {
                var staging = CreateStagingLayerMask(context.Device, CpuAccessFlags.None);
                staging.CopyTo(context, texture);
                staging.Dispose();
            }
        }

        /// <summary>
        /// Reads the layer mask data from a texture.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="texture">The source texture.</param>
        public unsafe void ReadLayerMask(IGraphicsContext context, Texture2D texture)
        {
            data = new ulong[texture.Width * texture.Height];
            if ((texture.CpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                uint bufferSize = height * (uint)texture.RowPitch;
                byte* buffer = AllocT<byte>(bufferSize);

                texture.Read(context, buffer, bufferSize);

                var pixel = (ulong*)buffer;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = pixel[i];
                }

                Free(buffer);
            }
            else
            {
                Texture2D staging = new(Format.R16G16B16A16UNorm, (int)width, (int)height, 1, 1, CpuAccessFlags.Read, GpuAccessFlags.None);

                texture.CopyTo(context, staging);

                uint bufferSize = height * (uint)texture.RowPitch;
                byte* buffer = AllocT<byte>(bufferSize);

                staging.Read(context, buffer, bufferSize);
                staging.Dispose();

                var pixel = (ulong*)buffer;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = pixel[i];
                }

                Free(buffer);
            }
        }
    }
}