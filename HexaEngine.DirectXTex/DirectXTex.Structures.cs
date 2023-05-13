using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexaEngine.DirectXTex
{
    public unsafe struct TexBlob
    {
        public readonly void* pBlob;

        public TexBlob()
        {
            pBlob = Native.NewBlob();
        }

        public TexBlob(void* blob)
        {
            pBlob = blob;
        }

        public void Initialize(ulong size)
        {
            Native.BlobInitialize(pBlob, size).ThrowIf();
        }

        public void Release()
        {
            fixed (void** p = &pBlob)
            {
                Native.BlobRelease(p);
            }
        }

        public void* GetBufferPointer()
        {
            return Native.BlobGetBufferPointer(pBlob);
        }

        public ulong GetBufferSize()
        {
            return Native.BlobGetBufferSize(pBlob);
        }

        public void Resize(ulong size)
        {
            Native.BlobResize(pBlob, size).ThrowIf();
        }

        public void Trim(ulong size)
        {
            Native.BlobTrim(pBlob, size).ThrowIf();
        }

        public Span<byte> ToBytes()
        {
            return new Span<byte>(GetBufferPointer(), (int)GetBufferSize());
        }
    }

    public unsafe struct Image
    {
        public ulong Width;
        public ulong Height;
        public Format Format;
        public ulong RowPitch;
        public ulong SlicePitch;
        public byte* Pixels;
    }

    public struct IPropertyBag2
    {
    }

    public struct IWICMetadataQueryReader
    {
    }

    public struct Rect
    {
        public ulong X;
        public ulong Y;
        public ulong W;
        public ulong H;

        public Rect(ulong x, ulong y, ulong w, ulong h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
    }

    public unsafe struct ScratchImage
    {
        public void* pScratchImage;

        public ScratchImage(in void* ptr)
        {
            pScratchImage = ptr;
        }

        public ScratchImage()
        {
            pScratchImage = Native.NewScratchImage();
        }

        public void Initialize(TexMetadata metadata, CPFlags flags)
        {
            Native.Initialize(pScratchImage, &metadata, flags).ThrowIf();
        }

        public void Initialize1D(Format fmt, ulong length, ulong arraySize, ulong mipLevels, CPFlags flags = CPFlags.None)
        {
            Native.Initialize1D(pScratchImage, fmt, length, arraySize, mipLevels, flags).ThrowIf();
        }

        public void Initialize2D(Format fmt, ulong width, ulong height, ulong arraySize, ulong mipLevels, CPFlags flags = CPFlags.None)
        {
            Native.Initialize2D(pScratchImage, fmt, width, height, arraySize, mipLevels, flags).ThrowIf();
        }

        public void Initialize3D(Format fmt, ulong width, ulong height, ulong depth, ulong mipLevels, CPFlags flags = CPFlags.None)
        {
            Native.Initialize3D(pScratchImage, fmt, width, height, depth, mipLevels, flags).ThrowIf();
        }

        public void InitializeCube(Format fmt, ulong width, ulong height, ulong nCubes, ulong mipLevels, CPFlags flags = CPFlags.None)
        {
            Native.InitializeCube(pScratchImage, fmt, width, height, nCubes, mipLevels, flags).ThrowIf();
        }

        public void InitializeFromImage(Image image, bool allow1D = false, CPFlags flags = CPFlags.None)
        {
            Native.InitializeFromImage(pScratchImage, &image, allow1D, flags).ThrowIf();
        }

        public void InitializeArrayFromImages(Image* images, ulong nImages, bool allow1D = false, CPFlags flags = CPFlags.None)
        {
            Native.InitializeArrayFromImages(pScratchImage, images, nImages, allow1D, flags).ThrowIf();
        }

        public void InitializeCubeFromImages(Image* images, ulong nImages, CPFlags flags = CPFlags.None)
        {
            Native.InitializeCubeFromImages(pScratchImage, images, nImages, flags).ThrowIf();
        }

        public void Initialize3DFromImages(Image* images, ulong depth, CPFlags flags = CPFlags.None)
        {
            Native.Initialize3DFromImages(pScratchImage, images, depth, flags).ThrowIf();
        }

        public void Release()
        {
            fixed (void** ptr = &pScratchImage)
            {
                Native.ScratchImageRelease(ptr);
            }
        }

        public bool OverrideFormat(Format f)
        {
            return Native.OverrideFormat(pScratchImage, f) != 0;
        }

        public TexMetadata GetMetadata()
        {
            return *Native.GetMetadata(pScratchImage);
        }

        public Image* GetImage(ulong mip, ulong item, ulong slice)
        {
            return Native.GetImage(pScratchImage, mip, item, slice);
        }

        public Image* GetImages()
        {
            return Native.GetImages(pScratchImage);
        }

        public ulong GetImageCount()
        {
            return Native.GetImageCount(pScratchImage);
        }

        public byte* GetPixels()
        {
            return Native.GetPixels(pScratchImage);
        }

        public ulong GetPixelsSize()
        {
            return Native.GetPixelsSize(pScratchImage);
        }

        public bool IsAlphaAllOpaque()
        {
            return Native.IsAlphaAllOpaque(pScratchImage) != 0;
        }
    }

    public unsafe struct TexMetadata
    {
        public ulong Width;

        /// <summary>
        /// Should be 1 for 1D textures
        /// </summary>
        public ulong Height;

        /// <summary>
        /// Should be 1 for 1D or 2D textures
        /// </summary>
        public ulong Depth;

        /// <summary>
        /// For cubemap, this is a multiple of 6
        /// </summary>
        public ulong ArraySize;

        public ulong MipLevels;
        public TexMiscFlags MiscFlags;
        public TexMiscFlags2 MiscFlags2;
        public Format Format;
        public TexDimension Dimension;

        public ulong ComputeIndex(ulong mip, ulong item, ulong slice)
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.ComputeIndex(@this, mip, item, slice);
            }
        }

        public bool IsCubemap()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.IsCubemap(@this);
            }
        }

        public bool IsPMAlpha()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.IsPMAlpha(@this);
            }
        }

        public void SetAlphaMode(TexAlphaMode mode)
        {
            fixed (TexMetadata* @this = &this)
            {
                Native.SetAlphaMode(@this, mode);
            }
        }

        public TexAlphaMode GetAlphaMode()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.GetAlphaMode(@this);
            }
        }

        public bool IsVolumemap()
        {
            fixed (TexMetadata* @this = &this)
            {
                return Native.IsVolumemap(@this);
            }
        }
    }
}