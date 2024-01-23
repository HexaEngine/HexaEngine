namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Structure describing a shader buffer.
    /// </summary>
    public struct ShaderBufferDesc
    {
        /// <summary>
        /// The name of the shader buffer.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the constant buffer.
        /// </summary>
        public CBufferType Type;

        /// <summary>
        /// Array of <see cref="ShaderVariableDesc"/> representing variables in the shader buffer.
        /// </summary>
        public ShaderVariableDesc[] Variables;

        /// <summary>
        /// The size of the shader buffer in bytes.
        /// </summary>
        public uint Size;

        /// <summary>
        /// User-defined flags associated with the shader buffer.
        /// </summary>
        public uint UFlags;
    }

    public struct ShaderVariableDesc
    {
        public string Name;
        public uint StartOffset;
        public uint Size;
        public uint UFlags;
        public unsafe void* DefaultValue;
        public uint StartTexture;
        public uint TextureSize;
        public uint StartSampler;
        public uint SamplerSize;
    }

    public enum ShaderVariableClass
    {
        Scalar = 0,
        Vector = 1,
        MatrixRows = 2,
        MatrixColumns = 3,
        Object = 4,
        Struct = 5,
        InterfaceClass = 6,
        InterfacePointer = 7,
        ForceDWord = int.MaxValue
    }

    public enum ShaderVariableType
    {
        Void = 0,
        Bool = 1,
        Int = 2,
        Float = 3,
        String = 4,
        Texture = 5,
        Texture1D = 6,
        Texture2D = 7,
        Texture3D = 8,
        TextureCube = 9,
        Sampler = 10,
        Sampler1D = 11,
        Sampler2D = 12,
        Sampler3D = 13,
        SamplerCube = 14,
        PixelShader = 15,
        VertexShader = 16,
        PixelFragment = 17,
        VertexFragment = 18,
        Uint = 19,
        Uint8 = 20,
        GeometryShader = 21,
        Rasterizer = 22,
        DepthStencil = 23,
        Blend = 24,
        Buffer = 25,
        CBuffer = 26,
        TBuffer = 27,
        Texture1DArray = 28,
        Texture2DArray = 29,
        RenderTargetView = 30,
        DepthStencilView = 31,
        Texture2DMS = 32,
        Texture2DMSArray = 33,
        TextureCubeArray = 34,
        HullShader = 35,
        DomainShader = 36,
        InterfacePointer = 37,
        ComputeShader = 38,
        Double = 39,
        RwTexture1D = 40,
        RwTexture1DArray = 41,
        RwTexture2D = 42,
        RwTexture2DArray = 43,
        RwTexture3D = 44,
        RwBuffer = 45,
        ByteAddressBuffer = 46,
        RwByteAddressBuffer = 47,
        StructuredBuffer = 48,
        RwStructuredBuffer = 49,
        AppendStructuredBuffer = 50,
        ConsumeStructuredBuffer = 51,
        Min8Float = 52,
        Min10Float = 53,
        Min16Float = 54,
        Min12Int = 55,
        Min16Int = 56,
        Min16UInt = 57,
        Int16 = 58,
        UInt16 = 59,
        Float16 = 60,
        Int64 = 61,
        UInt64 = 62,
        ForceDWord = int.MaxValue
    }

    public struct ShaderTypeDesc
    {
        public ShaderVariableClass Class;
        public ShaderVariableType Type;
        public uint Rows;
        public uint Columns;
        public uint Elements;
        public uint Members;
        public uint Offset;
        public unsafe byte* Name;
    }
}