namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Represents the description of a shader resource view.
    /// </summary>
    public struct ShaderResourceViewDescription
    {
        /// <summary>
        /// The format of the resource.
        /// </summary>
        public Format Format;

        /// <summary>
        /// The dimension of the view.
        /// </summary>
        public ShaderResourceViewDimension ViewDimension;

        /// <summary>
        /// Buffer shader resource view.
        /// </summary>
        public BufferShaderResourceView Buffer;

        /// <summary>
        /// 1D texture shader resource view.
        /// </summary>
        public Texture1DShaderResourceView Texture1D;

        /// <summary>
        /// 1D texture array shader resource view.
        /// </summary>
        public Texture1DArrayShaderResourceView Texture1DArray;

        /// <summary>
        /// 2D texture shader resource view.
        /// </summary>
        public Texture2DShaderResourceView Texture2D;

        /// <summary>
        /// 2D texture array shader resource view.
        /// </summary>
        public Texture2DArrayShaderResourceView Texture2DArray;

        /// <summary>
        /// 2D multisampled texture shader resource view.
        /// </summary>
        public Texture2DMultisampledShaderResourceView Texture2DMS;

        /// <summary>
        /// 2D multisampled texture array shader resource view.
        /// </summary>
        public Texture2DMultisampledArrayShaderResourceView Texture2DMSArray;

        /// <summary>
        /// 3D texture shader resource view.
        /// </summary>
        public Texture3DShaderResourceView Texture3D;

        /// <summary>
        /// Cube texture shader resource view.
        /// </summary>
        public TextureCubeShaderResourceView TextureCube;

        /// <summary>
        /// Cube texture array shader resource view.
        /// </summary>
        public TextureCubeArrayShaderResourceView TextureCubeArray;

        /// <summary>
        /// Extended buffer shader resource view.
        /// </summary>
        public BufferExtendedShaderResourceView BufferEx;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewDescription"/> struct.
        /// </summary>
        /// <param name="viewDimension">The <see cref="ShaderResourceViewDimension"/></param>
        /// <param name="format">The <see cref="Format"/> to use or <see cref="Format.Unknown"/>.</param>
        /// <param name="mostDetailedMip">Index of the most detailed mipmap level to use or first element for <see cref="ShaderResourceViewDimension.Buffer"/> or <see cref="ShaderResourceViewDimension.BufferExtended"/>.</param>
        /// <param name="mipLevels">The maximum number of mipmap levels for the view of the texture or num elements for <see cref="ShaderResourceViewDimension.Buffer"/> or <see cref="ShaderResourceViewDimension.BufferExtended"/>.</param>
        /// <param name="firstArraySlice">The index of the first texture to use in an array of textures or First2DArrayFace for <see cref="ShaderResourceViewDimension.TextureCubeArray"/>. </param>
        /// <param name="arraySize">Number of textures in the array or num cubes for <see cref="ShaderResourceViewDimension.TextureCubeArray"/>. </param>
        /// <param name="flags"><see cref="BufferExtendedShaderResourceViewFlags"/> for <see cref="ShaderResourceViewDimension.BufferExtended"/>.</param>
        public ShaderResourceViewDescription(
            ShaderResourceViewDimension viewDimension,
            Format format = Format.Unknown,
            int mostDetailedMip = 0,
            int mipLevels = -1,
            int firstArraySlice = 0,
            int arraySize = -1,
            BufferExtendedShaderResourceViewFlags flags = BufferExtendedShaderResourceViewFlags.None) : this()
        {
            Format = format;
            ViewDimension = viewDimension;
            switch (viewDimension)
            {
                case ShaderResourceViewDimension.Buffer:
                    Buffer.FirstElement = mostDetailedMip;
                    Buffer.NumElements = mipLevels;
                    break;

                case ShaderResourceViewDimension.Texture1D:
                    Texture1D.MostDetailedMip = mostDetailedMip;
                    Texture1D.MipLevels = mipLevels;
                    break;

                case ShaderResourceViewDimension.Texture1DArray:
                    Texture1DArray.MostDetailedMip = mostDetailedMip;
                    Texture1DArray.MipLevels = mipLevels;
                    Texture1DArray.FirstArraySlice = firstArraySlice;
                    Texture1DArray.ArraySize = arraySize;
                    break;

                case ShaderResourceViewDimension.Texture2D:
                    Texture2D.MostDetailedMip = mostDetailedMip;
                    Texture2D.MipLevels = mipLevels;
                    break;

                case ShaderResourceViewDimension.Texture2DArray:
                    Texture2DArray.MostDetailedMip = mostDetailedMip;
                    Texture2DArray.MipLevels = mipLevels;
                    Texture2DArray.FirstArraySlice = firstArraySlice;
                    Texture2DArray.ArraySize = arraySize;
                    break;

                case ShaderResourceViewDimension.Texture2DMultisampled:
                    break;

                case ShaderResourceViewDimension.Texture2DMultisampledArray:
                    Texture2DMSArray.FirstArraySlice = firstArraySlice;
                    Texture2DMSArray.ArraySize = arraySize;
                    break;

                case ShaderResourceViewDimension.Texture3D:
                    Texture3D.MostDetailedMip = mostDetailedMip;
                    Texture3D.MipLevels = mipLevels;
                    break;

                case ShaderResourceViewDimension.TextureCube:
                    TextureCube.MostDetailedMip = mostDetailedMip;
                    TextureCube.MipLevels = mipLevels;
                    break;

                case ShaderResourceViewDimension.TextureCubeArray:
                    TextureCubeArray.MostDetailedMip = mostDetailedMip;
                    TextureCubeArray.MipLevels = mipLevels;
                    TextureCubeArray.First2DArrayFace = firstArraySlice;
                    TextureCubeArray.NumCubes = arraySize;
                    break;

                case ShaderResourceViewDimension.BufferExtended:
                    BufferEx.FirstElement = mostDetailedMip;
                    BufferEx.NumElements = mipLevels;
                    BufferEx.Flags = flags;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewDescription"/> struct.
        /// </summary>
        /// <param name="buffer">Unused <see cref="IBuffer"/> </param>
        /// <param name="format"></param>
        /// <param name="firstElement"></param>
        /// <param name="numElements"></param>
        /// <param name="flags"></param>
        public ShaderResourceViewDescription(IBuffer buffer, Format format, int firstElement, int numElements, BufferExtendedShaderResourceViewFlags flags = BufferExtendedShaderResourceViewFlags.None)
            : this()
        {
            Format = format;
            ViewDimension = ShaderResourceViewDimension.BufferExtended;
            BufferEx.FirstElement = firstElement;
            BufferEx.NumElements = numElements;
            BufferEx.Flags = flags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewDescription"/> struct for a 1D texture or 1D texture array.
        /// </summary>
        /// <param name="texture">The 1D texture.</param>
        /// <param name="isArray">Indicates whether the texture is an array.</param>
        /// <param name="format">The format of the resource. Defaults to <see cref="Format.Unknown"/>.</param>
        /// <param name="mostDetailedMip">The most detailed mip level. Defaults to 0.</param>
        /// <param name="mipLevels">The number of mip levels. Defaults to -1.</param>
        /// <param name="firstArraySlice">The index of the first array slice. Defaults to 0.</param>
        /// <param name="arraySize">The number of array slices. Defaults to -1 for 1D textures.</param>
        public ShaderResourceViewDescription(
            ITexture1D texture,
            bool isArray,
            Format format = Format.Unknown,
            int mostDetailedMip = 0,
            int mipLevels = -1,
            int firstArraySlice = 0,
            int arraySize = -1)
            : this()
        {
            ViewDimension = isArray ? ShaderResourceViewDimension.Texture1DArray : ShaderResourceViewDimension.Texture1D;
            if (format == Format.Unknown
                || mipLevels == -1
                || (arraySize == -1 && ShaderResourceViewDimension.Texture1DArray == ViewDimension))
            {
                Texture1DDescription textureDesc = texture.Description;
                if (format == Format.Unknown)
                {
                    format = textureDesc.Format;
                }

                if (mipLevels == -1)
                {
                    mipLevels = textureDesc.MipLevels - mostDetailedMip;
                }

                if (arraySize == -1)
                {
                    arraySize = textureDesc.ArraySize - firstArraySlice;
                }
            }

            Format = format;
            switch (ViewDimension)
            {
                case ShaderResourceViewDimension.Texture1D:
                    Texture1D.MostDetailedMip = mostDetailedMip;
                    Texture1D.MipLevels = mipLevels;
                    break;

                case ShaderResourceViewDimension.Texture1DArray:
                    Texture1DArray.MostDetailedMip = mostDetailedMip;
                    Texture1DArray.MipLevels = mipLevels;
                    Texture1DArray.FirstArraySlice = firstArraySlice;
                    Texture1DArray.ArraySize = arraySize;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewDescription"/> struct.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="viewDimension"></param>
        /// <param name="format"></param>
        /// <param name="mostDetailedMip"></param>
        /// <param name="mipLevels"></param>
        /// <param name="firstArraySlice"></param>
        /// <param name="arraySize"></param>
        public ShaderResourceViewDescription(
            ITexture2D texture,
            ShaderResourceViewDimension viewDimension,
            Format format = Format.Unknown,
            int mostDetailedMip = 0,
            int mipLevels = -1,
            int firstArraySlice = 0,
            int arraySize = -1)
            : this()
        {
            ViewDimension = viewDimension;
            if (format == Format.Unknown
                || (mipLevels == -1 && viewDimension != ShaderResourceViewDimension.Texture2DMultisampled && viewDimension != ShaderResourceViewDimension.Texture2DMultisampledArray)
                || (arraySize == -1 && (ShaderResourceViewDimension.Texture2DArray == viewDimension || ShaderResourceViewDimension.Texture2DMultisampledArray == viewDimension || ShaderResourceViewDimension.TextureCubeArray == viewDimension)))
            {
                Texture2DDescription textureDesc = texture.Description;
                if (format == Format.Unknown)
                {
                    format = textureDesc.Format;
                }

                if (-1 == mipLevels)
                {
                    mipLevels = textureDesc.MipLevels - mostDetailedMip;
                }

                if (-1 == arraySize)
                {
                    arraySize = textureDesc.ArraySize - firstArraySlice;
                    if (viewDimension == ShaderResourceViewDimension.TextureCubeArray)
                    {
                        arraySize /= 6;
                    }
                }
            }
            Format = format;
            switch (viewDimension)
            {
                case ShaderResourceViewDimension.Texture2D:
                    Texture2D.MostDetailedMip = mostDetailedMip;
                    Texture2D.MipLevels = mipLevels;
                    break;

                case ShaderResourceViewDimension.Texture2DArray:
                    Texture2DArray.MostDetailedMip = mostDetailedMip;
                    Texture2DArray.MipLevels = mipLevels;
                    Texture2DArray.FirstArraySlice = firstArraySlice;
                    Texture2DArray.ArraySize = arraySize;
                    break;

                case ShaderResourceViewDimension.Texture2DMultisampled:
                    break;

                case ShaderResourceViewDimension.Texture2DMultisampledArray:
                    Texture2DMSArray.FirstArraySlice = firstArraySlice;
                    Texture2DMSArray.ArraySize = arraySize;
                    break;

                case ShaderResourceViewDimension.TextureCube:
                    TextureCube.MostDetailedMip = mostDetailedMip;
                    TextureCube.MipLevels = mipLevels;
                    break;

                case ShaderResourceViewDimension.TextureCubeArray:
                    TextureCubeArray.MostDetailedMip = mostDetailedMip;
                    TextureCubeArray.MipLevels = mipLevels;
                    TextureCubeArray.First2DArrayFace = firstArraySlice;
                    TextureCubeArray.NumCubes = arraySize;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewDescription"/> struct.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="format"></param>
        /// <param name="mostDetailedMip"></param>
        /// <param name="mipLevels"></param>
        public ShaderResourceViewDescription(
            ITexture3D texture,
            Format format = Format.Unknown,
            int mostDetailedMip = 0,
            int mipLevels = -1)
            : this()
        {
            ViewDimension = ShaderResourceViewDimension.Texture3D;
            if (format == Format.Unknown || mipLevels == -1)
            {
                Texture3DDescription textureDesc = texture.Description;
                if (format == Format.Unknown)
                {
                    format = textureDesc.Format;
                }

                if (mipLevels == -1)
                {
                    mipLevels = textureDesc.MipLevels - mostDetailedMip;
                }
            }

            Format = format;
            Texture3D.MostDetailedMip = mostDetailedMip;
            Texture3D.MipLevels = mipLevels;
        }
    }
}