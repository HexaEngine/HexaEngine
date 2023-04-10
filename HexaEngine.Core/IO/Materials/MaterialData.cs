namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Text;

    public class MaterialData
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

        public MaterialTextureDesc[] Textures;

        public string? VertexShader;
        public string? HullShader;
        public string? DomainShader;
        public string? GeometryShader;
        public string? PixelShader;

        public MaterialFlags Flags = MaterialFlags.Depth;

        public MaterialData()
        {
        }

        public bool HasTexture(TextureType type)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type && !string.IsNullOrWhiteSpace(tex.File))
                    return true;
            }
            return false;
        }

        public MaterialTextureDesc GetTexture(TextureType type)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type)
                    return tex;
            }
            return default;
        }

        public ShaderMacro[] GetShaderMacros()
        {
            var ao = HasTexture(TextureType.AmbientOcclusion);
            var rm = HasTexture(TextureType.RoughnessMetalness);
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

                new ShaderMacro("HasBaseColorTex", (HasTexture(TextureType.BaseColor) || HasTexture(TextureType.Diffuse)).ToHLSL()),
                new ShaderMacro("HasNormalTex",  HasTexture(TextureType.Normals).ToHLSL()),
                new ShaderMacro("HasHeightTex", HasTexture(TextureType.Height).ToHLSL()),
                new ShaderMacro("HasMetalnessTex", HasTexture(TextureType.Metalness).ToHLSL()),
                new ShaderMacro("HasRoughnessTex", HasTexture(TextureType.Roughness).ToHLSL()),
                new ShaderMacro("HasEmissiveTex", HasTexture(TextureType.Emissive).ToHLSL()),
                new ShaderMacro("HasAmbientOcclusionTex", ao.ToHLSL()),
                new ShaderMacro("HasRoughnessMetalnessTex", rm.ToHLSL()),
            };
            return macros;
        }

        public static MaterialData Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialData material = new();
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
            material.Flags = (MaterialFlags)src.ReadInt(endianness);

            var textureCount = src.ReadInt(endianness);
            material.Textures = new MaterialTextureDesc[textureCount];
            for (int i = 0; i < textureCount; i++)
            {
                material.Textures[i] = MaterialTextureDesc.Read(src, encoding, endianness);
            }

            material.VertexShader = src.ReadString(encoding, endianness);
            material.HullShader = src.ReadString(encoding, endianness);
            material.DomainShader = src.ReadString(encoding, endianness);
            material.GeometryShader = src.ReadString(encoding, endianness);
            material.PixelShader = src.ReadString(encoding, endianness);

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
            dst.WriteInt((int)Flags, endianness);

            dst.WriteInt(Textures.Length, endianness);
            for (int i = 0; i < Textures.Length; i++)
            {
                Textures[i].Write(dst, encoding, endianness);
            }

            dst.WriteString(VertexShader, encoding, endianness);
            dst.WriteString(HullShader, encoding, endianness);
            dst.WriteString(DomainShader, encoding, endianness);
            dst.WriteString(GeometryShader, encoding, endianness);
            dst.WriteString(PixelShader, encoding, endianness);
        }
    }
}