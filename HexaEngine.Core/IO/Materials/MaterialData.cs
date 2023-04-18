namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Text;

    public class MaterialData
    {
        public string Name = string.Empty;

        public MaterialProperty[] Properties;
        public MaterialTexture[] Textures;

        public string? VertexShader;
        public string? HullShader;
        public string? DomainShader;
        public string? GeometryShader;
        public string? PixelShader;
        public string? ComputeShader;

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

        public MaterialProperty GetProperty(MaterialPropertyType type)
        {
            for (int i = 0; i < Properties.Length; i++)
            {
                var prop = Properties[i];
                if (prop.Type == type)
                    return prop;
            }
            return default;
        }

        public bool TryGetProperty(MaterialPropertyType type, out MaterialProperty property)
        {
            for (int i = 0; i < Properties.Length; i++)
            {
                var prop = Properties[i];
                if (prop.Type == type)
                {
                    property = prop;
                    return true;
                }
            }
            property = default;
            return false;
        }

        public MaterialTexture GetTexture(TextureType type)
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
            var count = Properties.Length + Textures.Length;
            ShaderMacro[] shaderMacros = new ShaderMacro[count];
            int idx = 0;
            for (int i = 0; i < Properties.Length; i++)
            {
                shaderMacros[idx++] = Properties[i].AsShaderMacro();
            }
            for (int i = 0; i < Textures.Length; i++)
            {
                shaderMacros[idx++] = Textures[i].AsShaderMacro();
            }

            return shaderMacros;
        }

        public static MaterialData Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialData material = new();
            material.Name = src.ReadString(encoding, endianness);

            var propertyCount = src.ReadInt(endianness);
            material.Properties = new MaterialProperty[propertyCount];

            for (int i = 0; i < propertyCount; i++)
            {
                material.Properties[i].Read(src, encoding, endianness);
            }

            material.Flags = (MaterialFlags)src.ReadInt(endianness);

            var textureCount = src.ReadInt(endianness);
            material.Textures = new MaterialTexture[textureCount];
            for (int i = 0; i < textureCount; i++)
            {
                material.Textures[i] = MaterialTexture.Read(src, encoding, endianness);
            }

            material.VertexShader = src.ReadString(encoding, endianness);
            material.HullShader = src.ReadString(encoding, endianness);
            material.DomainShader = src.ReadString(encoding, endianness);
            material.GeometryShader = src.ReadString(encoding, endianness);
            material.PixelShader = src.ReadString(encoding, endianness);
            material.ComputeShader = src.ReadString(encoding, endianness);

            return material;
        }

        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);

            dst.WriteInt(Properties.Length, endianness);
            for (int i = 0; i < Properties.Length; i++)
            {
                Properties[i].Write(dst, encoding, endianness);
            }

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
            dst.WriteString(ComputeShader, encoding, endianness);
        }
    }
}