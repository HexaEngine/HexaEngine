namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;

    public struct MaterialShaderDesc
    {
        public Guid MaterialId;
        public InputElementDescription[] InputElements;
        public ShaderMacro[] Macros;
        public ShaderMacro[] MeshMacros;
        public MaterialShaderPassDesc[] Passes;

        public MaterialShaderDesc(MaterialData data, ShaderMacro[] meshMacros, InputElementDescription[] inputElements, params MaterialShaderPassDesc[] passes)
        {
            MaterialId = data.Guid;
            InputElements = inputElements;
            Macros = data.GetShaderMacros();
            MeshMacros = meshMacros;
            Passes = passes;
        }

        public MaterialShaderDesc(Guid materialId, ShaderMacro[] macros, ShaderMacro[] meshMacros, InputElementDescription[] inputElements, params MaterialShaderPassDesc[] passes)
        {
            MaterialId = materialId;
            InputElements = inputElements;
            Macros = macros;
            MeshMacros = meshMacros;
            Passes = passes;
        }
    }
}