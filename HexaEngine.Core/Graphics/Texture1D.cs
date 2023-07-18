namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public unsafe class Texture1D : ITexture1D
    {
        private readonly string dbgName;
        private Texture1DDescription description;
        private Format format;
        private int width;
        private int mipLevels;
        private int arraySize;
        private CpuAccessFlags cpuAccessFlags;
        private GpuAccessFlags gpuAccessFlags;
        private ResourceMiscFlag miscFlag;
        private bool canWrite;
        private bool canRead;
        private ITexture1D texture;
        private IShaderResourceView? srv;
        private IRenderTargetView? rtv;
        private bool isDirty;
        private bool disposedValue;
        private int rowPitch;
        private int slicePitch;
        private byte* local;

        public Texture1D(IGraphicsDevice device, TextureFileDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            texture = device.TextureLoader.LoadTexture1D(description.Path);
            texture.DebugName = dbgName;
            this.description = texture.Description;
            format = this.description.Format;
            width = this.description.Width;
            mipLevels = this.description.MipLevels;
            arraySize = this.description.ArraySize;
            cpuAccessFlags = this.description.CPUAccessFlags;
            gpuAccessFlags = this.description.Usage switch
            {
                Usage.Default => GpuAccessFlags.RW,
                Usage.Dynamic => GpuAccessFlags.Read,
                Usage.Staging => GpuAccessFlags.None,
                Usage.Immutable => GpuAccessFlags.Read,
                _ => throw new NotImplementedException(),
            };
            miscFlag = this.description.MiscFlags;

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture, new(width, 1));
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

        public Texture1D(IGraphicsDevice device, Format format, int width, int height, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            this.format = format;
            this.width = width;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            description = new(format, width, arraySize, mipLevels, BindFlags.ShaderResource | BindFlags.RenderTarget, Usage.Default, cpuAccessFlags, miscFlag);

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
                description.BindFlags = BindFlags.ShaderResource;
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
                description.BindFlags = BindFlags.None;
                canRead = true;
            }

            FormatHelper.ComputePitch(format, width, height, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * height);
                ZeroMemory(local, rowPitch * height);
            }

            texture = device.CreateTexture1D(description);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture, new(width, height));
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

        public Texture1D(IGraphicsDevice device, Texture1DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = GpuAccessFlags.None;
            miscFlag = description.MiscFlags;
            this.description = description;

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            texture = device.CreateTexture1D(description);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture, new(width, 1));
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

        public Texture1D(IGraphicsDevice device, Texture1DDescription description, SubresourceData[] initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = GpuAccessFlags.None;
            miscFlag = description.MiscFlags;
            this.description = description;

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            texture = device.CreateTexture1D(description, initialData);
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture, new(width, 1));
                rtv.DebugName = dbgName + ".RTV";
            }
            MemoryManager.Register(texture);
        }

        public Texture1D(IGraphicsDevice device, Texture1DDescription description, SubresourceData initialData, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            dbgName = $"Texture1D: {Path.GetFileNameWithoutExtension(filename)}, Line:{lineNumber}";
            format = description.Format;
            width = description.Width;
            mipLevels = description.MipLevels;
            arraySize = description.ArraySize;
            cpuAccessFlags = description.CPUAccessFlags;
            gpuAccessFlags = GpuAccessFlags.None;
            miscFlag = description.MiscFlags;
            this.description = description;

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }

            texture = device.CreateTexture1D(description, new SubresourceData[] { initialData });
            texture.DebugName = dbgName;

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            ArraySlices = new IRenderTargetView[description.ArraySize];

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture, new(width, 1));
                rtv.DebugName = dbgName + ".RTV";

                for (int i = 0; i < description.ArraySize; i++)
                {
                    ArraySlices[i] = device.CreateRenderTargetView(texture, new RenderTargetViewDescription(texture, arraySize > 1, firstArraySlice: i, arraySize: 1), new(width, 1));
                    ArraySlices[i].DebugName = dbgName + $".RTV.{i}";
                }
            }
            MemoryManager.Register(texture);
        }

        public static Task<Texture1D> CreateTextureAsync(IGraphicsDevice device, TextureFileDescription description)
        {
            return Task.Factory.StartNew((object? state) =>
            {
                var data = ((IGraphicsDevice, TextureFileDescription))state;
                return new Texture1D(data.Item1, data.Item2);
            }, (device, description));
        }

        public ResourceDimension Dimension => ResourceDimension.Texture1D;

        public Texture1DDescription Description => description;

        public Format Format => format;

        public int Width => width;

        public int MipLevels => mipLevels;

        public int ArraySize => arraySize;

        public CpuAccessFlags CpuAccessFlags => cpuAccessFlags;

        public GpuAccessFlags GpuAccessFlags => gpuAccessFlags;

        public ResourceMiscFlag MiscFlag => miscFlag;

        public bool CanWrite => canWrite;

        public bool CanRead => canRead;

        public int RowPitch => rowPitch;

        public int SlicePitch => slicePitch;

        public void* Local => local;

        public IShaderResourceView? SRV => srv;

        public IRenderTargetView? RTV => rtv;

        public IRenderTargetView[] ArraySlices;

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
            int y = (int)(uv.Y * 1);
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
                Memcpy(local, mapped.PData, (uint)1 * mapped.RowPitch, (uint)1 * mapped.RowPitch);
                context.Unmap(texture, subresource);
            }
            else
            {
                var mapped = context.Map(texture, subresource, MapMode.WriteDiscard, MapFlags.None);
                Memcpy(local, mapped.PData, (uint)1 * mapped.RowPitch, (uint)1 * mapped.RowPitch);
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
            Memcpy(mapped.PData, local, (uint)1 * mapped.RowPitch, (uint)1 * mapped.RowPitch);
            context.Unmap(texture, subresource);
        }

        public void Resize(IGraphicsDevice device, Format format, int width, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.Read, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            this.format = format;
            this.width = width;
            this.mipLevels = mipLevels;
            this.arraySize = arraySize;
            this.cpuAccessFlags = cpuAccessFlags;
            this.gpuAccessFlags = gpuAccessFlags;
            this.miscFlag = miscFlag;
            description = new(format, width, arraySize, mipLevels, BindFlags.ShaderResource | BindFlags.RenderTarget, Usage.Default, cpuAccessFlags, miscFlag);

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0 && (gpuAccessFlags & GpuAccessFlags.Read) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot read at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0 && (gpuAccessFlags & GpuAccessFlags.Write) != 0)
            {
                throw new ArgumentException("Cpu and Gpu cannot write at the same time");
            }

            if ((cpuAccessFlags & CpuAccessFlags.Write) != 0)
            {
                description.Usage = Usage.Dynamic;
                description.BindFlags = BindFlags.ShaderResource;
                canWrite = true;
            }

            if ((cpuAccessFlags & CpuAccessFlags.Read) != 0)
            {
                description.Usage = Usage.Staging;
                description.BindFlags = BindFlags.None;
                canRead = true;
            }

            FormatHelper.ComputePitch(format, width, 1, ref rowPitch, ref slicePitch, Textures.CPFlags.None);

            if (cpuAccessFlags != CpuAccessFlags.None)
            {
                local = (byte*)Alloc(rowPitch * 1);
                ZeroMemory(local, rowPitch * 1);
            }
            texture.Dispose();
            srv?.Dispose();
            rtv?.Dispose();
            MemoryManager.Unregister(texture);
            texture = device.CreateTexture1D(description);
            texture.DebugName = dbgName;
            MemoryManager.Register(texture);

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                srv = device.CreateShaderResourceView(texture);
                srv.DebugName = dbgName + ".SRV";
            }

            if ((description.BindFlags & BindFlags.RenderTarget) != 0)
            {
                rtv = device.CreateRenderTargetView(texture, new(width, 1));
                rtv.DebugName = dbgName + ".RTV";
            }
        }

        public void CopyTo(IGraphicsContext context, ITexture2D texture)
        {
            context.CopyResource(texture, this.texture);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                MemoryManager.Unregister(texture);
                texture.Dispose();
                srv?.Dispose();
                rtv?.Dispose();
                if (cpuAccessFlags != CpuAccessFlags.None)
                    Free(local);
                disposedValue = true;

                disposedValue = true;
            }
        }

        ~Texture1D()
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