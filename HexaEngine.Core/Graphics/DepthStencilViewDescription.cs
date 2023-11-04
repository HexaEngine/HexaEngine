namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents a description of a depth-stencil view.
    /// </summary>
    public struct DepthStencilViewDescription
    {
        /// <summary>
        /// Gets or sets the format of the depth-stencil view.
        /// </summary>
        public Format Format;

        /// <summary>
        /// Gets or sets the dimension of the depth-stencil view.
        /// </summary>
        public DepthStencilViewDimension ViewDimension;

        /// <summary>
        /// Gets or sets flags that determine how the depth-stencil view is created.
        /// </summary>
        public DepthStencilViewFlags Flags;

        /// <summary>
        /// Gets or sets the depth-stencil view for a 1D texture.
        /// </summary>
        public Texture1DDepthStencilView Texture1D;

        /// <summary>
        /// Gets or sets the depth-stencil view for an array of 1D textures.
        /// </summary>
        public Texture1DArrayDepthStencilView Texture1DArray;

        /// <summary>
        /// Gets or sets the depth-stencil view for a 2D texture.
        /// </summary>
        public Texture2DDepthStencilView Texture2D;

        /// <summary>
        /// Gets or sets the depth-stencil view for an array of 2D textures.
        /// </summary>
        public Texture2DArrayDepthStencilView Texture2DArray;

        /// <summary>
        /// Gets or sets the depth-stencil view for a multisampled 2D texture.
        /// </summary>
        public Texture2DMultisampledDepthStencilView Texture2DMS;

        /// <summary>
        /// Gets or sets the depth-stencil view for an array of multisampled 2D textures.
        /// </summary>
        public Texture2DMultisampledArrayDepthStencilView Texture2DMSArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilViewDescription"/> struct.
        /// </summary>
        /// <param name="viewDimension">The <see cref="DepthStencilViewDimension"/></param>
        /// <param name="format">The <see cref="Format"/> to use or <see cref="Format.Unknown"/>.</param>
        /// <param name="mipSlice">The index of the mipmap level to use mip slice.</param>
        /// <param name="firstArraySlice">The index of the first texture to use in an array of textures.</param>
        /// <param name="arraySize">Number of textures in the array.</param>
        /// <param name="flags"></param>
        public DepthStencilViewDescription(
            DepthStencilViewDimension viewDimension,
            Format format = Format.Unknown,
            int mipSlice = 0,
            int firstArraySlice = 0,
            int arraySize = -1,
            DepthStencilViewFlags flags = DepthStencilViewFlags.None) : this()
        {
            Format = format;
            ViewDimension = viewDimension;
            Flags = flags;
            switch (viewDimension)
            {
                case DepthStencilViewDimension.Texture1D:
                    Texture1D.MipSlice = mipSlice;
                    break;

                case DepthStencilViewDimension.Texture1DArray:
                    Texture1DArray.MipSlice = mipSlice;
                    Texture1DArray.FirstArraySlice = firstArraySlice;
                    Texture1DArray.ArraySize = arraySize;
                    break;

                case DepthStencilViewDimension.Texture2D:
                    Texture2D.MipSlice = mipSlice;
                    break;

                case DepthStencilViewDimension.Texture2DArray:
                    Texture2DArray.MipSlice = mipSlice;
                    Texture2DArray.FirstArraySlice = firstArraySlice;
                    Texture2DArray.ArraySize = arraySize;
                    break;

                case DepthStencilViewDimension.Texture2DMultisampled:
                    break;

                case DepthStencilViewDimension.Texture2DMultisampledArray:
                    Texture2DMSArray.FirstArraySlice = firstArraySlice;
                    Texture2DMSArray.ArraySize = arraySize;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilViewDescription"/> struct.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="isArray"></param>
        /// <param name="format"></param>
        /// <param name="mipSlice"></param>
        /// <param name="firstArraySlice"></param>
        /// <param name="arraySize"></param>
        /// <param name="flags"></param>
        public DepthStencilViewDescription(
            ITexture1D texture,
            bool isArray,
            Format format = Format.Unknown,
            int mipSlice = 0,
            int firstArraySlice = 0,
            int arraySize = -1,
            DepthStencilViewFlags flags = DepthStencilViewFlags.None) : this()
        {
            ViewDimension = isArray ? DepthStencilViewDimension.Texture1DArray : DepthStencilViewDimension.Texture1D;
            Flags = flags;
            if (format == Format.Unknown
                || (arraySize == -1 && DepthStencilViewDimension.Texture1DArray == ViewDimension))
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
                case DepthStencilViewDimension.Texture1D:
                    Texture1D.MipSlice = mipSlice;
                    break;

                case DepthStencilViewDimension.Texture1DArray:
                    Texture1DArray.MipSlice = mipSlice;
                    Texture1DArray.FirstArraySlice = firstArraySlice;
                    Texture1DArray.ArraySize = arraySize;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilViewDescription"/> struct.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="viewDimension"></param>
        /// <param name="format"></param>
        /// <param name="mipSlice"></param>
        /// <param name="firstArraySlice"></param>
        /// <param name="arraySize"></param>
        /// <param name="flags"></param>
        public DepthStencilViewDescription(
            ITexture2D texture,
            DepthStencilViewDimension viewDimension,
            Format format = Format.Unknown,
            int mipSlice = 0,
            int firstArraySlice = 0,
            int arraySize = -1,
            DepthStencilViewFlags flags = DepthStencilViewFlags.None) : this()
        {
            ViewDimension = viewDimension;
            Flags = flags;

            if (format == Format.Unknown
                || (-1 == arraySize && (DepthStencilViewDimension.Texture2DArray == viewDimension || DepthStencilViewDimension.Texture2DMultisampledArray == viewDimension)))
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
            switch (viewDimension)
            {
                case DepthStencilViewDimension.Texture2D:
                    Texture2D.MipSlice = mipSlice;
                    break;

                case DepthStencilViewDimension.Texture2DArray:
                    Texture2DArray.MipSlice = mipSlice;
                    Texture2DArray.FirstArraySlice = firstArraySlice;
                    Texture2DArray.ArraySize = arraySize;
                    break;

                case DepthStencilViewDimension.Texture2DMultisampled:
                    break;

                case DepthStencilViewDimension.Texture2DMultisampledArray:
                    Texture2DMSArray.FirstArraySlice = firstArraySlice;
                    Texture2DMSArray.ArraySize = arraySize;
                    break;

                default:
                    break;
            }
        }
    }
}