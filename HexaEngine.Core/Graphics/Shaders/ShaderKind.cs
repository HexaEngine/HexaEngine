namespace HexaEngine.Core.Graphics.Shaders
{
    /// <summary>
    /// Represents the kind of shader to be compiled.
    /// </summary>
    public enum ShaderKind
    {
        /// <summary>
        /// Forced shader kind. Forces the compiler to compile the source code as a vertex shader.
        /// </summary>
        VertexShader = 0,

        /// <summary>
        /// Forced shader kind. Forces the compiler to compile the source code as a fragment shader.
        /// </summary>
        FragmentShader = 1,

        /// <summary>
        /// Forced shader kind. Forces the compiler to compile the source code as a compute shader.
        /// </summary>
        ComputeShader = 2,

        /// <summary>
        /// Forced shader kind. Forces the compiler to compile the source code as a geometry shader.
        /// </summary>
        GeometryShader = 3,

        /// <summary>
        /// Forced shader kind. Forces the compiler to compile the source code as a tessellation control shader.
        /// </summary>
        TessControlShader = 4,

        /// <summary>
        /// Forced shader kind. Forces the compiler to compile the source code as a tessellation evaluation shader.
        /// </summary>
        TessEvaluationShader = 5,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        SpirvAssembly = 13,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        RaygenShader = 14,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        AnyhitShader = 0xF,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        ClosesthitShader = 0x10,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        MissShader = 17,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        IntersectionShader = 18,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        CallableShader = 19,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        TaskShader = 26,

        /// <summary>
        /// Default shader kind. Compiler will fall back to compile the source code as the specified kind of shader
        /// when #pragma annotation is not found in the source code.
        /// </summary>
        MeshShader = 27,
    }
}