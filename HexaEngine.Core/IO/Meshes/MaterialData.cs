namespace HexaEngine.Core.IO.Meshes
{
    using System.Numerics;
    using System.Text;
    using HexaEngine.Core.IO;

    public struct MaterialData
    {
        public string Name = string.Empty;
        public Vector3 BaseColor;
        public float Opacity;
        public float Specular;
        public float SpecularTint;
        public Vector3 SpecularColor;
        public float Ao;
        public float Metalness;
        public float Roughness;
        public float Cleancoat;
        public float CleancoatGloss;
        public float Sheen;
        public float SheenTint;
        public float Anisotropic;
        public float Subsurface;
        public Vector3 SubsurfaceColor;
        public Vector3 Emissive;

        public string BaseColorTextureMap = string.Empty;
        public string NormalTextureMap = string.Empty;
        public string DisplacementTextureMap = string.Empty;
        public string SpecularTextureMap = string.Empty;
        public string SpecularColorTextureMap = string.Empty;
        public string RoughnessTextureMap = string.Empty;
        public string MetalnessTextureMap = string.Empty;
        public string RoughnessMetalnessTextureMap = string.Empty;
        public string AoTextureMap = string.Empty;
        public string CleancoatTextureMap = string.Empty;
        public string CleancoatGlossTextureMap = string.Empty;
        public string SheenTextureMap = string.Empty;
        public string SheenTintTextureMap = string.Empty;
        public string AnisotropicTextureMap = string.Empty;
        public string SubsurfaceTextureMap = string.Empty;
        public string SubsurfaceColorTextureMap = string.Empty;
        public string EmissiveTextureMap = string.Empty;

        public MaterialData()
        {
        }

        public static MaterialData Read(Stream src, Encoding encoding)
        {
            MaterialData material;
            material.Name = src.ReadString(encoding);
            material.BaseColor = src.ReadVector3();
            material.Opacity = src.ReadFloat();
            material.Specular = src.ReadFloat();
            material.SpecularTint = src.ReadFloat();
            material.SpecularColor = src.ReadVector3();
            material.Ao = src.ReadFloat();
            material.Metalness = src.ReadFloat();
            material.Roughness = src.ReadFloat();
            material.Cleancoat = src.ReadFloat();
            material.CleancoatGloss = src.ReadFloat();
            material.Sheen = src.ReadFloat();
            material.SheenTint = src.ReadFloat();
            material.Anisotropic = src.ReadFloat();
            material.Subsurface = src.ReadFloat();
            material.SubsurfaceColor = src.ReadVector3();
            material.Emissive = src.ReadVector3();

            material.BaseColorTextureMap = src.ReadString(encoding);
            material.NormalTextureMap = src.ReadString(encoding);
            material.DisplacementTextureMap = src.ReadString(encoding);
            material.SpecularTextureMap = src.ReadString(encoding);
            material.SpecularColorTextureMap = src.ReadString(encoding);
            material.RoughnessTextureMap = src.ReadString(encoding);
            material.MetalnessTextureMap = src.ReadString(encoding);
            material.RoughnessMetalnessTextureMap = src.ReadString(encoding);
            material.AoTextureMap = src.ReadString(encoding);
            material.CleancoatTextureMap = src.ReadString(encoding);
            material.CleancoatGlossTextureMap = src.ReadString(encoding);
            material.SheenTextureMap = src.ReadString(encoding);
            material.SheenTintTextureMap = src.ReadString(encoding);
            material.AnisotropicTextureMap = src.ReadString(encoding);
            material.SubsurfaceTextureMap = src.ReadString(encoding);
            material.SubsurfaceColorTextureMap = src.ReadString(encoding);
            material.EmissiveTextureMap = src.ReadString(encoding);
            return material;
        }

        public void Write(Stream dst, Encoding encoding)
        {
            dst.WriteString(Name, encoding);
            dst.WriteVector3(BaseColor);
            dst.WriteFloat(Opacity);
            dst.WriteFloat(Specular);
            dst.WriteFloat(SpecularTint);
            dst.WriteVector3(SpecularColor);
            dst.WriteFloat(Ao);
            dst.WriteFloat(Metalness);
            dst.WriteFloat(Roughness);
            dst.WriteFloat(Cleancoat);
            dst.WriteFloat(CleancoatGloss);
            dst.WriteFloat(Sheen);
            dst.WriteFloat(SheenTint);
            dst.WriteFloat(Anisotropic);
            dst.WriteFloat(Subsurface);
            dst.WriteVector3(SubsurfaceColor);
            dst.WriteVector3(Emissive);

            dst.WriteString(BaseColorTextureMap, encoding);
            dst.WriteString(NormalTextureMap, encoding);
            dst.WriteString(DisplacementTextureMap, encoding);
            dst.WriteString(SpecularTextureMap, encoding);
            dst.WriteString(SpecularColorTextureMap, encoding);
            dst.WriteString(RoughnessTextureMap, encoding);
            dst.WriteString(MetalnessTextureMap, encoding);
            dst.WriteString(RoughnessMetalnessTextureMap, encoding);
            dst.WriteString(AoTextureMap, encoding);
            dst.WriteString(CleancoatTextureMap, encoding);
            dst.WriteString(CleancoatGlossTextureMap, encoding);
            dst.WriteString(SheenTextureMap, encoding);
            dst.WriteString(SheenTintTextureMap, encoding);
            dst.WriteString(AnisotropicTextureMap, encoding);
            dst.WriteString(SubsurfaceTextureMap, encoding);
            dst.WriteString(SubsurfaceColorTextureMap, encoding);
            dst.WriteString(EmissiveTextureMap, encoding);
        }
    }
}