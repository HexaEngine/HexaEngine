namespace HexaEngine.DirectXTex
{
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using Silk.NET.Direct3D12;
    using Silk.NET.DXGI;
    using System;
    using System.Runtime.InteropServices;

    public static unsafe class DirectXTex
    {
        #region Internal Utils

        internal static void ThrowIf(this int hresult)
        {
            HResult result = hresult;
            if (!result.IsSuccess)
                result.Throw();
        }

        internal static void ThrowIf(this uint hresult)
        {
            HResult result = unchecked((int)hresult);
            if (!result.IsSuccess)
                result.Throw();
        }

        #endregion Internal Utils

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

        public static HResult ComputePitch(Format format, ulong width, ulong height, ulong* rowPitch, ulong* slicePitch, CPFlags flags)
        {
            return Native.ComputePitch(format, width, height, rowPitch, slicePitch, flags);
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

        #region MetadataIO

        public static unsafe void GetMetadataFromDDSMemory(byte[] data, DDSFlags flags, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromDDSMemory(pSource, size, flags, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromDDSMemory(Span<byte> data, DDSFlags flags, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromDDSMemory(pSource, size, flags, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromDDSMemory(void* pSource, ulong size, DDSFlags flags, TexMetadata* metadata)
        {
            Native.GetMetadataFromDDSMemory(pSource, size, flags, metadata).ThrowIf();
        }

        public static unsafe void GetMetadataFromDDSFile(string szFile, DDSFlags flags, TexMetadata* metadata)
        {
            Native.GetMetadataFromDDSFile(szFile, flags, metadata);
        }

        public static unsafe void GetMetadataFromHDRMemory(byte[] data, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromHDRMemory(pSource, size, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromHDRMemory(Span<byte> data, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromHDRMemory(pSource, size, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromHDRMemory(void* pSource, ulong size, TexMetadata* metadata)
        {
            Native.GetMetadataFromHDRMemory(pSource, size, metadata).ThrowIf();
        }

        public static unsafe void GetMetadataFromHDRFile(string szFile, TexMetadata* metadata)
        {
            Native.GetMetadataFromHDRFile(szFile, metadata).ThrowIf();
        }

        public static unsafe void GetMetadataFromTGAMemory(byte[] data, TGAFlags flags, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromTGAMemory(pSource, size, flags, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromTGAMemory(Span<byte> data, TGAFlags flags, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromTGAMemory(pSource, size, flags, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromTGAMemory(void* pSource, ulong size, TGAFlags flags, TexMetadata* metadata)
        {
            Native.GetMetadataFromTGAMemory(pSource, size, flags, metadata).ThrowIf();
        }

        public static unsafe void GetMetadataFromTGAFile(string szFile, TGAFlags flags, TexMetadata* metadata)
        {
            Native.GetMetadataFromTGAFile(szFile, flags, metadata).ThrowIf();
        }

        public static unsafe void GetMetadataFromWICMemory(byte[] data, WICFlags flags, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromWICMemory(pSource, size, flags, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromWICMemory(Span<byte> data, WICFlags flags, TexMetadata* metadata)
        {
            ulong size = (ulong)data.Length;
            fixed (void* pSource = data)
            {
                Native.GetMetadataFromWICMemory(pSource, size, flags, metadata).ThrowIf();
            }
        }

        public static unsafe void GetMetadataFromWICMemory(void* pSource, ulong size, WICFlags flags, TexMetadata* metadata)
        {
            Native.GetMetadataFromWICMemory(pSource, size, flags, metadata).ThrowIf();
        }

        public static unsafe void GetMetadataFromWICFile(string szFile, WICFlags flags, TexMetadata* metadata)
        {
            Native.GetMetadataFromWICFile(szFile, flags, metadata).ThrowIf();
        }

        #endregion MetadataIO

        #region ImageIO

        #region DDS operations

        public static void LoadFromDDSMemory(byte[] data, DDSFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromDDSMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage).ThrowIf();
            }
        }

        public static void LoadFromDDSMemory(Span<byte> data, DDSFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromDDSMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage).ThrowIf();
            }
        }

        public static void LoadFromDDSFile(string path, DDSFlags flags, ScratchImage* image)
        {
            Native.LoadFromDDSFile(path, flags, null, image->pScratchImage).ThrowIf();
        }

        public static void SaveToDDSMemory(Image* image, DDSFlags flags, TexBlob* blob)
        {
            Native.SaveToDDSMemory(image, flags, blob->pBlob).ThrowIf();
        }

        public static void SaveToDDSMemory(Image* images, ulong nImages, TexMetadata* metadata, DDSFlags flags, TexBlob* blob)
        {
            Native.SaveToDDSMemory2(images, nImages, metadata, flags, blob->pBlob).ThrowIf();
        }

        public static void SaveToDDSMemory(ScratchImage* image, DDSFlags flags, TexBlob* blob)
        {
            TexMetadata metadata = image->GetMetadata();
            ulong nImages = image->GetImageCount();
            Image* images = image->GetImages();
            Native.SaveToDDSMemory2(images, nImages, &metadata, flags, blob->pBlob).ThrowIf();
        }

        public static void SaveToDDSFile(Image* image, DDSFlags flags, string path)
        {
            Native.SaveToDDSFile(image, flags, path).ThrowIf();
        }

        public static void SaveToDDSFile(Image* images, ulong nImages, TexMetadata* metadata, DDSFlags flags, string path)
        {
            Native.SaveToDDSFile2(images, nImages, metadata, flags, path).ThrowIf();
        }

        public static void SaveToDDSFile(ScratchImage* image, DDSFlags flags, string path)
        {
            TexMetadata metadata = image->GetMetadata();
            ulong nImages = image->GetImageCount();
            Image* images = image->GetImages();
            Native.SaveToDDSFile2(images, nImages, &metadata, flags, path).ThrowIf();
        }

        #endregion DDS operations

        #region HDR operations

        public static void LoadFromHDRMemory(byte[] data, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromHDRMemory(ptr, (ulong)data.Length, null, image->pScratchImage).ThrowIf();
            }
        }

        public static void LoadFromHDRMemory(Span<byte> data, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromHDRMemory(ptr, (ulong)data.Length, null, image->pScratchImage).ThrowIf();
            }
        }

        public static void LoadFromHDRMemory(void* pSource, ulong size, ScratchImage* image)
        {
            Native.LoadFromHDRMemory(pSource, size, null, image->pScratchImage).ThrowIf();
        }

        public static void LoadFromHDRFile(string path, ScratchImage* image)
        {
            Native.LoadFromHDRFile(path, null, image->pScratchImage).ThrowIf();
        }

        public static void SaveToHDRMemory(Image* image, TexBlob* blob)
        {
            Native.SaveToHDRMemory(image, blob->pBlob).ThrowIf();
        }

        public static void SaveToHDRMemory(ScratchImage* image, int index, TexBlob* blob)
        {
            Native.SaveToHDRMemory(&image->GetImages()[index], blob->pBlob).ThrowIf();
        }

        public static void SaveToHDRFile(Image* image, string path)
        {
            Native.SaveToHDRFile(image, path).ThrowIf();
        }

        public static void SaveToHDRFile(ScratchImage* image, int index, string path)
        {
            Native.SaveToHDRFile(&image->GetImages()[index], path).ThrowIf();
        }

        #endregion HDR operations

        #region TGA operations

        public static void LoadFromTGAMemory(byte[] data, TGAFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromTGAMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage).ThrowIf();
            }
        }

        public static void LoadFromTGAMemory(Span<byte> data, TGAFlags flags, ScratchImage* image)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromTGAMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage).ThrowIf();
            }
        }

        public static void LoadFromTGAMemory(void* pSource, ulong size, TGAFlags flags, ScratchImage* image)
        {
            Native.LoadFromTGAMemory(pSource, size, flags, null, image->pScratchImage).ThrowIf();
        }

        public static void LoadFromTGAFile(string path, TGAFlags flags, ScratchImage* image)
        {
            Native.LoadFromTGAFile(path, flags, null, image->pScratchImage).ThrowIf();
        }

        public static void SaveToTGAMemory(Image* image, TGAFlags flags, TexBlob* blob)
        {
            Native.SaveToTGAMemory(image, flags, blob->pBlob).ThrowIf();
        }

        public static void SaveToTGAMemory(ScratchImage* image, int index, TGAFlags flags, TexBlob* blob)
        {
            Native.SaveToTGAMemory(&image->GetImages()[index], flags, blob->pBlob).ThrowIf();
        }

        public static void SaveToTGAFile(Image* image, TGAFlags flags, string path)
        {
            Native.SaveToTGAFile(image, flags, path).ThrowIf();
        }

        public static void SaveToTGAFile(ScratchImage* image, int index, TGAFlags flags, string path)
        {
            Native.SaveToTGAFile(&image->GetImages()[index], flags, path).ThrowIf();
        }

        #endregion TGA operations

        #region WIC operations

        public static void LoadFromWICMemory(byte[] data, WICFlags flags, ScratchImage* image, GetMQR? getMQR = null)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromWICMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage, getMQR).ThrowIf();
            }
        }

        public static void LoadFromWICMemory(Span<byte> data, WICFlags flags, ScratchImage* image, GetMQR? getMQR = null)
        {
            fixed (byte* ptr = data)
            {
                Native.LoadFromWICMemory(ptr, (ulong)data.Length, flags, null, image->pScratchImage, getMQR).ThrowIf();
            }
        }

        public static void LoadFromWICMemory(void* pSource, ulong size, WICFlags flags, ScratchImage* image, GetMQR? getMQR = null)
        {
            Native.LoadFromWICMemory(pSource, size, flags, null, image->pScratchImage, getMQR).ThrowIf();
        }

        public static void LoadFromWICFile(string path, WICFlags flags, ScratchImage* image, GetMQR? getMQR = null)
        {
            Native.LoadFromWICFile(path, flags, null, image->pScratchImage, getMQR).ThrowIf();
        }

        public static void SaveToWICMemory(Image* image, WICFlags flags, Guid* containerGuid, TexBlob* blob, Guid* targetFormat = null, SetCustomProps? setCustomProps = null)
        {
            Native.SaveToWICMemory(image, flags, containerGuid, blob->pBlob, targetFormat, setCustomProps).ThrowIf();
        }

        public static void SaveToWICMemory(Image* images, ulong nImages, WICFlags flags, Guid* containerGuid, TexBlob* blob, Guid* targetFormat = null, SetCustomProps? setCustomProps = null)
        {
            Native.SaveToWICMemory2(images, nImages, flags, containerGuid, blob->pBlob, targetFormat, setCustomProps).ThrowIf();
        }

        public static void SaveToWICMemory(ScratchImage* image, WICFlags flags, Guid* containerGuid, TexBlob* blob, Guid* targetFormat = null, SetCustomProps? setCustomProps = null)
        {
            ulong nImages = image->GetImageCount();
            Image* images = image->GetImages();
            Native.SaveToWICMemory2(images, nImages, flags, containerGuid, blob->pBlob, targetFormat, setCustomProps).ThrowIf();
        }

        public static void SaveToWICFile(Image* image, WICFlags flags, Guid containerGuid, string path, Guid* targetFormat = null, SetCustomProps? setCustomProps = null)
        {
            Native.SaveToWICFile(image, flags, &containerGuid, path, targetFormat, setCustomProps).ThrowIf();
        }

        public static void SaveToWICFile(ScratchImage* image, int index, WICFlags flags, Guid* containerGuid, string path, Guid* targetFormat = null, SetCustomProps? setCustomProps = null)
        {
            Native.SaveToWICFile(&image->GetImages()[index], flags, containerGuid, path, targetFormat, setCustomProps).ThrowIf();
        }

        public static void SaveToWICFile(ScratchImage* image, WICFlags flags, Guid* containerGuid, string path, Guid* targetFormat = null, SetCustomProps? setCustomProps = null)
        {
            Native.SaveToWICFile2(image->GetImages(), image->GetImageCount(), flags, containerGuid, path, targetFormat, setCustomProps).ThrowIf();
        }

        #endregion WIC operations

        #endregion ImageIO

        #region Texture conversion, resizing, mipmap generation, and block compression

        public static unsafe void FlipRotate(Image* srcImage, TexFrFlags flags, ScratchImage* image)
        {
            Native.FlipRotate(srcImage, flags, image->pScratchImage).ThrowIf();
        }

        public static unsafe void FlipRotate(ScratchImage* srcImage, TexFrFlags flags, ScratchImage* result)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.FlipRotate2(srcImages, nimages, &metadata, flags, result->pScratchImage).ThrowIf();
        }

        public static unsafe void FlipRotate(Image* srcImages, ulong nimages, TexMetadata* metadata, TexFrFlags flags, ScratchImage* result)
        {
            Native.FlipRotate2(srcImages, nimages, metadata, flags, result->pScratchImage).ThrowIf();
        }

        // Flip and/or rotate image

        public static unsafe void Resize(Image* srcImage, ulong width, ulong height, TexFilterFlags filter, ScratchImage* image)
        {
            Native.Resize(srcImage, width, height, filter, image->pScratchImage).ThrowIf();
        }

        public static unsafe void Resize(ScratchImage* srcImage, ulong width, ulong height, TexFilterFlags filter, ScratchImage* result)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.Resize2(srcImages, nimages, &metadata, width, height, filter, result->pScratchImage).ThrowIf();
        }

        public static unsafe void Resize(Image* srcImages, ulong nimages, TexMetadata* metadata, ulong width, ulong height, TexFilterFlags filter, ScratchImage* result)
        {
            Native.Resize2(srcImages, nimages, metadata, width, height, filter, result->pScratchImage).ThrowIf();
        }

        public static unsafe void Convert(Image* srcImage, Format format, TexFilterFlags filter, float threshold, ScratchImage* image)
        {
            Native.Convert(srcImage, format, filter, threshold, image->pScratchImage).ThrowIf();
        }

        public static unsafe void Convert(ScratchImage* srcImage, Format format, TexFilterFlags filter, float threshold, ScratchImage* result)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.Convert2(srcImages, nimages, &metadata, format, filter, threshold, result->pScratchImage).ThrowIf();
        }

        public static unsafe void Convert(Image* srcImages, ulong nimages, TexMetadata* metadata, Format format, TexFilterFlags filter, float threshold, ScratchImage* result)
        {
            Native.Convert2(srcImages, nimages, metadata, format, filter, threshold, result->pScratchImage).ThrowIf();
        }

        // Convert the image to a new format

        public static unsafe void ConvertToSinglePlane(Image* srcImage, ScratchImage* image)
        {
            Native.ConvertToSinglePlane(srcImage, image->pScratchImage).ThrowIf();
        }

        public static unsafe void ConvertToSinglePlane(ScratchImage* srcImage, ScratchImage* image)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.ConvertToSinglePlane2(srcImages, nimages, &metadata, image->pScratchImage).ThrowIf();
        }

        public static unsafe void ConvertToSinglePlane(Image* srcImages, ulong nimages, TexMetadata* metadata, ScratchImage* image)
        {
            Native.ConvertToSinglePlane2(srcImages, nimages, metadata, image->pScratchImage).ThrowIf();
        }

        // Converts the image from a planar format to an equivalent non-planar format

        public static unsafe void GenerateMipMaps(Image* baseImage, TexFilterFlags filter, ulong levels, ScratchImage* mipChain, bool allow1D = false)
        {
            Native.GenerateMipMaps(baseImage, filter, levels, mipChain->pScratchImage, allow1D).ThrowIf();
        }

        public static unsafe void GenerateMipMaps(ScratchImage* srcImage, TexFilterFlags filter, ulong levels, ScratchImage* mipChain)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.GenerateMipMaps2(srcImages, nimages, &metadata, filter, levels, mipChain->pScratchImage).ThrowIf();
        }

        public static unsafe void GenerateMipMaps(Image* srcImages, ulong nimages, TexMetadata* metadata, TexFilterFlags filter, ulong levels, ScratchImage* mipChain)
        {
            Native.GenerateMipMaps2(srcImages, nimages, metadata, filter, levels, mipChain->pScratchImage).ThrowIf();
        }

        // levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
        // Defaults to Fant filtering which is equivalent to a box filter

        public static unsafe void GenerateMipMaps3D(Image* baseImages, ulong depth, TexFilterFlags filter, ulong levels, ScratchImage* mipChain)
        {
            Native.GenerateMipMaps3D(baseImages, depth, filter, levels, mipChain->pScratchImage).ThrowIf();
        }

        public static unsafe void GenerateMipMaps3D(ScratchImage* srcImage, TexFilterFlags filter, ulong levels, ScratchImage* mipChain)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            Native.GenerateMipMaps3D2(srcImages, nimages, filter, levels, mipChain->pScratchImage).ThrowIf();
        }

        // levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
        // Defaults to Fant filtering which is equivalent to a box filter

        public static unsafe void ScaleMipMapsAlphaForCoverage(Image* srcImages, ulong nimages, TexMetadata* metadata, ulong item, float alphaReference, ScratchImage* mipChain)
        {
            Native.ScaleMipMapsAlphaForCoverage(srcImages, nimages, metadata, item, alphaReference, mipChain->pScratchImage).ThrowIf();
        }

        public static unsafe void PremultiplyAlpha(Image* srcImage, TexPmAlphaFlags flags, ScratchImage* image)
        {
            Native.PremultiplyAlpha(srcImage, flags, image->pScratchImage).ThrowIf();
        }

        public static unsafe void PremultiplyAlpha(ScratchImage* srcImage, TexPmAlphaFlags flags, ScratchImage* result)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.PremultiplyAlpha2(srcImages, nimages, &metadata, flags, result->pScratchImage).ThrowIf();
        }

        public static unsafe void PremultiplyAlpha(Image* srcImages, ulong nimages, TexMetadata* metadata, TexPmAlphaFlags flags, ScratchImage* result)
        {
            Native.PremultiplyAlpha2(srcImages, nimages, metadata, flags, result->pScratchImage).ThrowIf();
        }

        public static unsafe void Compress(Image* srcImage, Format format, TexCompressFlags compress, float threshold, ScratchImage* cImage)
        {
            Native.Compress(srcImage, format, compress, threshold, cImage->pScratchImage).ThrowIf();
        }

        public static unsafe void Compress(ScratchImage* srcImage, Format format, TexCompressFlags compress, float threshold, ScratchImage* cImages)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.Compress2(srcImages, nimages, &metadata, format, compress, threshold, cImages->pScratchImage).ThrowIf();
        }

        public static unsafe void Compress(Image* srcImages, ulong nimages, TexMetadata* metadata, Format format, TexCompressFlags compress, float threshold, ScratchImage* cImages)
        {
            Native.Compress2(srcImages, nimages, metadata, format, compress, threshold, cImages->pScratchImage).ThrowIf();
        }

        // Note that threshold is only used by BC1. TEX_THRESHOLD_DEFAULT is a typical value to use

        public static unsafe void Compress(ID3D11Device* pDevice, Image* srcImage, Format format, TexCompressFlags compress, float alphaWeight, ScratchImage* image)
        {
            Native.Compress3(pDevice, srcImage, format, compress, alphaWeight, image->pScratchImage).ThrowIf();
        }

        public static unsafe void Compress(ID3D11Device* pDevice, ScratchImage* srcImage, Format format, TexCompressFlags compress, float alphaWeight, ScratchImage* cImages)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.Compress4(pDevice, srcImages, nimages, &metadata, format, compress, alphaWeight, cImages->pScratchImage).ThrowIf();
        }

        public static unsafe void Compress(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, Format format, TexCompressFlags compress, float alphaWeight, ScratchImage* cImages)
        {
            Native.Compress4(pDevice, srcImages, nimages, metadata, format, compress, alphaWeight, cImages->pScratchImage).ThrowIf();
        }

        // DirectCompute-based compression (alphaWeight is only used by BC7. 1.0 is the typical value to use)

        public static unsafe void Decompress(Image* cImage, Format format, ScratchImage* image)
        {
            Native.Decompress(cImage, format, image->pScratchImage).ThrowIf();
        }

        public static unsafe void Decompress(ScratchImage* srcImage, Format format, ScratchImage* images)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.Decompress2(srcImages, nimages, &metadata, format, images->pScratchImage).ThrowIf();
        }

        public static unsafe void Decompress(Image* cImages, ulong nimages, TexMetadata* metadata, Format format, ScratchImage* images)
        {
            Native.Decompress2(cImages, nimages, metadata, format, images->pScratchImage).ThrowIf();
        }

        #endregion Texture conversion, resizing, mipmap generation, and block compression

        #region Normal map operations

        public static void ComputeNormalMap(Image* srcImage, CNMAPFlags flags, float amplitude, Format format, ScratchImage* normalMap)
        {
            Native.ComputeNormalMap(srcImage, flags, amplitude, format, normalMap->pScratchImage).ThrowIf();
        }

        public static void ComputeNormalMap(ScratchImage* srcImage, CNMAPFlags flags, float amplitude, Format format, ScratchImage* normalMaps)
        {
            Image* srcImages = srcImage->GetImages();
            ulong nimages = srcImage->GetImageCount();
            TexMetadata metadata = srcImage->GetMetadata();
            Native.ComputeNormalMap2(srcImages, nimages, &metadata, flags, amplitude, format, normalMaps->pScratchImage).ThrowIf();
        }

        public static void ComputeNormalMap(Image* srcImages, ulong nimages, TexMetadata* metadata, CNMAPFlags flags, float amplitude, Format format, ScratchImage* normalMaps)
        {
            Native.ComputeNormalMap2(srcImages, nimages, metadata, flags, amplitude, format, normalMaps->pScratchImage).ThrowIf();
        }

        #endregion Normal map operations

        #region Misc image operations

        public static void CopyRectangle(Image* srcImage, Rect* srcRect, Image* dstImage, TexFilterFlags filter, ulong xOffset, ulong yOffset)
        {
            Native.CopyRectangle(srcImage, srcRect, dstImage, filter, xOffset, yOffset).ThrowIf();
        }

        public static void ComputeMSE(Image* image1, Image* image2, float* mse, float* mseV, CMSEFlags flags = CMSEFlags.Default)
        {
            Native.ComputeMSE(image1, image2, mse, mseV, flags).ThrowIf();
        }

        public static void EvaluateImage(Image* image, EvaluateFunc pixelFunc)
        {
            Native.EvaluateImage(image, pixelFunc).ThrowIf();
        }

        public static void EvaluateImage(Image* images, ulong nimages, TexMetadata* metadata, EvaluateFunc pixelFunc)
        {
            Native.EvaluateImage2(images, nimages, metadata, pixelFunc).ThrowIf();
        }

        public static void TransformImage(Image* image, TransformFunc pixelFunc, ScratchImage* result)
        {
            Native.TransformImage(image, pixelFunc, result->pScratchImage).ThrowIf();
        }

        public static void TransformImage(Image* srcImages, ulong nimages, TexMetadata* metadata, TransformFunc pixelFunc, ScratchImage* result)
        {
            Native.TransformImage2(srcImages, nimages, metadata, pixelFunc, result->pScratchImage).ThrowIf();
        }

        #endregion Misc image operations

        #region WIC utility code

        public static Guid* GetWICCodec(WICCodecs codec)
        {
            return Native.GetWICCodec(codec);
        }

        public static IWICImagingFactory* GetWICFactory(bool* iswic2)
        {
            return Native.GetWICFactory(iswic2);
        }

        public static void SetWICFactory(IWICImagingFactory* pWIC)
        {
            Native.SetWICFactory(pWIC);
        }

        #endregion WIC utility code

        #region DDS helper functions

        public static void EncodeDDSHeader(TexMetadata* metadata, DDSFlags flags, void* pDestination, ulong maxsize, ulong* required)
        {
            Native.EncodeDDSHeader(metadata, flags, pDestination, maxsize, required).ThrowIf();
        }

        #endregion DDS helper functions

        #region Direct3D 11 functions

        public static bool IsSupportedTexture(ID3D11Device* pDevice, TexMetadata* metadata)
        {
            return Native.IsSupportedTexture(pDevice, metadata);
        }

        public static void CreateTexture(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, ID3D11Resource** ppResource)
        {
            Native.CreateTexture(pDevice, srcImages, nimages, metadata, ppResource).ThrowIf();
        }

        public static void CreateShaderResourceView(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, ID3D11ShaderResourceView** ppSRV)
        {
            Native.CreateShaderResourceView(pDevice, srcImages, nimages, metadata, ppSRV).ThrowIf();
        }

        public static void CreateTextureEx(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, Usage usage, BindFlag bindFlags, CpuAccessFlag cpuAccessFlags, ResourceMiscFlag miscFlags, CreateTexFlags createTexFlags, ID3D11Resource** ppResource)
        {
            Native.CreateTextureEx(pDevice, srcImages, nimages, metadata, usage, bindFlags, cpuAccessFlags, miscFlags, createTexFlags, ppResource).ThrowIf();
        }

        public static void CreateTextureEx(ID3D11Device* pDevice, ScratchImage* img, Usage usage, BindFlag bindFlags, CpuAccessFlag cpuAccessFlags, ResourceMiscFlag miscFlags, CreateTexFlags createTexFlags, ID3D11Resource** ppResource)
        {
            Native.CreateTextureEx2(pDevice, img->pScratchImage, usage, bindFlags, cpuAccessFlags, miscFlags, createTexFlags, ppResource).ThrowIf();
        }

        public static void CreateShaderResourceViewEx(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, Usage usage, BindFlag bindFlags, CpuAccessFlag cpuAccessFlags, ResourceMiscFlag miscFlags, CreateTexFlags createTexFlags, ID3D11ShaderResourceView** ppSRV)
        {
            Native.CreateShaderResourceViewEx(pDevice, srcImages, nimages, metadata, usage, bindFlags, cpuAccessFlags, miscFlags, createTexFlags, ppSRV).ThrowIf();
        }

        public static void CaptureTexture(ID3D11Device* pDevice, ID3D11DeviceContext* pContext, ID3D11Resource* pSource, ScratchImage* result)
        {
            Native.CaptureTexture(pDevice, pContext, pSource, result->pScratchImage).ThrowIf();
        }

        #endregion Direct3D 11 functions

        #region Direct3D 12 functions

        public static unsafe bool IsSupportedTexture(ID3D12Device* pDevice, TexMetadata* metadata)
        {
            return Native.IsSupportedTextureD3D12(pDevice, metadata);
        }

        public static unsafe void CreateTexture(ID3D12Device* pDevice, TexMetadata* metadata, ID3D12Resource** ppResource)
        {
            Native.CreateTextureD3D12(pDevice, metadata, ppResource).ThrowIf();
        }

        public static unsafe void CreateTextureEx(ID3D12Device* pDevice, TexMetadata* metadata, ResourceFlags resFlags, CreateTexFlags createTexFlags, ID3D12Resource** ppResource)
        {
            Native.CreateTextureExD3D12(pDevice, metadata, resFlags, createTexFlags, ppResource).ThrowIf();
        }

        public static unsafe void PrepareUpload(ID3D12Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, Silk.NET.Direct3D12.SubresourceData** subresources, int* count)
        {
            Native.PrepareUpload(pDevice, srcImages, nimages, metadata, subresources, count).ThrowIf();
        }

        public static unsafe void CaptureTexture(ID3D12CommandQueue* pCommandQueue, ID3D12Resource* pSource, bool isCubeMap, void* result, ResourceStates beforeState = ResourceStates.RenderTarget, ResourceStates afterState = ResourceStates.RenderTarget)
        {
            Native.CaptureTextureD3D12(pCommandQueue, pSource, isCubeMap, result, beforeState, afterState).ThrowIf();
        }

        #endregion Direct3D 12 functions
    }
}