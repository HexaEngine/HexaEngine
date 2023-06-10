namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using Silk.NET.SDL;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe class RWTexture2D : ITexture2D, IDisposable
    {
        private readonly string dbgName;
        private Texture2DDescription description;
        private readonly Format format;
        private readonly int width;
        private readonly int height;
        private readonly int mipLevels;
        private readonly int arraySize;
        private readonly CpuAccessFlags cpuAccessFlags;
        private readonly ResourceMiscFlag miscFlag;
        private readonly bool canWrite;
        private readonly bool canRead;
        private readonly ITexture2D texture;
        private readonly IShaderResourceView? srv;
        private readonly int rowPitch;
        private readonly int slicePitch;
        private readonly byte* local;
        private bool disposedValue;
        private bool isDirty;

        public RWTexture2D(IGraphicsDevice device, Format format, int width, int height, int mipLevels, int arraySize, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"RWTexture2D: {filename}, Line:{lineNumber}";
            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.cpuAccessFlags = cpuAccessFlags;
            this.miscFlag = miscFlag;
            description = new()
            {
                Format = format,
                Width = width,
                Height = height,
                MipLevels = mipLevels,
                ArraySize = arraySize,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = cpuAccessFlags,
                MiscFlags = miscFlag,
                SampleDescription = SampleDescription.Default,
                Usage = Usage.Default,
            };

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
                description.BindFlags = BindFlags.None;
                canRead = true;
            }

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            local = (byte*)Alloc(rowPitch * height);
            Zero(local, rowPitch * height);
            texture = device.CreateTexture2D(description, null);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }
        }

        public ResourceDimension Dimension => ResourceDimension.Texture2D;

        public Texture2DDescription Description => description;

        public Format Format => format;

        public int Width => width;

        public int Height => height;

        public int MipLevels => mipLevels;

        public int ArraySize => arraySize;

        public CpuAccessFlags CpuAccessFlags => cpuAccessFlags;

        public ResourceMiscFlag MiscFlag => miscFlag;

        public bool CanWrite => canWrite;

        public bool CanRead => canRead;

        public int RowPitch => rowPitch;

        public int SlicePitch => slicePitch;

        public void* Local => local;

        public IShaderResourceView? SRV => srv;

        public nint NativePointer => texture.NativePointer;

        public string? DebugName { get => texture.DebugName; set => texture.DebugName = value; }

        public bool IsDisposed => texture.IsDisposed;

        public event EventHandler? OnDisposed
        {
            add
            {
                texture.OnDisposed += value;
            }

            remove
            {
                texture.OnDisposed -= value;
            }
        }

        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((byte*)local)[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                *(byte*)local = value;
                isDirty = true;
            }
        }

        public byte this[Vector2 index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((byte*)local)[ComputeIndex(index)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                *(byte*)local = value;
                isDirty = true;
            }
        }

        public int ComputeIndex(Vector2 uv)
        {
            int x = (int)(uv.X * Width);
            int y = (int)(uv.Y * Height);
            return x + y * RowPitch;
        }

        public bool Write(IGraphicsContext context)
        {
            if (!canWrite)
            {
                throw new InvalidOperationException();
            }

            if (isDirty)
            {
                for (int i = 0; i < mipLevels; i++)
                {
                    for (int j = 0; j < arraySize; j++)
                    {
                        Write(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j));
                    }
                }

                isDirty = false;
                return true;
            }
            return false;
        }

        public void Write(IGraphicsContext context, int subresource)
        {
            if (!canWrite)
            {
                throw new InvalidOperationException();
            }
            if (canRead)
            {
                var mapped = context.Map(texture, subresource, MapMode.Write, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
            else
            {
                var mapped = context.Map(texture, subresource, MapMode.WriteDiscard, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
        }

        public void Read(IGraphicsContext context)
        {
            if (!canRead)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < mipLevels; i++)
            {
                for (int j = 0; j < arraySize; j++)
                {
                    Read(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j));
                }
            }
        }

        public void Read(IGraphicsContext context, int subresource)
        {
            if (!canRead)
            {
                throw new InvalidOperationException();
            }

            var mapped = context.Map(texture, subresource, MapMode.Read, MapFlags.None);
            Memcpy(mapped.PData, local, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
            context.Unmap(texture, subresource);
        }

        public void CopyTo(IGraphicsContext context, ITexture2D texture)
        {
            context.CopyResource(texture, this.texture);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                texture.Dispose();
                srv?.Dispose();
                Free(local);
                disposedValue = true;
            }
        }

        ~RWTexture2D()
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

    public unsafe class RWTexture2D<T> : ITexture2D, IDisposable where T : unmanaged
    {
        private readonly string dbgName;
        private Texture2DDescription description;
        private readonly Format format;
        private readonly int width;
        private readonly int height;
        private readonly int mipLevels;
        private readonly int arraySize;
        private readonly bool canWrite;
        private readonly bool canRead;
        private readonly ITexture2D texture;
        private readonly IShaderResourceView? srv;
        private readonly int rowPitch;
        private readonly int slicePitch;
        private readonly T* local;
        private bool disposedValue;
        private bool isDirty;

        public RWTexture2D(IGraphicsDevice device, Format format, int width, int height, int mipLevels, int arraySize, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"RWTexture2D: {filename}, Line:{lineNumber}";
            this.format = format;
            this.width = width;
            this.height = height;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            description = new()
            {
                Format = format,
                Width = width,
                Height = height,
                MipLevels = mipLevels,
                ArraySize = arraySize,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = cpuAccessFlags,
                MiscFlags = miscFlag,
                SampleDescription = SampleDescription.Default,
                Usage = Usage.Default,
            };

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
                description.BindFlags = BindFlags.None;
                canRead = true;
            }

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            local = (T*)Alloc(rowPitch * height);
            Zero(local, rowPitch * height);
            texture = device.CreateTexture2D(description, null);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }
        }

        public ResourceDimension Dimension => ResourceDimension.Texture2D;

        public Texture2DDescription Description => description;

        public Format Format => format;

        public int Width => width;

        public int Height => height;

        public int MipLevels => mipLevels;

        public int ArraySize => arraySize;

        public bool CanWrite => canWrite;

        public bool CanRead => canRead;

        public int RowPitch => rowPitch;

        public int SlicePitch => slicePitch;

        public T* Local => local;

        public IShaderResourceView? SRV => srv;

        public nint NativePointer => texture.NativePointer;

        public string? DebugName { get => texture.DebugName; set => texture.DebugName = value; }

        public bool IsDisposed => texture.IsDisposed;

        public event EventHandler? OnDisposed
        {
            add
            {
                texture.OnDisposed += value;
            }

            remove
            {
                texture.OnDisposed -= value;
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => local[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                local[index] = value;
                isDirty = true;
            }
        }

        public T this[Vector2 index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => local[ComputeIndex(index)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                local[ComputeIndex(index)] = value;
                isDirty = true;
            }
        }

        public int ComputeIndex(Vector2 uv)
        {
            int x = (int)(uv.X * Width);
            int y = (int)(uv.Y * Height);
            return x + y * (RowPitch / sizeof(T));
        }

        public bool Write(IGraphicsContext context)
        {
            if (!canWrite)
            {
                throw new InvalidOperationException();
            }

            if (isDirty)
            {
                for (int i = 0; i < mipLevels; i++)
                {
                    for (int j = 0; j < arraySize; j++)
                    {
                        Write(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j));
                    }
                }

                isDirty = false;
                return true;
            }
            return false;
        }

        public void Write(IGraphicsContext context, int subresource)
        {
            if (!canWrite)
            {
                throw new InvalidOperationException();
            }
            if (canRead)
            {
                var mapped = context.Map(texture, subresource, MapMode.Write, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
            else
            {
                var mapped = context.Map(texture, subresource, MapMode.WriteDiscard, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
        }

        public void Read(IGraphicsContext context)
        {
            if (!canRead)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < mipLevels; i++)
            {
                for (int j = 0; j < arraySize; j++)
                {
                    Read(context, TextureHelper.ComputeSubresourceIndex2D(mipLevels, arraySize, i, j));
                }
            }
        }

        public void Read(IGraphicsContext context, int subresource)
        {
            if (!canRead)
            {
                throw new InvalidOperationException();
            }

            var mapped = context.Map(texture, subresource, MapMode.Read, MapFlags.None);
            Memcpy(mapped.PData, local, (uint)description.Height * mapped.RowPitch, (uint)description.Height * mapped.RowPitch);
            context.Unmap(texture, subresource);
        }

        public void CopyTo(IGraphicsContext context, ITexture2D texture)
        {
            context.CopyResource(texture, this.texture);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                texture.Dispose();
                srv?.Dispose();
                Free(local);
                disposedValue = true;
            }
        }

        ~RWTexture2D()
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