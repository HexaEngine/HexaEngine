namespace HexaEngine.D3D11
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.DirectXTex;
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
    using Usage = Core.Graphics.Usage;
    using Viewport = Mathematics.Viewport;

    public unsafe class D3D11GraphicsDevice : IGraphicsDevice
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
            };

            CreateDeviceFlag flags = CreateDeviceFlag.BgraSupport;

#if DEBUG
            flags |= CreateDeviceFlag.Debug;
#endif
            ID3D11Device* tempDevice;
            ID3D11DeviceContext* tempContext;

            D3DFeatureLevel level = 0;
            D3DFeatureLevel* levels = (D3DFeatureLevel*)Unsafe.AsPointer(ref levelsArr[0]);

            ResultCode code = (ResultCode)D3D11.CreateDevice((IDXGIAdapter*)adapter.IDXGIAdapter, D3DDriverType.Unknown, IntPtr.Zero, (uint)flags, levels, 3, D3D11.SdkVersion, &tempDevice, &level, &tempContext);

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

        public ISwapChain? SwapChain { get; }

        public IBuffer CreateBuffer(BufferDescription description)
        {
            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);
            Device->CreateBuffer(&desc, null, &buffer).ThrowHResult();
            return new D3D11Buffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : struct
        {
            if (description.ByteWidth == 0)
            {
                description.ByteWidth = Marshal.SizeOf<T>();
            }

            SubresourceData data;

            var basePtr = Marshal.AllocHGlobal(description.ByteWidth);
            data = new(basePtr, description.ByteWidth);
            Marshal.StructureToPtr(value, basePtr, true);

            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);
            Device->CreateBuffer(&desc, Utils.AsPointer(Helper.Convert(new SubresourceData[] { data })), &buffer).ThrowHResult();

            Marshal.FreeHGlobal(basePtr);

            return new D3D11Buffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : struct
        {
            BufferDescription description = new(0, bindFlags, usage, cpuAccessFlags, miscFlags);
            return CreateBuffer(value, description);
        }

        public IBuffer CreateBuffer<T>(T[] values, BufferDescription description) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            if (description.ByteWidth == 0)
            {
                description.ByteWidth = size * values.Length;
            }
            SubresourceData data;

            var basePtr = Marshal.AllocHGlobal(description.ByteWidth);
            data = new(basePtr, description.ByteWidth);
            long ptr = basePtr.ToInt64();
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.StructureToPtr(values[i], (IntPtr)ptr, true);
                ptr += size;
            }

            ID3D11Buffer* buffer;
            BufferDesc desc = Helper.Convert(description);
            Device->CreateBuffer(&desc, Utils.AsPointer(Helper.Convert(new SubresourceData[] { data })), &buffer).ThrowHResult();

            Marshal.FreeHGlobal(basePtr);

            return new D3D11Buffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T[] values, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : struct
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

        public ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[]? subresources)
        {
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

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            return CreateTexture1D(format, width, arraySize, mipLevels, subresources, bindFlags, Usage.Default, CpuAccessFlags.None, misc);
        }

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
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

        public ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[]? subresources)
        {
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

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            return CreateTexture2D(format, width, height, arraySize, mipLevels, subresources, bindFlags, Usage.Default, CpuAccessFlags.None, 1, 0, misc);
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

        public ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[]? subresources)
        {
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

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag misc)
        {
            return CreateTexture3D(format, width, height, depth, mipLevels, subresources, bindFlags, Usage.Default, CpuAccessFlags.None, misc);
        }

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            Texture3DDescription description = new(format, width, height, depth, mipLevels, bindFlags, usage, cpuAccessFlags, misc);
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

        public void SaveTexture1D(ITexture1D texture, string path)
        {
            SaveAuto(texture, path);
        }

        public void SaveTexture2D(ITexture2D texture, string path)
        {
            SaveAuto(texture, path);
        }

        public void SaveTexture3D(ITexture3D texture, string path)
        {
            SaveAuto(texture, path);
        }

        public void SaveTextureCube(ITexture2D texture, string path)
        {
            SaveAuto(texture, path);
        }

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

        public void SaveTexture1D(ITexture1D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

        public void SaveTexture2D(ITexture2D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

        public void SaveTexture3D(ITexture3D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

        public void SaveTextureCube(ITexture2D texture, Format format, string path)
        {
            SaveAuto(texture, format, path);
        }

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

        public IInputLayout CreateInputLayout(InputElementDescription[] inputElements, byte[] data)
        {
            Blob blob = new(data);
            ID3D11InputLayout* layout;
            InputElementDesc[] descs = Helper.Convert(inputElements);
            Device->CreateInputLayout(Utils.AsPointer(descs), (uint)descs.Length, blob.BufferPointer.ToPointer(), (uint)(int)blob.PointerSize, &layout).ThrowHResult();
            return new D3D11InputLayout(layout);
        }

        public IInputLayout CreateInputLayout(byte[] data)
        {
            Blob blob = new(data);
            ID3D11InputLayout* layout;
            CreateInputLayoutFromSignature(blob, &layout);
            return new D3D11InputLayout(layout);
        }

        public void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            ShaderCompiler.Compile(code, macros, entry, sourceName, profile, out shaderBlob, out errorBlob);
            if (errorBlob != null)
                ImGuiConsole.Log(errorBlob.AsString());
        }

        public void Compile(string code, string entry, string sourceName, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            Compile(code, Array.Empty<ShaderMacro>(), entry, sourceName, profile, out shaderBlob, out errorBlob);
        }

        public void Compile(string code, ShaderMacro[] macros, string entry, string sourceName, string profile, out Blob? shaderBlob)
        {
            Compile(code, macros, entry, sourceName, profile, out shaderBlob, out _);
        }

        public void Compile(string code, string entry, string sourceName, string profile, out Blob? shaderBlob)
        {
            Compile(code, entry, sourceName, profile, out shaderBlob, out _);
        }

        public void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            ShaderCompiler.Compile(FileSystem.ReadAllText(Paths.CurrentShaderPath + path), macros, entry, path, profile, out shaderBlob, out errorBlob);
            if (errorBlob != null)
                ImGuiConsole.Log(errorBlob.AsString());
        }

        public void CompileFromFile(string path, string entry, string profile, out Blob? shaderBlob, out Blob? errorBlob)
        {
            CompileFromFile(path, Array.Empty<ShaderMacro>(), entry, profile, out shaderBlob, out errorBlob);
        }

        public void CompileFromFile(string path, ShaderMacro[] macros, string entry, string profile, out Blob? shaderBlob)
        {
            CompileFromFile(path, macros, entry, profile, out shaderBlob, out _);
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

            InputElementDesc* ptr = Utils.AsPointer(inputElements);

            Device->CreateInputLayout(ptr, (uint)inputElements.Length, signature.BufferPointer.ToPointer(), (uint)(int)signature.PointerSize, layout);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
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

        public IQuery CreateQuery()
        {
            return CreateQuery(Query.Event);
        }

        public IQuery CreateQuery(Query type)
        {
            ID3D11Query* query;
            QueryDesc desc = new(Helper.Convert(type), 0);
            Device->CreateQuery(&desc, &query);
            return new D3D11Query(query);
        }

        /*
        public IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDesc)
        {
            Device->CreateUnorderedAccessView()
        }*/
    }

    public unsafe class D3D11Query : DeviceChildBase, IQuery
    {
        private ID3D11Query* query;

        public D3D11Query(ID3D11Query* query)
        {
            this.query = query;
            nativePointer = new(query);
        }

        protected override void DisposeCore()
        {
            query->Release();
        }
    }
}