namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;

    public struct MaterialShaderDesc
    {
        public Guid MaterialId;
        public InputElementDescription[] InputElements;
        public ShaderMacro[] Macros;
        public MaterialShaderPassDesc[] Passes;

        public MaterialShaderDesc(MaterialData data, InputElementDescription[] inputElements, params MaterialShaderPassDesc[] passes)
        {
            MaterialId = data.Guid;
            InputElements = inputElements;
            Macros = data.GetShaderMacros();
            Passes = passes;
        }

        public MaterialShaderDesc(Guid materialId, ShaderMacro[] macros, InputElementDescription[] inputElements, params MaterialShaderPassDesc[] passes)
        {
            MaterialId = materialId;
            InputElements = inputElements;
            Macros = macros;
            Passes = passes;
        }
    }
}