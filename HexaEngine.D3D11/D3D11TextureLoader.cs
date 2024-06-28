namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using Hexa.NET.DirectXTex;
    using Silk.NET.Direct3D11;
    using System.Diagnostics;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public unsafe class D3D11TextureLoader : ITextureLoader
    {
        private readonly IGraphicsDevice device;
        private TextureLoaderFlags flags = TextureLoaderFlags.None;
        private float scalingFactor = 1;

        public D3D11TextureLoader(IGraphicsDevice device)
        {
            this.device = device;
        }

        public IGraphicsDevice Device => device;

        /// <summary>
        /// The Flags are only used for the LoadTextureXD functions which only load textures from assets.
        /// </summary>
        public TextureLoaderFlags Flags { get => flags; set => flags = value; }

        /// <summary>
        /// The ScalingFactor is only used for the LoadTextureXD functions which only load textures from assets.
        /// </summary>
        public float ScalingFactor { get => scalingFactor; set => scalingFactor = value; }

        public IScratchImage CaptureTexture(IGraphicsContext context, IResource resource)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            DirectXTex.CaptureTexture((ID3D11Device*)device.NativePointer, (ID3D11DeviceContext*)context.NativePointer, (ID3D11Resource*)resource.NativePointer, image).ThrowHResult();

            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFormAssets(string path)
        {
            if (!FileSystem.TryOpenRead(path, out VirtualStream? fs))
            {
                Trace.WriteLine($"Warning couldn't find texture {path}");
                return default;
            }

            ScratchImage image = DirectXTex.CreateScratchImage();
            var data = fs.ReadBytes();
            string extension = Path.GetExtension(path);
            fixed (byte* p = data)
            {
                switch (extension)
                {
                    case ".dds":
                        DirectXTex.LoadFromDDSMemory(p, (nuint)data.Length, DDSFlags.None, null, image);
                        break;

                    case ".tga":
                        DirectXTex.LoadFromTGAMemory(p, (nuint)data.Length, TGAFlags.None, null, image);
                        break;

                    case ".hdr":
                        DirectXTex.LoadFromHDRMemory(p, (nuint)data.Length, null, image);
                        break;

                    default:
                        DirectXTex.LoadFromWICMemory(p, (nuint)data.Length, WICFlags.None, null, image, default);
                        break;
                }
            };
            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFormAssets(string path, TextureDimension dimension)
        {
            if (!FileSystem.TryOpenRead(path, out VirtualStream? fs))
            {
                Trace.WriteLine($"Warning couldn't find texture {path}");
                return InitFallback(dimension);
            }

            ScratchImage image = DirectXTex.CreateScratchImage();
            var data = fs.ReadBytes();
            string extension = Path.GetExtension(path);
            fixed (byte* p = data)
            {
                switch (extension)
                {
                    case ".dds":
                        DirectXTex.LoadFromDDSMemory(p, (nuint)data.Length, DDSFlags.None, null, image);
                        break;

                    case ".tga":
                        DirectXTex.LoadFromTGAMemory(p, (nuint)data.Length, TGAFlags.None, null, image);
                        break;

                    case ".hdr":
                        DirectXTex.LoadFromHDRMemory(p, (nuint)data.Length, null, image);
                        break;

                    default:
                        DirectXTex.LoadFromWICMemory(p, (nuint)data.Length, WICFlags.None, null, image, default);
                        break;
                }
            };
            return new D3DScratchImage(image);
        }

        private IScratchImage InitFallback(TextureDimension dimension)
        {
            Vector4 fallbackColor = new(1, 0, 1, 1);
            ScratchImage fallback = DirectXTex.CreateScratchImage();
            if (dimension == TextureDimension.Texture1D)
            {
                fallback.Initialize1D((int)Silk.NET.DXGI.Format.FormatR32G32B32A32Float, 1, 1, 1, CPFlags.None);
            }
            if (dimension == TextureDimension.Texture2D)
            {
                fallback.Initialize2D((int)Silk.NET.DXGI.Format.FormatR32G32B32A32Float, 1, 1, 1, 1, CPFlags.None);
            }
            if (dimension == TextureDimension.Texture3D)
            {
                fallback.Initialize3D((int)Silk.NET.DXGI.Format.FormatR32G32B32A32Float, 1, 1, 1, 1, CPFlags.None);
            }
            if (dimension == TextureDimension.TextureCube)
            {
                fallback.InitializeCube((int)Silk.NET.DXGI.Format.FormatR32G32B32A32Float, 1, 1, 1, 1, CPFlags.None);
            }
            var size = fallback.GetPixelsSize();
            for (ulong i = 0; i < 1; i++)
            {
                ((Vector4*)fallback.GetPixels())[i] = fallbackColor;
            }

            return new D3DScratchImage(fallback);
        }

        public IScratchImage LoadFormFile(string filename, TextureDimension dimension)
        {
            if (!File.Exists(filename))
            {
                return InitFallback(dimension);
            }

            return LoadFormFile(filename);
        }

        public IScratchImage LoadFormFile(string filename)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            string extension = Path.GetExtension(filename);

            if (string.IsNullOrWhiteSpace(extension))
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    DirectXTex.LoadFromDDSFile(filename, DDSFlags.None, null, image);
                }
                else
                {
                    DirectXTex.LoadFromTGAFile(filename, TGAFlags.None, null, image);
                }

                return new D3DScratchImage(image);
            }

            switch (extension)
            {
                case ".dds":
                    DirectXTex.LoadFromDDSFile(filename, DDSFlags.None, null, image);
                    break;

                case ".tga":
                    DirectXTex.LoadFromTGAFile(filename, TGAFlags.None, null, image);
                    break;

                case ".hdr":
                    DirectXTex.LoadFromHDRFile(filename, null, image);
                    break;

                default:
                    DirectXTex.LoadFromWICFile(filename, WICFlags.None, null, image, default);
                    break;
            };

            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFromMemory(string filename, Stream stream)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            string extension = Path.GetExtension(filename);

            fixed (byte* p = data)
            {
                switch (extension)
                {
                    case ".dds":
                        DirectXTex.LoadFromDDSMemory(p, (nuint)data.Length, DDSFlags.None, null, image).ThrowHResult();
                        break;

                    case ".tga":
                        DirectXTex.LoadFromTGAMemory(p, (nuint)data.Length, TGAFlags.None, null, image).ThrowHResult();
                        break;

                    case ".hdr":
                        DirectXTex.LoadFromHDRMemory(p, (nuint)data.Length, null, image).ThrowHResult();
                        break;

                    default:
                        DirectXTex.LoadFromWICMemory(p, (nuint)data.Length, WICFlags.None, null, image, default).ThrowHResult();
                        break;
                }
            };

            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFromMemory(Core.Graphics.Textures.TexFileFormat format, Stream stream)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            fixed (byte* p = data)
            {
                switch (format)
                {
                    case Core.Graphics.Textures.TexFileFormat.DDS:
                        DirectXTex.LoadFromDDSMemory(p, (nuint)data.Length, DDSFlags.None, null, image);
                        break;

                    case Core.Graphics.Textures.TexFileFormat.TGA:
                        DirectXTex.LoadFromTGAMemory(p, (nuint)data.Length, TGAFlags.None, null, image);
                        break;

                    case Core.Graphics.Textures.TexFileFormat.HDR:
                        DirectXTex.LoadFromHDRMemory(p, (nuint)data.Length, null, image);
                        break;

                    default:
                        DirectXTex.LoadFromWICMemory(p, (nuint)data.Length, WICFlags.None, null, image, default);
                        break;
                }
            };

            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFromMemory(Core.Graphics.Textures.TexFileFormat format, byte* data, nuint length)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();

            switch (format)
            {
                case Core.Graphics.Textures.TexFileFormat.DDS:
                    DirectXTex.LoadFromDDSMemory(data, length, DDSFlags.None, null, image);
                    break;

                case Core.Graphics.Textures.TexFileFormat.TGA:
                    DirectXTex.LoadFromTGAMemory(data, length, TGAFlags.None, null, image);
                    break;

                case Core.Graphics.Textures.TexFileFormat.HDR:
                    DirectXTex.LoadFromHDRMemory(data, length, null, image);
                    break;

                default:
                    DirectXTex.LoadFromWICMemory(data, length, WICFlags.None, null, image, default);
                    break;
            };

            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize(Core.Graphics.Textures.TexMetadata metadata, Core.Graphics.Textures.CPFlags flags)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            image.Initialize(Helper.Convert(metadata), Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize1D(Format fmt, int length, int arraySize, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            image.Initialize1D((int)Helper.Convert(fmt), (nuint)length, (nuint)arraySize, (nuint)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize2D(Format fmt, int width, int height, int arraySize, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            image.Initialize2D((int)Helper.Convert(fmt), (nuint)width, (nuint)height, (nuint)arraySize, (nuint)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize3D(Format fmt, int width, int height, int depth, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            image.Initialize3D((int)Helper.Convert(fmt), (nuint)width, (nuint)height, (nuint)depth, (nuint)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize3DFromImages(IImage[] images, int depth, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            Image[] images1 = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                images1[i] = ((D3DImage)images[i]).Image;
            }
            ScratchImage image = DirectXTex.CreateScratchImage();
            fixed (Image* pImages = images1)
            {
                image.Initialize3DFromImages(pImages, (nuint)depth, Helper.Convert(flags));
            }

            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeArrayFromImages(IImage[] images, bool allow1D = false, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            Image[] images1 = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                images1[i] = ((D3DImage)images[i]).Image;
            }
            ScratchImage image = DirectXTex.CreateScratchImage();
            fixed (Image* pImages = images1)
            {
                image.InitializeArrayFromImages(pImages, (nuint)images.Length, allow1D, Helper.Convert(flags));
            }

            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeCube(Format fmt, int width, int height, int nCubes, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = DirectXTex.CreateScratchImage();
            image.InitializeCube((int)Helper.Convert(fmt), (nuint)width, (nuint)height, (nuint)nCubes, (nuint)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeCubeFromImages(IImage[] images, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            Image[] images1 = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                images1[i] = ((D3DImage)images[i]).Image;
            }
            ScratchImage image = DirectXTex.CreateScratchImage();
            fixed (Image* pImages = images1)
            {
                image.InitializeCubeFromImages(pImages, (nuint)images.Length, Helper.Convert(flags));
            }

            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeFromImage(IImage image, bool allow1D = false, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage scratchImage = DirectXTex.CreateScratchImage();
            scratchImage.InitializeFromImage(((D3DImage)image).Image, allow1D, Helper.Convert(flags));
            return new D3DScratchImage(scratchImage);
        }

        public ITexture1D LoadTexture1D(string path, Core.Graphics.Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, Core.Graphics.ResourceMiscFlag misc)
        {
            var image = LoadFormAssets(path, TextureDimension.Texture1D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture1D(device, usage, bind, cpuAccess, misc);

            image.Dispose();
            return tex;
        }

        public ITexture2D LoadTexture2D(string path, Core.Graphics.Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, Core.Graphics.ResourceMiscFlag misc)
        {
            var image = LoadFormAssets(path, TextureDimension.Texture2D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture2D(device, usage, bind, cpuAccess, misc);

            image.Dispose();
            return tex;
        }

        public ITexture3D LoadTexture3D(string path, Core.Graphics.Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, Core.Graphics.ResourceMiscFlag misc)
        {
            var image = LoadFormAssets(path, TextureDimension.Texture3D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture3D(device, usage, bind, cpuAccess, misc);

            image.Dispose();
            return tex;
        }

        public ITexture1D LoadTexture1D(string path)
        {
            var image = LoadFormAssets(path, TextureDimension.Texture1D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture1D(device, Core.Graphics.Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, Core.Graphics.ResourceMiscFlag.None);

            image.Dispose();
            return tex;
        }

        public ITexture2D LoadTexture2D(string path)
        {
            var image = LoadFormAssets(path, TextureDimension.Texture2D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            Core.Graphics.ResourceMiscFlag miscFlag = 0;
            if (image.Metadata.IsCubemap())
            {
                miscFlag = Core.Graphics.ResourceMiscFlag.TextureCube;
            }

            var tex = image.CreateTexture2D(device, Core.Graphics.Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, miscFlag);

            image.Dispose();
            return tex;
        }

        public ITexture3D LoadTexture3D(string path)
        {
            var image = LoadFormAssets(path, TextureDimension.Texture3D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture3D(device, Core.Graphics.Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, Core.Graphics.ResourceMiscFlag.None);

            image.Dispose();
            return tex;
        }

        public ITexture1D LoadTexture1D(TextureFileDescription desc)
        {
            var image = LoadFormAssets(desc.Path, TextureDimension.Texture1D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1 && desc.MipLevels == 0)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture1D(device, desc.Usage, desc.BindFlags, desc.CPUAccessFlags, desc.Dimension == TextureDimension.TextureCube ? Core.Graphics.ResourceMiscFlag.TextureCube : Core.Graphics.ResourceMiscFlag.None);

            image.Dispose();
            return tex;
        }

        public ITexture2D LoadTexture2D(TextureFileDescription desc)
        {
            IScratchImage image;
            if (Path.IsPathFullyQualified(desc.Path))
            {
                image = LoadFormFile(desc.Path, desc.Dimension);
            }
            else
            {
                image = LoadFormAssets(desc.Path, desc.Dimension);
            }
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1 && desc.MipLevels == 0)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture2D(device, desc.Usage, desc.BindFlags, desc.CPUAccessFlags, desc.Dimension == TextureDimension.TextureCube ? Core.Graphics.ResourceMiscFlag.TextureCube : Core.Graphics.ResourceMiscFlag.None);

            image.Dispose();
            return tex;
        }

        public ITexture3D LoadTexture3D(TextureFileDescription desc)
        {
            var image = LoadFormAssets(desc.Path, TextureDimension.Texture3D);
            if ((flags & TextureLoaderFlags.Scale) != 0 && scalingFactor != 1)
            {
                var tmp = image.Resize(scalingFactor, Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }
            if ((flags & TextureLoaderFlags.GenerateMipMaps) != 0 && image.Metadata.MipLevels == 1 && image.Metadata.Width > 1 && image.Metadata.Height > 1 && desc.MipLevels == 0)
            {
                var tmp = image.GenerateMipMaps(Core.Graphics.Textures.TexFilterFlags.Default);
                image.Dispose();
                image = tmp;
            }

            var tex = image.CreateTexture3D(device, desc.Usage, desc.BindFlags, desc.CPUAccessFlags, desc.Dimension == TextureDimension.TextureCube ? Core.Graphics.ResourceMiscFlag.TextureCube : Core.Graphics.ResourceMiscFlag.None);

            image.Dispose();
            return tex;
        }
    }
}