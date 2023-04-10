using HexaEngine.Core.Graphics.Textures;

namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.DirectXTex;
    using HexaEngine.Mathematics;
    using Silk.NET.Direct3D11;
    using System.IO;
    using ResourceMiscFlag = Core.Graphics.ResourceMiscFlag;
    using TexCompressFlags = TexCompressFlags;
    using TexFilterFlags = TexFilterFlags;
    using Usage = Core.Graphics.Usage;

    public unsafe class D3DScratchImage : IScratchImage
    {
        private bool _disposed;
        private ScratchImage scImage;

        public D3DScratchImage(ScratchImage outScImage)
        {
            scImage = outScImage;
        }

        public Core.Graphics.Textures.TexMetadata Metadata => Helper.ConvertBack(scImage.GetMetadata());

        public int ImageCount => (int)scImage.GetImageCount();

        public IScratchImage Compress(IGraphicsDevice device, Format format, TexCompressFlags flags)
        {
            D3D11GraphicsDevice graphicsDevice = (D3D11GraphicsDevice)device;
            ScratchImage inScImage = scImage;
            ScratchImage outScImage = new();
            DirectXTex.Compress((ID3D11Device*)graphicsDevice.Device, &inScImage, Helper.Convert(format), Helper.Convert(flags), 0, &outScImage);
            return new D3DScratchImage(outScImage);
        }

        public IScratchImage Decompress(Format format)
        {
            ScratchImage inScImage = scImage;
            ScratchImage outScImage = new();
            DirectXTex.Decompress(&inScImage, Helper.Convert(format), &outScImage);
            return new D3DScratchImage(outScImage);
        }

        public IScratchImage Convert(Format format, TexFilterFlags flags)
        {
            ScratchImage inScImage = scImage;
            ScratchImage outScImage = new();
            DirectXTex.Convert(&inScImage, Helper.Convert(format), Helper.Convert(flags), 0, &outScImage);
            return new D3DScratchImage(outScImage);
        }

        public IScratchImage GenerateMipMaps(TexFilterFlags flags)
        {
            ScratchImage inScImage = scImage;
            ScratchImage outScImage = new();
            DirectXTex.GenerateMipMaps(&inScImage, Helper.Convert(flags), (ulong)MathUtil.ComputeMipLevels(Metadata.Width, Metadata.Height), &outScImage);
            return new D3DScratchImage(outScImage);
        }

        public IScratchImage Resize(float scale, TexFilterFlags flags)
        {
            ScratchImage inScImage = scImage;
            ScratchImage outScImage = new();
            DirectXTex.Resize(&inScImage, (ulong)(Metadata.Width * scale), (ulong)(Metadata.Height * scale), Helper.Convert(flags), &outScImage);
            return new D3DScratchImage(outScImage);
        }

        public IScratchImage Resize(int width, int height, TexFilterFlags flags)
        {
            ScratchImage inScImage = scImage;
            ScratchImage outScImage;
            DirectXTex.Resize(&inScImage, (ulong)width, (ulong)height, Helper.Convert(flags), &outScImage);
            return new D3DScratchImage(outScImage);
        }

        public ITexture1D CreateTexture1D(IGraphicsDevice device, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag)
        {
            ScratchImage inScImage = scImage;
            D3D11GraphicsDevice graphicsDevice = (D3D11GraphicsDevice)device;
            ID3D11Resource* resource;
            DirectXTex.CreateTextureEx((ID3D11Device*)graphicsDevice.Device, &inScImage, Helper.Convert(usage), Helper.Convert(bindFlags), Helper.Convert(accessFlags), Helper.Convert(miscFlag), false, &resource);
            Texture1DDescription texture = new(
                            Metadata.Format,
                            Metadata.Width,
                            Metadata.ArraySize,
                            Metadata.MipLevels,
                            bindFlags,
                            usage,
                            accessFlags,
                            miscFlag);

            return new D3D11Texture1D((ID3D11Texture1D*)resource, texture);
        }

        public ITexture2D CreateTexture2D(IGraphicsDevice device, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag)
        {
            ScratchImage inScImage = scImage;
            D3D11GraphicsDevice graphicsDevice = (D3D11GraphicsDevice)device;
            ID3D11Resource* resource;
            DirectXTex.CreateTextureEx((ID3D11Device*)graphicsDevice.Device, &inScImage, Helper.Convert(usage), Helper.Convert(bindFlags), Helper.Convert(accessFlags), Helper.Convert(miscFlag), false, &resource);
            Texture2DDescription texture = new(
                            Metadata.Format,
                            Metadata.Width,
                            Metadata.Height,
                            Metadata.ArraySize,
                            Metadata.MipLevels,
                            bindFlags,
                            usage,
                            accessFlags,
                            1,
                            0,
                            miscFlag);

            return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
        }

        public ITexture2D CreateTexture2D(IGraphicsDevice device, int index, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag)
        {
            ScratchImage inScImage = scImage;
            D3D11GraphicsDevice graphicsDevice = (D3D11GraphicsDevice)device;
            ID3D11Resource* resource;
            var images = inScImage.GetImages();
            var metadata = inScImage.GetMetadata();
            var image = images[index];
            metadata.Width = image.Width;
            metadata.Height = image.Height;
            metadata.ArraySize = 1;
            metadata.MipLevels = 1;
            metadata.MiscFlags = TexMiscFlags.None;
            DirectXTex.CreateTextureEx((ID3D11Device*)graphicsDevice.Device, &image, 1, &metadata, Helper.Convert(usage), Helper.Convert(bindFlags), Helper.Convert(accessFlags), Helper.Convert(miscFlag), false, &resource);
            Texture2DDescription texture = new(
                            Helper.ConvertBack(metadata.Format),
                            (int)metadata.Width,
                            (int)metadata.Height,
                            (int)metadata.ArraySize,
                            (int)metadata.MipLevels,
                            bindFlags,
                            usage,
                            accessFlags,
                            1,
                            0,
                            miscFlag);

            return new D3D11Texture2D((ID3D11Texture2D*)resource, texture);
        }

        public ITexture3D CreateTexture3D(IGraphicsDevice device, Usage usage, BindFlags bindFlags, CpuAccessFlags accessFlags, ResourceMiscFlag miscFlag)
        {
            ScratchImage inScImage = scImage;
            D3D11GraphicsDevice graphicsDevice = (D3D11GraphicsDevice)device;
            ID3D11Resource* resource;
            DirectXTex.CreateTextureEx((ID3D11Device*)graphicsDevice.Device, &inScImage, Helper.Convert(usage), Helper.Convert(bindFlags), Helper.Convert(accessFlags), Helper.Convert(miscFlag), false, &resource);
            Texture3DDescription texture = new(
                            Metadata.Format,
                            Metadata.Width,
                            Metadata.Height,
                            Metadata.Depth,
                            Metadata.MipLevels,
                            bindFlags,
                            usage,
                            accessFlags,
                            miscFlag);

            return new D3D11Texture3D((ID3D11Texture3D*)resource, texture);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                scImage.Release();
                scImage = default;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void SaveToFile(string path, TexFileFormat format, int flags)
        {
            ScratchImage image = scImage;
            switch (format)
            {
                case TexFileFormat.DDS:
                    DirectXTex.SaveToDDSFile(&image, (DDSFlags)flags, path);
                    break;

                case TexFileFormat.TGA:
                    DirectXTex.SaveToTGAFile(&image, 0, (TGAFlags)flags, path);
                    break;

                case TexFileFormat.HDR:
                    DirectXTex.SaveToHDRFile(&image, 0, path);
                    break;

                default:
                    DirectXTex.SaveToWICFile(&image, 0, (WICFlags)flags, DirectXTex.GetWICCodec(Helper.Convert(format)), path);
                    break;
            }
        }

        public void SaveToMemory(Stream stream, TexFileFormat format, int flags)
        {
            ScratchImage image = scImage;
            TexBlob blob;
            switch (format)
            {
                case TexFileFormat.DDS:
                    DirectXTex.SaveToDDSMemory(&image, (DDSFlags)flags, &blob);
                    break;

                case TexFileFormat.TGA:
                    DirectXTex.SaveToTGAMemory(&image, 0, (TGAFlags)flags, &blob);
                    break;

                case TexFileFormat.HDR:
                    DirectXTex.SaveToHDRMemory(&image, 0, &blob);
                    break;

                default:
                    DirectXTex.SaveToWICMemory(&image, (WICFlags)flags, DirectXTex.GetWICCodec(Helper.Convert(format)), &blob);
                    break;
            }

            stream.Write(blob.ToBytes());
            blob.Release();
        }

        public IImage[] GetImages()
        {
            IImage[] images = new IImage[ImageCount];
            var pImages = scImage.GetImages();
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = new D3DImage(pImages[i]);
            }
            return images;
        }
    }
}