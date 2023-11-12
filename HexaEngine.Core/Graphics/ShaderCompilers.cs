namespace HexaEngine.Core.Graphics
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a registry for <see cref="IShaderCompiler"/> instances associated with graphics backends.
    /// </summary>
    public static class ShaderCompilers
    {
        private static readonly Dictionary<GraphicsBackend, IShaderCompiler> compilers = [];

        /// <summary>
        /// Registers a shader compiler for a specific graphics backend, overwriting the existing compiler if already registered.
        /// </summary>
        /// <param name="backend">The graphics backend.</param>
        /// <param name="compiler">The shader compiler.</param>
        public static void Register(GraphicsBackend backend, IShaderCompiler compiler)
        {
            if (!compilers.TryAdd(backend, compiler))
            {
                compilers[backend] = compiler;
            }
        }

        /// <summary>
        /// Gets the shader compiler associated with a specific graphics backend.
        /// </summary>
        /// <param name="backend">The graphics backend.</param>
        /// <returns>The shader compiler associated with the specified backend.</returns>
        public static IShaderCompiler GetShaderCompiler(GraphicsBackend backend)
        {
            return compilers[backend];
        }
    }
}