// DirectXTexNet.h

#pragma once

//#using "DirectXTexNet.dll" as_friend

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

// these two alias should match setting in DirectXTexNet.cs
using Size_t = Int32;
using Size_T = Int64;

// just some aliases for better readability
using IWICImagingFactoryPtr = IntPtr;
using ID3D11DevicePtr = IntPtr;
using ID3D11ResourcePtr = IntPtr;
using ID3D11ShaderResourceViewPtr = IntPtr;
using ID3D11DeviceContextPtr = IntPtr;

namespace DirectXTexNet
{
	static Image^ ToManaged(_In_ const DirectX::Image& native, Object^ parent)
	{
		return gcnew Image(
			static_cast<Size_t>(native.width),
			static_cast<Size_t>(native.height),
			static_cast<DXGI_FORMAT>(native.format),
			native.rowPitch,
			native.slicePitch,
			IntPtr(native.pixels),
			parent);
	}

	static void FromManaged(Image^ managed, _Out_ DirectX::Image& native)
	{
		native.width = static_cast<size_t>(managed->Width);
		native.height = static_cast<size_t>(managed->Height);
		native.format = static_cast<::DXGI_FORMAT>(managed->Format);
		native.rowPitch = static_cast<size_t>(managed->RowPitch);
		native.slicePitch = static_cast<size_t>(managed->SlicePitch);
		native.pixels = static_cast<uint8_t*>(managed->Pixels.ToPointer());
	}

	static TexMetadata^ ToManaged(_In_ const DirectX::TexMetadata& native)
	{
		return gcnew TexMetadata(
			static_cast<Size_t>(native.width),
			static_cast<Size_t>(native.height),
			static_cast<Size_t>(native.depth),
			static_cast<Size_t>(native.arraySize),
			static_cast<Size_t>(native.mipLevels),
			static_cast<TEX_MISC_FLAG>(native.miscFlags),
			static_cast<TEX_MISC_FLAG2>(native.miscFlags2),
			static_cast<DXGI_FORMAT>(native.format),
			static_cast<TEX_DIMENSION>(native.dimension));
	}

	static void FromManaged(TexMetadata^ managed, _Out_ DirectX::TexMetadata& native)
	{
		native.width = static_cast<size_t>(managed->Width);
		native.height = static_cast<size_t>(managed->Height);
		native.depth = static_cast<size_t>(managed->Depth);
		native.arraySize = static_cast<size_t>(managed->ArraySize);
		native.mipLevels = static_cast<size_t>(managed->MipLevels);
		native.miscFlags = static_cast<uint32_t>(managed->MiscFlags);
		native.miscFlags2 = static_cast<uint32_t>(managed->MiscFlags2);
		native.format = static_cast<::DXGI_FORMAT>(managed->Format);
		native.dimension = static_cast<DirectX::TEX_DIMENSION>(managed->Dimension);
	}

	typedef void(__cdecl* EvaluatePixelsFunctionDeclaration)(_In_reads_(width) const DirectX::XMVECTOR* pixels, size_t width, size_t y);

	typedef void(__cdecl* TransformPixelsFunctionDeclaration)(_Out_writes_(width) DirectX::XMVECTOR* outPixels, _In_reads_(width) const DirectX::XMVECTOR* inPixels, size_t width, size_t y);

	ref class BlobImpl : public UnmanagedMemoryStream
	{
	public:

		~BlobImpl()
		{
			this->!BlobImpl();
		}

	protected:
		!BlobImpl()
		{
			if (this->blob_ != nullptr)
			{
				delete blob_;
				blob_ = nullptr;
			}
		}

	internal:
		BlobImpl(DirectX::Blob* blob) : UnmanagedMemoryStream(static_cast<uint8_t*>(blob->GetBufferPointer()), blob->GetBufferSize())
		{
			blob_ = blob;
		}

		DirectX::Blob* blob_;
	};

	ref class ScratchImageImpl abstract : public ScratchImage
	{
	internal:
		virtual size_t GetImageCountInternal() = 0;
		virtual const DirectX::Image* GetImagesInternal() = 0;
		virtual const DirectX::TexMetadata& GetMetadataInternal() = 0;

		const DirectX::Image* GetImageInternal(size_t index)
		{
			if (index >= GetImageCountInternal())
				throw gcnew IndexOutOfRangeException("The image index is out of range.");
			return &GetImagesInternal()[index];
		}

		const DirectX::Image* GetImageInternal(size_t mip, size_t item, size_t slice)
		{
			size_t index = this->GetMetadataInternal().ComputeIndex(mip, item, slice);
			return GetImageInternal(index);
		}

	public:
		Size_t GetImageCount() sealed override
		{
			return static_cast<Size_t>(GetImageCountInternal());
		}

		Size_t ComputeImageIndex(Size_t mip, Size_t item, Size_t slice) sealed override
		{
			return static_cast<Size_t>(this->GetMetadataInternal().ComputeIndex(mip, item, slice));
		}

		Image^ GetImage(Size_t index) sealed override
		{
			return ToManaged(*GetImageInternal(index), this);
		}

		Image^ GetImage(Size_t mip, Size_t item, Size_t slice) sealed override
		{
			return ToManaged(*GetImageInternal(mip, item, slice), this);
		}

		TexMetadata^ GetMetadata() sealed override
		{
			return ToManaged(this->GetMetadataInternal());
		}

		// creating image copies
		ScratchImage^ CreateImageCopy(Size_t imageIndex, bool allow1D, CP_FLAGS flags) override;

		ScratchImage^ CreateArrayCopy(Size_t startIndex, Size_t nImages, bool allow1D, CP_FLAGS flags) override;

		ScratchImage^ CreateCubeCopy(Size_t startIndex, Size_t nImages, CP_FLAGS flags) override;

		ScratchImage^ CreateVolumeCopy(Size_t startIndex, Size_t depth, CP_FLAGS flags) override;

		// saving images to file/memory
		UnmanagedMemoryStream^ SaveToDDSMemory(Size_t imageIndex, DDS_FLAGS flags) override;

		UnmanagedMemoryStream^ SaveToDDSMemory(DDS_FLAGS flags) override;

		void SaveToDDSFile(Size_t imageIndex, DDS_FLAGS flags, String^ szFile) override;

		void SaveToDDSFile(DDS_FLAGS flags, String^ szFile) override;

		UnmanagedMemoryStream^ SaveToHDRMemory(Size_t imageIndex) override;

		void SaveToHDRFile(Size_t imageIndex, String^ szFile) override;

		UnmanagedMemoryStream^ SaveToTGAMemory(Size_t imageIndex) override;

		void SaveToTGAFile(Size_t imageIndex, String^ szFile) override;

		UnmanagedMemoryStream^ SaveToWICMemory(Size_t imageIndex, WIC_FLAGS flags, Guid guidContainerFormat) override;

		UnmanagedMemoryStream^ SaveToWICMemory(Size_t startIndex, Size_t nImages, WIC_FLAGS flags, Guid guidContainerFormat) override;

		void SaveToWICFile(Size_t imageIndex, WIC_FLAGS flags, Guid guidContainerFormat, String^ szFile) override;

		void SaveToWICFile(Size_t startIndex, Size_t nImages, WIC_FLAGS flags, Guid guidContainerFormat, String^ szFile) override;

		UnmanagedMemoryStream^ SaveToJPGMemory(Size_t imageIndex, float quality) override;

		void SaveToJPGFile(Size_t imageIndex, float quality, String^ szFile) override;

		// Texture conversion, resizing, mipmap generation, and block compression
		ScratchImage^ FlipRotate(Size_t imageIndex, TEX_FR_FLAGS flags) override;

		ScratchImage^ FlipRotate(TEX_FR_FLAGS flags) override;

		ScratchImage^ Resize(Size_t imageIndex, Size_t width, Size_t height, TEX_FILTER_FLAGS filter) override;

		ScratchImage^ Resize(
			Size_t width,
			Size_t height,
			TEX_FILTER_FLAGS filter) override;

		ScratchImage^ Convert(Size_t imageIndex, DXGI_FORMAT format, TEX_FILTER_FLAGS filter, float threshold) override;

		ScratchImage^ Convert(
			DXGI_FORMAT format,
			TEX_FILTER_FLAGS filter,
			float threshold) override;

		ScratchImage^ ConvertToSinglePlane(Size_t imageIndex) override;

		ScratchImage^ ConvertToSinglePlane() override;

		ScratchImage^ CreateCopyWithEmptyMipMaps(Size_t levels, DXGI_FORMAT fmt, CP_FLAGS flags, bool zeroOutMipMaps) override;

		ScratchImage^ GenerateMipMaps(Size_t imageIndex, TEX_FILTER_FLAGS filter, Size_t levels, bool allow1D) override;

		ScratchImage^ GenerateMipMaps(
			TEX_FILTER_FLAGS filter,
			Size_t levels) override;

		ScratchImage^ GenerateMipMaps3D(
			Size_t startIndex,
			Size_t depth,
			TEX_FILTER_FLAGS filter,
			Size_t levels) override;

		ScratchImage^ GenerateMipMaps3D(
			TEX_FILTER_FLAGS filter,
			Size_t levels) override;

		ScratchImage^ PremultiplyAlpha(Size_t imageIndex, TEX_PMALPHA_FLAGS flags) override;

		ScratchImage^ PremultiplyAlpha(TEX_PMALPHA_FLAGS flags) override;

		ScratchImage^ Compress(Size_t imageIndex, DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float threshold) override;

		ScratchImage^ Compress(
			DXGI_FORMAT format,
			TEX_COMPRESS_FLAGS compress,
			float threshold) override;

		ScratchImage^ Compress(Size_t imageIndex, ID3D11DevicePtr pDevice, DXGI_FORMAT format, TEX_COMPRESS_FLAGS compress, float alphaWeight) override;

		ScratchImage^ Compress(
			ID3D11DevicePtr pDevice,
			DXGI_FORMAT format,
			TEX_COMPRESS_FLAGS compress,
			float alphaWeight) override;

		ScratchImage^ Decompress(Size_t imageIndex, DXGI_FORMAT format) override;

		ScratchImage^ Decompress(DXGI_FORMAT format) override;

		// Normal map operations
		ScratchImage^ ComputeNormalMap(Size_t imageIndex, CNMAP_FLAGS flags, float amplitude, DXGI_FORMAT format) override;

		ScratchImage^ ComputeNormalMap(
			CNMAP_FLAGS flags,
			float amplitude,
			DXGI_FORMAT format) override;

		// Misc image operations
		void EvaluateImage(Size_t imageIndex, EvaluatePixelsDelegate^ pixelFunc) override;

		void EvaluateImage(EvaluatePixelsDelegate^ pixelFunc) override;

		ScratchImage^ TransformImage(Size_t imageIndex, TransformPixelsDelegate^ pixelFunc) override;

		ScratchImage^ TransformImage(TransformPixelsDelegate^ pixelFunc) override;

		// Direct3D 11 functions
		ID3D11ResourcePtr CreateTexture(ID3D11DevicePtr pDevice) override;

		ID3D11ShaderResourceViewPtr CreateShaderResourceView(ID3D11DevicePtr pDevice) override;

		ID3D11ResourcePtr CreateTextureEx(
			ID3D11DevicePtr pDevice,
			D3D11_USAGE usage,
			D3D11_BIND_FLAG bindFlags,
			D3D11_CPU_ACCESS_FLAG cpuAccessFlags,
			D3D11_RESOURCE_MISC_FLAG miscFlags,
			bool forceSRGB) override;

		ID3D11ShaderResourceViewPtr CreateShaderResourceViewEx(
			ID3D11DevicePtr pDevice,
			D3D11_USAGE usage,
			D3D11_BIND_FLAG bindFlags,
			D3D11_CPU_ACCESS_FLAG cpuAccessFlags,
			D3D11_RESOURCE_MISC_FLAG miscFlags,
			bool forceSRGB) override;
	};

	ref class TempScratchImageImpl : ScratchImageImpl
	{
	public:
		property bool IsDisposed
		{
			bool get() override
			{
				return m_image == nullptr;
			}
		}

		bool OverrideFormat(DXGI_FORMAT newFormat) override
		{
			auto f = static_cast<::DXGI_FORMAT>(newFormat);

			if (!m_image)
				return false;

			if (!DirectX::IsValid(f) || DirectX::IsPlanar(f) || DirectX::IsPalettized(f))
				return false;

			for (size_t index = 0; index < m_nimages; ++index)
			{
				m_image[index].format = f;
			}

			m_metadata->format = f;

			return true;
		}

		bool OwnsData() override
		{
			return false;
		}

		IntPtr GetPixels() override
		{
			return IntPtr();
		}

		Size_T GetPixelsSize() override
		{
			return Size_T(-1);
		}

		bool IsAlphaAllOpaque() override
		{
			throw gcnew NotSupportedException("Not supported by temporary ScratchImage.");
		}

		~TempScratchImageImpl()
		{
			if (otherDisposables != nullptr)
			{
				for (int i = 0; i < otherDisposables->Length; i++)
				{
					delete otherDisposables[i];
				}
				otherDisposables = nullptr;
			}
			this->!TempScratchImageImpl();
		}

	protected:
		!TempScratchImageImpl()
		{
			otherDisposables = nullptr;
			origImages = nullptr;
			m_nimages = 0;
			if (this->m_metadata != nullptr)
			{
				delete m_metadata;
				m_metadata = nullptr;
			}
			if (this->m_image != nullptr)
			{
				delete[] m_image;
				m_image = nullptr;
			}
		}

	internal:
		size_t GetImageCountInternal() override
		{
			return m_nimages;
		}

		const DirectX::Image* GetImagesInternal() override
		{
			return m_image;
		}

		const DirectX::TexMetadata& GetMetadataInternal() override
		{
			return *m_metadata;
		}

		TempScratchImageImpl(array<Image^>^ _images, TexMetadata^ _metadata, array<IDisposable^>^ takeOwnershipOf)
		{
			m_metadata = new DirectX::TexMetadata;
			FromManaged(_metadata, *m_metadata);

			int length = _images->Length;
			m_nimages = static_cast<size_t>(length);
			m_image = new DirectX::Image[length];

			origImages = gcnew array<Image^>(length);

			for (int i = 0; i < length; i++)
			{
				Image^ origImage = _images[i];
				origImages[i] = origImage;
				FromManaged(origImage, m_image[i]);
			}

			if (takeOwnershipOf != nullptr)
			{
				otherDisposables = gcnew array<IDisposable^>(takeOwnershipOf->Length);
				for (int i = 0; i < otherDisposables->Length; i++)
				{
					otherDisposables[i] = takeOwnershipOf[i];
				}
			}
		}

	private:
		array<Image^>^ origImages;
		array<IDisposable^>^ otherDisposables;

		size_t                m_nimages;
		DirectX::TexMetadata* m_metadata;
		DirectX::Image* m_image;
	};

	ref class ActualScratchImageImpl : ScratchImageImpl
	{
	public:
		property bool IsDisposed
		{
			bool get() override
			{
				return scratchImage_ == nullptr;
			}
		}

		bool OverrideFormat(DXGI_FORMAT f) override
		{
			return scratchImage_->OverrideFormat(static_cast<::DXGI_FORMAT>(f));
		}

		bool OwnsData() override
		{
			return false;
		}

		IntPtr GetPixels() override
		{
			return IntPtr(scratchImage_->GetPixels());
		}

		Size_T GetPixelsSize() override
		{
			return scratchImage_->GetPixelsSize();
		}

		bool IsAlphaAllOpaque() override
		{
			return scratchImage_->IsAlphaAllOpaque();
		}

		~ActualScratchImageImpl()
		{
			this->!ActualScratchImageImpl();
		}

	protected:
		!ActualScratchImageImpl()
		{
			if (this->scratchImage_ != nullptr)
			{
				delete scratchImage_;
				scratchImage_ = nullptr;
			}
		}

	internal:
		size_t GetImageCountInternal() override
		{
			return scratchImage_->GetImageCount();
		}

		const DirectX::Image* GetImagesInternal() override
		{
			return scratchImage_->GetImages();
		}

		const DirectX::TexMetadata& GetMetadataInternal() override
		{
			return scratchImage_->GetMetadata();
		}

		ActualScratchImageImpl()
		{
			scratchImage_ = new DirectX::ScratchImage();
		}

		DirectX::ScratchImage* scratchImage_;
	};

	public ref class TexHelperImpl : public TexHelper
	{
	public:

#ifdef _OPENMP
		void SetOmpMaxThreadCount(int maxThreadCount) override;
#endif

		// DXGI Format Utilities
		bool  IsValid(DXGI_FORMAT fmt) override;

		bool IsCompressed(DXGI_FORMAT fmt)override;

		bool  IsPacked(DXGI_FORMAT fmt) override;

		bool IsVideo(DXGI_FORMAT fmt) override;

		bool  IsPlanar(DXGI_FORMAT fmt)override;

		bool  IsPalettized(DXGI_FORMAT fmt) override;

		bool  IsDepthStencil(DXGI_FORMAT fmt)override;

		bool  IsSRGB(DXGI_FORMAT fmt) override;

		bool  IsTypeless(DXGI_FORMAT fmt, bool partialTypeless) override;

		bool HasAlpha(DXGI_FORMAT fmt) override;

		Size_t BitsPerPixel(DXGI_FORMAT fmt) override;

		Size_t BitsPerColor(DXGI_FORMAT fmt) override;

		void ComputePitch(DXGI_FORMAT fmt, Size_t width, Size_t height,
			[Out] Size_T% rowPitch, [Out] Size_T% slicePitch, CP_FLAGS flags) override;

		Size_T ComputeScanlines(DXGI_FORMAT fmt, Size_t height) override;

		Size_t ComputeImageIndex(TexMetadata^ metadata, Size_t mip, Size_t item, Size_t slice) override;

		DXGI_FORMAT MakeSRGB(DXGI_FORMAT fmt) override;

		DXGI_FORMAT MakeTypeless(DXGI_FORMAT fmt) override;

		DXGI_FORMAT MakeTypelessUNORM(DXGI_FORMAT fmt) override;

		DXGI_FORMAT MakeTypelessFLOAT(DXGI_FORMAT fmt) override;

		// Get Texture metadata
		TexMetadata^ GetMetadataFromDDSMemory(IntPtr pSource, Size_T size, DDS_FLAGS flags) override;

		TexMetadata^ GetMetadataFromDDSFile(String^ szFile, DDS_FLAGS flags) override;

		TexMetadata^ GetMetadataFromHDRMemory(IntPtr pSource, Size_T size) override;

		TexMetadata^ GetMetadataFromHDRFile(String^ szFile) override;

		TexMetadata^ GetMetadataFromTGAMemory(IntPtr pSource, Size_T size) override;

		TexMetadata^ GetMetadataFromTGAFile(String^ szFile) override;

		TexMetadata^ GetMetadataFromWICMemory(IntPtr pSource, Size_T size, WIC_FLAGS flags) override;

		TexMetadata^ GetMetadataFromWICFile(String^ szFile, WIC_FLAGS flags) override;

		// create new ScratchImages
		ScratchImage^ Initialize(TexMetadata^ _metadata, CP_FLAGS flags) override;

		ScratchImage^ Initialize1D(DXGI_FORMAT fmt, Size_t length, Size_t arraySize, Size_t mipLevels, CP_FLAGS flags) override;

		ScratchImage^ Initialize2D(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_t arraySize, Size_t mipLevels, CP_FLAGS flags) override;

		ScratchImage^ Initialize3D(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_t depth, Size_t mipLevels, CP_FLAGS flags) override;

		ScratchImage^ InitializeCube(DXGI_FORMAT fmt, Size_t width, Size_t height, Size_t nCubes, Size_t mipLevels, CP_FLAGS flags) override;

		ScratchImage^ InitializeTemporary(array<Image^>^ images, TexMetadata^ metadata, ... array<IDisposable^>^ takeOwnershipOf) override;

		// Load Images
		ScratchImage^ LoadFromDDSMemory(IntPtr pSource, Size_T size, DDS_FLAGS flags) override;

		ScratchImage^ LoadFromDDSFile(String^ szFile, DDS_FLAGS flags) override;

		ScratchImage^ LoadFromHDRMemory(IntPtr pSource, Size_T size) override;

		ScratchImage^ LoadFromHDRFile(String^ szFile) override;

		ScratchImage^ LoadFromTGAMemory(IntPtr pSource, Size_T size) override;

		ScratchImage^ LoadFromTGAFile(String^ filename) override;

		ScratchImage^ LoadFromWICMemory(IntPtr pSource, Size_T size, WIC_FLAGS flags) override;

		ScratchImage^ LoadFromWICFile(String^ filename, WIC_FLAGS flags) override;

		// Misc image operations
		void CopyRectangle(
			Image^ srcImage,
			Size_t srcX,
			Size_t srcY,
			Size_t srcWidth,
			Size_t srcHeight,
			Image^ dstImage,
			TEX_FILTER_FLAGS filter,
			Size_t xOffset,
			Size_t yOffset) override;

		void ComputeMSE(Image^ image, Image^ image2, [Out] float% mse, [Out] MseV% mseV, CMSE_FLAGS flags) override;

		// WIC utility
		Guid GetWICCodec(WICCodecs codec) override;

		IWICImagingFactoryPtr GetWICFactory(bool iswic2) override;

		void SetWICFactory(IWICImagingFactoryPtr pWIC) override;

		// Direct3D 11 functions
		bool IsSupportedTexture(ID3D11DevicePtr pDevice, TexMetadata^ _metadata) override;

		ScratchImage^ CaptureTexture(ID3D11DevicePtr pDevice, ID3D11DeviceContextPtr pContext, ID3D11ResourcePtr pSource) override;
	};
}
