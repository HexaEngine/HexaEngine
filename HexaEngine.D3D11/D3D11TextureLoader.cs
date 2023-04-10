namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Lights;
    using HexaEngine.DirectXTex;
    using Silk.NET.Direct3D11;
    using System.IO;
    using System.Runtime.Intrinsics.X86;

    public unsafe class D3D11TextureLoader : ITextureLoader
    {
        public IScratchImage CaptureTexture(IGraphicsDevice device, IGraphicsContext context, IResource resource)
        {
            ScratchImage image = new();
            DirectXTex.CaptureTexture((ID3D11Device*)device.NativePointer, (ID3D11DeviceContext*)context.NativePointer, (ID3D11Resource*)resource.NativePointer, &image);

            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFormAssets(string filename)
        {
            if (!FileSystem.TryOpen(filename, out VirtualStream? fs))
                return default;

            ScratchImage image = new();
            var data = fs.ReadBytes();
            string extension = Path.GetExtension(filename);
            switch (extension)
            {
                case ".dds":
                    DirectXTex.LoadFromDDSMemory(data, DDSFlags.None, &image);
                    break;

                case ".tga":
                    DirectXTex.LoadFromTGAMemory(data, TGAFlags.None, &image);
                    break;

                case ".hdr":
                    DirectXTex.LoadFromHDRMemory(data, &image);
                    break;

                default:
                    DirectXTex.LoadFromWICMemory(data, WICFlags.None, &image, null);
                    break;
            };
            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFormFile(string filename)
        {
            ScratchImage image = new();
            string extension = Path.GetExtension(filename);
            switch (extension)
            {
                case ".dds":
                    DirectXTex.LoadFromDDSFile(filename, DDSFlags.None, &image);
                    break;

                case ".tga":
                    DirectXTex.LoadFromTGAFile(filename, TGAFlags.None, &image);
                    break;

                case ".hdr":
                    DirectXTex.LoadFromHDRFile(filename, &image);
                    break;

                default:
                    DirectXTex.LoadFromWICFile(filename, WICFlags.None, &image, null);
                    break;
            };

            return new D3DScratchImage(image);
        }

        public IScratchImage LoadFromMemory(string filename, Stream stream)
        {
            ScratchImage image = new();
            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            string extension = Path.GetExtension(filename);
            switch (extension)
            {
                case ".dds":
                    DirectXTex.LoadFromDDSMemory(data, DDSFlags.None, &image);
                    break;

                case ".tga":
                    DirectXTex.LoadFromTGAMemory(data, TGAFlags.None, &image);
                    break;

                case ".hdr":
                    DirectXTex.LoadFromHDRMemory(data, &image);
                    break;

                default:
                    DirectXTex.LoadFromWICMemory(data, WICFlags.None, &image, null);
                    break;
            };

            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize(Core.Graphics.Textures.TexMetadata metadata, Core.Graphics.Textures.CPFlags flags)
        {
            ScratchImage image = new();
            image.Initialize(Helper.Convert(metadata), Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize1D(Format fmt, int length, int arraySize, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = new();
            image.Initialize1D(Helper.Convert(fmt), (ulong)length, (ulong)arraySize, (ulong)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize2D(Format fmt, int width, int height, int arraySize, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = new();
            image.Initialize2D(Helper.Convert(fmt), (ulong)width, (ulong)height, (ulong)arraySize, (ulong)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize3D(Format fmt, int width, int height, int depth, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = new();
            image.Initialize3D(Helper.Convert(fmt), (ulong)width, (ulong)height, (ulong)depth, (ulong)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage Initialize3DFromImages(IImage[] images, int depth, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            Image[] images1 = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                images1[i] = ((D3DImage)images[i]).Image;
            }
            ScratchImage image = new();
            fixed (Image* pImages = images1)
                image.Initialize3DFromImages(pImages, (ulong)depth, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeArrayFromImages(IImage[] images, bool allow1D = false, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            Image[] images1 = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                images1[i] = ((D3DImage)images[i]).Image;
            }
            ScratchImage image = new();
            fixed (Image* pImages = images1)
                image.InitializeArrayFromImages(pImages, (ulong)images.Length, allow1D, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeCube(Format fmt, int width, int height, int nCubes, int mipLevels, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage image = new();
            image.InitializeCube(Helper.Convert(fmt), (ulong)width, (ulong)height, (ulong)nCubes, (ulong)mipLevels, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeCubeFromImages(IImage[] images, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            Image[] images1 = new Image[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                images1[i] = ((D3DImage)images[i]).Image;
            }
            ScratchImage image = new();
            fixed (Image* pImages = images1)
                image.InitializeCubeFromImages(pImages, (ulong)images.Length, Helper.Convert(flags));
            return new D3DScratchImage(image);
        }

        public IScratchImage InitializeFromImage(IImage image, bool allow1D = false, Core.Graphics.Textures.CPFlags flags = Core.Graphics.Textures.CPFlags.None)
        {
            ScratchImage scratchImage = new();
            scratchImage.InitializeFromImage(((D3DImage)image).Image, allow1D, Helper.Convert(flags));
            return new D3DScratchImage(scratchImage);
        }
    }
}