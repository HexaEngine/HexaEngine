namespace HexaEngine.Meshes
{
    public enum ModelMaterialShaderFlags
    {
        None = 0,
        Transparent = 1,
        Custom = 2,
        DepthTest = 4,
        Shadow = 8,
        AlphaTest = 16,
        TwoSided = 32,
        Tessellation = 64,
        Forward = 128,
        Deferred = 256,
        Bake = 512,
        DepthAlways = 1024,
    }
}