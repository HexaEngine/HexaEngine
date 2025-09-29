namespace HexaEngine.ShadingLang.LexicalAnalysis.HXSL
{
    using System.ComponentModel;

    public enum HXSLKeyword : int
    {
        Unknown,

        [Description("AppendStructuredBuffer")]
        AppendStructuredBuffer,

        [Description("asm")]
        Asm,

        [Description("asm_fragment")]
        AsmFragment,

        [Description("BlendState")]
        BlendState,

        [Description("bool")]
        Bool,

        [Description("break")]
        Break,

        [Description("Buffer")]
        Buffer,

        [Description("ByteAddressBuffer")]
        ByteAddressBuffer,

        [Description("case")]
        Case,

        [Description("cbuffer")]
        Cbuffer,

        [Description("centroid")]
        Centroid,

        [Description("class")]
        Class,

        [Description("column_major")]
        ColumnMajor,

        [Description("compile")]
        Compile,

        [Description("compile_fragment")]
        CompileFragment,

        [Description("CompileShader")]
        CompileShader,

        [Description("const")]
        Const,

        [Description("continue")]
        Continue,

        [Description("ComputeShader")]
        ComputeShader,

        [Description("ConsumeStructuredBuffer")]
        ConsumeStructuredBuffer,

        [Description("default")]
        Default,

        [Description("DepthStencilState")]
        DepthStencilState,

        [Description("DepthStencilView")]
        DepthStencilView,

        [Description("discard")]
        Discard,

        [Description("do")]
        Do,

        [Description("double")]
        Double,

        [Description("DomainShader")]
        DomainShader,

        [Description("dword")]
        Dword,

        [Description("else")]
        Else,

        [Description("export")]
        Export,

        [Description("extern")]
        Extern,

        [Description("false")]
        False,

        [Description("float")]
        Float,

        [Description("for")]
        For,

        [Description("fxgroup")]
        Fxgroup,

        [Description("GeometryShader")]
        GeometryShader,

        [Description("groupshared")]
        Groupshared,

        [Description("half")]
        Half,

        [Description("Hullshader")]
        Hullshader,

        [Description("if")]
        If,

        [Description("in")]
        In,

        [Description("inline")]
        Inline,

        [Description("inout")]
        Inout,

        [Description("InputPatch")]
        InputPatch,

        [Description("int")]
        Int,

        [Description("interface")]
        Interface,

        [Description("line")]
        Line,

        [Description("lineadj")]
        Lineadj,

        [Description("linear")]
        Linear,

        [Description("LineStream")]
        LineStream,

        [Description("matrix")]
        Matrix,

        [Description("min16float")]
        Min16float,

        [Description("min10float")]
        Min10float,

        [Description("min16int")]
        Min16int,

        [Description("min12int")]
        Min12int,

        [Description("min16uint")]
        Min16uint,

        [Description("namespace")]
        Namespace,

        [Description("nointerpolation")]
        Nointerpolation,

        [Description("noperspective")]
        Noperspective,

        [Description("NULL")]
        Null,

        [Description("out")]
        Out,

        [Description("OutputPatch")]
        OutputPatch,

        [Description("packoffset")]
        Packoffset,

        [Description("pass")]
        Pass,

        [Description("pixelfragment")]
        Pixelfragment,

        [Description("PixelShader")]
        PixelShader,

        [Description("point")]
        Point,

        [Description("PointStream")]
        PointStream,

        [Description("precise")]
        Precise,

        [Description("RasterizerState")]
        RasterizerState,

        [Description("RenderTargetView")]
        RenderTargetView,

        [Description("return")]
        Return,

        [Description("register")]
        Register,

        [Description("row_major")]
        RowMajor,

        [Description("RWBuffer")]
        RWBuffer,

        [Description("RWByteAddressBuffer")]
        RWByteAddressBuffer,

        [Description("RWStructuredBuffer")]
        RWStructuredBuffer,

        [Description("RWTexture1D")]
        RWTexture1D,

        [Description("RWTexture1DArray")]
        RWTexture1DArray,

        [Description("RWTexture2D")]
        RWTexture2D,

        [Description("RWTexture2DArray")]
        RWTexture2DArray,

        [Description("RWTexture3D")]
        RWTexture3D,

        [Description("sample")]
        Sample,

        [Description("sampler")]
        Sampler,

        [Description("SamplerState")]
        SamplerState,

        [Description("SamplerComparisonState")]
        SamplerComparisonState,

        [Description("shared")]
        Shared,

        [Description("snorm")]
        Snorm,

        [Description("stateblock")]
        Stateblock,

        [Description("stateblock_state")]
        StateblockState,

        [Description("static")]
        Static,

        [Description("string")]
        String,

        [Description("struct")]
        Struct,

        [Description("switch")]
        Switch,

        [Description("StructuredBuffer")]
        StructuredBuffer,

        [Description("tbuffer")]
        Tbuffer,

        [Description("technique")]
        Technique,

        [Description("technique10")]
        Technique10,

        [Description("technique11")]
        Technique11,

        [Description("texture")]
        Texture,

        [Description("Texture1D")]
        Texture1D,

        [Description("Texture1DArray")]
        Texture1DArray,

        [Description("Texture2D")]
        Texture2D,

        [Description("Texture2DArray")]
        Texture2DArray,

        [Description("Texture2DMS")]
        Texture2DMS,

        [Description("Texture2DMSArray")]
        Texture2DMSArray,

        [Description("Texture3D")]
        Texture3D,

        [Description("TextureCube")]
        TextureCube,

        [Description("TextureCubeArray")]
        TextureCubeArray,

        [Description("true")]
        True,

        [Description("typedef")]
        Typedef,

        [Description("triangle")]
        Triangle,

        [Description("triangleadj")]
        Triangleadj,

        [Description("TriangleStream")]
        TriangleStream,

        [Description("uint")]
        Uint,

        [Description("uniform")]
        Uniform,

        [Description("unorm")]
        Unorm,

        [Description("unsigned")]
        Unsigned,

        [Description("vector")]
        Vector,

        [Description("vertexfragment")]
        Vertexfragment,

        [Description("VertexShader")]
        VertexShader,

        [Description("void")]
        Void,

        [Description("volatile")]
        Volatile,

        [Description("while")]
        While,

        /// <summary>
        /// using keyword for custom lang.
        /// </summary>
        [Description("using")]
        Using,

        /// <summary>
        /// private keyword for custom lang.
        /// </summary>
        [Description("private")]
        Private,

        /// <summary>
        /// internal keyword for custom lang.
        /// </summary>
        [Description("internal")]
        Internal,

        /// <summary>
        /// private keyword for custom lang.
        /// </summary>
        [Description("public")]
        Public,
    }
}