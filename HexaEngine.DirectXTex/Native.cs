using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HexaEngine.DirectXTex
{
    public static partial class Native
    {
        public const string LibName = "HexaEngine.DirectXTex.Native.dll";

        #region DXGI Format Utilities

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsValid(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.U1)]
        public static unsafe partial byte IsCompressed(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsPacked(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsVideo(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsPlanar(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsPalettized(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsDepthStencil(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsSRGB(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsTypeless(in Format fmt, [MarshalAs(UnmanagedType.Bool)] in bool partialTypeless = true);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool HasAlpha(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial ulong BitsPerPixel(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial ulong BitsPerColor(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial FormatType FormatDataType(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int ComputePitch(in Format fmt, in ulong width, in ulong height, ulong* rowPitch, ulong* slicePitch, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial ulong ComputeScanlines(in Format fmt, in ulong height);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial Format MakeSRGB(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial Format MakeTypeless(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial Format MakeTypelessUNORM(in Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial Format MakeTypelessFLOAT(in Format fmt);

        #endregion DXGI Format Utilities

        #region TexMetadataMethods

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial ulong ComputeIndex(TexMetadata* metadata, in ulong mip, in ulong item, in ulong slice);

        // Returns ulong(-1) to indicate an out-of-range error
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsCubemap(TexMetadata* metadata);

        // Helper for miscFlags
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsPMAlpha(TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial void SetAlphaMode(TexMetadata* metadata, TexAlphaMode mode);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial TexAlphaMode GetAlphaMode(TexMetadata* metadata);

        // Helpers for miscFlags2
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsVolumemap(TexMetadata* metadata);

        // Helper for dimension

        #endregion TexMetadataMethods

        #region ScratchImageInternal

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial void* NewScratchImage();

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Initialize(void* img, in TexMetadata* mdata, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Initialize1D(void* img, in Format fmt, in ulong length, in ulong arraySize, in ulong mipLevels, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Initialize2D(void* img, in Format fmt, in ulong width, in ulong height, in ulong arraySize, in ulong mipLevels, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Initialize3D(void* img, in Format fmt, in ulong width, in ulong height, in ulong depth, in ulong mipLevels, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int InitializeCube(void* img, in Format fmt, in ulong width, in ulong height, in ulong nCubes, in ulong mipLevels, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int InitializeFromImage(void* img, in Image* srcImage, [MarshalAs(UnmanagedType.Bool)] in bool allow1D = false, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int InitializeArrayFromImages(void* img, Image* images, in ulong nImages, [MarshalAs(UnmanagedType.Bool)] in bool allow1D = false, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int InitializeCubeFromImages(void* img, Image* images, in ulong nImages, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Initialize3DFromImages(void* img, Image* images, in ulong depth, in CPFlags flags = CPFlags.NONE);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial void ScratchImageRelease(void** img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial byte OverrideFormat(void* img, in Format f);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial TexMetadata* GetMetadata(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial Image* GetImage(void* img, in ulong mip, in ulong item, in ulong slice);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial Image* GetImages(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial ulong GetImageCount(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial byte* GetPixels(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial ulong GetPixelsSize(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsAlphaAllOpaque(void* img);

        #endregion ScratchImageInternal

        #region BlobInternal

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial void* NewBlob();

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int BlobInitialize(void* blob, in ulong size);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial void BlobRelease(void** blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial void* BlobGetBufferPointer(void* blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial ulong BlobGetBufferSize(void* blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int BlobResize(void* blob, ulong size);

        // Reallocate for a new size
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int BlobTrim(void* blob, ulong size);

        // Shorten size without reallocation

        #endregion BlobInternal

        #region ImageIO

        // DDS operations
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromDDSMemory(void* pSource, in ulong size, in DDSFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromDDSFile(string szFile, in DDSFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToDDSMemory(in Image* image, in DDSFlags flags, TexBlob* blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToDDSMemory2(Image* images, in ulong nimages, in TexMetadata* metadata, in DDSFlags flags, TexBlob* blob);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToDDSFile(in Image* image, in DDSFlags flags, string szFile);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToDDSFile2(Image* images, in ulong nimages, in TexMetadata* metadata, in DDSFlags flags, string szFile);

        // HDR operations
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromHDRMemory(void* pSource, in ulong size, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromHDRFile(string szFile, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToHDRMemory(in Image* image, TexBlob* blob);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToHDRFile(in Image* image, string szFile);

        // TGA operations
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromTGAMemory(void* pSource, in ulong size, in TGAFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromTGAFile(string szFile, in TGAFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToTGAMemory(in Image* image, in TGAFlags flags, TexBlob* blob, TexMetadata* metadata = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToTGAFile(in Image* image, in TGAFlags flags, string szFile, TexMetadata* metadata = null);

        // WIC operations
#if !WIN32

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromWICMemory(void* pSource, in ulong size, in WICFlags flags, TexMetadata* metadata, void* image, delegate*<IWICMetadataQueryReader*, void> getMQR = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromWICFile(string szFile, in WICFlags flags, TexMetadata* metadata, void* image, delegate*<IWICMetadataQueryReader*, void> getMQR = null);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToWICMemory(in Image* image, in WICFlags flags, in Guid* guidContainerFormat, TexBlob* blob, Guid* targetFormat = null, delegate*<IPropertyBag2*, void> setCustomProps = null);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToWICMemory2(Image* images, in ulong nimages, in WICFlags flags, in Guid* guidContainerFormat, TexBlob* blob, Guid* targetFormat = null, delegate*<IPropertyBag2*, void> setCustomProps = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToWICFile(in Image* image, in WICFlags flags, in Guid* guidContainerFormat, string szFile, Guid* targetFormat = null, delegate*<IPropertyBag2*, void> setCustomProps = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToWICFile2(Image* images, in ulong nimages, in WICFlags flags, in Guid* guidContainerFormat, string szFile, Guid* targetFormat = null, delegate*<IPropertyBag2*, void> setCustomProps = null);

#endif // WIN32

        // Compatability helpers
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromTGAMemory2(void* pSource, in ulong size, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int LoadFromTGAFile2(string szFile, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToTGAMemory2(in Image* image, TexBlob* blob, TexMetadata* metadata = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int SaveToTGAFile2(in Image* image, string szFile, TexMetadata* metadata = null);

        #endregion ImageIO

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int FlipRotate(in Image* srcImage, in TexFrFlags flags, void* image);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int FlipRotate2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in TexFrFlags flags, void* result);

        // Flip and/or rotate image

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Resize(in Image* srcImage, in ulong width, in ulong height, in TexFilterFlags filter, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Resize2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in ulong width, in ulong height, in TexFilterFlags filter, void* result);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Convert(in Image* srcImage, in Format format, in TexFilterFlags filter, in float threshold, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Convert2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in Format format, in TexFilterFlags filter, in float threshold, void* result);

        // Convert the image to a new format

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int ConvertToSinglePlane(in Image* srcImage, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int ConvertToSinglePlane2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, void* image);

        // Converts the image from a planar format to an equivalent non-planar format

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int GenerateMipMaps(in Image* baseImage, in TexFilterFlags filter, in ulong levels, void* mipChain, [MarshalAs(UnmanagedType.Bool)] in bool allow1D = false);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int GenerateMipMaps2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in TexFilterFlags filter, in ulong levels, void* mipChain);

        // levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
        // Defaults to Fant filtering which is equivalent to a box filter

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int GenerateMipMaps3D(Image* baseImages, in ulong depth, in TexFilterFlags filter, in ulong levels, void* mipChain);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int GenerateMipMaps3D2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in TexFilterFlags filter, in ulong levels, void* mipChain);

        // levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
        // Defaults to Fant filtering which is equivalent to a box filter

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int ScaleMipMapsAlphaForCoverage(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in ulong item, in float alphaReference, void* mipChain);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int PremultiplyAlpha(in Image* srcImage, in TexPmAlphaFlags flags, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int PremultiplyAlpha2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in TexPmAlphaFlags flags, void* result);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Compress(in Image* srcImage, in Format format, in TexCompressFlags compress, in float threshold, void* cImage);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Compress2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in Format format, in TexCompressFlags compress, in float threshold, void* cImages);

        // Note that threshold is only used by BC1. TEX_THRESHOLD_DEFAULT is a typical value to use

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Compress3(in ID3D11Device* pDevice, in Image* srcImage, in Format format, in TexCompressFlags compress, in float alphaWeight, void* image);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Compress4(in ID3D11Device* pDevice, in Image* srcImages, in ulong nimages, in TexMetadata* metadata, in Format format, in TexCompressFlags compress, in float alphaWeight, void* cImages);

        // DirectCompute-based compression (alphaWeight is only used by BC7. 1.0 is the typical value to use)

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Decompress(in Image* cImage, in Format format, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int Decompress2(Image* cImages, in ulong nimages, in TexMetadata* metadata, in Format format, void* images);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int ComputeNormalMap(in Image* srcImage, in CNMAPFlags flags, in float amplitude, in Format format, void* normalMap);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int ComputeNormalMap2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in CNMAPFlags flags, in float amplitude, in Format format, void* normalMaps);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int CopyRectangle(in Image* srcImage, in Rect* srcRect, in Image* dstImage, in TexFilterFlags filter, in ulong xOffset, in ulong yOffset);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int ComputeMSE(in Image* image1, in Image* image2, float* mse, float* mseV, in CMSEFlags flags = CMSEFlags.DEFAULT);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int EvaluateImage(in Image* image, in delegate*<Vector4*, ulong, ulong, void> pixelFunc);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int EvaluateImage2(Image* images, in ulong nimages, in TexMetadata* metadata, in delegate*<Vector4*, ulong, ulong, void> pixelFunc);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int TransformImage(in Image* image, in delegate*<Vector4*, Vector4*, ulong, ulong, void> pixelFunc, void* result);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int TransformImage2(Image* srcImages, in ulong nimages, in TexMetadata* metadata, in delegate*<Vector4*, Vector4*, ulong, ulong, void> pixelFunc, void* result);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial Guid* GetWICCodec(in WICCodecs codec);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial IWICImagingFactory* GetWICFactory(bool* iswic2);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial void SetWICFactory(IWICImagingFactory* pWIC);

        //---------------------------------------------------------------------------------
        // DDS helper functions
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int EncodeDDSHeader(in TexMetadata* metadata, DDSFlags flags, void* pDestination, in ulong maxsize, ulong* required);

        //---------------------------------------------------------------------------------
        // Direct3D 11 functions
        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool IsSupportedTexture(in ID3D11Device* pDevice, in TexMetadata* metadata);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int CreateTexture(in ID3D11Device* pDevice, Image* srcImages, in ulong nimages, in TexMetadata* metadata, ID3D11Resource** ppResource);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int CreateShaderResourceView(in ID3D11Device* pDevice, Image* srcImages, in ulong nimages, in TexMetadata* metadata, ID3D11ShaderResourceView** ppSRV);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int CreateTextureEx(in ID3D11Device* pDevice, Image* srcImages, in ulong nimages, in TexMetadata* metadata, in Usage usage, in uint bindFlags, in uint cpuAccessFlags, in uint miscFlags, [MarshalAs(UnmanagedType.Bool)] in bool forceSRGB, ID3D11Resource** ppResource);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int CreateTextureEx2(in ID3D11Device* pDevice, void* img, in uint usage, in uint bindFlags, in uint cpuAccessFlags, in uint miscFlags, [MarshalAs(UnmanagedType.Bool)] in bool forceSRGB, ID3D11Resource** ppResource);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int CreateShaderResourceViewEx(in ID3D11Device* pDevice, Image* srcImages, in ulong nimages, in TexMetadata* metadata, in Usage usage, in BindFlag bindFlags, in CpuAccessFlag cpuAccessFlags, in ResourceMiscFlag miscFlags, [MarshalAs(UnmanagedType.Bool)] in bool forceSRGB, ID3D11ShaderResourceView** ppSRV);

        [SupportedOSPlatform("windows")]
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        public static unsafe partial int CaptureTexture(in ID3D11Device* pDevice, in ID3D11DeviceContext* pContext, in ID3D11Resource* pSource, void* result);

        //---------------------------------------------------------------------------------
        // Direct3D 12 functions
#if D3D12
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern bool IsSupportedTexture(in ID3D12Device* pDevice, in TexMetadata* metadata);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int CreateTexture(in ID3D12Device* pDevice, in TexMetadata* metadata, ID3D12Resource** ppResource);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int CreateTextureEx(in ID3D12Device* pDevice, in TexMetadata* metadata, in D3D12_RESOURCE_FLAGS resFlags, in bool forceSRGB, ID3D12Resource** ppResource);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int PrepareUpload(in ID3D12Device* pDevice, Image* srcImages, in ulong nimages, in TexMetadata* metadata, std::vector<D3D12_SUBRESOURCE_DATA> &subresources);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static unsafe extern int CaptureTexture(in ID3D12CommandQueue* pCommandQueue, in ID3D12Resource* pSource, in bool isCubeMap, void* result, in D3D12_RESOURCE_STATES beforeState = D3D12_RESOURCE_STATE_RENDER_TARGET, in D3D12_RESOURCE_STATES afterState = D3D12_RESOURCE_STATE_RENDER_TARGET);
#endif
    }
}