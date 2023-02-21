namespace HexaEngine.Core.Meshes
{
    using HexaEngine.Core.IO.Meshes;
    using System.Numerics;

    public struct CBMaterial
    {
        public Vector4 Color;

        public float Roughness;
        public float Metalness;
        public float Specular;
        public float SpecularTint;

        public float Sheen;
        public float SheenTint;
        public float Clearcoat;
        public float ClearcoatGloss;

        public float Anisotropic;
        public float Subsurface;
        public float Ao;
        public float Padding0;

        public Vector3 Emissive;
        public float Padding1;

        public int HasDisplacementMap;
        public int HasAlbedoMap;
        public int HasNormalMap;
        public int HasRoughnessMap;
        public int HasMetalnessMap;
        public int HasEmissiveMap;
        public int HasAoMap;
        public int HasRoughnessMetalnessMap;

        public CBMaterial(MaterialData material)
        {
            Color = new(material.BaseColor, 1);
            Roughness = material.Roughness;
            Metalness = material.Metalness;
            Specular = material.Specular;
            SpecularTint = material.SpecularTint;
            Sheen = material.Sheen;
            SheenTint = material.SheenTint;
            Clearcoat = material.Cleancoat;
            ClearcoatGloss = material.CleancoatGloss;
            Anisotropic = material.Anisotropic;
            Subsurface = material.Subsurface;
            Ao = material.Ao;
            Emissive = material.Emissive;

            HasAlbedoMap = string.IsNullOrEmpty(material.BaseColorTextureMap) ? 0 : 1;
            HasNormalMap = string.IsNullOrEmpty(material.NormalTextureMap) ? 0 : 1;
            HasDisplacementMap = string.IsNullOrEmpty(material.DisplacementTextureMap) ? 0 : 1;

            HasMetalnessMap = string.IsNullOrEmpty(material.MetalnessTextureMap) ? 0 : 1;
            HasRoughnessMap = string.IsNullOrEmpty(material.RoughnessTextureMap) ? 0 : 1;
            HasEmissiveMap = string.IsNullOrEmpty(material.EmissiveTextureMap) ? 0 : 1;
            HasAoMap = string.IsNullOrEmpty(material.AoTextureMap) ? 0 : 1;

            HasRoughnessMetalnessMap = string.IsNullOrEmpty(material.RoughnessMetalnessTextureMap) ? 0 : 1;
        }

        public static implicit operator CBMaterial(MaterialData material)
        {
            return new(material);
        }
    }
}