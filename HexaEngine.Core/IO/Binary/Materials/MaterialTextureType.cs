namespace HexaEngine.Core.IO.Binary.Materials
{

    /// <summary>
    /// Enumeration representing different types of material textures.
    /// </summary>
    public enum MaterialTextureType
    {
        /// <summary>
        /// No specific texture type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Diffuse texture type.
        /// </summary>
        Diffuse = 1,

        /// <summary>
        /// Specular texture type.
        /// </summary>
        Specular = 2,

        /// <summary>
        /// Ambient texture type.
        /// </summary>
        Ambient = 3,

        /// <summary>
        /// Emissive texture type.
        /// </summary>
        Emissive = 4,

        /// <summary>
        /// Heightmap texture type.
        /// </summary>
        Height = 5,

        /// <summary>
        /// Normal map texture type.
        /// </summary>
        Normal = 6,

        /// <summary>
        /// Shininess map texture type.
        /// </summary>
        Shininess = 7,

        /// <summary>
        /// Opacity map texture type.
        /// </summary>
        Opacity = 8,

        /// <summary>
        /// Displacement map texture type.
        /// </summary>
        Displacement = 9,

        /// <summary>
        /// Combined texture type for Ambient Occlusion, Roughness, and Metallic.
        /// </summary>
        AmbientOcclusionRoughnessMetallic = 10,

        /// <summary>
        /// Reflection map texture type.
        /// </summary>
        Reflection = 11,

        /// <summary>
        /// Base color texture type.
        /// </summary>
        BaseColor = 12,

        /// <summary>
        /// Normal map in camera space texture type.
        /// </summary>
        NormalCamera = 13,

        /// <summary>
        /// Emission color texture type.
        /// </summary>
        EmissionColor = 14,

        /// <summary>
        /// Metallic texture type.
        /// </summary>
        Metallic = 0xF,

        /// <summary>
        /// Roughness texture type.
        /// </summary>
        Roughness = 0x10,

        /// <summary>
        /// Ambient Occlusion texture type.
        /// </summary>
        AmbientOcclusion = 17,

        /// <summary>
        /// Unknown texture type.
        /// </summary>
        Unknown = 18,

        /// <summary>
        /// Sheen texture type.
        /// </summary>
        Sheen = 19,

        /// <summary>
        /// Clearcoat texture type.
        /// </summary>
        Clearcoat = 20,

        /// <summary>
        /// Transmission map texture type.
        /// </summary>
        Transmission = 21,

        /// <summary>
        /// Combined texture type for Roughness and Metallic.
        /// </summary>
        RoughnessMetallic = 22,
    }
}