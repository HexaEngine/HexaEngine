#include "HexaTexture.h"
#include <wincodec.h>

#pragma region IPropertyBag2 methods

HRESULT Read(IPropertyBag2* p, _In_ ULONG cProperties, __RPC__in_ecount_full(cProperties) PROPBAG2* pPropBag, __RPC__in_opt IErrorLog* pErrLog, __RPC__out_ecount_full(cProperties) VARIANT* pvarValue, __RPC__inout_ecount_full_opt(cProperties) HRESULT* phrError)
{
	return p->Read(cProperties, pPropBag, pErrLog, pvarValue, phrError);
}

HRESULT Write(IPropertyBag2* p, _In_ ULONG cProperties, __RPC__in_ecount_full(cProperties) PROPBAG2* pPropBag, __RPC__in_ecount_full(cProperties) VARIANT* pvarValue)
{
	return p->Write(cProperties, pPropBag, pvarValue);
}

HRESULT CountProperties(IPropertyBag2* p, __RPC__out ULONG* pcProperties)
{
	return p->CountProperties(pcProperties);
}

HRESULT GetPropertyInfo(IPropertyBag2* p, _In_ ULONG iProperty, _In_ ULONG cProperties, __RPC__out_ecount_full(cProperties) PROPBAG2* pPropBag, __RPC__out ULONG* pcProperties)
{
	return p->GetPropertyInfo(iProperty, cProperties, pPropBag, pcProperties);
}

HRESULT LoadObject(IPropertyBag2* p, __RPC__in LPCOLESTR pstrName, _In_ DWORD dwHint, __RPC__in_opt IUnknown* pUnkObject, __RPC__in_opt IErrorLog* pErrLog)
{
	return p->LoadObject(pstrName, dwHint, pUnkObject, pErrLog);
}

#pragma endregion

#pragma region IWICMetadataQueryReader methods

HRESULT GetContainerFormat(IWICMetadataQueryReader* p, __RPC__out GUID* pguidContainerFormat)
{
	return p->GetContainerFormat(pguidContainerFormat);
}

HRESULT GetLocation(IWICMetadataQueryReader* p, _In_ UINT cchMaxLength, __RPC__inout_ecount_full_opt(cchMaxLength) WCHAR* wzNamespace, __RPC__out UINT* pcchActualLength)
{
	return p->GetLocation(cchMaxLength, wzNamespace, pcchActualLength);
}

HRESULT GetMetadataByName(IWICMetadataQueryReader* p, __RPC__in LPCWSTR wzName, __RPC__inout_opt PROPVARIANT* pvarValue)
{
	return p->GetMetadataByName(wzName, pvarValue);
}

HRESULT GetEnumerator(IWICMetadataQueryReader* p, __RPC__deref_out_opt IEnumString** ppIEnumString)
{
	return p->GetEnumerator(ppIEnumString);
}

#pragma endregion

#pragma region DXGI Format Utilities

bool IsValid(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsValid(fmt);
}
bool IsCompressed(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsCompressed(fmt);
}
bool IsPacked(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsPacked(fmt);
}
bool IsVideo(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsVideo(fmt);
}
bool IsPlanar(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsPlanar(fmt);
}
bool IsPalettized(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsPalettized(fmt);
}
bool IsDepthStencil(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsDepthStencil(fmt);
}
bool IsSRGB(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::IsSRGB(fmt);
}
bool IsTypeless(_In_ DXGI_FORMAT fmt, _In_ bool partialTypeless) noexcept
{
	return DirectX::IsTypeless(fmt, partialTypeless);
}

bool HasAlpha(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::HasAlpha(fmt);
}

size_t BitsPerPixel(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::BitsPerPixel(fmt);
}

size_t BitsPerColor(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::BitsPerColor(fmt);
}

DirectX::FORMAT_TYPE FormatDataType(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::FormatDataType(fmt);
}
HRESULT ComputePitch(_In_ DXGI_FORMAT fmt, _In_ size_t width, _In_ size_t height, _Out_ size_t& rowPitch, _Out_ size_t& slicePitch, _In_ DirectX::CP_FLAGS flags) noexcept
{
	return DirectX::ComputePitch(fmt, width, height, rowPitch, slicePitch, flags);
}

size_t ComputeScanlines(_In_ DXGI_FORMAT fmt, _In_ size_t height) noexcept
{
	return DirectX::ComputeScanlines(fmt, height);
}

DXGI_FORMAT MakeSRGB(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::MakeSRGB(fmt);
}
DXGI_FORMAT MakeTypeless(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::MakeTypeless(fmt);
}
DXGI_FORMAT MakeTypelessUNORM(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::MakeTypelessUNORM(fmt);
}
DXGI_FORMAT MakeTypelessFLOAT(_In_ DXGI_FORMAT fmt) noexcept
{
	return DirectX::MakeTypelessFLOAT(fmt);
}

#pragma endregion

#pragma region MetadataIO

HRESULT GetMetadataFromDDSMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _In_ DirectX::DDS_FLAGS flags, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromDDSMemory(pSource, size, flags, metadata);
}

HRESULT GetMetadataFromDDSFile(_In_z_ const wchar_t* szFile, _In_ DirectX::DDS_FLAGS flags, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromDDSFile(szFile, flags, metadata);
}

HRESULT GetMetadataFromHDRMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromHDRMemory(pSource, size, metadata);
}

HRESULT GetMetadataFromHDRFile(_In_z_ const wchar_t* szFile, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromHDRFile(szFile, metadata);
}

HRESULT GetMetadataFromTGAMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _In_ DirectX::TGA_FLAGS flags, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromTGAMemory(pSource, size, flags, metadata);
}

HRESULT GetMetadataFromTGAFile(_In_z_ const wchar_t* szFile, _In_ DirectX::TGA_FLAGS flags, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromTGAFile(szFile, flags, metadata);
}

HRESULT GetMetadataFromWICMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _In_ DirectX::WIC_FLAGS flags, _Out_ DirectX::TexMetadata& metadata)
{
	return DirectX::GetMetadataFromWICMemory(pSource, size, flags, metadata);
}

HRESULT GetMetadataFromWICFile(_In_z_ const wchar_t* szFile, _In_ DirectX::WIC_FLAGS flags, _Out_ DirectX::TexMetadata& metadata)
{
	return DirectX::GetMetadataFromWICFile(szFile, flags, metadata);
}

HRESULT GetMetadataFromTGAMemory2(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromTGAMemory(pSource, size, metadata);
}

HRESULT GetMetadataFromTGAFile2(_In_z_ const wchar_t* szFile, _Out_ DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::GetMetadataFromTGAFile(szFile, metadata);
}

#pragma endregion

#pragma region TexMetadata Methods

size_t ComputeIndex(DirectX::TexMetadata* metadata, _In_ size_t mip, _In_ size_t item, _In_ size_t slice) noexcept
{
	return metadata->ComputeIndex(mip, item, slice);
}
// Returns size_t(-1) to indicate an out-of-range error

bool IsCubemap(DirectX::TexMetadata* metadata) noexcept
{
	return metadata->IsCubemap();
}
// Helper for miscFlags

bool IsPMAlpha(DirectX::TexMetadata* metadata) noexcept
{
	return metadata->IsPMAlpha();
}
void SetAlphaMode(DirectX::TexMetadata* metadata, DirectX::TEX_ALPHA_MODE mode) noexcept
{
	metadata->SetAlphaMode(mode);
}
DirectX::TEX_ALPHA_MODE GetAlphaMode(DirectX::TexMetadata* metadata) noexcept
{
	return metadata->GetAlphaMode();
}
// Helpers for miscFlags2

bool IsVolumemap(DirectX::TexMetadata* metadata) noexcept
{
	return metadata->IsVolumemap();
}
// Helper for dimension

#pragma endregion

#pragma region ScratchImage Methods

DirectX::ScratchImage* NewScratchImage()
{
	return new DirectX::ScratchImage();
}

HRESULT Initialize(DirectX::ScratchImage* img, _In_ const DirectX::TexMetadata& mdata, _In_ DirectX::CP_FLAGS flags) noexcept
{
	return img->Initialize(mdata, flags);
}

HRESULT Initialize1D(DirectX::ScratchImage* img, _In_ DXGI_FORMAT fmt, _In_ size_t length, _In_ size_t arraySize, _In_ size_t mipLevels, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->Initialize1D(fmt, length, arraySize, mipLevels, flags);
	return result;
}

HRESULT Initialize2D(DirectX::ScratchImage* img, _In_ DXGI_FORMAT fmt, _In_ size_t width, _In_ size_t height, _In_ size_t arraySize, _In_ size_t mipLevels, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->Initialize2D(fmt, width, height, arraySize, mipLevels, flags);
	return result;
}

HRESULT Initialize3D(DirectX::ScratchImage* img, _In_ DXGI_FORMAT fmt, _In_ size_t width, _In_ size_t height, _In_ size_t depth, _In_ size_t mipLevels, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->Initialize3D(fmt, width, height, depth, mipLevels, flags);
	return result;
}

HRESULT InitializeCube(DirectX::ScratchImage* img, _In_ DXGI_FORMAT fmt, _In_ size_t width, _In_ size_t height, _In_ size_t nCubes, _In_ size_t mipLevels, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->InitializeCube(fmt, width, height, nCubes, mipLevels, flags);
	return result;
}

HRESULT InitializeFromImage(DirectX::ScratchImage* img, _In_ const DirectX::Image srcImage, _In_ bool allow1D, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->InitializeFromImage(srcImage, allow1D, flags);
	return result;
}

HRESULT InitializeArrayFromImages(DirectX::ScratchImage* img, _In_reads_(nImages) const DirectX::Image* images, _In_ size_t nImages, _In_ bool allow1D, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->InitializeArrayFromImages(images, nImages, allow1D, flags);
	return result;
}

HRESULT InitializeCubeFromImages(DirectX::ScratchImage* img, _In_reads_(nImages) const DirectX::Image* images, _In_ size_t nImages, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->InitializeCubeFromImages(images, nImages, flags);
	return result;
}

HRESULT Initialize3DFromImages(DirectX::ScratchImage* img, _In_reads_(depth) const DirectX::Image* images, _In_ size_t depth, _In_ DirectX::CP_FLAGS flags) noexcept
{
	HRESULT result = img->Initialize3DFromImages(images, depth, flags);
	return result;
}

void ScratchImageRelease(DirectX::ScratchImage** img) noexcept
{
	(*img)->Release();
	delete* img;
	*img = nullptr;
}

bool OverrideFormat(DirectX::ScratchImage* img, _In_ DXGI_FORMAT f) noexcept
{
	return img->OverrideFormat(f);
}

const DirectX::TexMetadata& GetMetadata(DirectX::ScratchImage* img) noexcept
{
	return img->GetMetadata();
}

const DirectX::Image* GetImage(DirectX::ScratchImage* img, _In_ size_t mip, _In_ size_t item, _In_ size_t slice) noexcept
{
	return img->GetImage(mip, item, slice);
}

const DirectX::Image* GetImages(DirectX::ScratchImage* img) noexcept
{
	return img->GetImages();
}

size_t GetImageCount(DirectX::ScratchImage* img) noexcept
{
	return img->GetImageCount();
}

uint8_t* GetPixels(DirectX::ScratchImage* img) noexcept
{
	return img->GetPixels();
}

size_t GetPixelsSize(DirectX::ScratchImage* img) noexcept
{
	return img->GetPixelsSize();
}

bool IsAlphaAllOpaque(DirectX::ScratchImage* img)
{
	return img->IsAlphaAllOpaque();
}

#pragma endregion

#pragma region Blob Methods

DirectX::Blob* NewBlob()
{
	return new DirectX::Blob();
}

HRESULT BlobInitialize(DirectX::Blob* blob, _In_ size_t size) noexcept
{
	return blob->Initialize(size);
}

void BlobRelease(DirectX::Blob** blob) noexcept
{
	(*blob)->Release();
	delete* blob;
	*blob = nullptr;
}

void* BlobGetBufferPointer(DirectX::Blob* blob) noexcept
{
	return blob->GetBufferPointer();
}
size_t BlobGetBufferSize(DirectX::Blob* blob) noexcept
{
	return blob->GetBufferSize();
}

HRESULT BlobResize(DirectX::Blob* blob, size_t size) noexcept
{
	return blob->Resize(size);
}
// Reallocate for a new size

HRESULT BlobTrim(DirectX::Blob* blob, size_t size) noexcept
{
	return blob->Trim(size);
}
// Shorten size without reallocation

#pragma endregion

#pragma region ImageIO

// DDS operations
HRESULT LoadFromDDSMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _In_ DirectX::DDS_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromDDSMemory(pSource, size, flags, metadata, *image);
}

HRESULT LoadFromDDSFile(_In_z_ const wchar_t* szFile, _In_ DirectX::DDS_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromDDSFile(szFile, flags, metadata, *image);
}

HRESULT SaveToDDSMemory(_In_ const DirectX::Image& image, _In_ DirectX::DDS_FLAGS flags, _Out_ DirectX::Blob& blob) noexcept
{
	return DirectX::SaveToDDSMemory(image, flags, blob);
}

HRESULT SaveToDDSMemory2(_In_reads_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::DDS_FLAGS flags, _Out_ DirectX::Blob& blob) noexcept
{
	return DirectX::SaveToDDSMemory(images, nimages, metadata, flags, blob);
}

HRESULT SaveToDDSFile(_In_ const DirectX::Image& image, _In_ DirectX::DDS_FLAGS flags, _In_z_ const wchar_t* szFile) noexcept
{
	return DirectX::SaveToDDSFile(image, flags, szFile);
}

HRESULT SaveToDDSFile2(_In_reads_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::DDS_FLAGS flags, _In_z_ const wchar_t* szFile) noexcept
{
	return DirectX::SaveToDDSFile(images, nimages, metadata, flags, szFile);
}

// HDR operations
HRESULT LoadFromHDRMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromHDRMemory(pSource, size, metadata, *image);
}

HRESULT LoadFromHDRFile(_In_z_ const wchar_t* szFile, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromHDRFile(szFile, metadata, *image);
}

HRESULT SaveToHDRMemory(_In_ const DirectX::Image& image, _Out_ DirectX::Blob& blob) noexcept
{
	return DirectX::SaveToHDRMemory(image, blob);
}

HRESULT SaveToHDRFile(_In_ const DirectX::Image& image, _In_z_ const wchar_t* szFile) noexcept
{
	return DirectX::SaveToHDRFile(image, szFile);
}

// TGA operations
HRESULT LoadFromTGAMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _In_ DirectX::TGA_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromTGAMemory(pSource, size, flags, metadata, *image);
}

HRESULT LoadFromTGAFile(_In_z_ const wchar_t* szFile, _In_ DirectX::TGA_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromTGAFile(szFile, flags, metadata, *image);
}

HRESULT SaveToTGAMemory(_In_ const DirectX::Image& image, _In_ DirectX::TGA_FLAGS flags, _Out_ DirectX::Blob& blob, _In_opt_ const DirectX::TexMetadata* metadata) noexcept
{
	return DirectX::SaveToTGAMemory(image, flags, blob, metadata);
}

HRESULT SaveToTGAFile(_In_ const DirectX::Image& image, _In_ DirectX::TGA_FLAGS flags, _In_z_ const wchar_t* szFile, _In_opt_ const DirectX::TexMetadata* metadata) noexcept
{
	return DirectX::SaveToTGAFile(image, flags, szFile, metadata);
}

// WIC operations
#ifdef WIN32
HRESULT LoadFromWICMemory(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _In_ DirectX::WIC_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image, GetMQR getMQR)
{
	return DirectX::LoadFromWICMemory(pSource, size, flags, metadata, *image, getMQR);
}
HRESULT LoadFromWICFile(_In_z_ const wchar_t* szFile, _In_ DirectX::WIC_FLAGS flags, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image, GetMQR getMQR)
{
	return DirectX::LoadFromWICFile(szFile, flags, metadata, *image, getMQR);
}

HRESULT SaveToWICMemory(_In_ const DirectX::Image& image, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _Out_ DirectX::Blob& blob, _In_opt_ const GUID* targetFormat, SetCustomProps customProps)
{
	return DirectX::SaveToWICMemory(image, flags, guidContainerFormat, blob, targetFormat, customProps);
}
HRESULT SaveToWICMemory2(_In_count_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _Out_ DirectX::Blob& blob, _In_opt_ const GUID* targetFormat, SetCustomProps customProps)
{
	return DirectX::SaveToWICMemory(images, nimages, flags, guidContainerFormat, blob, targetFormat, customProps);
}

HRESULT SaveToWICFile(_In_ const DirectX::Image& image, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _In_z_ const wchar_t* szFile, _In_opt_ const GUID* targetFormat, SetCustomProps customProps)
{
	return DirectX::SaveToWICFile(image, flags, guidContainerFormat, szFile, targetFormat, customProps);
}
HRESULT SaveToWICFile2(_In_count_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ DirectX::WIC_FLAGS flags, _In_ REFGUID guidContainerFormat, _In_z_ const wchar_t* szFile, _In_opt_ const GUID* targetFormat, SetCustomProps customProps)
{
	return DirectX::SaveToWICFile(images, nimages, flags, guidContainerFormat, szFile, targetFormat, customProps);
}

#endif // WIN32

// Compatability helpers
HRESULT LoadFromTGAMemory2(_In_reads_bytes_(size) const void* pSource, _In_ size_t size, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromTGAMemory(pSource, size, metadata, *image);
}
HRESULT LoadFromTGAFile2(_In_z_ const wchar_t* szFile, _Out_opt_ DirectX::TexMetadata* metadata, DirectX::ScratchImage* image) noexcept
{
	return DirectX::LoadFromTGAFile(szFile, metadata, *image);
}

HRESULT SaveToTGAMemory2(_In_ const DirectX::Image& image, _Out_ DirectX::Blob& blob, _In_opt_ const DirectX::TexMetadata* metadata) noexcept
{
	return DirectX::SaveToTGAMemory(image, blob, metadata);
}
HRESULT SaveToTGAFile2(_In_ const DirectX::Image& image, _In_z_ const wchar_t* szFile, _In_opt_ const DirectX::TexMetadata* metadata) noexcept
{
	return DirectX::SaveToTGAFile(image, szFile, metadata);
}

#pragma endregion

#pragma region Texture conversion, resizing, mipmap generation, and block compression

#ifdef WIN32
HRESULT FlipRotate(_In_ const DirectX::Image& srcImage, _In_ DirectX::TEX_FR_FLAGS flags, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::FlipRotate(srcImage, flags, image);
}
HRESULT FlipRotate2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_FR_FLAGS flags, _Out_ DirectX::ScratchImage& result) noexcept
{
	return DirectX::FlipRotate(srcImages, nimages, metadata, flags, result);
}
// Flip and/or rotate image
#endif

HRESULT Resize(_In_ const DirectX::Image& srcImage, _In_ size_t width, _In_ size_t height, _In_ DirectX::TEX_FILTER_FLAGS filter, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::Resize(srcImage, width, height, filter, image);
}
HRESULT Resize2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ size_t width, _In_ size_t height, _In_ DirectX::TEX_FILTER_FLAGS filter, _Out_ DirectX::ScratchImage& result) noexcept
{
	return DirectX::Resize(srcImages, nimages, metadata, width, height, filter, result);
}

HRESULT Convert(_In_ const DirectX::Image& srcImage, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ float threshold, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::Convert(srcImage, format, filter, threshold, image);
}
HRESULT Convert2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ float threshold, _Out_ DirectX::ScratchImage& result) noexcept
{
	return DirectX::Convert(srcImages, nimages, metadata, format, filter, threshold, result);
}
// Convert the image to a new format

HRESULT ConvertToSinglePlane(_In_ const DirectX::Image& srcImage, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::ConvertToSinglePlane(srcImage, image);
}
HRESULT ConvertToSinglePlane2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::ConvertToSinglePlane(srcImages, nimages, metadata, image);
}
// Converts the image from a planar format to an equivalent non-planar format

HRESULT GenerateMipMaps(_In_ const DirectX::Image& baseImage, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Inout_ DirectX::ScratchImage& mipChain, _In_ bool allow1D) noexcept
{
	return DirectX::GenerateMipMaps(baseImage, filter, levels, mipChain, allow1D);
}
HRESULT GenerateMipMaps2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Inout_ DirectX::ScratchImage& mipChain)
{
	return DirectX::GenerateMipMaps(srcImages, nimages, metadata, filter, levels, mipChain);
}
// levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
// Defaults to Fant filtering which is equivalent to a box filter

HRESULT GenerateMipMaps3D(_In_reads_(depth) const DirectX::Image* baseImages, _In_ size_t depth, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Out_ DirectX::ScratchImage& mipChain) noexcept
{
	return DirectX::GenerateMipMaps3D(baseImages, depth, filter, levels, mipChain);
}
HRESULT GenerateMipMaps3D2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t levels, _Out_ DirectX::ScratchImage& mipChain)
{
	return DirectX::GenerateMipMaps3D(srcImages, nimages, filter, levels, mipChain);
}
// levels of '0' indicates a full mipchain, otherwise is generates that number of total levels (including the source base image)
// Defaults to Fant filtering which is equivalent to a box filter

HRESULT ScaleMipMapsAlphaForCoverage(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ size_t item, _In_ float alphaReference, _Inout_ DirectX::ScratchImage& mipChain) noexcept
{
	return DirectX::ScaleMipMapsAlphaForCoverage(srcImages, nimages, metadata, item, alphaReference, mipChain);
}

HRESULT PremultiplyAlpha(_In_ const DirectX::Image& srcImage, _In_ DirectX::TEX_PMALPHA_FLAGS flags, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::PremultiplyAlpha(srcImage, flags, image);
}
HRESULT PremultiplyAlpha2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::TEX_PMALPHA_FLAGS flags, _Out_ DirectX::ScratchImage& result) noexcept
{
	return DirectX::PremultiplyAlpha(srcImages, nimages, metadata, flags, result);
}

HRESULT Compress(_In_ const DirectX::Image& srcImage, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_COMPRESS_FLAGS compress, _In_ float threshold, _Out_ DirectX::ScratchImage& cImage) noexcept
{
	return DirectX::Compress(srcImage, format, compress, threshold, cImage);
}
HRESULT Compress2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_COMPRESS_FLAGS compress, _In_ float threshold, _Out_ DirectX::ScratchImage& cImages) noexcept
{
	return DirectX::Compress(srcImages, nimages, metadata, format, compress, threshold, cImages);
}
// Note that thresholdsrcImages used by BC1. TEX_THRESHOLD_DEFAULT is a typical value to use

#if defined(__d3d11_h__) || defined(__d3d11_x_h__)
HRESULT Compress3(_In_ ID3D11Device* pDevice, _In_ const DirectX::Image& srcImage, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_COMPRESS_FLAGS compress, _In_ float alphaWeight, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::Compress(pDevice, srcImage, format, compress, alphaWeight, image);
}
HRESULT Compress4(_In_ ID3D11Device* pDevice, _In_ const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DXGI_FORMAT format, _In_ DirectX::TEX_COMPRESS_FLAGS compress, _In_ float alphaWeight, _Out_ DirectX::ScratchImage& cImages) noexcept
{
	return DirectX::Compress(pDevice, srcImages, nimages, metadata, format, compress, alphaWeight, cImages);
}

// DirectCompute-based compression (alphaWeight is only used by BC7. 1.0 is the typical value to use)
#endif

HRESULT Decompress(_In_ const DirectX::Image& cImage, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& image) noexcept
{
	return DirectX::Decompress(cImage, format, image);
}
HRESULT Decompress2(_In_reads_(nimages) const DirectX::Image* cImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& images) noexcept
{
	return DirectX::Decompress(cImages, nimages, metadata, format, images);
}

#pragma endregion

#pragma region Normal map operations

HRESULT ComputeNormalMap(_In_ const DirectX::Image& srcImage, _In_ DirectX::CNMAP_FLAGS flags, _In_ float amplitude, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& normalMap) noexcept
{
	return DirectX::ComputeNormalMap(srcImage, flags, amplitude, format, normalMap);
}
HRESULT ComputeNormalMap2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ DirectX::CNMAP_FLAGS flags, _In_ float amplitude, _In_ DXGI_FORMAT format, _Out_ DirectX::ScratchImage& normalMaps) noexcept
{
	return DirectX::ComputeNormalMap(srcImages, nimages, metadata, flags, amplitude, format, normalMaps);
}

#pragma endregion

#pragma region Misc image operations

HRESULT CopyRectangle(_In_ const DirectX::Image& srcImage, _In_ const DirectX::Rect& srcRect, _In_ const DirectX::Image& dstImage, _In_ DirectX::TEX_FILTER_FLAGS filter, _In_ size_t xOffset, _In_ size_t yOffset) noexcept
{
	return DirectX::CopyRectangle(srcImage, srcRect, dstImage, filter, xOffset, yOffset);
}

HRESULT ComputeMSE(_In_ const DirectX::Image& image1, _In_ const DirectX::Image& image2, _Out_ float& mse, _Out_writes_opt_(4) float* mseV, _In_ DirectX::CMSE_FLAGS flags) noexcept
{
	return DirectX::ComputeMSE(image1, image2, mse, mseV, flags);
}

HRESULT EvaluateImage(_In_ const DirectX::Image& image, _In_ EvaluateImageFunc pixelFunc)
{
	return DirectX::EvaluateImage(image, pixelFunc);
}
HRESULT EvaluateImage2(_In_reads_(nimages) const DirectX::Image* images, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ EvaluateImageFunc pixelFunc)
{
	return DirectX::EvaluateImage(images, nimages, metadata, pixelFunc);
}

HRESULT TransformImage(_In_ const DirectX::Image& image, _In_ TransformImageFunc pixelFunc, DirectX::ScratchImage& result)
{
	return DirectX::TransformImage(image, pixelFunc, result);
}
HRESULT TransformImage2(_In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ TransformImageFunc pixelFunc, DirectX::ScratchImage& result)
{
	return DirectX::TransformImage(srcImages, nimages, metadata, pixelFunc, result);
}

#pragma endregion

#pragma region WIC utility code

#ifdef WIN32
REFGUID GetWICCodec(_In_ DirectX::WICCodecs codec) noexcept
{
	return DirectX::GetWICCodec(codec);
}

IWICImagingFactory* GetWICFactory(bool& iswic2) noexcept
{
	return DirectX::GetWICFactory(iswic2);
}
void SetWICFactory(_In_opt_ IWICImagingFactory* pWIC) noexcept
{
	DirectX::SetWICFactory(pWIC);
}
#endif

#pragma endregion

#pragma region DDS helper functions

HRESULT EncodeDDSHeader(_In_ const DirectX::TexMetadata& metadata, DirectX::DDS_FLAGS flags, _Out_writes_bytes_to_opt_(maxsize, required) void* pDestination, _In_ size_t maxsize, _Out_ size_t& required) noexcept
{
	return DirectX::EncodeDDSHeader(metadata, flags, pDestination, maxsize, required);
}

#pragma endregion

#pragma region Direct3D 11 functions

#if defined(__d3d11_h__) || defined(__d3d11_x_h__)
bool IsSupportedTexture(_In_ ID3D11Device* pDevice, _In_ const DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::IsSupportedTexture(pDevice, metadata);
}

HRESULT CreateTexture(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _Outptr_ ID3D11Resource** ppResource) noexcept
{
	return DirectX::CreateTexture(pDevice, srcImages, nimages, metadata, ppResource);
}

HRESULT CreateShaderResourceView(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _Outptr_ ID3D11ShaderResourceView** ppSRV) noexcept
{
	return DirectX::CreateShaderResourceView(pDevice, srcImages, nimages, metadata, ppSRV);
}

HRESULT CreateTextureEx(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ D3D11_USAGE usage, _In_ unsigned int bindFlags, _In_ unsigned int cpuAccessFlags, _In_ unsigned int miscFlags, _In_ bool forceSRGB, _Outptr_ ID3D11Resource** ppResource) noexcept
{
	return DirectX::CreateTextureEx(pDevice, srcImages, nimages, metadata, usage, bindFlags, cpuAccessFlags, miscFlags, forceSRGB, ppResource);
}

HRESULT CreateTextureEx2(_In_ ID3D11Device* pDevice, _In_ DirectX::ScratchImage* img, _In_ uint32_t usage, _In_ uint32_t bindFlags, _In_ uint32_t cpuAccessFlags, _In_ uint32_t miscFlags, _In_ bool forceSRGB, _Outptr_ ID3D11Resource** ppResource) noexcept
{
	return DirectX::CreateTextureEx(pDevice, img->GetImages(), img->GetImageCount(), img->GetMetadata(), (D3D11_USAGE)usage, bindFlags, cpuAccessFlags, miscFlags, forceSRGB, ppResource);
}

HRESULT CreateShaderResourceViewEx(_In_ ID3D11Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, _In_ D3D11_USAGE usage, _In_ unsigned int bindFlags, _In_ unsigned int cpuAccessFlags, _In_ unsigned int miscFlags, _In_ bool forceSRGB, _Outptr_ ID3D11ShaderResourceView** ppSRV) noexcept
{
	return DirectX::CreateShaderResourceViewEx(pDevice, srcImages, nimages, metadata, usage, bindFlags, cpuAccessFlags, miscFlags, forceSRGB, ppSRV);
}

HRESULT CaptureTexture(_In_ ID3D11Device* pDevice, _In_ ID3D11DeviceContext* pContext, _In_ ID3D11Resource* pSource, _Out_ DirectX::ScratchImage& result) noexcept
{
	return DirectX::CaptureTexture(pDevice, pContext, pSource, result);
}
#endif

#pragma endregion

#pragma region Direct3D 12 functions

#if defined(__d3d12_h__) || defined(__d3d12_x_h__) || defined(__XBOX_D3D12_X__)
bool IsSupportedTexture(_In_ ID3D12Device* pDevice, _In_ const DirectX::TexMetadata& metadata) noexcept
{
	return DirectX::IsSupportedTexture(pDevice, metadata);
}

HRESULT CreateTexture(_In_ ID3D12Device* pDevice, _In_ const DirectX::TexMetadata& metadata, _Outptr_ ID3D12Resource** ppResource) noexcept
{
	return DirectX::CreateTexture(pDevice, metadata, ppResource);
}

HRESULT CreateTextureEx(_In_ ID3D12Device* pDevice, _In_ const DirectX::TexMetadata& metadata, _In_ D3D12_RESOURCE_FLAGS resFlags, _In_ bool forceSRGB, _Outptr_ ID3D12Resource** ppResource) noexcept
{
	return DirectX::CreateTextureEx(pDevice, metadata, resFlags, forceSRGB, ppResource);
}

HRESULT PrepareUpload(_In_ ID3D12Device* pDevice, _In_reads_(nimages) const DirectX::Image* srcImages, _In_ size_t nimages, _In_ const DirectX::TexMetadata& metadata, std::vector<D3D12_SUBRESOURCE_DATA>& subresources)
{
	return DirectX::PrepareUpload(pDevice, srcImages, nimages, metadata, subresources);
}

HRESULT CaptureTexture(_In_ ID3D12CommandQueue* pCommandQueue, _In_ ID3D12Resource* pSource, _In_ bool isCubeMap, _Out_ DirectX::ScratchImage& result, _In_ D3D12_RESOURCE_STATES beforeState, _In_ D3D12_RESOURCE_STATES afterState) noexcept
{
	return DirectX::CaptureTexture(pCommandQueue, pSource, isCubeMap, result, beforeState, afterState);
}
#endif

#pragma endregion