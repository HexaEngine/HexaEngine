namespace HexaEngine.Core.Graphics
{
    using System.Collections.Generic;

    public static class ShaderCompilers
    {
        private static Dictionary<RenderBackend, IShaderCompiler> compilers = new();

        public static void Register(RenderBackend backend, IShaderCompiler compiler)
        {
            compilers.Add(backend, compiler);
        }

        public static IShaderCompiler GetShaderCompiler(RenderBackend backend)
        {
            return compilers[backend];
        }
    }
}