namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a render target view.
    /// </summary>
    public struct RenderTargetViewDescription : IEquatable<RenderTargetViewDescription>
    {
        /// <summary>
        /// The data format of the resource.
        /// </summary>
        public Format Format;

        /// <summary>
        /// The dimension of the render target view.
        /// </summary>
        public RenderTargetViewDimension ViewDimension;

        /// <summary>
        /// Render target view for a buffer.
        /// </summary>
        public BufferRenderTargetView Buffer;

        /// <summary>
        /// Render target view for a 1D texture.
        /// </summary>
        public Texture1DRenderTargetView Texture1D;

        /// <summary>
        /// Render target view for a 1D texture array.
        /// </summary>
        public Texture1DArrayRenderTargetView Texture1DArray;

        /// <summary>
        /// Render target view for a 2D texture.
        /// </summary>
        public Texture2DRenderTargetView Texture2D;

        /// <summary>
        /// Render target view for a 2D texture array.
        /// </summary>
        public Texture2DArrayRenderTargetView Texture2DArray;

        /// <summary>
        /// Render target view for a 2D multisampled texture.
        /// </summary>
        public Texture2DMultisampledRenderTargetView Texture2DMS;

        /// <summary>
        /// Render target view for a 2D multisampled texture array.
        /// </summary>
        public Texture2DMultisampledArrayRenderTargetView Texture2DMSArray;

        /// <summary>
        /// Render target view for a 3D texture.
        /// </summary>
        public Texture3DRenderTargetView Texture3D;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewDescription"/> struct.
        /// </summary>
        /// <param name="viewDimension">The <see cref="RenderTargetViewDimension"/></param>
        /// <param name="format">The <see cref="Format"/> to use or <see cref="Format.Unknown"/>.</param>
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
        /// <param name="buffer">Unused <see cref="IBuffer"/> </param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewDescription"/> struct.
        /// </summary>
        /// <param name="texture">The texture resource.</param>
        /// <param name="isArray">Indicates whether the view is an array.</param>
        /// <param name="format">The data format of the resource. Use <see cref="Format.Unknown"/> to infer from the texture.</param>
        /// <param name="mipSlice">The index of the mip level to use. Default is 0.</param>
        /// <param name="firstArraySlice">The index of the first texture to use in an array. Default is 0.</param>
        /// <param name="arraySize">Number of textures in the array. Default is -1, which infers from the texture.</param>
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

        public override readonly bool Equals(object? obj)
        {
            return obj is RenderTargetViewDescription description && Equals(description);
        }

        public readonly bool Equals(RenderTargetViewDescription other)
        {
            return Format == other.Format &&
                   ViewDimension == other.ViewDimension &&
                   Buffer.Equals(other.Buffer) &&
                   EqualityComparer<Texture1DRenderTargetView>.Default.Equals(Texture1D, other.Texture1D) &&
                   EqualityComparer<Texture1DArrayRenderTargetView>.Default.Equals(Texture1DArray, other.Texture1DArray) &&
                   EqualityComparer<Texture2DRenderTargetView>.Default.Equals(Texture2D, other.Texture2D) &&
                   EqualityComparer<Texture2DArrayRenderTargetView>.Default.Equals(Texture2DArray, other.Texture2DArray) &&
                   EqualityComparer<Texture2DMultisampledRenderTargetView>.Default.Equals(Texture2DMS, other.Texture2DMS) &&
                   EqualityComparer<Texture2DMultisampledArrayRenderTargetView>.Default.Equals(Texture2DMSArray, other.Texture2DMSArray) &&
                   EqualityComparer<Texture3DRenderTargetView>.Default.Equals(Texture3D, other.Texture3D);
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Format);
            hash.Add(ViewDimension);
            hash.Add(Buffer);
            hash.Add(Texture1D);
            hash.Add(Texture1DArray);
            hash.Add(Texture2D);
            hash.Add(Texture2DArray);
            hash.Add(Texture2DMS);
            hash.Add(Texture2DMSArray);
            hash.Add(Texture3D);
            return hash.ToHashCode();
        }

        public static bool operator ==(RenderTargetViewDescription left, RenderTargetViewDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RenderTargetViewDescription left, RenderTargetViewDescription right)
        {
            return !(left == right);
        }
    }
}