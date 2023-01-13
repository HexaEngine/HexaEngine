namespace HexaEngine.D3D11
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.DirectXTex;
    using HexaEngine.IO;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using Format = Core.Graphics.Format;
    using Query = Core.Graphics.Query;
    using ResourceMiscFlag = Core.Graphics.ResourceMiscFlag;
    using SubresourceData = Core.Graphics.SubresourceData;
    using D3D11SubresourceData = Silk.NET.Direct3D11.SubresourceData;
    using Usage = Core.Graphics.Usage;
    using Viewport = Mathematics.Viewport;
    using BepuPhysics.Trees;

    public unsafe partial class D3D11GraphicsDevice : IGraphicsDevice
    {
        internal readonly D3D11 D3D11;
        private bool disposedValue;

        public ID3D11Device1* Device;
        public ID3D11DeviceContext1* DeviceContext;
#if DEBUG
        internal ID3D11Debug* Debug;
#endif

        [SupportedOSPlatform("windows")]
        public D3D11GraphicsDevice(DXGIAdapter adapter, SdlWindow? window)
        {
            D3D11 = D3D11.GetApi();

#if D3D11On12

#else
            D3DFeatureLevel[] levelsArr = new D3DFeatureLevel[]
            {
                D3DFeatureLevel.Level121,
                D3DFeatureLevel.Level120,
                D3DFeatureLevel.Level111,
                D3DFeatureLevel.Level110
            };

            CreateDeviceFlag flags = CreateDeviceFlag.BgraSupport;

#if DEBUG
            flags |= CreateDeviceFlag.Debug;
#endif
            ID3D11Device* tempDevice;
            ID3D11DeviceContext* tempContext;

            D3DFeatureLevel level = 0;
            D3DFeatureLevel* levels = (D3DFeatureLevel*)Unsafe.AsPointer(ref levelsArr[0]);

            ResultCode code = (ResultCode)D3D11.CreateDevice((IDXGIAdapter*)adapter.IDXGIAdapter, D3DDriverType.Unknown, IntPtr.Zero, (uint)flags, levels, 4, D3D11.SdkVersion, &tempDevice, &level, &tempContext);

            ID3D11Device1* device;
            ID3D11DeviceContext1* context;
            tempDevice->QueryInterface(Utils.Guid(ID3D11Device1.Guid), (void**)&device);
            tempContext->QueryInterface(Utils.Guid(ID3D11DeviceContext1.Guid), (void**)&context);
            Device = device;
            DeviceContext = context;
            tempDevice->Release();
            tempContext->Release();

            NativePointer = new(device);

#if DEBUG
            ID3D11Debug* debug;
            Device->QueryInterface(Utils.Guid(ID3D11Debug.Guid), (void**)&debug);
            Debug = debug;
#endif
#endif
            Context = new D3D11GraphicsContext(this);

            if (window == null) return;

            SwapChain = adapter.CreateSwapChainForWindow(this, window);
        }

        public IGraphicsContext Context { get; }

        public IntPtr NativePointer { get; }

        public string? DebugName { get; set; } = string.Empty;

        public bool IsDisposed => disposedValue;

        public event EventHandler? OnDisposed;

        public ISwapChain? SwapChain { get; }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc)
        {
            return new ComputePipeline(this, desc);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc)
        {
            return new GraphicsPipeline(this, desc);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, ShaderMacro[] macros)
        {
            return new GraphicsPipeline(this, desc, macros);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions)
        {
            return new GraphicsPipeline(this, desc, elementDescriptions);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros)
        {
            return new GraphicsPipeline(this, desc, inputElements, macros);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state)
        {
            return new GraphicsPipeline(this, desc, state);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            return new GraphicsPipeline(this, desc, state, macros);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] elementDescriptions)
        {
            return new GraphicsPipeline(this, desc, state, elementDescriptions);
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros)
        {
            return new GraphicsPipeline(this, desc, state, inputElements, macros);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer CreateBuffer(BufferDescription description)
        {
            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);
            Device->CreateBuffer(&desc, null, &buffer).ThrowHResult();
            return new D3D11Buffer(buffer, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : struct
        {
            if (description.ByteWidth == 0)
            {
                description.ByteWidth = Marshal.SizeOf<T>();
            }

            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);

            var data = Alloc(description.ByteWidth);
            Marshal.StructureToPtr(value, (nint)data, true);
            D3D11SubresourceData* bufferData = Alloc(new D3D11SubresourceData(data, (uint)description.ByteWidth));

            Device->CreateBuffer(&desc, bufferData, &buffer).ThrowHResult();
            Free(bufferData);
            Free(data);

            return new D3D11Buffer(buffer, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : struct
        {
            BufferDescription description = new(0, bindFlags, usage, cpuAccessFlags, miscFlags);
            return CreateBuffer(value, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged
        {
            uint size = (uint)(sizeof(T) * count);
            ID3D11Buffer* buffer;
            description.ByteWidth = (int)size;
            BufferDesc desc = Helper.Convert(description);
            var data = Helper.Convert(new SubresourceData(values, description.ByteWidth));
            Device->CreateBuffer(&desc, &data, &buffer).ThrowHResult();
            return new D3D11Buffer(buffer, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer CreateBuffer<T>(T* values, uint count, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            BufferDescription description = new(0, bindFlags, usage, cpuAccessFlags, miscFlags);
            return CreateBuffer(values, count, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDepthStencilView CreateDepthStencilView(IResource resource)
        {
            DepthStencilViewDescription description;
            if (resource is ITexture1D texture1d)
            {
                description = new(texture1d, texture1d.Description.ArraySize > 1);
            }
            else if (resource is ITexture2D texture2d)
            {
                DepthStencilViewDimension dimension;
                if (texture2d.Description.ArraySize > 1)
                {
                    if (texture2d.Description.SampleDescription.Count > 1)
                        dimension = DepthStencilViewDimension.Texture2DMultisampledArray;
                    else
                        dimension = DepthStencilViewDimension.Texture2DArray;
                }
                else
                    dimension = DepthStencilViewDimension.Texture2D;
                description = new(texture2d, dimension);
            }
            else
            {
                throw new NotSupportedException();
            }

            return CreateDepthStencilView(resource, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description)
        {
            ID3D11DepthStencilView* view;
            DepthStencilViewDesc desc = Helper.Convert(description);
            Device->CreateDepthStencilView((ID3D11Resource*)resource.NativePointer, ref desc, &view).ThrowHResult();
            return new D3D11DepthStencilView(view, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IRenderTargetView CreateRenderTargetView(IResource resource, Viewport viewport)
        {
            RenderTargetViewDescription description;
            if (resource is IBuffer)
            {
                throw new NotImplementedException();
            }
            else if (resource is ITexture1D texture1d)
            {
                description = new(texture1d, texture1d.Description.ArraySize > 1);
            }
            else if (resource is ITexture2D texture2d)
            {
                RenderTargetViewDimension dimension;
                if (texture2d.Description.ArraySize > 1)
                {
                    if (texture2d.Description.SampleDescription.Count > 1)
                        dimension = RenderTargetViewDimension.Texture2DMultisampledArray;
                    else
                        dimension = RenderTargetViewDimension.Texture2DArray;
                }
                else
                    dimension = RenderTargetViewDimension.Texture2D;
                description = new(texture2d, dimension);
            }
            else if (resource is ITexture3D texture3d)
            {
                description = new(texture3d);
            }
            else
            {
                throw new NotSupportedException();
            }

            return CreateRenderTargetView(resource, description, viewport);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport)
        {
            ID3D11RenderTargetView* rtv;
            RenderTargetViewDesc desc = Helper.Convert(description);
            Device->CreateRenderTargetView((ID3D11Resource*)resource.NativePointer, &desc, &rtv).ThrowHResult();
            return new D3D11RenderTargetView(rtv, description, viewport);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ISamplerState CreateSamplerState(SamplerDescription description)
        {
            ID3D11SamplerState* sampler;
            SamplerDesc desc = Helper.Convert(description);
            Device->CreateSamplerState(ref desc, &sampler).ThrowHResult();
            return new D3D11SamplerState(sampler, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IShaderResourceView CreateShaderResourceView(IResource resource)
        {
            ShaderResourceViewDescription description;
            if (resource is IBuffer)
            {
                throw new NotImplementedException();
            }
            else if (resource is ITexture1D texture1d)
            {
                description = new(texture1d, texture1d.Description.ArraySize > 1);
            }
            else if (resource is ITexture2D texture2d)
            {
                ShaderResourceViewDimension dimension;
                if (texture2d.Description.ArraySize > 1)
                {
                    if (texture2d.Description.SampleDescription.Count > 1)
                        dimension = ShaderResourceViewDimension.Texture2DMultisampledArray;
                    else
                        dimension = ShaderResourceViewDimension.Texture2DArray;
                }
                else
                    dimension = ShaderResourceViewDimension.Texture2D;
                if (texture2d.Description.MiscFlags.HasFlag(ResourceMiscFlag.TextureCube))
                {
                    dimension = texture2d.Description.ArraySize / 6 > 1 ? ShaderResourceViewDimension.TextureCubeArray : ShaderResourceViewDimension.TextureCube;
                }
                description = new(texture2d, dimension);
            }
            else if (resource is ITexture3D texture3d)
            {
                description = new(texture3d);
            }
            else
            {
                throw new NotSupportedException();
            }

            return CreateShaderResourceView(resource, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IShaderResourceView CreateShaderResourceView(IResource resource, ShaderResourceViewDescription description)
        {
            ID3D11ShaderResourceView* srv;
            ShaderResourceViewDesc desc = Helper.Convert(description);
            Device->CreateShaderResourceView((ID3D11Resource*)resource.NativePointer, &desc, &srv).ThrowHResult();
            return new D3D11ShaderResourceView(srv, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IShaderResourceView CreateShaderResourceView(IBuffer buffer)
        {
            ID3D11ShaderResourceView* srv;
            Device->CreateShaderResourceView((ID3D11Resource*)buffer.NativePointer, null, &srv).ThrowHResult();
            return new D3D11ShaderResourceView(srv, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture1D CreateTexture1D(Texture1DDescription description)
        {
            ID3D11Texture1D* texture;
            Texture1DDesc desc = Helper.Convert(description);
            Device->CreateTexture1D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture1D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[]? subresources)
        {
            ID3D11Texture1D* texture;
            Texture1DDesc desc = Helper.Convert(description);
            if (subresources != null)
            {
                D3D11SubresourceData* data = Alloc<D3D11SubresourceData>(subresources.Length);
                Helper.Convert(subresources, data);
                Device->CreateTexture1D(&desc, data, &texture).ThrowHResult();
                Free(data);
            }
            else
            {
                Device->CreateTexture1D(&desc, null, &texture).ThrowHResult();
            }
            return new D3D11Texture1D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            return CreateTexture1D(format, width, arraySize, mipLevels, subresources, bindFlags, Usage.Default, CpuAccessFlags.None, misc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            Texture1DDescription description = new(format, width, arraySize, mipLevels, bindFlags, usage, cpuAccessFlags, misc);
            ID3D11Texture1D* texture;
            Texture1DDesc desc = Helper.Convert(description);

            if (subresources != null)
            {
                D3D11SubresourceData* data = Alloc<D3D11SubresourceData>(subresources.Length);
                Helper.Convert(subresources, data);
                Device->CreateTexture1D(&desc, data, &texture).ThrowHResult();
                Free(data);
            }
            else
            {
                Device->CreateTexture1D(&desc, null, &texture).ThrowHResult();
            }

            return new D3D11Texture1D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D CreateTexture2D(Texture2DDescription description)
        {
            ID3D11Texture2D* texture;
            Texture2DDesc desc = Helper.Convert(description);
            Device->CreateTexture2D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture2D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[]? subresources)
        {
            ID3D11Texture2D* texture;
            Texture2DDesc desc = Helper.Convert(description);
            if (subresources != null)
            {
                D3D11SubresourceData* data = Alloc<D3D11SubresourceData>(subresources.Length);
                Helper.Convert(subresources, data);
                Device->CreateTexture2D(&desc, data, &texture).ThrowHResult();
                Free(data);
            }
            else
            {
                Device->CreateTexture2D(&desc, null, &texture).ThrowHResult();
            }
            return new D3D11Texture2D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            return CreateTexture2D(format, width, height, arraySize, mipLevels, subresources, bindFlags, Usage.Default, CpuAccessFlags.None, 1, 0, misc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            Texture2DDescription description = new(format, width, height, arraySize, mipLevels, bindFlags, usage, cpuAccessFlags, sampleCount, sampleQuality, misc);
            ID3D11Texture2D* texture;
            Texture2DDesc desc = Helper.Convert(description);

            if (subresources != null)
            {
                D3D11SubresourceData* data = Alloc<D3D11SubresourceData>(subresources.Length);
                Helper.Convert(subresources, data);
                Device->CreateTexture2D(&desc, data, &texture).ThrowHResult();
                Free(data);
            }
            else
            {
                Device->CreateTexture2D(&desc, null, &texture).ThrowHResult();
            }

            return new D3D11Texture2D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture3D CreateTexture3D(Texture3DDescription description)
        {
            ID3D11Texture3D* texture;
            Texture3DDesc desc = Helper.Convert(description);
            Device->CreateTexture3D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture3D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[]? subresources)
        {
            ID3D11Texture3D* texture;
            Texture3DDesc desc = Helper.Convert(description);
            if (subresources != null)
            {
                D3D11SubresourceData* data = Alloc<D3D11SubresourceData>(subresources.Length);
                Helper.Convert(subresources, data);
                Device->CreateTexture3D(&desc, data, &texture).ThrowHResult();
            }
            else
            {
                Device->CreateTexture3D(&desc, null, &texture).ThrowHResult();
            }
            return new D3D11Texture3D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            return CreateTexture3D(format, width, height, depth, mipLevels, subresources, bindFlags, Usage.Default, CpuAccessFlags.None, misc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            Texture3DDescription description = new(format, width, height, depth, mipLevels, bindFlags, usage, cpuAccessFlags, misc);
            ID3D11Texture3D* texture;
            Texture3DDesc desc = Helper.Convert(description);

            if (subresources != null)
            {
                D3D11SubresourceData* data = Alloc<D3D11SubresourceData>(subresources.Length);
                Helper.Convert(subresources, data);
                Device->CreateTexture3D(&desc, data, &texture).ThrowHResult();
            }
            else
            {
                Device->CreateTexture3D(&desc, null, &texture).ThrowHResult();
            }

            return new D3D11Texture3D(texture, description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture1D LoadTexture1D(string path)
        {
            return (ITexture1D)LoadTextureAuto(path, TextureDimension.Texture1D);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture1D LoadTexture1D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            return (ITexture1D)LoadTextureAuto(path, TextureDimension.Texture1D, usage, bind, cpuAccess, misc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D LoadTexture2D(string path)
        {
            return (ITexture2D)LoadTextureAuto(path, TextureDimension.Texture2D);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D LoadTexture2D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            return (ITexture2D)LoadTextureAuto(path, TextureDimension.Texture2D, usage, bind, cpuAccess, misc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture3D LoadTexture3D(string path)
        {
            return (ITexture3D)LoadTextureAuto(path, TextureDimension.Texture3D);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture3D LoadTexture3D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            return (ITexture3D)LoadTextureAuto(path, TextureDimension.Texture3D, usage, bind, cpuAccess, misc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D LoadTextureCube(string path)
        {
            return (ITexture2D)LoadTextureAuto(path, TextureDimension.TextureCube);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITexture2D LoadTextureCube(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            return (ITexture2D)LoadTextureAuto(path, TextureDimension.TextureCube, usage, bind, cpuAccess, misc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IResource LoadTextureAuto(string path, TextureDimension dimension)
        {
            ScratchImage image = LoadAuto(path);
            if (image.pScratchImage == null)
            {
                return InitFallback(dimension);
            }
            TexMetadata metadata = image.GetMetadata();

            ResourceMiscFlag optionFlags = metadata.IsCubemap() ? ResourceMiscFlag.TextureCube : ResourceMiscFlag.None;
            ID3D11Resource* resource;
            DirectXTex.CreateTextureEx((ID3D11Device*)(IntPtr)Device, &image, Silk.NET.Direct3D11.Usage.Immutable, BindFlag.ShaderResource, 0, 0, false, &resource);
            image.Release();
            switch (dimension)
            {
                case TextureDimension.Texture1D:
                    {
                        Texture1DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.ArraySize,
                            (int)metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture1D((ID3D11Texture1D*)resource, texture);
                    }

                case TextureDimension.Texture2D:
                    {
                        Texture2DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.Height,
                            (int)metadata.ArraySize,
                            (int)metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
                    }

                case TextureDimension.Texture3D:
                    {
                        Texture3DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.Height,
                            (int)metadata.Depth,
                            (int)metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture3D((ID3D11Texture3D*)resource, texture);
                    }

                case TextureDimension.TextureCube:
                    {
                        Texture2DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.Height,
                            (int)metadata.Depth,
                            (int)metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IResource LoadTextureAuto(string path, TextureDimension dimension, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            ScratchImage image = LoadAuto(path);
            if (image.pScratchImage == null)
            {
                return InitFallback(dimension);
            }
            TexMetadata metadata = image.GetMetadata();

            ID3D11Resource* resource;
            DirectXTex.CreateTextureEx((ID3D11Device*)(IntPtr)Device, &image, Helper.Convert(usage), Helper.Convert(bind), Helper.Convert(cpuAccess), Helper.Convert(misc), false, &resource);
            image.Release();
            switch (dimension)
            {
                case TextureDimension.Texture1D:
                    {
                        Texture1DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.ArraySize,
                            (int)metadata.MipLevels,
                            miscFlags: misc);
                        return new D3D11Texture1D((ID3D11Texture1D*)resource, texture);
                    }

                case TextureDimension.Texture2D:
                    {
                        Texture2DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.Height,
                            (int)metadata.ArraySize,
                            (int)metadata.MipLevels,
                            miscFlags: misc);
                        return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
                    }

                case TextureDimension.Texture3D:
                    {
                        Texture3DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.Height,
                            (int)metadata.Depth,
                            (int)metadata.MipLevels,
                            miscFlags: misc);
                        return new D3D11Texture3D((ID3D11Texture3D*)resource, texture);
                    }

                case TextureDimension.TextureCube:
                    {
                        Texture2DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.Height,
                            (int)metadata.Depth,
                            (int)metadata.MipLevels,
                            miscFlags: misc);
                        return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe IResource InitFallback(TextureDimension dimension)
        {
            SubresourceData fallback;
            Vector4[] values = new Vector4[16];
            for (int i = 0; i < 16; i++)
            {
                values[i] = new(1f, 0.752941176f, 0.796078431f, 1f);
            }
            fixed (byte* ptr = MemoryMarshal.AsBytes(values.AsSpan()))
            {
                fallback = new(ptr, sizeof(Vector4));
            }

            if (dimension == TextureDimension.Texture1D)
            {
                return CreateTexture1D(Format.RGBA32Float, 16, 1, 1, new SubresourceData[] { fallback }, BindFlags.ShaderResource, Usage.Immutable);
            }
            if (dimension == TextureDimension.Texture2D)
            {
                return CreateTexture2D(Format.RGBA32Float, 4, 4, 1, 1, new SubresourceData[] { fallback }, BindFlags.ShaderResource, Usage.Immutable);
            }
            if (dimension == TextureDimension.Texture3D)
            {
                fallback.SlicePitch = 1;
                return CreateTexture3D(Format.RGBA32Float, 4, 4, 1, 1, new SubresourceData[] { fallback, }, BindFlags.ShaderResource, Usage.Immutable);
            }
            if (dimension == TextureDimension.TextureCube)
            {
                return CreateTexture2D(Format.RGBA32Float, 4, 4, 6, 1, new SubresourceData[] { fallback, fallback, fallback, fallback, fallback, fallback }, BindFlags.ShaderResource, Usage.Immutable, misc: ResourceMiscFlag.TextureCube);
            }

            throw new ArgumentOutOfRangeException(nameof(dimension));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ScratchImage LoadAuto(string path)
        {
            if (!FileSystem.TryOpen(path, out VirtualStream? fs))
                return default;

            ScratchImage image = new();
            var data = fs.GetBytes();
            string extension = Path.GetExtension(path);
            switch (extension)
            {
                case ".dds":
                    DirectXTex.LoadFromDDSMemory(data, DDSFlags.None, &image);
                    break;

                case ".tga":
                    DirectXTex.LoadFromTGAMemory(data, TGAFlags.TGA_FLAGS_NONE, &image);
                    break;

                case ".hdr":
                    DirectXTex.LoadFromHDRMemory(data, &image);
                    break;

                default:
                    DirectXTex.LoadFromWICMemory(data, WICFlags.NONE, &image, null);
                    break;
            };
            return image;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTexture1D(ITexture1D texture, string path)
        {
            SaveAuto(texture, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTexture2D(ITexture2D texture, string path)
        {
            SaveAuto(texture, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTexture3D(ITexture3D texture, string path)
        {
            SaveAuto(texture, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTextureCube(ITexture2D texture, string path)
        {
            SaveAuto(texture, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveAuto(IResource resource, string path)
        {
            ScratchImage image = new();
            DirectXTex.CaptureTexture((ID3D11Device*)NativePointer, (ID3D11DeviceContext*)Context.NativePointer, (ID3D11Resource*)resource.NativePointer, &image);
            switch (Path.GetExtension(path))
            {
                case ".dds":
                    DirectXTex.SaveToDDSFile(&image, DDSFlags.None, path);
                    break;

                case ".tga":
                    DirectXTex.SaveToTGAFile(&image, 0, TGAFlags.TGA_FLAGS_NONE, path);
                    break;

                case ".hdr":
                    DirectXTex.SaveToHDRFile(&image, 0, path);
                    break;

                default:
                    DirectXTex.SaveToWICFile(&image, 0, WICFlags.NONE, DirectXTex.GetWICCodec(WICCodecs.PNG), path);
                    break;
            }
            image.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTexture1D(ITexture1D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTexture2D(ITexture2D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTexture3D(ITexture3D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveTextureCube(ITexture2D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveAuto(IResource resource, Format format, string path)
        {
            ScratchImage image = new();
            DirectXTex.CaptureTexture((ID3D11Device*)NativePointer, (ID3D11DeviceContext*)Context.NativePointer, (ID3D11Resource*)resource.NativePointer, &image);
            /*if (DirectXTex.IsCompressed(Helper.Convert(format)))
            {
                TexCompressFlags flags = TexCompressFlags.PARALLEL;
                if (format == Format.BC7RGBAUNorm)
                    flags |= TexCompressFlags.BC7_QUICK;
                ScratchImage image1 = new();
                DirectXTex.Compress((ID3D11Device*)NativePointer, &image, Helper.Convert(format), flags, 1f, &image1);
                image.Release();
                image = image1;
            }
            else
            {
                ScratchImage image1 = new();
                DirectXTex.Convert(&image, Helper.Convert(format), TexFilterFlags.DEFAULT, 0.5f, &image1);
                image.Release();
                image = image1;
            }*/
            switch (Path.GetExtension(path))
            {
                case ".dds":
                    DirectXTex.SaveToDDSFile(&image, DDSFlags.None, path);
                    break;

                case ".tga":
                    DirectXTex.SaveToTGAFile(&image, 0, TGAFlags.TGA_FLAGS_NONE, path);
                    break;

                case ".hdr":
                    DirectXTex.SaveToHDRFile(&image, 0, path);
                    break;

                default:
                    DirectXTex.SaveToWICFile(&image, 0, WICFlags.NONE, DirectXTex.GetWICCodec(WICCodecs.PNG), path);
                    break;
            }
            image.Release();
        }

        [Flags]
        public enum RegisterComponentMaskFlags : byte
        {
            None = 0,
            ComponentX = 1,
            ComponentY = 2,
            ComponentZ = 4,
            ComponentW = 8,
            All
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateInputLayoutFromSignature(Blob shader, Blob signature, ID3D11InputLayout** layout)
        {
            ID3D11ShaderReflection* reflection;
            ShaderCompiler.Reflect(shader, ID3D11ShaderReflection.Guid, (void**)&reflection);
            ShaderDesc desc;
            reflection->GetDesc(&desc);

            InputElementDesc* inputElements = Alloc<InputElementDesc>(desc.InputParameters);
            for (uint i = 0; i < desc.InputParameters; i++)
            {
                SignatureParameterDesc parameterDesc;
                reflection->GetInputParameterDesc(i, &parameterDesc);

                InputElementDesc inputElement = new()
                {
                    SemanticName = parameterDesc.SemanticName,
                    SemanticIndex = parameterDesc.SemanticIndex,
                    InputSlot = 0,
                    AlignedByteOffset = D3D11.AppendAlignedElement,
                    InputSlotClass = Silk.NET.Direct3D11.InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                };

                if (parameterDesc.Mask == (byte)RegisterComponentMaskFlags.ComponentX)
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32G32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32G32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32G32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32G32B32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32G32B32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32G32B32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ | RegisterComponentMaskFlags.ComponentW))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32G32B32A32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32G32B32A32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32G32B32A32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                inputElements[i] = inputElement;
            }

            reflection->Release();
            Device->CreateInputLayout(inputElements, desc.InputParameters, signature.BufferPointer.ToPointer(), (uint)(int)signature.PointerSize, layout).ThrowHResult();
            Free(inputElements);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CreateInputLayoutFromSignature(Shader* shader, Blob signature, ID3D11InputLayout** layout)
        {
            ID3D11ShaderReflection* reflection;
            ShaderCompiler.Reflect(shader, ID3D11ShaderReflection.Guid, (void**)&reflection);
            ShaderDesc desc;
            reflection->GetDesc(&desc);

            InputElementDesc* inputElements = Alloc<InputElementDesc>(desc.InputParameters);
            for (uint i = 0; i < desc.InputParameters; i++)
            {
                SignatureParameterDesc parameterDesc;
                reflection->GetInputParameterDesc(i, &parameterDesc);

                InputElementDesc inputElement = new()
                {
                    SemanticName = parameterDesc.SemanticName,
                    SemanticIndex = parameterDesc.SemanticIndex,
                    InputSlot = 0,
                    AlignedByteOffset = D3D11.AppendAlignedElement,
                    InputSlotClass = Silk.NET.Direct3D11.InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                };

                if (parameterDesc.Mask == (byte)RegisterComponentMaskFlags.ComponentX)
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32G32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32G32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32G32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32G32B32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32G32B32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32G32B32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                if (parameterDesc.Mask == (byte)(RegisterComponentMaskFlags.ComponentX | RegisterComponentMaskFlags.ComponentY | RegisterComponentMaskFlags.ComponentZ | RegisterComponentMaskFlags.ComponentW))
                {
                    inputElement.Format = parameterDesc.ComponentType switch
                    {
                        D3DRegisterComponentType.D3DRegisterComponentUint32 => Silk.NET.DXGI.Format.FormatR32G32B32A32Uint,
                        D3DRegisterComponentType.D3DRegisterComponentSint32 => Silk.NET.DXGI.Format.FormatR32G32B32A32Sint,
                        D3DRegisterComponentType.D3DRegisterComponentFloat32 => Silk.NET.DXGI.Format.FormatR32G32B32A32Float,
                        _ => Silk.NET.DXGI.Format.FormatUnknown,
                    };
                }

                inputElements[i] = inputElement;
            }

            reflection->Release();
            Device->CreateInputLayout(inputElements, desc.InputParameters, signature.BufferPointer.ToPointer(), (uint)(int)signature.PointerSize, layout).ThrowHResult();
            Free(inputElements);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                OnDisposed?.Invoke(this, EventArgs.Empty);
                SwapChain?.Dispose();
                Context.Dispose();
                Device->Release();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

#if DEBUG
                Debug->ReportLiveDeviceObjects(RldoFlags.Detail | RldoFlags.IgnoreInternal);
                Debug->Release();
#endif

                LeakTracer.ReportLiveInstances();

                D3D11.Dispose();

                disposedValue = true;
            }
        }

        ~D3D11GraphicsDevice()
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IQuery CreateQuery()
        {
            return CreateQuery(Query.Event);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IQuery CreateQuery(Query type)
        {
            ID3D11Query* query;
            QueryDesc desc = new(Helper.Convert(type), 0);
            Device->CreateQuery(&desc, &query);
            return new D3D11Query(query);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IGraphicsContext CreateDeferredContext()
        {
            ID3D11DeviceContext1* context;
            Device->CreateDeferredContext1(0, &context);
            return new D3D11GraphicsContext(this, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description)
        {
            ID3D11UnorderedAccessView* view;
            UnorderedAccessViewDesc desc = Helper.Convert(description);
            Device->CreateUnorderedAccessView((ID3D11Resource*)resource.NativePointer, &desc, &view);
            return new D3D11UnorderedAccessView(view, description);
        }
    }
}