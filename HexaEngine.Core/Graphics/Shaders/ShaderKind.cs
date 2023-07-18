namespace HexaEngine.Core.Graphics.Shaders
{
    public enum ShaderKind
    {
        //
        // Summary:
        //     Forced shader kinds. These shader kinds force the compiler to compile the source
        //     code as the specified kind of shader.
        VertexShader = 0,

        //
        // Summary:
        //     Forced shader kinds. These shader kinds force the compiler to compile the source
        //     code as the specified kind of shader.
        FragmentShader = 1,

        //
        // Summary:
        //     Forced shader kinds. These shader kinds force the compiler to compile the source
        //     code as the specified kind of shader.
        ComputeShader = 2,

        //
        // Summary:
        //     Forced shader kinds. These shader kinds force the compiler to compile the source
        //     code as the specified kind of shader.
        GeometryShader = 3,

        //
        // Summary:
        //     Forced shader kinds. These shader kinds force the compiler to compile the source
        //     code as the specified kind of shader.
        TessControlShader = 4,

        //
        // Summary:
        //     Forced shader kinds. These shader kinds force the compiler to compile the source
        //     code as the specified kind of shader.
        TessEvaluationShader = 5,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        SpirvAssembly = 13,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        RaygenShader = 14,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        AnyhitShader = 0xF,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        ClosesthitShader = 0x10,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        MissShader = 17,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        IntersectionShader = 18,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        CallableShader = 19,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        TaskShader = 26,

        //
        // Summary:
        //     Default shader kinds. Compiler will fall back to compile the source code as the
        //     specified kind of shader when #pragma annotation is not found in the source code.
        MeshShader = 27,
    }
}