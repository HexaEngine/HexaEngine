namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Text;

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

        public MaterialTexture[] Textures;

        public MaterialFlags Flags = MaterialFlags.Depth;

        public MaterialData()
        {
        }

        public ShaderMacro[] GetShaderMacros()
        {
            ShaderMacro[] macros =
            {
                new ShaderMacro("BaseColor", new Vector4(BaseColor, Opacity).ToHLSL()),
                new ShaderMacro("Roughness", Roughness.ToHLSL()),
                new ShaderMacro("Metalness", Metalness.ToHLSL()),
                new ShaderMacro("Specular", Specular.ToHLSL()),
                new ShaderMacro("SpecularTint", SpecularTint.ToHLSL()),
                new ShaderMacro("Sheen", Sheen.ToHLSL()),
                new ShaderMacro("SheenTint", SheenTint.ToHLSL()),
                new ShaderMacro("Clearcoat", Cleancoat.ToHLSL()),
                new ShaderMacro("ClearcoatGloss", CleancoatGloss.ToHLSL()),
                new ShaderMacro("Anisotropic", Anisotropic.ToHLSL()),
                new ShaderMacro("Subsurface", Subsurface.ToHLSL()),
                new ShaderMacro("Ao", Ao.ToHLSL()),
                new ShaderMacro("Emissive", Emissive.ToHLSL()),

                new ShaderMacro("HasBaseColorTex", string.IsNullOrEmpty(BaseColorTextureMap).ToHLSL()),
                new ShaderMacro("HasNormalTex", string.IsNullOrEmpty(NormalTextureMap).ToHLSL()),
                new ShaderMacro("HasDisplacementTex", string.IsNullOrEmpty(DisplacementTextureMap).ToHLSL()),
                new ShaderMacro("HasMetalnessTex", string.IsNullOrEmpty(MetalnessTextureMap).ToHLSL()),
                new ShaderMacro("HasRoughnessTex", string.IsNullOrEmpty(RoughnessTextureMap).ToHLSL()),
                new ShaderMacro("HasEmissiveTex", string.IsNullOrEmpty(EmissiveTextureMap).ToHLSL()),
                new ShaderMacro("HasAoTex", string.IsNullOrEmpty(AoTextureMap).ToHLSL()),
                new ShaderMacro("HasRoughnessMetalnessTex", string.IsNullOrEmpty(RoughnessMetalnessTextureMap).ToHLSL()),
            };
            return macros;
        }

        public static MaterialData Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialData material;
            material.Name = src.ReadString(encoding, endianness);
            material.BaseColor = src.ReadVector3(endianness);
            material.Opacity = src.ReadFloat(endianness);
            material.Specular = src.ReadFloat(endianness);
            material.SpecularTint = src.ReadFloat(endianness);
            material.SpecularColor = src.ReadVector3(endianness);
            material.Ao = src.ReadFloat(endianness);
            material.Metalness = src.ReadFloat(endianness);
            material.Roughness = src.ReadFloat(endianness);
            material.Cleancoat = src.ReadFloat(endianness);
            material.CleancoatGloss = src.ReadFloat(endianness);
            material.Sheen = src.ReadFloat(endianness);
            material.SheenTint = src.ReadFloat(endianness);
            material.Anisotropic = src.ReadFloat(endianness);
            material.Subsurface = src.ReadFloat(endianness);
            material.SubsurfaceColor = src.ReadVector3(endianness);
            material.Emissive = src.ReadVector3(endianness);

            material.BaseColorTextureMap = src.ReadString(encoding, endianness);
            material.NormalTextureMap = src.ReadString(encoding, endianness);
            material.DisplacementTextureMap = src.ReadString(encoding, endianness);
            material.SpecularTextureMap = src.ReadString(encoding, endianness);
            material.SpecularColorTextureMap = src.ReadString(encoding, endianness);
            material.RoughnessTextureMap = src.ReadString(encoding, endianness);
            material.MetalnessTextureMap = src.ReadString(encoding, endianness);
            material.RoughnessMetalnessTextureMap = src.ReadString(encoding, endianness);
            material.AoTextureMap = src.ReadString(encoding, endianness);
            material.CleancoatTextureMap = src.ReadString(encoding, endianness);
            material.CleancoatGlossTextureMap = src.ReadString(encoding, endianness);
            material.SheenTextureMap = src.ReadString(encoding, endianness);
            material.SheenTintTextureMap = src.ReadString(encoding, endianness);
            material.AnisotropicTextureMap = src.ReadString(encoding, endianness);
            material.SubsurfaceTextureMap = src.ReadString(encoding, endianness);
            material.SubsurfaceColorTextureMap = src.ReadString(encoding, endianness);
            material.EmissiveTextureMap = src.ReadString(encoding, endianness);
            material.Flags = MaterialFlags.Depth;

            var textureCount = src.ReadInt(endianness);
            material.Textures = new MaterialTexture[textureCount];
            for (int i = 0; i < textureCount; i++)
            {
                material.Textures[i] = MaterialTexture.Read(src, encoding, endianness);
            }

            return material;
        }

        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);
            dst.WriteVector3(BaseColor, endianness);
            dst.WriteFloat(Opacity, endianness);
            dst.WriteFloat(Specular, endianness);
            dst.WriteFloat(SpecularTint, endianness);
            dst.WriteVector3(SpecularColor, endianness);
            dst.WriteFloat(Ao, endianness);
            dst.WriteFloat(Metalness, endianness);
            dst.WriteFloat(Roughness, endianness);
            dst.WriteFloat(Cleancoat, endianness);
            dst.WriteFloat(CleancoatGloss, endianness);
            dst.WriteFloat(Sheen, endianness);
            dst.WriteFloat(SheenTint, endianness);
            dst.WriteFloat(Anisotropic, endianness);
            dst.WriteFloat(Subsurface, endianness);
            dst.WriteVector3(SubsurfaceColor, endianness);
            dst.WriteVector3(Emissive, endianness);

            dst.WriteString(BaseColorTextureMap, encoding, endianness);
            dst.WriteString(NormalTextureMap, encoding, endianness);
            dst.WriteString(DisplacementTextureMap, encoding, endianness);
            dst.WriteString(SpecularTextureMap, encoding, endianness);
            dst.WriteString(SpecularColorTextureMap, encoding, endianness);
            dst.WriteString(RoughnessTextureMap, encoding, endianness);
            dst.WriteString(MetalnessTextureMap, encoding, endianness);
            dst.WriteString(RoughnessMetalnessTextureMap, encoding, endianness);
            dst.WriteString(AoTextureMap, encoding, endianness);
            dst.WriteString(CleancoatTextureMap, encoding, endianness);
            dst.WriteString(CleancoatGlossTextureMap, encoding, endianness);
            dst.WriteString(SheenTextureMap, encoding, endianness);
            dst.WriteString(SheenTintTextureMap, encoding, endianness);
            dst.WriteString(AnisotropicTextureMap, encoding, endianness);
            dst.WriteString(SubsurfaceTextureMap, encoding, endianness);
            dst.WriteString(SubsurfaceColorTextureMap, encoding, endianness);
            dst.WriteString(EmissiveTextureMap, encoding, endianness);

            dst.WriteInt(Textures.Length, endianness);
            for (int i = 0; i < Textures.Length; i++)
            {
                Textures[i].Write(dst, encoding, endianness);
            }
        }
    }
}