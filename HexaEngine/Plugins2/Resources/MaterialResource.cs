namespace HexaEngine.Plugins2.Resources
{
    using HexaEngine.Objects;
    using System;
    using System.Numerics;

    public class MaterialResource : Resource
    {
        private string normalTextureMap = string.Empty;
        private string displacementTextureMap = string.Empty;
        private string baseColorTextureMap = string.Empty;
        private string specularTextureMap = string.Empty;
        private string specularColorTextureMap = string.Empty;
        private string roughnessTextureMap = string.Empty;
        private string metalnessTextureMap = string.Empty;
        private string roughnessmetalnessTextureMap = string.Empty;
        private string aoTextureMap = string.Empty;
        private string cleancoatTextureMap = string.Empty;
        private string cleancoatGlossTextureMap = string.Empty;
        private string sheenTextureMap = string.Empty;
        private string sheenTintTextureMap = string.Empty;
        private string anisotropicTextureMap = string.Empty;
        private string subsurfaceTextureMap = string.Empty;
        private string subsurfaceColorTextureMap = string.Empty;
        private string emissivenessTextureMap = string.Empty;

        private Vector3 baseColor;
        private float opacity;
        private float specular;
        private float specularTint;
        private Vector3 specularColor;
        private float ao;
        private float metalness;
        private float roughness;
        private float cleancoat;
        private float cleancoatGloss;
        private float sheen;
        private float sheenTint;
        private float anisotropic;
        private float subsurface;
        private Vector3 subsurfaceColor;
        private Vector3 emissivness;

        public MaterialResource(Material material) : base(material.Name)
        {
            baseColor = material.BaseColor;
            opacity = material.Opacity;
            specular = material.Specular;
        }

        public override ResourceType Type => ResourceType.Material;

        public Material GetMaterial()
        {
            return new();
        }

        public override int Read(ReadOnlySpan<byte> source)
        {
            throw new NotImplementedException();
        }

        public override int Size()
        {
            throw new NotImplementedException();
        }

        public override int Write(Span<byte> source)
        {
            throw new NotImplementedException();
        }
    }
}