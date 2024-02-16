namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents a layer mask used for terrain rendering.
    /// </summary>
    public class LayerMask
    {
        private uint width;
        private uint height;
        private ushort[] data;

#nullable disable

        private LayerMask()
        {
        }

#nullable restore

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerMask"/> class with the specified width, height, and data.
        /// </summary>
        /// <param name="width">The width of the layer mask.</param>
        /// <param name="height">The height of the layer mask.</param>
        /// <param name="data">The data of the layer mask.</param>
        public LayerMask(uint width, uint height, ushort[] data)
        {
            this.width = width;
            this.height = height;
            this.data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerMask"/> class with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the layer mask.</param>
        /// <param name="height">The height of the layer mask.</param>
        public LayerMask(uint width = 1024, uint height = 1024)
        {
            this.width = width;
            this.height = height;
            data = new ushort[width * height * 4];
        }

        /// <summary>
        /// Gets or sets the layer mask data at the specified index.
        /// </summary>
        /// <param name="index">The index of the layer mask data.</param>
        /// <returns>The layer mask data at the specified index.</returns>
        public unsafe Vector4 this[uint index]
        {
            get
            {
                var idx = index * 4;
                fixed (ushort* pData = data)
                {
                    return new Vector4(pData[idx], pData[idx + 1], pData[idx + 2], pData[idx + 3]) / 65535.0f;
                }
            }
            set
            {
                var idx = index * 4;
                fixed (ushort* pData = data)
                {
                    pData[idx] = (ushort)(value.X * 65535.0f);
                    pData[idx + 1] = (ushort)(value.Y * 65535.0f);
                    pData[idx + 2] = (ushort)(value.Z * 65535.0f);
                    pData[idx + 3] = (ushort)(value.W * 65535.0f);
                }
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
                var idx = GetIndexFor(x, y);
                fixed (ushort* pData = data)
                {
                    return new Vector4(pData[idx], pData[idx + 1], pData[idx + 2], pData[idx + 3]) / 65535.0f;
                }
            }
            set
            {
                var idx = GetIndexFor(x, y);
                fixed (ushort* pData = data)
                {
                    pData[idx] = (ushort)(value.X * 65535.0f);
                    pData[idx + 1] = (ushort)(value.Y * 65535.0f);
                    pData[idx + 2] = (ushort)(value.Z * 65535.0f);
                    pData[idx + 3] = (ushort)(value.W * 65535.0f);
                }
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

        /// <summary>
        /// Gets the index for the specified coordinates in the terrain.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The index.</returns>
        public uint GetIndexFor(uint x, uint y)
        {
            return y * width + x * 4;
        }

        /// <summary>
        /// Reads a <see cref="LayerMask"/> from a stream.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        /// <returns>The read height map.</returns>
        public static LayerMask ReadFrom(Stream src, Endianness endianness)
        {
            LayerMask layerMask = new();
            layerMask.Read(src, endianness);
            return layerMask;
        }

        /// <summary>
        /// Reads the layer mask data from a stream.
        /// </summary>
        /// <param name="src">The source stream.</param>
        /// <param name="endianness">The endianness used for reading.</param>
        public void Read(Stream src, Endianness endianness)
        {
            width = src.ReadUInt32(endianness);
            height = src.ReadUInt32(endianness);
            data = new ushort[width * height * 4];

            for (uint i = 0; i < data.Length; i++)
            {
                data[i] = src.ReadUInt16(endianness);
            }
        }

        /// <summary>
        /// Writes the layer mask data to a stream.
        /// </summary>
        /// <param name="dst">The destination stream.</param>
        /// <param name="endianness">The endianness used for writing.</param>
        public void Write(Stream dst, Endianness endianness)
        {
            dst.WriteUInt32(width, endianness);
            dst.WriteUInt32(height, endianness);

            for (uint i = 0; i < data.Length; i++)
            {
                dst.WriteUInt16(data[i], endianness);
            }
        }

        /// <summary>
        /// Writes the layer mask data to a texture.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="gpuAccessFlags">The GPU access flags.</param>
        /// <param name="cpuAccessFlags">The CPU access flags.</param>
        public unsafe Texture2D CreateLayerMask(IGraphicsDevice device, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None)
        {
            Texture2DDescription desc = new(Format.R16G16B16A16Float, (int)width, (int)height, 1, 1, gpuAccessFlags, cpuAccessFlags);
            Texture2D layerMask;

            fixed (ushort* pData = data)
            {
                layerMask = new(device, desc, new SubresourceData(pData, (int)(width * sizeof(ushort) * 4)));
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
            Texture2DDescription desc = new(Format.R16G16B16A16Float, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, Usage.Staging, accessFlags);
            Texture2D heightMap;
            fixed (ushort* pData = data)
            {
                heightMap = new(device, desc, new SubresourceData(pData, (int)(width * sizeof(float) * 4)));
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
                var pixel = (float*)texture.Local;
                for (int i = 0; i < data.Length; i++)
                {
                    pixel[i] = data[i];
                }
                texture.Write(context);
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
            data = new ushort[texture.Width * texture.Height * 4];
            if ((texture.CpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                texture.Read(context);
                var pixel = (ushort*)texture.Local;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = pixel[i];
                }
            }
            else
            {
                Texture2DDescription desc = new(Format.R16G16B16A16Float, (int)width, (int)height, 1, 1, BindFlags.ShaderResource, Usage.Staging, CpuAccessFlags.Read);
                Texture2D staging = new(context.Device, desc);

                texture.CopyTo(context, staging);

                staging.Read(context);
                var pixel = (ushort*)texture.Local;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = pixel[i];
                }
                staging.Dispose();
            }
        }
    }
}