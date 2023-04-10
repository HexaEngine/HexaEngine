namespace HexaEngine.Core.IO.Assets
{
    public enum AssetType : ulong
    {
        Binary = 0,
        MaterialLibrary,
        TextureFile,
        ModelFile,
        FontFile,
        AudioFile,
        ShaderSource,
        ShaderBytecode,
    }
}