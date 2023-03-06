namespace HexaEngine.SPIRVCross
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcReflectedResource
    {
        public uint Id;
        public uint BaseTypeId;
        public uint TypeId;
        public unsafe byte* Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcEntryPoint
    {
        public SpvExecutionModel ExecutionModel;
        public unsafe byte* Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcCombinedImageSampler
    {
        public uint CombinedId;
        public uint ImageId;
        public uint SamplerId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcSpecializationConstant
    {
        public uint Id;
        public uint ConstantId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcBufferRange
    {
        public uint Index;
        public nuint Offset;
        public nuint Range;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcHlslRootConstants
    {
        public uint Start;
        public uint End;
        public uint Binding;
        public uint Space;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcHlslVertexAttributeRemap
    {
        public uint Location;
        public unsafe byte* Semantic;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcMslVertexAttribute
    {
        public uint Location;
        public uint MslBuffer;
        public uint MslOffset;
        public uint MslStride;
        public bool PerInstance;
        public SpvcMslVertexFormat Format;
        public SpvBuiltIn Builtin;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcMslShaderInput
    {
        public uint Location;
        public SpvcMslVertexFormat Format;
        public SpvBuiltIn Builtin;
        public uint Vecsize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcMslResourceBinding
    {
        public SpvExecutionModel Stage;
        public uint DescSet;
        public uint Binding;
        public uint MslBuffer;
        public uint MslTexture;
        public uint MslSampler;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcMslConstexprSampler
    {
        public SpvcMslSamplerCoord Coord;
        public SpvcMslSamplerFilter MinFilter;
        public SpvcMslSamplerFilter MagFilter;
        public SpvcMslSamplerMipFilter MipFilter;
        public SpvcMslSamplerAddress SAddress;
        public SpvcMslSamplerAddress TAddress;
        public SpvcMslSamplerAddress RAddress;
        public SpvcMslSamplerCompareFunc CompareFunc;
        public SpvcMslSamplerBorderColor BorderColor;
        public float LodClampMin;
        public float LodClampMax;
        public int MaxAnisotropy;
        public bool CompareEnable;
        public bool LodClampEnable;
        public bool AnisotropyEnable;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcMslSamplerYcbcrConversion
    {
        public uint Planes;
        public SpvcMslFormatResolution Resolution;
        public SpvcMslSamplerFilter ChromaFilter;
        public SpvcMslChromaLocation XChromaOffset;
        public SpvcMslChromaLocation YChromaOffset;
        public SpvcMslComponentSwizzle Swizzle0;
        public SpvcMslComponentSwizzle Swizzle1;
        public SpvcMslComponentSwizzle Swizzle2;
        public SpvcMslComponentSwizzle Swizzle3;
        public SpvcMslSamplerYcbcrModelConversion YcbcrModel;
        public SpvcMslSamplerYcbcrRange YcbcrRange;
        public uint Bpc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcHlslResourceBindingMapping
    {
        public uint RegisterSpace;
        public uint RegisterBinding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpvcHlslResourceBinding
    {
        public SpvExecutionModel Stage;
        public uint DescSet;
        public uint Binding;
        public SpvcHlslResourceBindingMapping Cbv;
        public SpvcHlslResourceBindingMapping Uav;
        public SpvcHlslResourceBindingMapping Srv;
        public SpvcHlslResourceBindingMapping Sampler;
    }
}