namespace HexaEngine.Core.Meshes
{
    using System.Numerics;

    public struct CBMaterial
    {
        public Vector4 Color;
        public Vector4 RMAo;
        public Vector4 Emissive;
        public Vector4 Anisotropic;

        public int HasDisplacementMap;
        public int HasAlbedoMap;
        public int HasNormalMap;
        public int HasRoughnessMap;
        public int HasMetalnessMap;
        public int HasEmissiveMap;
        public int HasAoMap;
        public int HasRoughnessMetalnessMap;

        public CBMaterial(MaterialDesc material)
        {
            Color = new(material.BaseColor, 1);
            Emissive = new(material.Emissive, 1);
            RMAo = new(material.Roughness, material.Metalness, material.Ao, 1);
            Anisotropic = new(material.Anisotropic, 0, 0, 0);

            HasAlbedoMap = string.IsNullOrEmpty(material.BaseColorTextureMap) ? 0 : 1;
            HasNormalMap = string.IsNullOrEmpty(material.NormalTextureMap) ? 0 : 1;
            HasDisplacementMap = string.IsNullOrEmpty(material.DisplacementTextureMap) ? 0 : 1;

            HasMetalnessMap = string.IsNullOrEmpty(material.MetalnessTextureMap) ? 0 : 1;
            HasRoughnessMap = string.IsNullOrEmpty(material.RoughnessTextureMap) ? 0 : 1;
            HasEmissiveMap = string.IsNullOrEmpty(material.EmissiveTextureMap) ? 0 : 1;
            HasAoMap = string.IsNullOrEmpty(material.AoTextureMap) ? 0 : 1;

            HasRoughnessMetalnessMap = string.IsNullOrEmpty(material.RoughnessMetalnessTextureMap) ? 0 : 1;
        }

        public static implicit operator CBMaterial(MaterialDesc material)
        {
            return new(material);
        }
    }
}