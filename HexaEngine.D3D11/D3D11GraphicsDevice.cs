﻿namespace HexaEngine.D3D11
{
    using DirectXTexNet;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using Format = Core.Graphics.Format;
    using ResourceMiscFlag = Core.Graphics.ResourceMiscFlag;
    using SubresourceData = Core.Graphics.SubresourceData;
    using Usage = Core.Graphics.Usage;
    using Viewport = Mathematics.Viewport;

    public unsafe class D3D11GraphicsDevice : IGraphicsDevice
    {
        internal readonly DXGI DXGI;
        internal readonly D3D11 D3D11;
        private bool disposedValue;

        internal IDXGIFactory2* IDXGIFactory;
        internal IDXGIAdapter1* IDXGIAdapter;

        internal ID3D11Device1* Device;
        internal ID3D11DeviceContext1* DeviceContext;
        internal ID3D11Debug* Debug;

        internal IDXGISwapChain1* swapChain;

        [SupportedOSPlatform("windows")]
        public D3D11GraphicsDevice(SdlWindow window)
        {
            DXGI = DXGI.GetApi();
            D3D11 = D3D11.GetApi();

            IDXGIFactory2* factory;
            DXGI.CreateDXGIFactory2(0, Utils.Guid(IDXGIFactory2.Guid), (void**)&factory);
            IDXGIFactory = factory;

            IDXGIAdapter = GetHardwareAdapter();

#if D3D11On12

#else
            D3DFeatureLevel[] levelsArr = new D3DFeatureLevel[]
            {
                D3DFeatureLevel.D3DFeatureLevel121,
                D3DFeatureLevel.D3DFeatureLevel120,
                D3DFeatureLevel.D3DFeatureLevel111,
            };

            CreateDeviceFlag flags = CreateDeviceFlag.CreateDeviceBgraSupport;

#if DEBUG
            flags |= CreateDeviceFlag.CreateDeviceDebug;
#endif
            ID3D11Device* tempDevice;
            ID3D11DeviceContext* tempContext;

            D3DFeatureLevel level = 0;
            D3DFeatureLevel* levels = (D3DFeatureLevel*)Unsafe.AsPointer(ref levelsArr[0]);

            ResultCode code = (ResultCode)D3D11.CreateDevice((IDXGIAdapter*)IDXGIAdapter, D3DDriverType.D3DDriverTypeUnknown, IntPtr.Zero, (uint)flags, levels, 3, D3D11.SdkVersion, &tempDevice, &level, &tempContext);

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

            SwapChainDesc1 desc = new()
            {
                Width = (uint)window.Width,
                Height = (uint)window.Height,
                Format = Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm,
                BufferCount = 2,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SampleDesc = new(1, 0),
                Scaling = Scaling.ScalingStretch,
                SwapEffect = SwapEffect.SwapEffectFlipSequential,
                Flags = (uint)(SwapChainFlag.SwapChainFlagAllowModeSwitch | SwapChainFlag.SwapChainFlagAllowTearing)
            };

            SwapChainFullscreenDesc fullscreenDesc = new()
            {
                Windowed = 1,
                RefreshRate = new Rational(0, 1),
                Scaling = ModeScaling.ModeScalingUnspecified,
                ScanlineOrdering = ModeScanlineOrder.ModeScanlineOrderUnspecified,
            };

            IDXGISwapChain1* pswapChain;
            IntPtr hwnd = window.GetHWND();
            IDXGIFactory->CreateSwapChainForHwnd((IUnknown*)device, hwnd, &desc, &fullscreenDesc, null, &pswapChain);
            IDXGIFactory->MakeWindowAssociation(hwnd, 1 << 0);

            swapChain = pswapChain;

            SwapChain = new DXGISwapChain(this, (SwapChainFlag)desc.Flags);
        }

        public IGraphicsContext Context { get; }

        public IntPtr NativePointer { get; }
        public static Guid WKPDID_D3DDebugObjectName = new(0x429b8c22, 0x9188, 0x4b0c, 0x87, 0x42, 0xac, 0xb0, 0xbf, 0x85, 0xc2, 0x00);

        public string? DebugName { get; set; } = string.Empty;
        public ISwapChain SwapChain { get; }

        public IBuffer CreateBuffer(BufferDescription description)
        {
            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);
            Device->CreateBuffer(&desc, null, &buffer).ThrowHResult();
            return new D3D11Buffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged
        {
            if (description.ByteWidth == 0)
            {
                description.ByteWidth = sizeof(T);
            }

            SubresourceData data;
            fixed (void* ptr = new T[] { value })
            {
                data = new(ptr, description.ByteWidth);
            }
            var datas = Helper.Convert(new SubresourceData[] { data });

            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);
            Device->CreateBuffer(&desc, Utils.AsPointer(datas), &buffer).ThrowHResult();
            return new D3D11Buffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            BufferDescription description = new(0, bindFlags, usage, cpuAccessFlags, miscFlags);
            return CreateBuffer(value, description);
        }

        public IBuffer CreateBuffer<T>(T[] values, BufferDescription description) where T : unmanaged
        {
            if (description.ByteWidth == 0)
            {
                description.ByteWidth = sizeof(T) * values.Length;
            }

            SubresourceData data;
            fixed (void* ptr = values)
            {
                data = new(ptr, description.ByteWidth);
            }
            var datas = Helper.Convert(new SubresourceData[] { data });

            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);
            Device->CreateBuffer(&desc, Utils.AsPointer(datas), &buffer).ThrowHResult();
            return new D3D11Buffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T[] values, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            BufferDescription description = new(0, bindFlags, usage, cpuAccessFlags, miscFlags);
            return CreateBuffer(values, description);
        }

        public IBlendState CreateBlendState(BlendDescription description)
        {
            ID3D11BlendState* blend;
            BlendDesc desc = Helper.Convert(description);
            Device->CreateBlendState(ref desc, &blend).ThrowHResult();
            return new D3D11BlendState(blend, description);
        }

        public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description)
        {
            ID3D11DepthStencilState* state;
            DepthStencilDesc desc = Helper.Convert(description);
            Device->CreateDepthStencilState(ref desc, &state).ThrowHResult();
            return new D3D11DepthStencilState(state, description);
        }

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

        public IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description)
        {
            ID3D11DepthStencilView* view;
            DepthStencilViewDesc desc = Helper.Convert(description);
            Device->CreateDepthStencilView((ID3D11Resource*)resource.NativePointer, ref desc, &view).ThrowHResult();
            return new D3D11DepthStencilView(view, description);
        }

        public IRasterizerState CreateRasterizerState(RasterizerDescription description)
        {
            ID3D11RasterizerState* state;
            RasterizerDesc desc = Helper.Convert(description);
            Device->CreateRasterizerState(ref desc, &state).ThrowHResult();
            return new D3D11RasterizerState(state, description);
        }

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

        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport)
        {
            ID3D11RenderTargetView* rtv;
            RenderTargetViewDesc desc = Helper.Convert(description);
            Device->CreateRenderTargetView((ID3D11Resource*)resource.NativePointer, &desc, &rtv).ThrowHResult();
            return new D3D11RenderTargetView(rtv, description, viewport);
        }

        public ISamplerState CreateSamplerState(SamplerDescription description)
        {
            ID3D11SamplerState* sampler;
            SamplerDesc desc = Helper.Convert(description);
            Device->CreateSamplerState(ref desc, &sampler).ThrowHResult();
            return new D3D11SamplerState(sampler, description);
        }

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
                    dimension = ShaderResourceViewDimension.TextureCube;
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

        public IShaderResourceView CreateShaderResourceView(IResource resource, ShaderResourceViewDescription description)
        {
            ID3D11ShaderResourceView* srv;
            ShaderResourceViewDesc desc = Helper.Convert(description);
            Device->CreateShaderResourceView((ID3D11Resource*)resource.NativePointer, &desc, &srv).ThrowHResult();
            return new D3D11ShaderResourceView(srv, description);
        }

        public ITexture1D CreateTexture1D(Texture1DDescription description)
        {
            ID3D11Texture1D* texture;
            Texture1DDesc desc = Helper.Convert(description);
            Device->CreateTexture1D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture1D(texture, description);
        }

        public ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[] subresources)
        {
            ID3D11Texture1D* texture;
            Texture1DDesc desc = Helper.Convert(description);
            var data = Helper.Convert(subresources);
            Device->CreateTexture1D(ref desc, Utils.AsPointer(data), &texture).ThrowHResult();
            return new D3D11Texture1D(texture, description);
        }

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, object value, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            Texture1DDescription description = new(format, width, arraySize, mipLevels, bindFlags, miscFlags: misc);
            ID3D11Texture1D* texture;
            Texture1DDesc desc = Helper.Convert(description);
            Device->CreateTexture1D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture1D(texture, description);
        }

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[] subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            Texture1DDescription description = new(format, width, arraySize, mipLevels, bindFlags, usage, cpuAccessFlags, misc);
            ID3D11Texture1D* texture;
            Texture1DDesc desc = Helper.Convert(description);

            if (subresources != null)
            {
                var data = Helper.Convert(subresources);
                Device->CreateTexture1D(ref desc, Utils.AsPointer(data), &texture).ThrowHResult();
            }
            else
            {
                Device->CreateTexture1D(ref desc, null, &texture).ThrowHResult();
            }

            return new D3D11Texture1D(texture, description);
        }

        public ITexture2D CreateTexture2D(Texture2DDescription description)
        {
            ID3D11Texture2D* texture;
            Texture2DDesc desc = Helper.Convert(description);
            Device->CreateTexture2D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture2D(texture, description);
        }

        public ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[] subresources)
        {
            ID3D11Texture2D* texture;
            Texture2DDesc desc = Helper.Convert(description);
            var data = Helper.Convert(subresources);
            Device->CreateTexture2D(ref desc, Utils.AsPointer(data), &texture).ThrowHResult();
            return new D3D11Texture2D(texture, description);
        }

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, object value, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            Texture2DDescription description = new(format, width, height, arraySize, mipLevels, bindFlags, miscFlags: misc);
            ID3D11Texture2D* texture;
            Texture2DDesc desc = Helper.Convert(description);
            Device->CreateTexture2D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture2D(texture, description);
        }

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            Texture2DDescription description = new(format, width, height, arraySize, mipLevels, bindFlags, usage, cpuAccessFlags, sampleCount, sampleQuality, misc);
            ID3D11Texture2D* texture;
            Texture2DDesc desc = Helper.Convert(description);

            if (subresources != null)
            {
                var data = Helper.Convert(subresources);
                Device->CreateTexture2D(ref desc, Utils.AsPointer(data), &texture).ThrowHResult();
            }
            else
            {
                Device->CreateTexture2D(ref desc, null, &texture).ThrowHResult();
            }

            return new D3D11Texture2D(texture, description);
        }

        public ITexture3D CreateTexture3D(Texture3DDescription description)
        {
            ID3D11Texture3D* texture;
            Texture3DDesc desc = Helper.Convert(description);
            Device->CreateTexture3D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture3D(texture, description);
        }

        public ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[] subresources)
        {
            ID3D11Texture3D* texture;
            Texture3DDesc desc = Helper.Convert(description);
            var data = Helper.Convert(subresources);
            Device->CreateTexture3D(ref desc, Utils.AsPointer(data), &texture).ThrowHResult();
            return new D3D11Texture3D(texture, description);
        }

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, object value, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            Texture3DDescription description = new(format, width, height, height, mipLevels, bindFlags, miscFlags: misc);
            ID3D11Texture3D* texture;
            Texture3DDesc desc = Helper.Convert(description);
            Device->CreateTexture3D(ref desc, null, &texture).ThrowHResult();
            return new D3D11Texture3D(texture, description);
        }

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[] subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            Texture3DDescription description = new(format, width, height, height, mipLevels, bindFlags, usage, cpuAccessFlags, misc);
            ID3D11Texture3D* texture;
            Texture3DDesc desc = Helper.Convert(description);

            if (subresources != null)
            {
                var data = Helper.Convert(subresources);
                Device->CreateTexture3D(ref desc, Utils.AsPointer(data), &texture).ThrowHResult();
            }
            else
            {
                Device->CreateTexture3D(ref desc, null, &texture).ThrowHResult();
            }

            return new D3D11Texture3D(texture, description);
        }

        public ITexture1D LoadTexture1D(string path)
        {
            return (ITexture1D)LoadTextureAuto(path, TextureDimension.Texture1D);
        }

        public ITexture2D LoadTexture2D(string path)
        {
            return (ITexture2D)LoadTextureAuto(path, TextureDimension.Texture2D);
        }

        public ITexture3D LoadTexture3D(string path)
        {
            return (ITexture3D)LoadTextureAuto(path, TextureDimension.Texture3D);
        }

        public ITexture2D LoadTextureCube(string path)
        {
            return (ITexture2D)LoadTextureAuto(path, TextureDimension.TextureCube);
        }

        private IResource LoadTextureAuto(string path, TextureDimension dimension)
        {
            ScratchImage image = LoadAuto(path);
            if (image == null)
            {
                return InitFallback(dimension);
            }
            TexMetadata metadata = image.GetMetadata();
            if (!metadata.IsCompressed())
            {
                ScratchImage image1 = image.GenerateMipMaps(0, TEX_FILTER_FLAGS.DEFAULT, Nucleus.Settings.MipLevels, true);
                image.Dispose();
                image = image1;
            }

            metadata = image.GetMetadata();
            ResourceMiscFlag optionFlags = metadata.IsCubemap() ? ResourceMiscFlag.TextureCube : ResourceMiscFlag.None;
            var resource = image.CreateTextureEx((IntPtr)Device, D3D11_USAGE.DEFAULT, D3D11_BIND_FLAG.SHADER_RESOURCE, 0, (D3D11_RESOURCE_MISC_FLAG)Helper.Convert(optionFlags), false);

            switch (dimension)
            {
                case TextureDimension.Texture1D:
                    {
                        Texture1DDescription texture = new(
                            Helper.ConvertBack((Silk.NET.DXGI.Format)metadata.Format),
                            metadata.Width,
                            metadata.ArraySize,
                            metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture1D((ID3D11Texture1D*)resource, texture);
                    }

                case TextureDimension.Texture2D:
                    {
                        Texture2DDescription texture = new(
                            Helper.ConvertBack((Silk.NET.DXGI.Format)metadata.Format),
                            metadata.Width,
                            metadata.Height,
                            metadata.ArraySize,
                            metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
                    }

                case TextureDimension.Texture3D:
                    {
                        Texture3DDescription texture = new(
                            Helper.ConvertBack((Silk.NET.DXGI.Format)metadata.Format),
                            metadata.Width,
                            metadata.Height,
                            metadata.Depth,
                            metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture3D((ID3D11Texture3D*)resource, texture);
                    }

                case TextureDimension.TextureCube:
                    {
                        Texture2DDescription texture = new(
                            Helper.ConvertBack((Silk.NET.DXGI.Format)metadata.Format),
                            metadata.Width,
                            metadata.Height,
                            metadata.Depth,
                            metadata.MipLevels,
                            miscFlags: optionFlags);
                        return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension));
            }
        }

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
                return CreateTexture1D(Format.RGBA32Float, 16, 1, 1, new SubresourceData[] { fallback }, BindFlags.ShaderResource);
            }
            if (dimension == TextureDimension.Texture2D)
            {
                return CreateTexture2D(Format.RGBA32Float, 4, 4, 1, 1, new SubresourceData[] { fallback }, BindFlags.ShaderResource);
            }
            if (dimension == TextureDimension.TextureCube)
            {
                return CreateTexture2D(Format.RGBA32Float, 4, 4, 6, 1, new SubresourceData[] { fallback, fallback, fallback, fallback, fallback, fallback }, BindFlags.ShaderResource, ResourceMiscFlag.TextureCube);
            }

            throw new ArgumentOutOfRangeException(nameof(dimension));
        }

        private ScratchImage LoadAuto(string path)
        {
            VirtualStream fs = FileSystem.Open(path);
            if (fs == null)
                return null;
            IntPtr ptr = fs.GetIntPtr(out _);
            string extension = Path.GetExtension(path);
            return extension switch
            {
                ".dds" => TexHelper.Instance.LoadFromDDSMemory(ptr, fs.Length, DDS_FLAGS.NONE),
                ".tga" => TexHelper.Instance.LoadFromTGAMemory(ptr, fs.Length),
                ".hdr" => TexHelper.Instance.LoadFromHDRMemory(ptr, fs.Length),
                _ => TexHelper.Instance.LoadFromWICMemory(ptr, fs.Length, WIC_FLAGS.NONE),
            };
        }

        public IVertexShader CreateVertexShader(byte[] bytecode)
        {
            ID3D11VertexShader* vs;
            Device->CreateVertexShader(Utils.AsPointer(bytecode), (nuint)bytecode.Length, null, &vs).ThrowHResult();
            return new D3D11VertexShader(vs);
        }

        public IHullShader CreateHullShader(byte[] bytecode)
        {
            ID3D11HullShader* hs;
            Device->CreateHullShader(Utils.AsPointer(bytecode), (nuint)bytecode.Length, null, &hs).ThrowHResult();
            return new D3D11HullShader(hs);
        }

        public IDomainShader CreateDomainShader(byte[] bytecode)
        {
            ID3D11DomainShader* ds;
            Device->CreateDomainShader(Utils.AsPointer(bytecode), (nuint)bytecode.Length, null, &ds).ThrowHResult();
            return new D3D11DomainShader(ds);
        }

        public IGeometryShader CreateGeometryShader(byte[] bytecode)
        {
            ID3D11GeometryShader* gs;
            Device->CreateGeometryShader(Utils.AsPointer(bytecode), (nuint)bytecode.Length, null, &gs).ThrowHResult();
            return new D3D11GeometryShader(gs);
        }

        public IPixelShader CreatePixelShader(byte[] bytecode)
        {
            ID3D11PixelShader* ps;
            Device->CreatePixelShader(Utils.AsPointer(bytecode), (nuint)bytecode.Length, null, &ps).ThrowHResult();
            return new D3D11PixelShader(ps);
        }

        public IComputeShader CreateComputeShader(byte[] bytecode)
        {
            throw new NotImplementedException();
        }

        public IInputLayout CreateInputLayout(InputElementDescription[] inputElements, Blob vertexShaderBlob)
        {
            ID3D11InputLayout* layout;
            InputElementDesc[] descs = Helper.Convert(inputElements);
            Device->CreateInputLayout(Utils.AsPointer(descs), (uint)descs.Length, vertexShaderBlob.BufferPointer.ToPointer(), (uint)(int)vertexShaderBlob.PointerSize, &layout).ThrowHResult();
            return new D3D11InputLayout(layout);
        }

        public IInputLayout CreateInputLayout(byte[] data)
        {
            Blob blob = new(data);
            ID3D11InputLayout* layout;
            CreateInputLayoutFromSignature(blob, &layout);
            return new D3D11InputLayout(layout);
        }

        public void Compile(string code, string entry, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            ShaderCompiler.Compile(code, entry, sourceName, profile, out shaderBlob, out errorBlob);
        }

        public void Compile(string code, string entry, string sourceName, string profile, out Blob? shaderBlob)
        {
            Compile(code, entry, sourceName, profile, out shaderBlob, out _);
        }

        public void CompileFromFile(string path, string entry, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            ShaderCompiler.Compile(FileSystem.ReadAllText(Paths.CurrentShaderPath + path), entry, path, profile, out shaderBlob, out errorBlob);
            if (errorBlob != null)
                ImGuiConsole.Log(errorBlob.AsString());
        }

        public void CompileFromFile(string path, string entry, string profile, out Blob? shaderBlob)
        {
            CompileFromFile(path, entry, profile, out shaderBlob, out _);
        }

        private void CreateInputLayoutFromSignature(Blob shader, ID3D11InputLayout** layout) => CreateInputLayoutFromSignature(shader, ShaderCompiler.GetInputSignature(shader), layout);

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

        private void CreateInputLayoutFromSignature(Blob shader, Blob signature, ID3D11InputLayout** layout)
        {
            ID3D11ShaderReflection* reflection;
            ShaderCompiler.Reflect(shader, ID3D11ShaderReflection.Guid, (void**)&reflection);
            ShaderDesc desc;
            reflection->GetDesc(&desc);

            InputElementDesc[] inputElements = new InputElementDesc[desc.InputParameters];
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
                    InputSlotClass = Silk.NET.Direct3D11.InputClassification.InputPerVertexData,
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

            InputElementDesc* ptr = Utils.AsPointer(inputElements);

            Device->CreateInputLayout(ptr, (uint)inputElements.Length, signature.BufferPointer.ToPointer(), (uint)(int)signature.PointerSize, layout);
        }

        private IDXGIAdapter1* GetHardwareAdapter()
        {
            IDXGIAdapter1* adapter = null;
            Guid* adapterGuid = Utils.Guid(IDXGIAdapter1.Guid);
            IDXGIFactory6* factory6;
            IDXGIFactory->QueryInterface(Utils.Guid(IDXGIFactory6.Guid), (void**)&factory6);

            if (factory6 != null)
            {
                for (uint adapterIndex = 0;
                    (ResultCode)factory6->EnumAdapterByGpuPreference(adapterIndex, GpuPreference.GpuPreferenceHighPerformance, adapterGuid, (void**)&adapter) !=
                    ResultCode.DXGI_ERROR_NOT_FOUND;
                    adapterIndex++)
                {
                    AdapterDesc1 desc;
                    adapter->GetDesc1(&desc);
                    string name = new(desc.Description);

                    Trace.WriteLine($"Found Adapter {name}");

                    if (((AdapterFlag)desc.Flags & AdapterFlag.AdapterFlagSoftware) != AdapterFlag.AdapterFlagNone)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter->Release();
                        continue;
                    }

                    Trace.WriteLine($"Using {name}");

                    return adapter;
                }

                factory6->Release();
            }

            if (adapter == null)
                for (uint adapterIndex = 0;
                    (ResultCode)IDXGIFactory->EnumAdapters1(adapterIndex, &adapter) != ResultCode.DXGI_ERROR_NOT_FOUND;
                    adapterIndex++)
                {
                    AdapterDesc1 desc;
                    adapter->GetDesc1(&desc);
                    string name = new(desc.Description);

                    Trace.WriteLine($"Found Adapter {name}");

                    if (((AdapterFlag)desc.Flags & AdapterFlag.AdapterFlagSoftware) != AdapterFlag.AdapterFlagNone)
                    {
                        // Don't select the Basic Render Driver adapter.
                        adapter->Release();
                        continue;
                    }

                    return adapter;
                }

            return adapter;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                SwapChain.Dispose();
                Context.Dispose();
                Device->Release();
                IDXGIAdapter->Release();
                IDXGIFactory->Release();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

                Debug->ReportLiveDeviceObjects(RldoFlags.RldoDetail | RldoFlags.RldoIgnoreInternal);
                Debug->Release();

                LeakTracer.ReportLiveInstances();

                DXGI.Dispose();
                D3D11.Dispose();

                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~D3D11GraphicsDevice()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}