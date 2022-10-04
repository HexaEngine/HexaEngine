namespace HexaEngine.DirectXTex
{
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.DXGI;
    using System;
    using System.Runtime.CompilerServices;

    public static unsafe class DirectXTex
    {
        #region DXGI Format Utilities

        public static bool IsValid(Format format)
        {
            return Native.IsValid(format) != 0;
        }

        public static bool IsCompressed(Format format)
        {
            return Native.IsCompressed(format) != 0;
        }

        public static bool IsPacked(Format format)
        {
            return Native.IsPacked(format) != 0;
        }

        public static bool IsVideo(Format format)
        {
            return Native.IsVideo(format) != 0;
        }

        public static bool IsPlanar(Format format)
        {
            return Native.IsPlanar(format) != 0;
        }

        public static bool IsPalettized(Format format)
        {
            return Native.IsPalettized(format) != 0;
        }

        public static bool IsDepthStencil(Format format)
        {
            return Native.IsDepthStencil(format) != 0;
        }

        public static bool IsSRGB(Format format)
        {
            return Native.IsSRGB(format) != 0;
        }

        public static bool IsTypeless(Format format, bool partialTypeless = true)
        {
            return Native.IsTypeless(format, partialTypeless) != 0;
        }

        public static bool HasAlpha(Format format)
        {
            return Native.HasAlpha(format) != 0;
        }

        public static ulong BitsPerPixel(Format format)
        {
            return Native.BitsPerPixel(format);
        }

        public static ulong BitsPerColor(Format format)
        {
            return Native.BitsPerColor(format);
        }

        public static FormatType FormatDataType(Format format)
        {
            return Native.FormatDataType(format);
        }

        public static HResult ComputePitch(Format format, ulong width, ulong height, ref ulong rowPitch, ref ulong slicePitch, CPFlags flags)
        {
            return Native.ComputePitch(format, width, height, (ulong*)Unsafe.AsPointer(ref rowPitch), (ulong*)Unsafe.AsPointer(ref slicePitch), flags);
        }

        public static ulong ComputeScanlines(Format format, ulong height)
        {
            return Native.ComputeScanlines(format, height);
        }

        public static Format MakeSRGB(Format format)
        {
            return Native.MakeSRGB(format);
        }

        public static Format MakeTypeless(Format format)
        {
            return Native.MakeTypeless(format);
        }

        public static Format MakeTypelessUNORM(Format format)
        {
            return Native.MakeTypelessUNORM(format);
        }

        public static Format MakeTypelessFLOAT(Format format)
        {
            return Native.MakeTypelessFLOAT(format);
        }

        #endregion DXGI Format Utilities

        public static void Compress(Image* srcImage, Format fmt, TexCompressFlags flags, float threshold, ScratchImage* image)
        {
            Native.Compress(srcImage, fmt, flags, threshold, image->pScratchImage);
        }

        public static void Compress(Image* srcImages, ulong nImages, TexMetadata* metadata, Format fmt, TexCompressFlags flags, float threshold, ScratchImage* image)
        {
            Native.Compress2(srcImages, nImages, metadata, fmt, flags, threshold, image->pScratchImage);
        }

        public static void Compress(ScratchImage* srcImage, Format fmt, TexCompressFlags flags, float alphaWeight, ScratchImage* image)
        {
            TexMetadata metadata = srcImage->GetMetadata();
            ulong nImages = srcImage->GetImageCount();
            Image* srcImages = srcImage->GetImages();
            Native.Compress2(srcImages, nImages, &metadata, fmt, flags, alphaWeight, image->pScratchImage);
        }

        public static void Compress(ID3D11Device* device, Image* srcImage, Format fmt, TexCompressFlags flags, float alphaWeight, ScratchImage* image)
        {
            Native.Compress3(device, srcImage, fmt, flags, alphaWeight, image->pScratchImage);
        }

        public static void Compress(ID3D11Device* device, ScratchImage* srcImage, Format fmt, TexCompressFlags flags, float alphaWeight, ScratchImage* image)
        {
            TexMetadata metadata = srcImage->GetMetadata();
            ulong nImages = srcImage->GetImageCount();
            Image* srcImages = srcImage->GetImages();
            HResult result = Native.Compress4(device, srcImages, nImages, &metadata, fmt, flags, alphaWeight, image->pScratchImage);
        }

        public static void Convert(Image* srcImage, Format fmt, TexFilterFlags flags, float threshold, ScratchImage* image)
        {
            Native.Convert(srcImage, fmt, flags, threshold, image->pScratchImage);
        }

        public static void Convert(Image* srcImages, ulong nImages, TexMetadata* metadata, Format fmt, TexFilterFlags flags, float threshold, ScratchImage* image)
        {
            Native.Convert2(srcImages, nImages, metadata, fmt, flags, threshold, image->pScratchImage);
        }

        public static void Convert(ScratchImage* srcImage, Format fmt, TexFilterFlags flags, float threshold, ScratchImage* image)
        {
            TexMetadata metadata = srcImage->GetMetadata();
            ulong nImages = srcImage->GetImageCount();
            Image* srcImages = srcImage->GetImages();
            Native.Convert2(srcImages, nImages, &metadata, fmt, flags, threshold, image->pScratchImage);
        }

        public static void LoadFromDDSMemory(byte[] data, DDSFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromDDSMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage);
            }
        }

        public static void LoadFromDDSMemory(Span<byte> data, DDSFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromDDSMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage);
            }
        }

        public static void LoadFromDDSMemory(void* pSource, ulong size, DDSFlags flags, ScratchImage* image)
        {
            Native.LoadFromDDSMemory(pSource, size, flags, null, image->pScratchImage);
        }

        public static void LoadFromTGAMemory(byte[] data, TGAFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromTGAMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage);
            }
        }

        public static void LoadFromTGAMemory(Span<byte> data, TGAFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromTGAMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage);
            }
        }

        public static void LoadFromTGAMemory(void* pSource, ulong size, TGAFlags flags, ScratchImage* image)
        {
            Native.LoadFromTGAMemory(pSource, size, flags, null, image->pScratchImage);
        }

        public static void LoadFromHDRMemory(byte[] data, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromHDRMemory(ptr, (ulong)data.Length, null, image->pScratchImage);
            }
        }

        public static void LoadFromHDRMemory(Span<byte> data, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromHDRMemory(ptr, (ulong)data.Length, null, image->pScratchImage);
            }
        }

        public static void LoadFromHDRMemory(void* pSource, ulong size, ScratchImage* image)
        {
            Native.LoadFromHDRMemory(pSource, size, null, image->pScratchImage);
        }

        public static void LoadFromWICMemory(byte[] data, WICFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromWICMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage);
            }
        }

        public static void LoadFromWICMemory(Span<byte> data, WICFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromWICMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage);
            }
        }

        public static void LoadFromWICMEMORY(void* pSource, ulong size, WICFlags flags, ScratchImage* image)
        {
            Native.LoadFromWICMemory(pSource, size, flags, null, image->pScratchImage);
        }

        public static void CreateTextureEx(ID3D11Device* device, ScratchImage* image, Usage usage, BindFlag bind, CpuAccessFlag cpu, ResourceMiscFlag misc, bool forceSRGB, ID3D11Resource** resource)
        {
            HResult result = Native.CreateTextureEx2(device, image->pScratchImage, (uint)usage, (uint)bind, (uint)cpu, (uint)misc, forceSRGB, resource);
        }

        public static void CreateTextureEx(ID3D11Device* device, Image* images, ulong nImages, TexMetadata* metadata, Usage usage, BindFlag bind, CpuAccessFlag cpu, ResourceMiscFlag misc, bool forceSRGB, ID3D11Resource** resource)
        {
            Native.CreateTextureEx(device, images, nImages, metadata, usage, (uint)bind, (uint)cpu, (uint)misc, forceSRGB, resource);
        }

        public static void CaptureTexture(ID3D11Device* device, ID3D11DeviceContext* context, ID3D11Resource* resource, ScratchImage* image)
        {
            Native.CaptureTexture(device, context, resource, image->pScratchImage);
        }

        public static void SaveToDDSMemory(Image* image, DDSFlags flags, TexBlob* blob)
        {
            Native.SaveToDDSMemory(image, flags, blob);
        }

        public static void SaveToDDSMemory(Image* images, ulong nImages, TexMetadata* metadata, DDSFlags flags, TexBlob* blob)
        {
            Native.SaveToDDSMemory2(images, nImages, metadata, flags, blob);
        }

        public static void SaveToDDSMemory(ScratchImage* image, DDSFlags flags, TexBlob* blob)
        {
            TexMetadata metadata = image->GetMetadata();
            ulong nImages = image->GetImageCount();
            Image* images = image->GetImages();
            Native.SaveToDDSMemory2(images, nImages, &metadata, flags, blob);
        }

        public static void SaveToDDSFile(Image* image, DDSFlags flags, string path)
        {
            Native.SaveToDDSFile(image, flags, path);
        }

        public static void SaveToDDSFile(Image* images, ulong nImages, TexMetadata* metadata, DDSFlags flags, string path)
        {
            Native.SaveToDDSFile2(images, nImages, metadata, flags, path);
        }

        public static void SaveToDDSFile(ScratchImage* image, DDSFlags flags, string path)
        {
            TexMetadata metadata = image->GetMetadata();
            ulong nImages = image->GetImageCount();
            Image* images = image->GetImages();
            Native.SaveToDDSFile2(images, nImages, &metadata, flags, path);
        }

        public static void SaveToTGAFile(Image* image, TGAFlags flags, string path)
        {
            Native.SaveToTGAFile(image, flags, path);
        }

        public static void SaveToTGAFile(ScratchImage* image, int index, TGAFlags flags, string path)
        {
            Image* images = image->GetImages();
            Native.SaveToTGAFile(&images[index], flags, path);
        }

        public static void SaveToHDRFile(Image* image, string path)
        {
            Native.SaveToHDRFile(image, path);
        }

        public static void SaveToHDRFile(ScratchImage* image, int index, string path)
        {
            Image* images = image->GetImages();
            Native.SaveToHDRFile(&images[index], path);
        }

        public static void SaveToWICFile(Image* image, WICFlags flags, Guid containerGuid, string path)
        {
            Native.SaveToWICFile(image, flags, &containerGuid, path);
        }

        public static void SaveToWICFile(ScratchImage* image, int index, WICFlags flags, Guid* containerGuid, string path)
        {
            Image* images = image->GetImages();
            Native.SaveToWICFile(&images[index], flags, containerGuid, path);
        }

        public static Guid* GetWICCodec(WICCodecs codecs)
        {
            return Native.GetWICCodec(codecs);
        }
    }
}