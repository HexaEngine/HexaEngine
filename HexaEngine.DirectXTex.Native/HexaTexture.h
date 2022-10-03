#include "DirectXTex/DirectXTex.h"
#include <dxgiformat.h>

#define API __declspec(dllexport)

#ifdef __cplusplus
extern "C"
{
#endif

#pragma region DXGI Format Utilities

	API constexpr bool IsValid(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsCompressed(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsPacked(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsVideo(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsPlanar(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsPalettized(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsDepthStencil(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsSRGB(_In_ DXGI_FORMAT& fmt) noexcept;
	API bool IsTypeless(_In_ DXGI_FORMAT& fmt, _In_ bool& partialTypeless) noexcept;

	API bool HasAlpha(_In_ DXGI_FORMAT& fmt) noexcept;

	API size_t BitsPerPixel(_In_ DXGI_FORMAT& fmt) noexcept;

	API size_t BitsPerColor(_In_ DXGI_FORMAT& fmt) noexcept;

	API DirectX::FORMAT_TYPE FormatDataType(_In_ DXGI_FORMAT& fmt) noexcept;
	API HRESULT ComputePitch(_In_ DXGI_FORMAT fmt, _In_ size_t width, _In_ size_t height, _Out_ size_t& rowPitch, _Out_ size_t& slicePitch, _In_ DirectX::CP_FLAGS flags = DirectX::CP_FLAGS_NONE) noexcept;

	API size_t ComputeScanlines(_In_ DXGI_FORMAT fmt, _In_ size_t height) noexcept;

	API DXGI_FORMAT MakeSRGB(_In_ DXGI_FORMAT fmt) noexcept;
	API DXGI_FORMAT MakeTypeless(_In_ DXGI_FORMAT fmt) noexcept;
	API DXGI_FORMAT MakeTypelessUNORM(_In_ DXGI_FORMAT fmt) noexcept;
	API DXGI_FORMAT MakeTypelessFLOAT(_In_ DXGI_FORMAT fmt) noexcept;

#pragma endregion DXGI Format Utilities

#pragma region TexMetadataMethods

	API size_t ComputeIndex(DirectX::TexMetadata* metadata, _In_ size_t mip, _In_ size_t item, _In_ size_t slice) noexcept;
	// Returns size_t(-1) to indicate an out-of-range error

	API bool IsCubemap(DirectX::TexMetadata* metadata) noexcept;
	// Helper for miscFlags

	API bool IsPMAlpha(DirectX::TexMetadata* metadata) noexcept;
	API void SetAlphaMode(DirectX::TexMetadata* metadata, DirectX::TEX_ALPHA_MODE mode) noexcept;
	API DirectX::TEX_ALPHA_MODE GetAlphaMode(DirectX::TexMetadata* metadata) noexcept;
	// Helpers for miscFlags2

	API bool IsVolumemap(DirectX::TexMetadata* metadata) noexcept;
	// Helper for dimension

#pragma endregion TexMetadataMethods

#pragma region ScratchImageInternal

	API DirectX::ScratchImage* NewScratchImage();

	API HRESULT Initialize(DirectX::ScratchImage* img, _In_ const DirectX::TexMetadata* mdata, _In_ DirectX::CP_FLAGS& flags) noexcept;

	API HRESULT Initialize1D(DirectX::ScratchImage* img, _In_ DXGI_FORMAT& fmt, _In_ size_t& length, _In_ size_t& arraySize, _In_ size_t& mipLevels, _In_ DirectX::CP_FLAGS& flags) noexcept;
	API HRESULT Initialize2D(DirectX::ScratchImage* img, _In_ DXGI_FORMAT& fmt, _In_ size_t& width, _In_ size_t& height, _In_ size_t& arraySize, _In_ size_t& mipLevels, _In_ DirectX::CP_FLAGS& flags) noexcept;
	API HRESULT Initialize3D(DirectX::ScratchImage* img, _In_ DXGI_FORMAT& fmt, _In_ size_t& width, _In_ size_t& height, _In_ size_t& depth, _In_ size_t& mipLevels, _In_ DirectX::CP_FLAGS& flags) noexcept;
	API HRESULT InitializeCube(DirectX::ScratchImage* img, _In_ DXGI_FORMAT& fmt, _In_ size_t& width, _In_ size_t& height, _In_ size_t& nCubes, _In_ size_t& mipLevels, _In_ DirectX::CP_FLAGS& flags) noexcept;

	API HRESULT InitializeFromImage(DirectX::ScratchImage* img, _In_ const DirectX::Image& srcImage, _In_ bool allow1D = false, _In_ DirectX::CP_FLAGS flags = DirectX::CP_FLAGS_NONE) noexcept;
	API HRESULT InitializeArrayFromImages(DirectX::ScratchImage* img, _In_reads_(nImages) const DirectX::Image* images, _In_ size_t nImages, _In_ bool allow1D = false, _In_ DirectX::CP_FLAGS flags = DirectX::CP_FLAGS_NONE) noexcept;
	API HRESULT InitializeCubeFromImages(DirectX::ScratchImage* img, _In_reads_(nImages) const DirectX::Image* images, _In_ size_t nImages, _In_ DirectX::CP_FLAGS flags = DirectX::CP_FLAGS_NONE) noexcept;
	API HRESULT Initialize3DFromImages(DirectX::ScratchImage* img, _In_reads_(depth) const DirectX::Image* images, _In_ size_t depth, _In_ DirectX::CP_FLAGS flags = DirectX::CP_FLAGS_NONE) noexcept;

	API void ScratchImageRelease(DirectX::ScratchImage** img) noexcept;

	API bool OverrideFormat(DirectX::ScratchImage* img, _In_ DXGI_FORMAT& f) noexcept;

	API const DirectX::TexMetadata& GetMetadata(DirectX::ScratchImage* img) noexcept;
	API const DirectX::Image* GetImage(DirectX::ScratchImage* img, _In_ size_t mip, _In_ size_t item, _In_ size_t slice) noexcept;

	API const DirectX::Image* GetImages(DirectX::ScratchImage* img) noexcept;
	API size_t GetImageCount(DirectX::ScratchImage* img) noexcept;

	API uint8_t* GetPixels(DirectX::ScratchImage* img) noexcept;
	API size_t GetPixelsSize(DirectX::ScratchImage* img) noexcept;

	API bool IsAlphaAllOpaque(DirectX::ScratchImage* img);

#pragma endregion ScratchImageInternal

#pragma region BlobInternal

	API DirectX::Blob* NewBlob();

	API HRESULT BlobInitialize(DirectX::Blob* blob, _In_ size_t size) noexcept;

	API void BlobRelease(DirectX::Blob** blob) noexcept;

	API void* BlobGetBufferPointer(DirectX::Blob* blob) noexcept;
	API size_t BlobGetBufferSize(DirectX::Blob* blob) noexcept;

	API HRESULT BlobResize(DirectX::Blob* blob, size_t size) noexcept;
	// Reallocate for a new size

	API HRESULT BlobTrim(DirectX::Blob* blob, size_t size) noexcept;
	// Shorten size without reallocation

#pragma endregion BlobInternal

#pragma region ImageIO

	// DDS operations
	API HRESULT LoadFromDDSMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t& size, _In_ DirectX::DDS_FLAGS& flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;
	API HRESULT LoadFromDDSFile(_In_z_ const wchar_t* szFile, _In_ DirectX::DDS_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;

	API HRESULT SaveToDDSMemory(_In_ const DirectX::Image& image, _In_ DirectX::DDS_FLAGS flags, _Out_ DirectX::Blob& blob) noexcept;
	API HRESULT SaveToDDSMemory2(_In_reads_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::DDS_FLAGS flags, _Out_ DirectX::Blob& blob) noexcept;

	API HRESULT SaveToDDSFile(_In_ const DirectX::Image& image, _In_ DirectX::DDS_FLAGS flags, _In_z_ const wchar_t* szFile) noexcept;
	API HRESULT SaveToDDSFile2(_In_reads_(nimages) const DirectX::Image* images, _In_ size_t& nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::DDS_FLAGS flags, _In_z_ const wchar_t* szFile) noexcept;

	// HDR operations
	API HRESULT LoadFromHDRMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;
	API HRESULT LoadFromHDRFile(_In_z_ const wchar_t* szFile, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;

	API HRESULT SaveToHDRMemory(_In_ const DirectX::Image& image, _Out_ DirectX::Blob& blob) noexcept;
	API HRESULT SaveToHDRFile(_In_ const DirectX::Image& image, _In_z_ const wchar_t* szFile) noexcept;

	// TGA operations
	API HRESULT LoadFromTGAMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _In_ DirectX::TGA_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;
	API HRESULT LoadFromTGAFile(_In_z_ const wchar_t* szFile, _In_ DirectX::TGA_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;

	API HRESULT SaveToTGAMemory(_In_ const DirectX::Image& image, _In_ DirectX::TGA_FLAGS flags, _Out_ DirectX::Blob& blob, _In_opt_ const DirectX::TexMetadata* metadata = nullptr) noexcept;
	API HRESULT SaveToTGAFile(_In_ const DirectX::Image& image, _In_ DirectX::TGA_FLAGS flags, _In_z_ const wchar_t* szFile, _In_opt_ const DirectX::TexMetadata* metadata = nullptr) noexcept;

	// WIC operations
#ifdef WIN32
	API HRESULT LoadFromWICMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t& size, _In_ DirectX::WIC_FLAGS& flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image, _In_opt_ std::function<void __cdecl(IWICMetadataQueryReader*)> getMQR = nullptr);
	API HRESULT LoadFromWICFile(_In_z_ const wchar_t* szFile, _In_ DirectX::WIC_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image, _In_opt_ std::function<void __cdecl(IWICMetadataQueryReader*)> getMQR = nullptr);

	API HRESULT SaveToWICMemory(_In_ const DirectX::Image& image, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _Out_ DirectX::Blob& blob, _In_opt_ const GUID* targetFormat = nullptr, _In_opt_ std::function<void __cdecl(IPropertyBag2*)> setCustomProps = nullptr);
	API HRESULT SaveToWICMemory2(_In_count_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _Out_ DirectX::Blob& blob, _In_opt_ const GUID* targetFormat = nullptr, _In_opt_ std::function<void __cdecl(IPropertyBag2*)> setCustomProps = nullptr);

	API HRESULT SaveToWICFile(_In_ const DirectX::Image& image, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _In_z_ const wchar_t* szFile, _In_opt_ const GUID* targetFormat = nullptr, _In_opt_ std::function<void __cdecl(IPropertyBag2*)> setCustomProps = nullptr);
	API HRESULT SaveToWICFile2(_In_count_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _In_z_ const wchar_t* szFile, _In_opt_ const GUID* targetFormat = nullptr, _In_opt_ std::function<void __cdecl(IPropertyBag2*)> setCustomProps = nullptr);
#endif // WIN32

	// Compatability helpers
	API HRESULT LoadFromTGAMemory2(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;
	API HRESULT LoadFromTGAFile2(_In_z_ const wchar_t* szFile, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept;

	API HRESULT SaveToTGAMemory2(_In_ const DirectX::Image& image, _Out_ DirectX::Blob& blob, _In_opt_ const DirectX::TexMetadata* metadata = nullptr) noexcept;
	API HRESULT SaveToTGAFile2(_In_ const DirectX::Image& image, _In_z_ const wchar_t* szFile, _In_opt_ const DirectX::TexMetadata* metadata = nullptr) noexcept;

#pragma endregion ImageIO

#ifdef WIN32
	API HRESULT FlipRotate(_In_ const DirectX::Image& srcImage, _In_ DirectX::TEX_FR_FLAGS flags, _Out_ DirectX::ScratchImage& image) noexcept;
	API HRESULT FlipRotate2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_FR_FLAGS flags, _Out_ DirectX::ScratchImage& result) noexcept;
	// Flip and/or rotate image
#endif

	API HRESULT Resize(_In_ const DirectX::Image& srcImage, _In_ size_t width, _In_ size_t height, _In_ DirectX::TEX_FILTER_FLAGS filter, _Out_ DirectX::ScratchImage& image) noexcept;
	API HRESULT Resize2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ size_t width, _In_ size_t height, _In_ DirectX::TEX_FILTER_FLAGS filter, _Out_ DirectX::ScratchImage& result) noexcept;

	API HRESULT Convert(_In_ const DirectX::Image& srcImage, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ float threshold, _Out_ DirectX::ScratchImage& image) noexcept;
	API HRESULT Convert2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ float threshold, _Out_ DirectX::ScratchImage& result) noexcept;
	// Convert the image to a new format

	API HRESULT ConvertToSinglePlane(_In_ const DirectX::Image& srcImage, _Out_ DirectX::ScratchImage& image) noexcept;
	API HRESULT ConvertToSinglePlane2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _Out_ DirectX::ScratchImage& image) noexcept;
	// Converts the image from a planar format to an equivalent non-planar format

	API HRESULT GenerateMipMaps(_In_ const DirectX::Image& baseImage, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Inout_ DirectX::ScratchImage& mipChain, _In_ bool allow1D = false) noexcept;
	API HRESULT GenerateMipMaps2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Inout_ DirectX::ScratchImage& mipChain);
	// levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
	// Defaults to Fant filtering which is equivalent to a box filter

	API HRESULT GenerateMipMaps3D(_In_reads_(depth) const DirectX::Image* baseImages, _In_ size_t depth, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Out_ DirectX::ScratchImage& mipChain) noexcept;
	API HRESULT GenerateMipMaps3D2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Out_ DirectX::ScratchImage& mipChain);
	// levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
	// Defaults to Fant filtering which is equivalent to a box filter

	API HRESULT ScaleMipMapsAlphaForCoverage(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ size_t item, _In_ float alphaReference, _Inout_ DirectX::ScratchImage& mipChain) noexcept;

	API HRESULT PremultiplyAlpha(_In_ const DirectX::Image& srcImage, _In_ DirectX::TEX_PMALPHA_FLAGS flags, _Out_ DirectX::ScratchImage& image) noexcept;
	API HRESULT PremultiplyAlpha2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_PMALPHA_FLAGS flags, _Out_ DirectX::ScratchImage& result) noexcept;

	API HRESULT Compress(_In_ const DirectX::Image& srcImage, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_COMPRESS_FLAGS compress, _In_ float threshold, _Out_ DirectX::ScratchImage& cImage) noexcept;
	API HRESULT Compress2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_COMPRESS_FLAGS compress, _In_ float threshold, _Out_ DirectX::ScratchImage& cImages) noexcept;
	// Note that threshold is only used by BC1. TEX_THRESHOLD_DEFAULT is a typical value to use

#if defined(__d3d11_h__) || defined(__d3d11_x_h__)
	API HRESULT Compress3(_In_ ID3D11Device* pDevice, _In_ const DirectX::Image& srcImage, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_COMPRESS_FLAGS compress, _In_ float alphaWeight, _Out_ DirectX::ScratchImage& image) noexcept;
	API HRESULT Compress4(_In_ ID3D11Device* pDevice, _In_ const DirectX::Image* srcImages, _In_ size_t& nimages, _In_ const DirectX::TexMetadata* metadata, _In_ DXGI_FORMAT& format, _In_ DirectX::TEX_COMPRESS_FLAGS& compress, _In_ float& alphaWeight, _Out_ DirectX::ScratchImage& cImages) noexcept;
	// DirectCompute-based compression (alphaWeight is only used by BC7. 1.0 is the typical value to use)
#endif

	API HRESULT Decompress(_In_ const DirectX::Image& cImage, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& image) noexcept;
	API HRESULT Decompress2(_In_reads_(nimages) const DirectX::Image* cImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& images) noexcept;

	API HRESULT ComputeNormalMap(_In_ const DirectX::Image& srcImage, _In_ DirectX::CNMAP_FLAGS flags, _In_ float amplitude, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& normalMap) noexcept;
	API HRESULT ComputeNormalMap2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::CNMAP_FLAGS flags, _In_ float amplitude, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& normalMaps) noexcept;

	API HRESULT CopyRectangle(_In_ const DirectX::Image& srcImage, _In_ const DirectX::Rect& srcRect, _In_ const DirectX::Image& dstImage, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t xOffset, _In_ size_t yOffset) noexcept;

	API HRESULT ComputeMSE(_In_ const DirectX::Image& image1, _In_ const DirectX::Image& image2, _Out_ float& mse, _Out_writes_opt_(4) float* mseV, _In_ DirectX::CMSE_FLAGS flags = DirectX::CMSE_DEFAULT) noexcept;

	API HRESULT EvaluateImage(_In_ const DirectX::Image& image, _In_ std::function<void(_In_reads_(width) const DirectX::XMVECTOR* pixels, size_t width, size_t y)> pixelFunc);
	API HRESULT EvaluateImage2(_In_reads_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ std::function<void(_In_reads_(width) const DirectX::XMVECTOR* pixels, size_t width, size_t y)> pixelFunc);

	API HRESULT TransformImage(_In_ const DirectX::Image& image, _In_ std::function<void(_Out_writes_(width) DirectX::XMVECTOR* outPixels, _In_reads_(width) const DirectX::XMVECTOR* inPixels, size_t width, size_t y)> pixelFunc, DirectX::ScratchImage& result);
	API HRESULT TransformImage2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ std::function<void(_Out_writes_(width) DirectX::XMVECTOR* outPixels, _In_reads_(width) const DirectX::XMVECTOR* inPixels, size_t width, size_t y)> pixelFunc, DirectX::ScratchImage& result);

#ifdef WIN32
	API REFGUID GetWICCodec(_In_ DirectX::WICCodecs codec) noexcept;

	API IWICImagingFactory* GetWICFactory(bool& iswic2) noexcept;
	API void SetWICFactory(_In_opt_ IWICImagingFactory* pWIC) noexcept;
#endif

	//---------------------------------------------------------------------------------
	// DDS helper functions
	API HRESULT EncodeDDSHeader(_In_ const DirectX::TexMetadata& metadata, DirectX::DDS_FLAGS flags, _Out_writes_bytes_to_opt_(maxsize, required) void* pDestination, _In_ size_t maxsize, _Out_ size_t& required) noexcept;

	//---------------------------------------------------------------------------------
	// Direct3D 11 functions
#if defined(__d3d11_h__) || defined(__d3d11_x_h__)
	API bool IsSupportedTexture(_In_ ID3D11Device* pDevice, _In_ const DirectX::TexMetadata& metadata) noexcept;

	API HRESULT CreateTexture(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _Outptr_ ID3D11Resource** ppResource) noexcept;

	API HRESULT CreateShaderResourceView(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _Outptr_ ID3D11ShaderResourceView** ppSRV) noexcept;

	API HRESULT CreateTextureEx(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ D3D11_USAGE usage, _In_ unsigned int bindFlags, _In_ unsigned int cpuAccessFlags, _In_ unsigned int miscFlags, _In_ bool forceSRGB, _Outptr_ ID3D11Resource** ppResource) noexcept;

	API HRESULT CreateTextureEx2(_In_ ID3D11Device* pDevice, _In_ DirectX::ScratchImage* img, _In_ uint32_t& usage, _In_ uint32_t& bindFlags, _In_ uint32_t& cpuAccessFlags, _In_ uint32_t& miscFlags, _In_ bool& forceSRGB, _Outptr_ ID3D11Resource** ppResource) noexcept;

	API HRESULT CreateShaderResourceViewEx(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ D3D11_USAGE usage, _In_ unsigned int bindFlags, _In_ unsigned int cpuAccessFlags, _In_ unsigned int miscFlags, _In_ bool forceSRGB, _Outptr_ ID3D11ShaderResourceView** ppSRV) noexcept;

	API HRESULT CaptureTexture(_In_ ID3D11Device* pDevice, _In_ ID3D11DeviceContext* pContext, _In_ ID3D11Resource* pSource, _Out_ DirectX::ScratchImage& result) noexcept;
#endif

	//---------------------------------------------------------------------------------
	// Direct3D 12 functions
#if defined(__d3d12_h__) || defined(__d3d12_x_h__) || defined(__XBOX_D3D12_X__)
	API bool IsSupportedTexture(_In_ ID3D12Device* pDevice, _In_ const DirectX::TexMetadata& metadata) noexcept;

	API HRESULT CreateTexture(_In_ ID3D12Device* pDevice, _In_ const DirectX::TexMetadata& metadata, _Outptr_ ID3D12Resource** ppResource) noexcept;

	API HRESULT CreateTextureEx(_In_ ID3D12Device* pDevice, _In_ const DirectX::TexMetadata& metadata, _In_ D3D12_RESOURCE_FLAGS resFlags, _In_ bool forceSRGB, _Outptr_ ID3D12Resource** ppResource) noexcept;

	API HRESULT PrepareUpload(_In_ ID3D12Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, std::vector<D3D12_SUBRESOURCE_DATA>& subresources);

	API HRESULT CaptureTexture(_In_ ID3D12CommandQueue* pCommandQueue, _In_ ID3D12Resource* pSource, _In_ bool isCubeMap, _Out_ DirectX::ScratchImage& result, _In_ D3D12_RESOURCE_STATES beforeState = D3D12_RESOURCE_STATE_RENDER_TARGET, _In_ D3D12_RESOURCE_STATES afterState = D3D12_RESOURCE_STATE_RENDER_TARGET) noexcept;
#endif

#ifdef __cplusplus
}
#endif