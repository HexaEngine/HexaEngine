namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Metadata;
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Text;

    public class MaterialData
    {
        public static readonly MaterialData Empty = new() { Properties = new MaterialProperty[] { new MaterialProperty("BaseColor", MaterialPropertyType.BaseColor, Endianness.LittleEndian, new Vector4(1, 0, 1, 1)) }, Textures = Array.Empty<MaterialTexture>(), Shaders = Array.Empty<MaterialShader>(), Metadata = Metadata.Empty };

        public string Name = string.Empty;

        public MaterialProperty[] Properties;
        public MaterialTexture[] Textures;
        public MaterialShader[] Shaders;

        public Metadata Metadata;

        public MaterialFlags Flags = MaterialFlags.Depth;

        public MaterialData()
        {
            Properties = Array.Empty<MaterialProperty>();
            Textures = Array.Empty<MaterialTexture>();
            Shaders = Array.Empty<MaterialShader>();
            Metadata = new();
        }

        public MaterialData(string name)
        {
            Name = name;
            Properties = Array.Empty<MaterialProperty>();
            Textures = Array.Empty<MaterialTexture>();
            Shaders = Array.Empty<MaterialShader>();
            Metadata = new();
        }

        public bool HasProperty(MaterialPropertyType type)
        {
            for (int i = 0; i < Properties.Length; i++)
            {
                var tex = Properties[i];
                if (tex.Type == type && tex.ValueType != MaterialValueType.Unknown)
                {
                    return true;
                }
            }
            return false;
        }

        public MaterialProperty GetProperty(MaterialPropertyType type)
        {
            for (int i = 0; i < Properties.Length; i++)
            {
                var prop = Properties[i];
                if (prop.Type == type)
                {
                    return prop;
                }
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

        public bool HasTexture(MaterialTextureType type)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type && !string.IsNullOrWhiteSpace(tex.File))
                {
                    return true;
                }
            }
            return false;
        }

        public MaterialTexture GetTexture(MaterialTextureType type)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type)
                {
                    return tex;
                }
            }
            return default;
        }

        public bool TryGetTexture(MaterialTextureType type, out MaterialTexture texture)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type)
                {
                    texture = tex;
                    return true;
                }
            }
            texture = default;
            return false;
        }

        public bool HasShader(MaterialShaderType type)
        {
            for (int i = 0; i < Shaders.Length; i++)
            {
                var tex = Shaders[i];
                if (tex.Type == type && tex.Type != MaterialShaderType.Unknown)
                {
                    return true;
                }
            }
            return false;
        }

        public MaterialShader GetShader(MaterialShaderType type)
        {
            for (int i = 0; i < Shaders.Length; i++)
            {
                var shader = Shaders[i];
                if (shader.Type == type)
                {
                    return shader;
                }
            }
            return default;
        }

        public bool TryGetShader(MaterialShaderType type, out MaterialShader shader)
        {
            for (int i = 0; i < Shaders.Length; i++)
            {
                var shd = Shaders[i];
                if (shd.Type == type)
                {
                    shader = shd;
                    return true;
                }
            }
            shader = default;
            return false;
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

            var propertyCount = src.ReadInt32(endianness);
            material.Properties = new MaterialProperty[propertyCount];

            for (int i = 0; i < propertyCount; i++)
            {
                material.Properties[i].Read(src, encoding, endianness);
            }

            material.Flags = (MaterialFlags)src.ReadInt32(endianness);

            var textureCount = src.ReadInt32(endianness);
            material.Textures = new MaterialTexture[textureCount];
            for (int i = 0; i < textureCount; i++)
            {
                material.Textures[i] = MaterialTexture.Read(src, encoding, endianness);
            }

            var shaderCount = src.ReadInt32(endianness);
            material.Shaders = new MaterialShader[shaderCount];
            for (int i = 0; i < shaderCount; i++)
            {
                material.Shaders[i] = MaterialShader.Read(src, encoding, endianness);
            }

            material.Metadata = Metadata.ReadFrom(src, encoding, endianness);

            return material;
        }

        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);

            dst.WriteInt32(Properties.Length, endianness);
            for (int i = 0; i < Properties.Length; i++)
            {
                Properties[i].Write(dst, encoding, endianness);
            }

            dst.WriteInt32((int)Flags, endianness);

            dst.WriteInt32(Textures.Length, endianness);
            for (int i = 0; i < Textures.Length; i++)
            {
                Textures[i].Write(dst, encoding, endianness);
            }

            dst.WriteInt32(Shaders.Length, endianness);
            for (int i = 0; i < Shaders.Length; i++)
            {
                Shaders[i].Write(dst, encoding, endianness);
            }

            Metadata.Write(dst, encoding, endianness);
        }
    }
}