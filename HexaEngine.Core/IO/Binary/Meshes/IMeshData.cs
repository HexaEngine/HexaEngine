namespace HexaEngine.Core.IO.Binary.Meshes
{
    using HexaEngine.Core.Graphics;

    public interface IMeshData
    {
        public string Name { get; }

        public Guid Guid { get; }

        InputElementDescription[] InputElements { get; }

        ShaderMacro[] GetShaderMacros();
    }
}