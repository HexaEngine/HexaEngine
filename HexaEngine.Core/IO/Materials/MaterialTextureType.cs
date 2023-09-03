namespace HexaEngine.Core.IO.Materials
{
    public enum MaterialTextureType
    {
        None = 0,
        Diffuse = 1,
        Specular = 2,
        Ambient = 3,
        Emissive = 4,
        Height = 5,
        Normal = 6,
        Shininess = 7,
        Opacity = 8,
        Displacement = 9,
        AmbientOcclusionRoughnessMetalness = 10,
        Reflection = 11,
        BaseColor = 12,
        NormalCamera = 13,
        EmissionColor = 14,
        Metalness = 0xF,
        Roughness = 0x10,
        AmbientOcclusion = 17,
        Sheen = 19,
        Clearcoat = 20,
        Transmission = 21,
        RoughnessMetalness = 22,
        Unknown = 18
    }
}