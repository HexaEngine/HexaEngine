namespace HexaEngine.Core.IO.Binary.Shaders
{
    /// <summary>
    /// Specifies flags that control the compilation and optimization behavior of a shader.
    /// </summary>
    [Flags]
    public enum ShaderFlags
    {
        /// <summary>
        /// No flags specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Compile the shader with debug information.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Strip the debug information from the compiled shader.
        /// </summary>
        StripDebug = 2,

        /// <summary>
        /// Optimize the shader for better performance, with lower optimization level.
        /// </summary>
        Optimization1 = 4,

        /// <summary>
        /// Optimize the shader for better performance, with higher optimization level.
        /// </summary>
        Optimization2 = 8,
    }
}