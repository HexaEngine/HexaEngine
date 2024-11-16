namespace HexaEngine.Core.IO.Json.Materials
{
    using HexaEngine.Core.IO.Binary.Materials;
    using System.Collections.Generic;

    public class JsonMaterialFile
    {
        public string Name = null!;
        public List<MaterialProperty> Properties = [];
        public List<MaterialTexture> Textures = [];
        public List<MaterialShader> Shaders = [];
        public MaterialFlags Flags = MaterialFlags.DepthTest;
    }
}