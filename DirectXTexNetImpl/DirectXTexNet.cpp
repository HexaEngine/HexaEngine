#include "stdafx.h"

#include "DirectXTexNet.h"

#ifdef _OPENMP
#include <omp.h>
#pragma warning(disable : 4616 6993)
#endif
#include <wincodec.h>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace DirectXTexNet
{
#ifdef _OPENMP
	void TexHelperImpl::SetOmpMaxThreadCount(int maxThreadCount)
	{
		omp_set_num_threads(maxThreadCount);
	}
#endif

	//---------------------------------------------------------------------------------
	// DXGI Format Utilities
	bool TexHelperImpl::IsValid(DXGI_FORMAT fmt)
	{
		return DirectX::IsValid(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsCompressed(DXGI_FORMAT fmt)
	{
		return DirectX::IsCompressed(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsPacked(DXGI_FORMAT fmt)
	{
		return DirectX::IsPacked(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsVideo(DXGI_FORMAT fmt)
	{
		return DirectX::IsVideo(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsPlanar(DXGI_FORMAT fmt)
	{
		return DirectX::IsPlanar(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsPalettized(DXGI_FORMAT fmt)
	{
		return DirectX::IsPalettized(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsDepthStencil(DXGI_FORMAT fmt)
	{
		return DirectX::IsDepthStencil(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsSRGB(DXGI_FORMAT fmt)
	{
		return DirectX::IsSRGB(static_cast<::DXGI_FORMAT>(fmt));
	}
	bool TexHelperImpl::IsTypeless(DXGI_FORMAT fmt, bool partialTypeless)
	{
		return DirectX::IsTypeless(static_cast<::DXGI_FORMAT>(fmt), partialTypeless);
	}
	bool TexHelperImpl::HasAlpha(DXGI_FORMAT fmt)
	{
		return DirectX::HasAlpha(static_cast<::DXGI_FORMAT>(fmt));
	}
	Size_t TexHelperImpl::BitsPerPixel(DXGI_FORMAT fmt)
	{
		return static_cast<Size_t>(DirectX::BitsPerPixel(static_cast<::DXGI_FORMAT>(fmt)));
	}
	Size_t TexHelperImpl::BitsPerColor(DXGI_FORMAT fmt)
	{
		return static_cast<Size_t>(DirectX::BitsPerColor(static_cast<::DXGI_FORMAT>(fmt)));
	}
	void TexHelperImpl::ComputePitch(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_T% rowPitch, Size_T% slicePitch, CP_FLAGS flags)
	{
		size_t _rowPitch;
		size_t _slicePitch;
		DirectX::ComputePitch(static_cast<::DXGI_FORMAT>(fmt), width, height, _rowPitch, _slicePitch, static_cast<DirectX::CP_FLAGS>(flags));
		rowPitch = (_rowPitch);
		slicePitch = (_slicePitch);
	}
	Size_T TexHelperImpl::ComputeScanlines(DXGI_FORMAT fmt, Size_t height)
	{
		return (DirectX::ComputeScanlines(static_cast<::DXGI_FORMAT>(fmt), height));
	}
	Size_t TexHelperImpl::ComputeImageIndex(TexMetadata^ metadata, Size_t mip, Size_t item, Size_t slice)
	{
		DirectX::TexMetadata native;
		FromManaged(metadata, native);
		return static_cast<Size_t>(native.ComputeIndex(mip, item, slice));
	}
	DXGI_FORMAT TexHelperImpl::MakeSRGB(DXGI_FORMAT fmt)
	{
		return static_cast<DXGI_FORMAT>(DirectX::MakeSRGB(static_cast<::DXGI_FORMAT>(fmt)));
	}
	DXGI_FORMAT TexHelperImpl::MakeTypeless(DXGI_FORMAT fmt)
	{
		return static_cast<DXGI_FORMAT>(DirectX::MakeTypeless(static_cast<::DXGI_FORMAT>(fmt)));
	}
	DXGI_FORMAT TexHelperImpl::MakeTypelessUNORM(DXGI_FORMAT fmt)
	{
		return static_cast<DXGI_FORMAT>(DirectX::MakeTypelessUNORM(static_cast<::DXGI_FORMAT>(fmt)));
	}
	DXGI_FORMAT TexHelperImpl::MakeTypelessFLOAT(DXGI_FORMAT fmt)
	{
		return static_cast<DXGI_FORMAT>(DirectX::MakeTypelessFLOAT(static_cast<::DXGI_FORMAT>(fmt)));
	}

	//---------------------------------------------------------------------------------
	// Texture metadata
	TexMetadata^ TexHelperImpl::GetMetadataFromDDSMemory(IntPtr pSource, Size_T size, DDS_FLAGS flags)
	{
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromDDSMemory(pSource.ToPointer(), static_cast<size_t>(size), static_cast<DirectX::DDS_FLAGS>(flags), native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}
	TexMetadata^ TexHelperImpl::GetMetadataFromDDSFile(String^ szFile, DDS_FLAGS flags)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromDDSFile(filenameCStr, static_cast<DirectX::DDS_FLAGS>(flags), native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}
	TexMetadata^ TexHelperImpl::GetMetadataFromHDRMemory(IntPtr pSource, Size_T size)
	{
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromHDRMemory(pSource.ToPointer(), static_cast<size_t>(size), native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}
	TexMetadata^ TexHelperImpl::GetMetadataFromHDRFile(String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromHDRFile(filenameCStr, native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}
	TexMetadata^ TexHelperImpl::GetMetadataFromTGAMemory(IntPtr pSource, Size_T size)
	{
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromTGAMemory(pSource.ToPointer(), static_cast<size_t>(size), native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}
	TexMetadata^ TexHelperImpl::GetMetadataFromTGAFile(String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromTGAFile(filenameCStr, native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}
	TexMetadata^ TexHelperImpl::GetMetadataFromWICMemory(IntPtr pSource, Size_T size, WIC_FLAGS flags)
	{
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromWICMemory(pSource.ToPointer(), static_cast<size_t>(size), static_cast<DirectX::WIC_FLAGS>(flags), native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}
	TexMetadata^ TexHelperImpl::GetMetadataFromWICFile(String^ szFile, WIC_FLAGS flags)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);
		DirectX::TexMetadata native;
		auto hr = DirectX::GetMetadataFromWICFile(filenameCStr, static_cast<DirectX::WIC_FLAGS>(flags), native);
		Marshal::ThrowExceptionForHR(hr);
		return ToManaged(native);
	}

	//---------------------------------------------------------------------------------
	// Basic ScratchImage methods
	ScratchImage^ TexHelperImpl::Initialize(TexMetadata^ _metadata, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			DirectX::TexMetadata native;
			FromManaged(_metadata, native);
			HRESULT hr = image->scratchImage_->Initialize(native, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::Initialize1D(DXGI_FORMAT fmt, Size_t length, Size_t arraySize, Size_t mipLevels, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->Initialize1D(static_cast<::DXGI_FORMAT>(fmt), length, arraySize, mipLevels, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::Initialize2D(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_t arraySize, Size_t mipLevels, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->Initialize2D(static_cast<::DXGI_FORMAT>(fmt), width, height, arraySize, mipLevels, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::Initialize3D(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_t depth, Size_t mipLevels, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->Initialize3D(static_cast<::DXGI_FORMAT>(fmt), width, height, depth, mipLevels, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::InitializeCube(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_t nCubes, Size_t mipLevels, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->InitializeCube(static_cast<::DXGI_FORMAT>(fmt), width, height, nCubes, mipLevels, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::InitializeTemporary(array<Image^>^ images, TexMetadata^ metadata, ... array<IDisposable^>^ takeOwnershipOf)
	{
		if (images->LongLength > INT_MAX)
		{
			throw gcnew ArgumentOutOfRangeException("To many images in the array");
		}
		return gcnew TempScratchImageImpl(images, metadata, takeOwnershipOf);
	}
	ScratchImage^ ScratchImageImpl::CreateImageCopy(Size_t imageIndex, bool allow1D, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->InitializeFromImage(*GetImageInternal(imageIndex), allow1D, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::CreateArrayCopy(Size_t startIndex, Size_t nImages, bool allow1D, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->InitializeArrayFromImages(GetImagesInternal() + static_cast<size_t>(startIndex), nImages, allow1D, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::CreateCubeCopy(Size_t startIndex, Size_t nImages, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->InitializeCubeFromImages(GetImagesInternal() + static_cast<size_t>(startIndex), nImages, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::CreateVolumeCopy(Size_t startIndex, Size_t depth, CP_FLAGS flags)
	{
		ActualScratchImageImpl^ image = gcnew ActualScratchImageImpl();

		try
		{
			HRESULT hr = image->scratchImage_->Initialize3DFromImages(GetImagesInternal() + static_cast<size_t>(startIndex), depth, static_cast<DirectX::CP_FLAGS>(flags));
			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::CreateCopyWithEmptyMipMaps(Size_t levels, DXGI_FORMAT fmt, CP_FLAGS flags, bool zeroOutMipMaps)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto metaData = GetMetadataInternal();

			HRESULT hr;
			if (metaData.miscFlags & DirectX::TEX_MISC_TEXTURECUBE)
			{
				hr = image->scratchImage_->InitializeCube(static_cast<::DXGI_FORMAT>(fmt), metaData.width, metaData.height, metaData.arraySize / 6, levels, static_cast<DirectX::CP_FLAGS>(flags));
			}
			else
			{
				hr = image->scratchImage_->Initialize2D(static_cast<::DXGI_FORMAT>(fmt), metaData.width, metaData.height, metaData.arraySize, levels, static_cast<DirectX::CP_FLAGS>(flags));
			}

			Marshal::ThrowExceptionForHR(hr);

			// Copy each array entry.
			for (size_t item = 0; item < metaData.arraySize; item++)
			{
				hr = DirectX::CopyRectangle(
					*GetImageInternal(0, item, 0),
					DirectX::Rect(0, 0, metaData.width, metaData.height),
					*image->scratchImage_->GetImage(0, item, 0),
					DirectX::TEX_FILTER_DEFAULT,	// Filter -- not needed since source and dest format are the same.
					0,
					0);
				Marshal::ThrowExceptionForHR(hr);

				if (zeroOutMipMaps)
				{
					// Zero out mipchain.
					for (size_t mip = 1; mip < image->scratchImage_->GetMetadata().mipLevels; mip++)
					{
						auto* mipImage = image->GetImageInternal(mip, item, 0);
						auto* data = mipImage->pixels;
						auto size = mipImage->rowPitch * mipImage->height;
						ZeroMemory(data, size);
					}
				}
			}

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}

	//---------------------------------------------------------------------------------
	// Image I/O

	// DDS operations
	ScratchImage^ TexHelperImpl::LoadFromDDSMemory(IntPtr pSource, Size_T size, DDS_FLAGS flags)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromDDSMemory(pSource.ToPointer(), static_cast<size_t>(size), static_cast<DirectX::DDS_FLAGS>(flags), nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::LoadFromDDSFile(String^ szFile, DDS_FLAGS flags)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);

		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromDDSFile(filenameCStr, static_cast<DirectX::DDS_FLAGS>(flags), nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	UnmanagedMemoryStream^ ScratchImageImpl::SaveToDDSMemory(Size_t imageIndex, DDS_FLAGS flags)
	{
		DirectX::Blob* blob = new DirectX::Blob();
		try
		{
			auto hr = DirectX::SaveToDDSMemory(*GetImageInternal(imageIndex), static_cast<DirectX::DDS_FLAGS>(flags), *blob);

			Marshal::ThrowExceptionForHR(hr);

			return gcnew BlobImpl(blob);
		}
		catch (Exception^)
		{
			delete blob;
			throw;
		}
	}
	UnmanagedMemoryStream^ ScratchImageImpl::SaveToDDSMemory(DDS_FLAGS flags)
	{
		DirectX::Blob* blob = new DirectX::Blob();
		try
		{
			auto hr = DirectX::SaveToDDSMemory(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), static_cast<DirectX::DDS_FLAGS>(flags), *blob);

			Marshal::ThrowExceptionForHR(hr);

			return gcnew BlobImpl(blob);
		}
		catch (Exception^)
		{
			delete blob;
			throw;
		}
	}
	void ScratchImageImpl::SaveToDDSFile(Size_t imageIndex, DDS_FLAGS flags, String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);

		auto hr = DirectX::SaveToDDSFile(*GetImageInternal(imageIndex), static_cast<DirectX::DDS_FLAGS>(flags), filenameCStr);

		Marshal::ThrowExceptionForHR(hr);
	}
	void ScratchImageImpl::SaveToDDSFile(DDS_FLAGS flags, String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);

		auto hr = DirectX::SaveToDDSFile(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), static_cast<DirectX::DDS_FLAGS>(flags), filenameCStr);

		Marshal::ThrowExceptionForHR(hr);
	}

	// HDR operations
	ScratchImage^ TexHelperImpl::LoadFromHDRMemory(IntPtr pSource, Size_T size)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromHDRMemory(pSource.ToPointer(), static_cast<size_t>(size), nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::LoadFromHDRFile(String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);

		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromHDRFile(filenameCStr, nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	UnmanagedMemoryStream^ ScratchImageImpl::SaveToHDRMemory(Size_t imageIndex)
	{
		DirectX::Blob* blob = new DirectX::Blob();
		try
		{
			auto hr = DirectX::SaveToHDRMemory(*GetImageInternal(imageIndex), *blob);

			Marshal::ThrowExceptionForHR(hr);

			return gcnew BlobImpl(blob);
		}
		catch (Exception^)
		{
			delete blob;
			throw;
		}
	}
	void ScratchImageImpl::SaveToHDRFile(Size_t imageIndex, String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);

		auto hr = DirectX::SaveToHDRFile(*GetImageInternal(imageIndex), filenameCStr);

		Marshal::ThrowExceptionForHR(hr);
	}

	// TGA operations
	ScratchImage^ TexHelperImpl::LoadFromTGAMemory(IntPtr pSource, Size_T size)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromTGAMemory(pSource.ToPointer(), static_cast<size_t>(size), nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::LoadFromTGAFile(String^ filename)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(filename);

		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromTGAFile(filenameCStr, nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	UnmanagedMemoryStream^ ScratchImageImpl::SaveToTGAMemory(Size_t imageIndex)
	{
		DirectX::Blob* blob = new DirectX::Blob();
		try
		{
			auto hr = DirectX::SaveToTGAMemory(*GetImageInternal(imageIndex), *blob);

			Marshal::ThrowExceptionForHR(hr);

			return gcnew BlobImpl(blob);
		}
		catch (Exception^)
		{
			delete blob;
			throw;
		}
	}
	void ScratchImageImpl::SaveToTGAFile(Size_t imageIndex, String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);

		auto hr = DirectX::SaveToTGAFile(*GetImageInternal(imageIndex), filenameCStr);

		Marshal::ThrowExceptionForHR(hr);
	}

	// WIC operations
	ScratchImage^ TexHelperImpl::LoadFromWICMemory(IntPtr pSource, Size_T size, WIC_FLAGS flags)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromWICMemory(pSource.ToPointer(), static_cast<size_t>(size), static_cast<DirectX::WIC_FLAGS>(flags), nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ TexHelperImpl::LoadFromWICFile(String^ filename, WIC_FLAGS flags)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(filename);

		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::LoadFromWICFile(filenameCStr, static_cast<DirectX::WIC_FLAGS>(flags), nullptr, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	UnmanagedMemoryStream^ ScratchImageImpl::SaveToWICMemory(Size_t imageIndex, WIC_FLAGS flags, Guid guidContainerFormat)
	{
		DirectX::Blob* blob = new DirectX::Blob();
		try
		{
			pin_ptr<Guid> ptrGuid = &guidContainerFormat;
			GUID* ptrGUID = (GUID*)ptrGuid;
			auto hr = DirectX::SaveToWICMemory(*GetImageInternal(imageIndex), static_cast<DirectX::WIC_FLAGS>(flags), *ptrGUID, *blob);

			Marshal::ThrowExceptionForHR(hr);
			return gcnew BlobImpl(blob);
		}
		catch (Exception^)
		{
			delete blob;
			throw;
		}
	}
	UnmanagedMemoryStream^ ScratchImageImpl::SaveToWICMemory(Size_t startIndex, Size_t nImages, WIC_FLAGS flags, Guid guidContainerFormat)
	{
		DirectX::Blob* blob = new DirectX::Blob();
		try
		{
			pin_ptr<Guid> ptrGuid = &guidContainerFormat;
			GUID* ptrGUID = (GUID*)ptrGuid;
			auto hr = DirectX::SaveToWICMemory(GetImagesInternal() + static_cast<size_t>(startIndex), nImages, static_cast<DirectX::WIC_FLAGS>(flags), *ptrGUID, *blob);

			Marshal::ThrowExceptionForHR(hr);

			return gcnew BlobImpl(blob);
		}
		catch (Exception^)
		{
			delete blob;
			throw;
		}
	}
	void ScratchImageImpl::SaveToWICFile(Size_t imageIndex, WIC_FLAGS flags, Guid guidContainerFormat, String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);
		pin_ptr<Guid> ptrGuid = &guidContainerFormat;
		GUID* ptrGUID = (GUID*)ptrGuid;
		auto hr = DirectX::SaveToWICFile(*GetImageInternal(imageIndex), static_cast<DirectX::WIC_FLAGS>(flags), *ptrGUID, filenameCStr);

		Marshal::ThrowExceptionForHR(hr);
	}
	void ScratchImageImpl::SaveToWICFile(Size_t startIndex, Size_t nImages, WIC_FLAGS flags, Guid guidContainerFormat, String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);

		pin_ptr<Guid> ptrGuid = &guidContainerFormat;
		GUID* ptrGUID = (GUID*)ptrGuid;
		auto hr = DirectX::SaveToWICFile(GetImagesInternal() + static_cast<size_t>(startIndex), nImages, static_cast<DirectX::WIC_FLAGS>(flags), *ptrGUID, filenameCStr);

		Marshal::ThrowExceptionForHR(hr);
	}

	HRESULT SaveToJPGMemoryHelper(const DirectX::Image& image, float quality, DirectX::Blob& blob)
	{
		return DirectX::SaveToWICMemory(image, DirectX::WIC_FLAGS_NONE, GUID_ContainerFormatJpeg, blob, nullptr, [&](IPropertyBag2* props)
			{
				PROPBAG2 options[1] = { 0 };
				options[0].pstrName = const_cast<wchar_t*>(L"ImageQuality");

				VARIANT varValues[1];
				varValues[0].vt = VT_R4;
				varValues[0].fltVal = quality;

				(void)props->Write(1, options, varValues);
			});
	}

	UnmanagedMemoryStream^ ScratchImageImpl::SaveToJPGMemory(Size_t imageIndex, float quality)
	{
		DirectX::Blob* blob = new DirectX::Blob();
		try
		{
			auto hr = SaveToJPGMemoryHelper(*GetImageInternal(imageIndex), quality, *blob);

			Marshal::ThrowExceptionForHR(hr);
			return gcnew BlobImpl(blob);
		}
		catch (Exception^)
		{
			delete blob;
			throw;
		}
	}

	HRESULT SaveToJPGFileHelper(const DirectX::Image& image, float quality, const wchar_t* filenameCStr)
	{
		return DirectX::SaveToWICFile(image, DirectX::WIC_FLAGS_NONE, GUID_ContainerFormatJpeg, filenameCStr, nullptr, [&](IPropertyBag2* props)
			{
				PROPBAG2 options[1] = { 0 };
				options[0].pstrName = const_cast<wchar_t*>(L"ImageQuality");

				VARIANT varValues[1];
				varValues[0].vt = VT_R4;
				varValues[0].fltVal = quality;

				(void)props->Write(1, options, varValues);
			});
	}

	void ScratchImageImpl::SaveToJPGFile(Size_t imageIndex, float quality, String^ szFile)
	{
		pin_ptr<const wchar_t> filenameCStr = PtrToStringChars(szFile);
		auto hr = SaveToJPGFileHelper(*GetImageInternal(imageIndex), quality, filenameCStr);

		Marshal::ThrowExceptionForHR(hr);
	}

	//---------------------------------------------------------------------------------
	// Texture conversion, resizing, mipmap generation, and block compression
	ScratchImage^ ScratchImageImpl::FlipRotate(Size_t imageIndex, TEX_FR_FLAGS flags)
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::FlipRotate(*GetImageInternal(imageIndex), static_cast<DirectX::TEX_FR_FLAGS>(flags), *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::FlipRotate(TEX_FR_FLAGS flags)
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::FlipRotate(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), static_cast<DirectX::TEX_FR_FLAGS>(flags), *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Resize(Size_t imageIndex, Size_t width, Size_t height, TEX_FILTER_FLAGS filter)
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Resize(*GetImageInternal(imageIndex), width, height, static_cast<DirectX::TEX_FILTER_FLAGS>(filter), *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Resize(Size_t width, Size_t height, TEX_FILTER_FLAGS filter)
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Resize(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), width, height, static_cast<DirectX::TEX_FILTER_FLAGS>(filter), *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Convert(Size_t imageIndex, DXGI_FORMAT format, TEX_FILTER_FLAGS filter, float threshold)
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Convert(*GetImageInternal(imageIndex), static_cast<::DXGI_FORMAT>(format), static_cast<DirectX::TEX_FILTER_FLAGS>(filter), threshold, *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Convert(DXGI_FORMAT format, TEX_FILTER_FLAGS filter, float threshold)
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Convert(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), static_cast<::DXGI_FORMAT>(format), static_cast<DirectX::TEX_FILTER_FLAGS>(filter), threshold, *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::ConvertToSinglePlane(Size_t imageIndex)
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::ConvertToSinglePlane(*GetImageInternal(imageIndex), *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::ConvertToSinglePlane()
	{
		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::ConvertToSinglePlane(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::GenerateMipMaps(Size_t imageIndex, TEX_FILTER_FLAGS filter, Size_t levels, bool allow1D)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::GenerateMipMaps(*GetImageInternal(imageIndex), static_cast<DirectX::TEX_FILTER_FLAGS>(filter), levels, *image->scratchImage_, allow1D);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::GenerateMipMaps(TEX_FILTER_FLAGS filter, Size_t levels)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::GenerateMipMaps(GetImagesInternal(), GetImageCountInternal(),
				GetMetadataInternal(), static_cast<DirectX::TEX_FILTER_FLAGS>(filter), levels, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^ e)
		{
			delete image;
			throw e;
		}
	}
	ScratchImage^ ScratchImageImpl::GenerateMipMaps3D(Size_t startIndex, Size_t depth, TEX_FILTER_FLAGS filter, Size_t levels)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::GenerateMipMaps3D(GetImagesInternal() + static_cast<size_t>(startIndex), depth, static_cast<DirectX::TEX_FILTER_FLAGS>(filter), levels, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::GenerateMipMaps3D(TEX_FILTER_FLAGS filter, Size_t levels)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::GenerateMipMaps3D(GetImagesInternal(), GetImageCountInternal(),
				GetMetadataInternal(), static_cast<DirectX::TEX_FILTER_FLAGS>(filter), levels, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::PremultiplyAlpha(Size_t imageIndex, TEX_PMALPHA_FLAGS flags)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::PremultiplyAlpha(*GetImageInternal(imageIndex), static_cast<DirectX::TEX_PMALPHA_FLAGS>(flags), *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::PremultiplyAlpha(TEX_PMALPHA_FLAGS flags)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::PremultiplyAlpha(GetImagesInternal(), GetImageCountInternal(),
				GetMetadataInternal(), static_cast<DirectX::TEX_PMALPHA_FLAGS>(flags), *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Compress(Size_t imageIndex, DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float threshold)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Compress(*GetImageInternal(imageIndex), static_cast<::DXGI_FORMAT>(format), static_cast<DirectX::TEX_COMPRESS_FLAGS>(compress), threshold, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Compress(DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float threshold)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Compress(
				GetImagesInternal(),
				GetImageCountInternal(),
				GetMetadataInternal(),
				static_cast<::DXGI_FORMAT>(format),
				static_cast<DirectX::TEX_COMPRESS_FLAGS>(compress),
				threshold,
				*image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Compress(Size_t imageIndex, ID3D11DevicePtr pDevice, DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float alphaWeight)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Compress(static_cast<ID3D11Device*>(pDevice.ToPointer()), *GetImageInternal(imageIndex), static_cast<::DXGI_FORMAT>(format), static_cast<DirectX::TEX_COMPRESS_FLAGS>(compress), alphaWeight, *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Compress(ID3D11DevicePtr pDevice, DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float alphaWeight)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Compress(
				static_cast<ID3D11Device*>(pDevice.ToPointer()),
				GetImagesInternal(),
				GetImageCountInternal(),
				GetMetadataInternal(),
				static_cast<::DXGI_FORMAT>(format),
				static_cast<DirectX::TEX_COMPRESS_FLAGS>(compress),
				alphaWeight,
				*image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Decompress(Size_t imageIndex, DXGI_FORMAT format)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Decompress(*GetImageInternal(imageIndex), static_cast<::DXGI_FORMAT>(format), *image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::Decompress(DXGI_FORMAT format)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::Decompress(
				GetImagesInternal(),
				GetImageCountInternal(),
				GetMetadataInternal(),
				static_cast<::DXGI_FORMAT>(format),
				*image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}

	//---------------------------------------------------------------------------------
	// Normal map operations
	ScratchImage^ ScratchImageImpl::ComputeNormalMap(Size_t imageIndex, CNMAP_FLAGS flags, float amplitude, DXGI_FORMAT format)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::ComputeNormalMap(
				*GetImageInternal(imageIndex),
				static_cast<DirectX::CNMAP_FLAGS>(flags),
				amplitude,
				static_cast<::DXGI_FORMAT>(format),
				*image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);
			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::ComputeNormalMap(CNMAP_FLAGS flags, float amplitude, DXGI_FORMAT format)
	{
		auto image = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::ComputeNormalMap(
				GetImagesInternal(),
				GetImageCountInternal(),
				GetMetadataInternal(),
				static_cast<DirectX::CNMAP_FLAGS>(flags),
				amplitude,
				static_cast<::DXGI_FORMAT>(format),
				*image->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return image;
		}
		catch (Exception^)
		{
			delete image;
			throw;
		}
	}

	//---------------------------------------------------------------------------------
	// Misc image operations
	void TexHelperImpl::CopyRectangle(
		Image^ srcImage,
		Size_t srcX,
		Size_t srcY,
		Size_t srcWidth,
		Size_t srcHeight,
		Image^ dstImage,
		TEX_FILTER_FLAGS filter,
		Size_t xOffset,
		Size_t yOffset)
	{
		DirectX::Image nativeSrc, nativeDst;
		FromManaged(srcImage, nativeSrc);
		FromManaged(dstImage, nativeDst);
		DirectX::Rect rect = DirectX::Rect(srcX, srcY, srcWidth, srcHeight);
		auto hr = DirectX::CopyRectangle(nativeSrc, rect, nativeDst, static_cast<DirectX::TEX_FILTER_FLAGS>(filter), xOffset, yOffset);

		Marshal::ThrowExceptionForHR(hr);
	}
	void TexHelperImpl::ComputeMSE(Image^ image, Image^ image2, float% mse, MseV% mseV, CMSE_FLAGS flags)
	{
		DirectX::Image nativeImage, nativeImage2;
		FromManaged(image, nativeImage);
		FromManaged(image2, nativeImage2);
		pin_ptr<float> ptrMse = &mse;
		pin_ptr<MseV> ptrMseV = &mseV;

		auto hr = DirectX::ComputeMSE(nativeImage, nativeImage2, *(float*)ptrMse, (float*)ptrMseV, static_cast<DirectX::CMSE_FLAGS>(flags));

		Marshal::ThrowExceptionForHR(hr);
	}
	void ScratchImageImpl::EvaluateImage(Size_t imageIndex, EvaluatePixelsDelegate^ pixelFunc)
	{
		auto func = static_cast<EvaluatePixelsFunctionDeclaration>(Marshal::GetFunctionPointerForDelegate(pixelFunc).ToPointer());

		auto hr = DirectX::EvaluateImage(*GetImageInternal(imageIndex), func);

		Marshal::ThrowExceptionForHR(hr);
	}
	void ScratchImageImpl::EvaluateImage(EvaluatePixelsDelegate^ pixelFunc)
	{
		auto func = static_cast<EvaluatePixelsFunctionDeclaration>(Marshal::GetFunctionPointerForDelegate(pixelFunc).ToPointer());

		auto hr = DirectX::EvaluateImage(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), func);

		Marshal::ThrowExceptionForHR(hr);
	}
	ScratchImage^ ScratchImageImpl::TransformImage(Size_t imageIndex, TransformPixelsDelegate^ pixelFunc)
	{
		auto func = static_cast<TransformPixelsFunctionDeclaration>(Marshal::GetFunctionPointerForDelegate(pixelFunc).ToPointer());

		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::TransformImage(*GetImageInternal(imageIndex), func, *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
	ScratchImage^ ScratchImageImpl::TransformImage(TransformPixelsDelegate^ pixelFunc)
	{
		auto func = static_cast<TransformPixelsFunctionDeclaration>(Marshal::GetFunctionPointerForDelegate(pixelFunc).ToPointer());

		auto result = gcnew ActualScratchImageImpl();
		try
		{
			auto hr = DirectX::TransformImage(GetImagesInternal(), GetImageCountInternal(), GetMetadataInternal(), func, *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}

	//---------------------------------------------------------------------------------
	// WIC utility code
	Guid TexHelperImpl::GetWICCodec(WICCodecs codec)
	{
		auto guid = DirectX::GetWICCodec(static_cast<DirectX::WICCodecs>(codec));
		return *(Guid*)&guid;
	}
	IWICImagingFactoryPtr TexHelperImpl::GetWICFactory(bool iswic2)
	{
		return IWICImagingFactoryPtr(IntPtr(DirectX::GetWICFactory(iswic2)));
	}
	void TexHelperImpl::SetWICFactory(IWICImagingFactoryPtr pWIC)
	{
		auto factoryRaw = static_cast<::IWICImagingFactory*>(pWIC.ToPointer());
		DirectX::SetWICFactory(factoryRaw);
	}

	//---------------------------------------------------------------------------------
	// Direct3D 11 functions
	bool TexHelperImpl::IsSupportedTexture(ID3D11DevicePtr pDevice, TexMetadata^ _metadata)
	{
		DirectX::TexMetadata native;
		FromManaged(_metadata, native);

		auto deviceRaw = static_cast<ID3D11Device*>(pDevice.ToPointer());

		return DirectX::IsSupportedTexture(deviceRaw, native);
	}
	ID3D11ResourcePtr ScratchImageImpl::CreateTexture(ID3D11DevicePtr pDevice)
	{
		auto deviceRaw = static_cast<ID3D11Device*>(pDevice.ToPointer());

		ID3D11Resource* texture = nullptr;
		HRESULT hr = DirectX::CreateTexture(
			deviceRaw,
			GetImagesInternal(),
			GetImageCountInternal(),
			GetMetadataInternal(),
			&texture);

		Marshal::ThrowExceptionForHR(hr);

		return ID3D11ResourcePtr(IntPtr(texture));
	}
	ID3D11ResourcePtr ScratchImageImpl::CreateTextureEx(ID3D11DevicePtr pDevice, D3D11_USAGE usage, D3D11_BIND_FLAG bindFlags, D3D11_CPU_ACCESS_FLAG cpuAccessFlags, D3D11_RESOURCE_MISC_FLAG miscFlags, bool forceSRGB)
	{
		auto deviceRaw = static_cast<ID3D11Device*>(pDevice.ToPointer());

		ID3D11Resource* texture = nullptr;
		HRESULT hr = DirectX::CreateTextureEx(
			deviceRaw,
			GetImagesInternal(),
			GetImageCountInternal(),
			GetMetadataInternal(),
			static_cast<::D3D11_USAGE>(usage),
			static_cast<::D3D11_BIND_FLAG>(bindFlags),
			static_cast<::D3D11_CPU_ACCESS_FLAG>(cpuAccessFlags),
			static_cast<::D3D11_RESOURCE_MISC_FLAG>(miscFlags),
			forceSRGB,
			&texture);

		Marshal::ThrowExceptionForHR(hr);

		return ID3D11ResourcePtr(IntPtr(texture));
	}
	ID3D11ShaderResourceViewPtr ScratchImageImpl::CreateShaderResourceView(ID3D11DevicePtr pDevice)
	{
		auto deviceRaw = static_cast<ID3D11Device*>(pDevice.ToPointer());

		ID3D11ShaderResourceView* texture = nullptr;
		HRESULT hr = DirectX::CreateShaderResourceView(
			deviceRaw,
			GetImagesInternal(),
			GetImageCountInternal(),
			GetMetadataInternal(),
			&texture);

		Marshal::ThrowExceptionForHR(hr);

		return ID3D11ShaderResourceViewPtr(IntPtr(texture));
	}
	ID3D11ShaderResourceViewPtr ScratchImageImpl::CreateShaderResourceViewEx(ID3D11DevicePtr pDevice, D3D11_USAGE usage, D3D11_BIND_FLAG bindFlags, D3D11_CPU_ACCESS_FLAG cpuAccessFlags, D3D11_RESOURCE_MISC_FLAG miscFlags, bool forceSRGB)
	{
		auto deviceRaw = static_cast<ID3D11Device*>(pDevice.ToPointer());

		ID3D11ShaderResourceView* texture = nullptr;
		HRESULT hr = DirectX::CreateShaderResourceViewEx(
			deviceRaw,
			GetImagesInternal(),
			GetImageCountInternal(),
			GetMetadataInternal(),
			static_cast<::D3D11_USAGE>(usage),
			static_cast<::D3D11_BIND_FLAG>(bindFlags),
			static_cast<::D3D11_CPU_ACCESS_FLAG>(cpuAccessFlags),
			static_cast<::D3D11_RESOURCE_MISC_FLAG>(miscFlags),
			forceSRGB,
			&texture);

		Marshal::ThrowExceptionForHR(hr);

		return ID3D11ShaderResourceViewPtr(IntPtr(texture));
	}
	ScratchImage^ TexHelperImpl::CaptureTexture(ID3D11DevicePtr pDevice, ID3D11DeviceContextPtr pContext, ID3D11ResourcePtr pSource)
	{
		auto deviceRaw = static_cast<ID3D11Device*>(pDevice.ToPointer());
		auto contextRaw = static_cast<ID3D11DeviceContext*>(pContext.ToPointer());
		auto sourceRaw = static_cast<ID3D11Resource*>(pSource.ToPointer());

		auto result = gcnew ActualScratchImageImpl();
		try
		{
			HRESULT hr = DirectX::CaptureTexture(deviceRaw, contextRaw, sourceRaw, *result->scratchImage_);

			Marshal::ThrowExceptionForHR(hr);

			return result;
		}
		catch (Exception^)
		{
			delete result;
			throw;
		}
	}
}