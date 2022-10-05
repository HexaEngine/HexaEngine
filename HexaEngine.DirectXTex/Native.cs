using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Numerics;
using System.Runtime.InteropServices;

namespace HexaEngine.DirectXTex
{
    public unsafe delegate void GetMQR(IWICMetadataQueryReader* pMqr);

    public unsafe delegate void SetCustomProps(IPropertyBag2* pbag);

    //std::function<void(XMVECTOR* pixels, ulong width, ulong y)> pixelFunc
    public unsafe delegate void EvaluateFunc(Vector4* pixels, ulong width, ulong y);

    //std::function<void(XMVECTOR* outPixels, XMVECTOR* inPixels, ulong width, ulong y)> pixelFunc
    public unsafe delegate void TransformFunc(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y);

    internal static partial class Native
    {
        internal const string LibName = "HexaEngine.DirectXTex.Native.dll";

        #region ScratchImage Methods

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial void* NewScratchImage();

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Initialize(void* img, TexMetadata* mdata, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Initialize1D(void* img, Format fmt, ulong length, ulong arraySize, ulong mipLevels, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Initialize2D(void* img, Format fmt, ulong width, ulong height, ulong arraySize, ulong mipLevels, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Initialize3D(void* img, Format fmt, ulong width, ulong height, ulong depth, ulong mipLevels, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int InitializeCube(void* img, Format fmt, ulong width, ulong height, ulong nCubes, ulong mipLevels, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int InitializeFromImage(void* img, Image* srcImage, [MarshalAs(UnmanagedType.Bool)] bool allow1D = false, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int InitializeArrayFromImages(void* img, Image* images, ulong nImages, [MarshalAs(UnmanagedType.Bool)] bool allow1D = false, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int InitializeCubeFromImages(void* img, Image* images, ulong nImages, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Initialize3DFromImages(void* img, Image* images, ulong depth, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial void ScratchImageRelease(void** img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte OverrideFormat(void* img, Format f);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial TexMetadata* GetMetadata(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial Image* GetImage(void* img, ulong mip, ulong item, ulong slice);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial Image* GetImages(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial ulong GetImageCount(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte* GetPixels(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial ulong GetPixelsSize(void* img);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsAlphaAllOpaque(void* img);

        #endregion ScratchImage Methods

        #region TexMetadata Methods

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial ulong ComputeIndex(TexMetadata* metadata, ulong mip, ulong item, ulong slice);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool IsCubemap(TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool IsPMAlpha(TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial void SetAlphaMode(TexMetadata* metadata, TexAlphaMode mode);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial TexAlphaMode GetAlphaMode(TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool IsVolumemap(TexMetadata* metadata);

        #endregion TexMetadata Methods

        #region Blob Methods

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial void* NewBlob();

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int BlobInitialize(void* blob, ulong size);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial void BlobRelease(void** blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial void* BlobGetBufferPointer(void* blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial ulong BlobGetBufferSize(void* blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int BlobResize(void* blob, ulong size);

        // Reallocate for a new size
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int BlobTrim(void* blob, ulong size);

        #endregion Blob Methods

        #region DXGI Format Utilities

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsValid(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsCompressed(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsPacked(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsVideo(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsPlanar(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsPalettized(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsDepthStencil(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsSRGB(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte IsTypeless(Format fmt, [MarshalAs(UnmanagedType.Bool)] bool partialTypeless = true);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial byte HasAlpha(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial ulong BitsPerPixel(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial ulong BitsPerColor(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial FormatType FormatDataType(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int ComputePitch(Format fmt, ulong width, ulong height, ulong* rowPitch, ulong* slicePitch, CPFlags flags = CPFlags.None);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial ulong ComputeScanlines(Format fmt, ulong height);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial Format MakeSRGB(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial Format MakeTypeless(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial Format MakeTypelessUNORM(Format fmt);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial Format MakeTypelessFLOAT(Format fmt);

        #endregion DXGI Format Utilities

        #region MetadataIO

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromDDSMemory(void* pSource, ulong size, DDSFlags flags, TexMetadata* metadata);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromDDSFile(string szFile, DDSFlags flags, TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromHDRMemory(void* pSource, ulong size, TexMetadata* metadata);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromHDRFile(string szFile, TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromTGAMemory(void* pSource, ulong size, TGAFlags flags, TexMetadata* metadata);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromTGAFile(string szFile, TGAFlags flags, TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromWICMemory(void* pSource, ulong size, WICFlags flags, TexMetadata* metadata);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial uint GetMetadataFromWICFile(string szFile, WICFlags flags, TexMetadata* metadata);

        #endregion MetadataIO

        #region ImageIO

        #region DDS operations

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromDDSMemory(void* pSource, ulong size, DDSFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromDDSFile(string szFile, DDSFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToDDSMemory(Image* image, DDSFlags flags, void* blob);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToDDSMemory2(Image* images, ulong nimages, TexMetadata* metadata, DDSFlags flags, void* blob);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToDDSFile(Image* image, DDSFlags flags, string szFile);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToDDSFile2(Image* images, ulong nimages, TexMetadata* metadata, DDSFlags flags, string szFile);

        #endregion DDS operations

        #region HDR operations

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromHDRMemory(void* pSource, ulong size, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromHDRFile(string szFile, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToHDRMemory(Image* image, void* blob);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToHDRFile(Image* image, string szFile);

        #endregion HDR operations

        #region TGA operations

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromTGAMemory(void* pSource, ulong size, TGAFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromTGAFile(string szFile, TGAFlags flags, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToTGAMemory(Image* image, TGAFlags flags, void* blob, TexMetadata* metadata = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToTGAFile(Image* image, TGAFlags flags, string szFile, TexMetadata* metadata = null);

        #endregion TGA operations

        #region WIC operations

#if !WIN32

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromWICMemory(void* pSource, ulong size, WICFlags flags, TexMetadata* metadata, void* image, [MarshalAs(UnmanagedType.FunctionPtr)] GetMQR? getMQR = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromWICFile(string szFile, WICFlags flags, TexMetadata* metadata, void* image, [MarshalAs(UnmanagedType.FunctionPtr)] GetMQR? getMQR = null);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToWICMemory(Image* image, WICFlags flags, Guid* guidContainerFormat, void* blob, Guid* targetFormat = null, [MarshalAs(UnmanagedType.FunctionPtr)] SetCustomProps? setCustomProps = null);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToWICMemory2(Image* images, ulong nimages, WICFlags flags, Guid* guidContainerFormat, void* blob, Guid* targetFormat = null, [MarshalAs(UnmanagedType.FunctionPtr)] SetCustomProps? setCustomProps = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToWICFile(Image* image, WICFlags flags, Guid* guidContainerFormat, string szFile, Guid* targetFormat = null, [MarshalAs(UnmanagedType.FunctionPtr)] SetCustomProps? setCustomProps = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToWICFile2(Image* images, ulong nimages, WICFlags flags, Guid* guidContainerFormat, string szFile, Guid* targetFormat = null, [MarshalAs(UnmanagedType.FunctionPtr)] SetCustomProps? setCustomProps = null);

#endif // WIN32

        #endregion WIC operations

        #region Compatability helpers

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromTGAMemory2(void* pSource, ulong size, TexMetadata* metadata, void* image);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int LoadFromTGAFile2(string szFile, TexMetadata* metadata, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToTGAMemory2(Image* image, void* blob, TexMetadata* metadata = null);

        [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf16)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int SaveToTGAFile2(Image* image, string szFile, TexMetadata* metadata = null);

        #endregion Compatability helpers

        #endregion ImageIO

        #region Texture conversion, resizing, mipmap generation, and block compression

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int FlipRotate(Image* srcImage, TexFrFlags flags, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int FlipRotate2(Image* srcImages, ulong nimages, TexMetadata* metadata, TexFrFlags flags, void* result);

        // Flip and/or rotate image
        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Resize(Image* srcImage, ulong width, ulong height, TexFilterFlags filter, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Resize2(Image* srcImages, ulong nimages, TexMetadata* metadata, ulong width, ulong height, TexFilterFlags filter, void* result);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Convert(Image* srcImage, Format format, TexFilterFlags filter, float threshold, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Convert2(Image* srcImages, ulong nimages, TexMetadata* metadata, Format format, TexFilterFlags filter, float threshold, void* result);

        // Convert the image to a new format

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int ConvertToSinglePlane(Image* srcImage, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int ConvertToSinglePlane2(Image* srcImages, ulong nimages, TexMetadata* metadata, void* image);

        // Converts the image from a planar format to an equivalent non-planar format

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int GenerateMipMaps(Image* baseImage, TexFilterFlags filter, ulong levels, void* mipChain, [MarshalAs(UnmanagedType.Bool)] bool allow1D = false);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int GenerateMipMaps2(Image* srcImages, ulong nimages, TexMetadata* metadata, TexFilterFlags filter, ulong levels, void* mipChain);

        // levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
        // Defaults to Fant filtering which is equivalent to a box filter

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int GenerateMipMaps3D(Image* baseImages, ulong depth, TexFilterFlags filter, ulong levels, void* mipChain);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int GenerateMipMaps3D2(Image* srcImages, ulong nimages, TexMetadata* metadata, TexFilterFlags filter, ulong levels, void* mipChain);

        // levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
        // Defaults to Fant filtering which is equivalent to a box filter

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int ScaleMipMapsAlphaForCoverage(Image* srcImages, ulong nimages, TexMetadata* metadata, ulong item, float alphaReference, void* mipChain);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int PremultiplyAlpha(Image* srcImage, TexPmAlphaFlags flags, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int PremultiplyAlpha2(Image* srcImages, ulong nimages, TexMetadata* metadata, TexPmAlphaFlags flags, void* result);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Compress(Image* srcImage, Format format, TexCompressFlags compress, float threshold, void* cImage);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Compress2(Image* srcImages, ulong nimages, TexMetadata* metadata, Format format, TexCompressFlags compress, float threshold, void* cImages);

        // Note that threshold is only used by BC1. TEX_THRESHOLD_DEFAULT is a typical value to use

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Compress3(ID3D11Device* pDevice, Image* srcImage, Format format, TexCompressFlags compress, float alphaWeight, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Compress4(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, Format format, TexCompressFlags compress, float alphaWeight, void* cImages);

        // DirectCompute-based compression (alphaWeight is only used by BC7. 1.0 is the typical value to use)

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Decompress(Image* cImage, Format format, void* image);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int Decompress2(Image* cImages, ulong nimages, TexMetadata* metadata, Format format, void* images);

        #endregion Texture conversion, resizing, mipmap generation, and block compression

        #region Normal map operations

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int ComputeNormalMap(Image* srcImage, CNMAPFlags flags, float amplitude, Format format, void* normalMap);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int ComputeNormalMap2(Image* srcImages, ulong nimages, TexMetadata* metadata, CNMAPFlags flags, float amplitude, Format format, void* normalMaps);

        #endregion Normal map operations

        #region Misc image operations

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int CopyRectangle(Image* srcImage, Rect* srcRect, Image* dstImage, TexFilterFlags filter, ulong xOffset, ulong yOffset);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int ComputeMSE(Image* image1, Image* image2, float* mse, float* mseV, CMSEFlags flags = CMSEFlags.Default);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int EvaluateImage(Image* image, [MarshalAs(UnmanagedType.FunctionPtr)] EvaluateFunc pixelFunc);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int EvaluateImage2(Image* images, ulong nimages, TexMetadata* metadata, [MarshalAs(UnmanagedType.FunctionPtr)] EvaluateFunc pixelFunc);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int TransformImage(Image* image, [MarshalAs(UnmanagedType.FunctionPtr)] TransformFunc pixelFunc, void* result);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int TransformImage2(Image* srcImages, ulong nimages, TexMetadata* metadata, [MarshalAs(UnmanagedType.FunctionPtr)] TransformFunc pixelFunc, void* result);

        #endregion Misc image operations

        #region WIC utility code

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial Guid* GetWICCodec(WICCodecs codec);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial IWICImagingFactory* GetWICFactory(bool* iswic2);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial void SetWICFactory(IWICImagingFactory* pWIC);

        #endregion WIC utility code

        #region DDS helper functions

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int EncodeDDSHeader(TexMetadata* metadata, DDSFlags flags, void* pDestination, ulong maxsize, ulong* required);

        #endregion DDS helper functions

        #region Direct3D 11 functions

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool IsSupportedTexture(ID3D11Device* pDevice, TexMetadata* metadata);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int CreateTexture(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, ID3D11Resource** ppResource);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int CreateShaderResourceView(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, ID3D11ShaderResourceView** ppSRV);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int CreateTextureEx(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, Usage usage, BindFlag bindFlags, CpuAccessFlag cpuAccessFlags, ResourceMiscFlag miscFlags, [MarshalAs(UnmanagedType.Bool)] bool forceSRGB, ID3D11Resource** ppResource);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int CreateTextureEx2(ID3D11Device* pDevice, void* img, Usage usage, BindFlag bindFlags, CpuAccessFlag cpuAccessFlags, ResourceMiscFlag miscFlags, [MarshalAs(UnmanagedType.Bool)] bool forceSRGB, ID3D11Resource** ppResource);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int CreateShaderResourceViewEx(ID3D11Device* pDevice, Image* srcImages, ulong nimages, TexMetadata* metadata, Usage usage, BindFlag bindFlags, CpuAccessFlag cpuAccessFlags, ResourceMiscFlag miscFlags, [MarshalAs(UnmanagedType.Bool)] bool forceSRGB, ID3D11ShaderResourceView** ppSRV);

        [LibraryImport(LibName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static unsafe partial int CaptureTexture(ID3D11Device* pDevice, ID3D11DeviceContext* pContext, ID3D11Resource* pSource, void* result);

        #endregion Direct3D 11 functions

        #region Direct3D 12 functions

#if D3D12
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern bool IsSupportedTexture( ID3D12Device* pDevice,  TexMetadata* metadata);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern int CreateTexture( ID3D12Device* pDevice,  TexMetadata* metadata, ID3D12Resource** ppResource);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern int CreateTextureEx( ID3D12Device* pDevice,  TexMetadata* metadata,  D3D12_RESOURCE_FLAGS resFlags,  bool forceSRGB, ID3D12Resource** ppResource);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern int PrepareUpload( ID3D12Device* pDevice, Image* srcImages,  ulong nimages,  TexMetadata* metadata, std::vector<D3D12_SUBRESOURCE_DATA> &subresources);
    [DllImport(LibName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    internal static unsafe extern int CaptureTexture( ID3D12CommandQueue* pCommandQueue,  ID3D12Resource* pSource,  bool isCubeMap, void* result,  D3D12_RESOURCE_STATES beforeState = D3D12_RESOURCE_STATE_RENDER_TARGET,  D3D12_RESOURCE_STATES afterState = D3D12_RESOURCE_STATE_RENDER_TARGET);
#endif

        #endregion Direct3D 12 functions
    }
}