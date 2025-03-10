﻿namespace HexaEngine.Core.IO.Binary.Materials
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Metadata;
    using System.Text;

    /// <summary>
    /// Represents data for a material, including properties, textures, shaders, and metadata.
    /// </summary>
    public class MaterialFile : MaterialData
    {
        private Artifact? associatedArtifact;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFile"/> class.
        /// </summary>
        public MaterialFile()
        {
            Properties = [];
            Textures = [];
            Shaders = [];
            Metadata = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFile"/> class from a <see cref="MaterialData"/>.
        /// </summary>
        public MaterialFile(MaterialData material) : this(material.Name, material.Guid, material.Properties, material.Textures, material.Shaders, material.Metadata.Clone())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialData"/> class from another MaterialData.
        /// </summary>
        /// <param name="name">The name of the material.</param>
        /// <param name="guid">The GUID of the material.</param>
        /// <param name="material">The source material.</param>
        public MaterialFile(string name, Guid guid, MaterialData material)
        {
            Name = name;
            Guid = guid;
            Properties = material.Properties.Select(x => x.Clone()).ToList();
            Textures = material.Textures.Select(x => x.Clone()).ToList();
            Shaders = material.Shaders.Select(x => x.Clone()).ToList();
            Metadata = material.Metadata.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFile"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the material.</param>
        /// <param name="guid">The GUID of the material.</param>
        /// <param name="metadata">The optional metadata associated with the material. If not provided, an empty metadata instance is used.</param>
        public MaterialFile(string name, Guid guid, Metadata? metadata = null)
        {
            Name = name;
            Guid = guid;
            Properties = [];
            Textures = [];
            Shaders = [];
            Metadata = metadata ?? new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialFile"/> class with the specified name, properties, textures, shaders, and optional metadata.
        /// </summary>
        /// <param name="name">The name of the material.</param>
        /// <param name="guid">The GUID of the material.</param>
        /// <param name="properties">The list of material properties.</param>
        /// <param name="textures">The list of material textures.</param>
        /// <param name="shaders">The list of material shaders.</param>
        /// <param name="metadata">The optional metadata associated with the material. If not provided, an empty metadata instance is used.</param>
        public MaterialFile(string name, Guid guid, List<MaterialProperty> properties, List<MaterialTexture> textures, List<MaterialShader> shaders, Metadata? metadata = null)
        {
            Name = name;
            Guid = guid;
            Properties = [.. properties];
            Textures = [.. textures];
            Shaders = [.. shaders];
            Metadata = metadata ?? new();
        }

        public static MaterialFile ReadFrom(Artifact artifact)
        {
            MaterialFile material = new();
            using var stream = artifact.OpenRead();
            material.Read(stream);
            material.Associate(artifact);
            return material;
        }

        private void Associate(Artifact? artifact)
        {
            if (associatedArtifact == artifact) return;

            if (associatedArtifact != null)
            {
                associatedArtifact.Changed -= ArtifactChanged;
            }

            if (artifact != null)
            {
                artifact.Changed += ArtifactChanged;
            }

            associatedArtifact = artifact;
        }

        private void ArtifactChanged(object? sender, ArtifactChangedArgs e)
        {
            if (e.Type == ArtifactChangeType.Updated)
            {
                try
                {
                    var artifact = e.Artifact;
                    using var stream = artifact.OpenRead();
                    Read(stream);
                    updatedEventHandlers?.Invoke(this, this);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Reads a <see cref="MaterialFile"/> instance from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="src">The <see cref="Stream"/> to read from.</param>
        /// <returns>A <see cref="MaterialFile"/> instance read from the stream.</returns>
        public static MaterialFile ReadFrom(Stream src)
        {
            MaterialFile material = new();
            material.Read(src);
            return material;
        }

        public void Read(Stream src)
        {
            MaterialFileHeader header = default;
            header.Read(src, out Version version);
            Encoding encoding = header.Encoding ?? Encoding.UTF8;
            Endianness endianness = header.Endianness;
            Name = src.ReadString(encoding, endianness) ?? string.Empty;
            Guid = src.ReadGuid(endianness);

            var propertyCount = src.ReadInt32(endianness);
            Properties.Clear();
            Properties.Capacity = propertyCount;
            for (int i = 0; i < propertyCount; i++)
            {
                Properties.Add(MaterialProperty.Read(src, encoding, endianness));
            }

            Flags = (MaterialFlags)src.ReadInt32(endianness);

            var textureVersion = version >= new Version(2, 0, 0, 1) ? new Version(2, 0, 0, 0) : new Version(1, 0, 0, 0);

            var textureCount = src.ReadInt32(endianness);
            Textures.Clear();
            Textures.Capacity = textureCount;
            for (int i = 0; i < textureCount; i++)
            {
                Textures.Add(MaterialTexture.Read(src, encoding, endianness, textureVersion));
            }

            var shaderCount = src.ReadInt32(endianness);
            Shaders.Clear();
            Shaders.Capacity = shaderCount;
            for (int i = 0; i < shaderCount; i++)
            {
                Shaders.Add(MaterialShader.Read(src, encoding, endianness));
            }

            var passCount = src.ReadInt32(endianness);
            Passes.Clear();
            Passes.Capacity = passCount;
            for (int i = 0; i < passCount; i++)
            {
                Passes.Add(MaterialShaderPass.Read(src, encoding, endianness));
            }

            Metadata = Metadata.ReadFrom(src, encoding, endianness);
        }

        /// <summary>
        /// Writes the <see cref="MaterialFile"/> instance to the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to write the <see cref="MaterialFile"/> to.</param>
        /// <param name="encoding">The character encoding to use for writing strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian)
        {
            FileStream? stream = null;
            try
            {
                stream = File.Create(path);
                Write(stream, encoding, endianness);
            }
            finally
            {
                stream?.Dispose();
            }
        }

        /// <summary>
        /// Writes the <see cref="MaterialFile"/> instance to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="dst">The <see cref="Stream"/> to write to.</param>
        /// <param name="encoding">The character encoding to use for writing strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public override void Write(Stream dst, Encoding encoding, Endianness endianness = Endianness.LittleEndian)
        {
            MaterialFileHeader header = new()
            {
                Encoding = encoding,
                Endianness = endianness
            };
            header.Write(dst);
            base.Write(dst, encoding, endianness);
        }

        /// <summary>
        /// Deep clones a <see cref="MaterialFile"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MaterialFile"/> instance.</returns>
        public new MaterialFile Clone()
        {
            MaterialFile materialData = new()
            {
                Name = (string)Name.Clone(),
                Properties = Properties.Select(x => x.Clone()).ToList(),
                Textures = Textures.Select(x => x.Clone()).ToList(),
                Shaders = Shaders.Select(x => x.Clone()).ToList(),
                Metadata = Metadata.Clone(),
                Flags = Flags
            };
            return materialData;
        }

        public void SetPropertyBool(MaterialPropertyType type, bool value)
        {
            int index = GetPropertyIndex(type);
            if (index == -1)
            {
                MaterialProperty property = new(type.ToString(), type, MaterialValueType.Bool, Endianness.LittleEndian);
                property.SetBool(value);
                Properties.Add(property);
                return;
            }
            Properties[index].SetBool(value);
        }

        public void SetPropertyFloat(MaterialPropertyType type, float value)
        {
            int index = GetPropertyIndex(type);
            if (index == -1)
            {
                MaterialProperty property = new(type.ToString(), type, MaterialValueType.Float, Endianness.LittleEndian);
                Properties.Add(property);
                return;
            }
            Properties[index].SetFloat(value);
        }
    }
}