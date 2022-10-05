﻿using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace HexaEngine.DirectXTex
{
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
}