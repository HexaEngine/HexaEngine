﻿namespace HexaEngine.Core.Graphics
{
    public struct RenderTargetViewDescription
    {
        public Format Format;
        public RenderTargetViewDimension ViewDimension;
        public BufferRenderTargetView Buffer;
        public Texture1DRenderTargetView Texture1D;
        public Texture1DArrayRenderTargetView Texture1DArray;
        public Texture2DRenderTargetView Texture2D;
        public Texture2DArrayRenderTargetView Texture2DArray;
        public Texture2DMultisampledRenderTargetView Texture2DMS;
        public Texture2DMultisampledArrayRenderTargetView Texture2DMSArray;
        public Texture3DRenderTargetView Texture3D;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewDescription"/> struct.
        /// </summary>
        /// <param name="viewDimension">The <see cref="RenderTargetViewDimension"/></param>
        /// <param name="format">The <see cref="DXGI.Format"/> to use or <see cref="Format.Unknown"/>.</param>
        /// <param name="mipSlice">The index of the mipmap level to use mip slice. or first element for <see cref="RenderTargetViewDimension.Buffer"/>.</param>
        /// <param name="firstArraySlice">The index of the first texture to use in an array of textures or NumElements for <see cref="RenderTargetViewDimension.Buffer"/>, FirstWSlice for <see cref="RenderTargetViewDimension.Texture3D"/>.</param>
        /// <param name="arraySize">Number of textures in the array or WSize for <see cref="RenderTargetViewDimension.Texture3D"/>. </param>
        public RenderTargetViewDescription(
            RenderTargetViewDimension viewDimension,
            Format format = Format.Unknown,
            int mipSlice = 0,
            int firstArraySlice = 0,
            int arraySize = -1) : this()
        {
            Format = format;
            ViewDimension = viewDimension;
            switch (viewDimension)
            {
                case RenderTargetViewDimension.Buffer:
                    Buffer.FirstElement = mipSlice;
                    Buffer.NumElements = firstArraySlice;
                    break;

                case RenderTargetViewDimension.Texture1D:
                    Texture1D.MipSlice = mipSlice;
                    break;

                case RenderTargetViewDimension.Texture1DArray:
                    Texture1DArray.MipSlice = mipSlice;
                    Texture1DArray.FirstArraySlice = firstArraySlice;
                    Texture1DArray.ArraySize = arraySize;
                    break;

                case RenderTargetViewDimension.Texture2D:
                    Texture2D.MipSlice = mipSlice;
                    break;

                case RenderTargetViewDimension.Texture2DArray:
                    Texture2DArray.MipSlice = mipSlice;
                    Texture2DArray.FirstArraySlice = firstArraySlice;
                    Texture2DArray.ArraySize = arraySize;
                    break;

                case RenderTargetViewDimension.Texture2DMultisampled:
                    break;

                case RenderTargetViewDimension.Texture2DMultisampledArray:
                    Texture2DMSArray.FirstArraySlice = firstArraySlice;
                    Texture2DMSArray.ArraySize = arraySize;
                    break;

                case RenderTargetViewDimension.Texture3D:
                    Texture3D.MipSlice = mipSlice;
                    Texture3D.FirstWSlice = firstArraySlice;
                    Texture3D.WSize = arraySize;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewDescription"/> struct.
        /// </summary>
        /// <param name="buffer">Unused <see cref="ID3D11Buffer"/> </param>
        /// <param name="format"></param>
        /// <param name="firstElement"></param>
        /// <param name="numElements"></param>
        public RenderTargetViewDescription(IBuffer buffer, Format format, int firstElement, int numElements)
            : this()
        {
            Format = format;
            ViewDimension = RenderTargetViewDimension.Buffer;
            Buffer.FirstElement = firstElement;
            Buffer.NumElements = numElements;
        }

        public RenderTargetViewDescription(
            ITexture1D texture,
            bool isArray,
            Format format = Format.Unknown,
            int mipSlice = 0,
            int firstArraySlice = 0,
            int arraySize = -1)
            : this()
        {
            ViewDimension = isArray ? RenderTargetViewDimension.Texture1DArray : RenderTargetViewDimension.Texture1D;
            if (format == Format.Unknown
                || (arraySize == -1 && RenderTargetViewDimension.Texture1DArray == ViewDimension))
            {
                var textureDesc = texture.Description;
                if (format == Format.Unknown)
                {
                    format = textureDesc.Format;
                }

                if (arraySize == -1)
                {
                    arraySize = textureDesc.ArraySize - firstArraySlice;
                }
            }

            Format = format;
            switch (ViewDimension)
            {
                case RenderTargetViewDimension.Texture1D:
                    Texture1D.MipSlice = mipSlice;
                    break;

                case RenderTargetViewDimension.Texture1DArray:
                    Texture1DArray.MipSlice = mipSlice;
                    Texture1DArray.FirstArraySlice = firstArraySlice;
                    Texture1DArray.ArraySize = arraySize;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewDescription"/> struct.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="viewDimension"></param>
        /// <param name="format"></param>
        /// <param name="mipSlice"></param>
        /// <param name="firstArraySlice"></param>
        /// <param name="arraySize"></param>
        public RenderTargetViewDescription(
            ITexture2D texture,
            RenderTargetViewDimension viewDimension,
            Format format = Format.Unknown,
            int mipSlice = 0,
            int firstArraySlice = 0,
            int arraySize = -1)
            : this()
        {
            ViewDimension = viewDimension;
            if (format == Format.Unknown
                || (-1 == arraySize && (RenderTargetViewDimension.Texture2DArray == viewDimension || RenderTargetViewDimension.Texture2DMultisampledArray == viewDimension)))
            {
                var textureDesc = texture.Description;
                if (format == Format.Unknown)
                {
                    format = textureDesc.Format;
                }

                if (-1 == arraySize)
                {
                    arraySize = textureDesc.ArraySize - firstArraySlice;
                }
            }
            Format = format;
            switch (viewDimension)
            {
                case RenderTargetViewDimension.Texture2D:
                    Texture2D.MipSlice = mipSlice;
                    break;

                case RenderTargetViewDimension.Texture2DArray:
                    Texture2DArray.MipSlice = mipSlice;
                    Texture2DArray.FirstArraySlice = firstArraySlice;
                    Texture2DArray.ArraySize = arraySize;
                    break;

                case RenderTargetViewDimension.Texture2DMultisampled:
                    break;

                case RenderTargetViewDimension.Texture2DMultisampledArray:
                    Texture2DMSArray.FirstArraySlice = firstArraySlice;
                    Texture2DMSArray.ArraySize = arraySize;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewDescription"/> struct.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="format"></param>
        /// <param name="mipSlice"></param>
        /// <param name="firstWSlice"></param>
        /// <param name="wSize"></param>
        public RenderTargetViewDescription(
            ITexture3D texture,
            Format format = Format.Unknown,
            int mipSlice = 0,
            int firstWSlice = 0,
            int wSize = -1)
            : this()
        {
            ViewDimension = RenderTargetViewDimension.Texture3D;
            if (format == Format.Unknown || wSize == -1)
            {
                var textureDesc = texture.Description;
                if (format == Format.Unknown)
                {
                    format = textureDesc.Format;
                }

                if (wSize == -1)
                {
                    wSize = textureDesc.Depth - firstWSlice;
                }
            }

            Format = format;
            Texture3D.MipSlice = mipSlice;
            Texture3D.FirstWSlice = firstWSlice;
            Texture3D.WSize = wSize;
        }
    }
}