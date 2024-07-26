namespace HexaEngine.Core.IO.Binary.Materials
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Metadata;
    using HexaEngine.Core.Logging;
    using Hexa.NET.Mathematics;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents data for a material, including properties, textures, shaders, and metadata.
    /// </summary>
    public class MaterialData
    {
        /// <summary>
        /// An empty instance of <see cref="MaterialData"/> with default properties.
        /// </summary>
        public static readonly MaterialData Empty = new() { Properties = [new MaterialProperty("BaseColor", MaterialPropertyType.BaseColor, Endianness.LittleEndian, new Vector4(1, 0, 1, 1))], Textures = [], Shaders = [], Metadata = Metadata.Empty };

        /// <summary>
        /// Gets or sets the name of the material.
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// Gets or sets the GUID of the material. (Do not use, unless you know what you are doing. Used for internal resource management.)
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// Gets or sets the list of material properties.
        /// </summary>
        public List<MaterialProperty> Properties;

        /// <summary>
        /// Gets or sets the list of material textures.
        /// </summary>
        public List<MaterialTexture> Textures;

        /// <summary>
        /// Gets or sets the list of material shaders.
        /// </summary>
        public List<MaterialShader> Shaders;

        /// <summary>
        /// Gets or sets the list of material shader passes.
        /// </summary>
        public List<MaterialShaderPass> Passes;

        /// <summary>
        /// Gets or sets the metadata associated with the material.
        /// </summary>
        public Metadata Metadata;

        /// <summary>
        /// Gets or sets the flags associated with the material.
        /// </summary>
        public MaterialFlags Flags = MaterialFlags.DepthTest;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialData"/> class.
        /// </summary>
        public MaterialData()
        {
            Properties = [];
            Textures = [];
            Shaders = [];
            Passes = [];
            Metadata = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialData"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the material.</param>
        /// <param name="metadata">The optional metadata associated with the material. If not provided, an empty metadata instance is used.</param>
        public MaterialData(string name, Metadata? metadata = null)
        {
            Name = name;
            Properties = [];
            Textures = [];
            Shaders = [];
            Passes = [];
            Metadata = metadata ?? new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialData"/> class with the specified name, properties, textures, shaders, and optional metadata.
        /// </summary>
        /// <param name="name">The name of the material.</param>
        /// <param name="properties">The list of material properties.</param>
        /// <param name="textures">The list of material textures.</param>
        /// <param name="shaders">The list of material shaders.</param>
        /// <param name="passes">The list of material shader passes.</param>
        /// <param name="metadata">The optional metadata associated with the material. If not provided, an empty metadata instance is used.</param>
        public MaterialData(string name, List<MaterialProperty> properties, List<MaterialTexture> textures, List<MaterialShader> shaders, List<MaterialShaderPass> passes, Metadata? metadata = null)
        {
            Name = name;
            Properties = new(properties);
            Textures = new(textures);
            Shaders = new(shaders);
            Passes = new(passes);
            Metadata = metadata ?? new();
        }

        /// <summary>
        /// Checks whether the material has a property of the specified type with a known value type.
        /// </summary>
        /// <param name="type">The type of the material property.</param>
        /// <returns><c>true</c> if the material has a property of the specified type; otherwise, <c>false</c>.</returns>
        public bool HasProperty(MaterialPropertyType type)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                var tex = Properties[i];
                if (tex.Type == type && tex.ValueType != MaterialValueType.Unknown)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the material property of the specified type.
        /// </summary>
        /// <param name="type">The type of the material property.</param>
        /// <returns>The material property of the specified type if found; otherwise, the default material property.</returns>
        public MaterialProperty GetProperty(MaterialPropertyType type)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                var prop = Properties[i];
                if (prop.Type == type)
                {
                    return prop;
                }
            }
            return default;
        }

        /// <summary>
        /// Tries to get the material property of the specified type.
        /// </summary>
        /// <param name="type">The type of the material property.</param>
        /// <param name="property">When this method returns, contains the material property of the specified type if found; otherwise, the default material property.</param>
        /// <returns><c>true</c> if the material has a property of the specified type; otherwise, <c>false</c>.</returns>
        public bool TryGetProperty(MaterialPropertyType type, out MaterialProperty property)
        {
            for (int i = 0; i < Properties.Count; i++)
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

        /// <summary>
        /// Gets the index of the material property based on the specified type.
        /// </summary>
        /// <param name="type">The type of material property.</param>
        /// <returns>
        /// The index of the material property if found; otherwise, -1.
        /// </returns>
        public int GetPropertyIndex(MaterialPropertyType type)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                var prop = Properties[i];
                if (prop.Type == type)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks whether the material has a texture of the specified type with a non-empty file path.
        /// </summary>
        /// <param name="type">The type of the material texture.</param>
        /// <returns><c>true</c> if the material has a texture of the specified type; otherwise, <c>false</c>.</returns>
        public bool HasTexture(MaterialTextureType type)
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type && tex.File != Guid.Empty)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the material texture of the specified type.
        /// </summary>
        /// <param name="type">The type of the material texture.</param>
        /// <returns>The material texture of the specified type if found; otherwise, the default material texture.</returns>
        public MaterialTexture GetTexture(MaterialTextureType type)
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type)
                {
                    return tex;
                }
            }
            return default;
        }

        /// <summary>
        /// Tries to get the material texture of the specified type.
        /// </summary>
        /// <param name="type">The type of the material texture.</param>
        /// <param name="texture">When this method returns, contains the material texture of the specified type if found; otherwise, the default material texture.</param>
        /// <returns><c>true</c> if the material has a texture of the specified type; otherwise, <c>false</c>.</returns>
        public bool TryGetTexture(MaterialTextureType type, out MaterialTexture texture)
        {
            for (int i = 0; i < Textures.Count; i++)
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

        /// <summary>
        /// Gets the index of the texture description based on the specified type.
        /// </summary>
        /// <param name="type">The type of material texture.</param>
        /// <returns>
        /// The index of the texture description if found; otherwise, -1.
        /// </returns>
        public int GetTextureIndex(MaterialTextureType type)
        {
            for (int i = 0; i < Textures.Count; i++)
            {
                var tex = Textures[i];
                if (tex.Type == type)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Checks whether the material has a shader of the specified type.
        /// </summary>
        /// <param name="type">The type of the material shader.</param>
        /// <returns><c>true</c> if the material has a shader of the specified type; otherwise, <c>false</c>.</returns>
        public bool HasShader(MaterialShaderType type)
        {
            for (int i = 0; i < Shaders.Count; i++)
            {
                var tex = Shaders[i];
                if (tex.Type == type && tex.Type != MaterialShaderType.Unknown)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the material shader of the specified type.
        /// </summary>
        /// <param name="type">The type of the material shader.</param>
        /// <returns>The material shader of the specified type if found; otherwise, the default material shader.</returns>
        public MaterialShader GetShader(MaterialShaderType type)
        {
            for (int i = 0; i < Shaders.Count; i++)
            {
                var shader = Shaders[i];
                if (shader.Type == type)
                {
                    return shader;
                }
            }
            return default;
        }

        /// <summary>
        /// Tries to get the material shader of the specified type.
        /// </summary>
        /// <param name="type">The type of the material shader.</param>
        /// <param name="shader">When this method returns, contains the material shader of the specified type if found; otherwise, the default material shader.</param>
        /// <returns><c>true</c> if the material has a shader of the specified type; otherwise, <c>false</c>.</returns>
        public bool TryGetShader(MaterialShaderType type, out MaterialShader shader)
        {
            for (int i = 0; i < Shaders.Count; i++)
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

        /// <summary>
        /// Gets the index of the material shader based on the specified type.
        /// </summary>
        /// <param name="type">The type of material shader.</param>
        /// <returns>
        /// The index of the material shader if found; otherwise, -1.
        /// </returns>
        public int GetShaderIndex(MaterialShaderType type)
        {
            for (int i = 0; i < Shaders.Count; i++)
            {
                var shd = Shaders[i];
                if (shd.Type == type)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets an array of <see cref="ShaderMacro"/> instances representing the material's properties and textures.
        /// </summary>
        /// <returns>An array of <see cref="ShaderMacro"/> instances.</returns>
        public ShaderMacro[] GetShaderMacros()
        {
            var count = Properties.Count + Textures.Count;
            ShaderMacro[] shaderMacros = new ShaderMacro[count];
            int idx = 0;
            for (int i = 0; i < Properties.Count; i++)
            {
                shaderMacros[idx++] = Properties[i].AsShaderMacro();
            }
            for (int i = 0; i < Textures.Count; i++)
            {
                shaderMacros[idx++] = Textures[i].AsShaderMacro();
            }

            return shaderMacros;
        }

        /// <summary>
        /// Reads a <see cref="MaterialData"/> instance from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="src">The <see cref="Stream"/> to read from.</param>
        /// <param name="encoding">The character encoding to use for reading strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        /// <returns>A <see cref="MaterialData"/> instance read from the stream.</returns>
        public static MaterialData Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialData material = new();
            material.Name = src.ReadString(encoding, endianness) ?? string.Empty;
            material.Guid = src.ReadGuid(endianness);

            var propertyCount = src.ReadInt32(endianness);
            material.Properties.Capacity = propertyCount;

            for (int i = 0; i < propertyCount; i++)
            {
                material.Properties.Add(MaterialProperty.Read(src, encoding, endianness));
            }

            material.Flags = (MaterialFlags)src.ReadInt32(endianness);

            var textureCount = src.ReadInt32(endianness);
            material.Textures.Capacity = textureCount;
            for (int i = 0; i < textureCount; i++)
            {
                material.Textures.Add(MaterialTexture.Read(src, encoding, endianness));
            }

            var shaderCount = src.ReadInt32(endianness);
            material.Shaders.Capacity = shaderCount;
            for (int i = 0; i < shaderCount; i++)
            {
                material.Shaders.Add(MaterialShader.Read(src, encoding, endianness));
            }

            var passCount = src.ReadInt32(endianness);
            material.Passes.Capacity = passCount;
            for (int i = 0; i < passCount; i++)
            {
                material.Passes.Add(MaterialShaderPass.Read(src, encoding, endianness));
            }

            material.Metadata = Metadata.ReadFrom(src, encoding, endianness);

            return material;
        }

        /// <summary>
        /// Writes the <see cref="MaterialData"/> instance to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="dst">The <see cref="Stream"/> to write to.</param>
        /// <param name="encoding">The character encoding to use for writing strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public virtual void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);
            dst.WriteGuid(Guid, endianness);

            dst.WriteInt32(Properties.Count, endianness);
            for (int i = 0; i < Properties.Count; i++)
            {
                Properties[i].Write(dst, encoding, endianness);
            }

            dst.WriteInt32((int)Flags, endianness);

            dst.WriteInt32(Textures.Count, endianness);
            for (int i = 0; i < Textures.Count; i++)
            {
                Textures[i].Write(dst, encoding, endianness);
            }

            dst.WriteInt32(Shaders.Count, endianness);
            for (int i = 0; i < Shaders.Count; i++)
            {
                Shaders[i].Write(dst, encoding, endianness);
            }

            dst.WriteInt32(Passes.Count, endianness);
            for (int i = 0; i < Passes.Count; i++)
            {
                Passes[i].Write(dst, encoding, endianness);
            }

            Metadata.Write(dst, encoding, endianness);
        }

        /// <summary>
        /// Deep clones a <see cref="MaterialData"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MaterialData"/> instance.</returns>
        public MaterialData Clone()
        {
            MaterialData materialData = new()
            {
                Name = (string)Name.Clone(),
                Properties = Properties.Select(x => x.Clone()).ToList(),
                Textures = Textures.Select(x => x.Clone()).ToList(),
                Shaders = Shaders.Select(x => x.Clone()).ToList(),
                Passes = Passes.Select(x => x.Clone()).ToList(),
                Metadata = Metadata.Clone(),
                Flags = Flags
            };

            return materialData;
        }

        public static MaterialData GetMaterial(AssetRef assetRef, ILogger logger)
        {
            if (assetRef == AssetRef.Empty)
            {
                return Empty;
            }
            else
            {
                Artifact? artifact = ArtifactDatabase.GetArtifact(assetRef);
                if (artifact == null)
                {
                    logger.Warn($"Failed to load material {assetRef}");
                    return Empty;
                }
                if (artifact.Type != AssetType.Material)
                {
                    logger.Warn($"Failed to load material {assetRef}, asset was {artifact.Type} but needs to be {AssetType.Material}");
                    return Empty;
                }

                Stream? stream = null;

                try
                {
                    stream = artifact.OpenRead();
                    MaterialFile materialFile = MaterialFile.Read(stream);
                    return materialFile;
                }
                catch (Exception e)
                {
                    logger.Log(e);
                    logger.Warn($"Failed to load material {assetRef}");
                    return Empty;
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }
    }
}