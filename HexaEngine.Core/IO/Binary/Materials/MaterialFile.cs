namespace HexaEngine.Core.IO.Binary.Materials
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Metadata;
    using HexaEngine.Mathematics;
    using System.Text;

    /// <summary>
    /// Represents data for a material, including properties, textures, shaders, and metadata.
    /// </summary>
    public class MaterialFile : MaterialData
    {
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
            Properties = new(properties);
            Textures = new(textures);
            Shaders = new(shaders);
            Metadata = metadata ?? new();
        }

        /// <summary>
        /// Reads a <see cref="MaterialFile"/> instance from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="src">The <see cref="Stream"/> to read from.</param>
        /// <returns>A <see cref="MaterialFile"/> instance read from the stream.</returns>
        public static MaterialFile Read(Stream src)
        {
            MaterialFileHeader header = default;
            header.Read(src);
            Encoding encoding = header.Encoding ?? Encoding.UTF8;
            Endianness endianness = header.Endianness;

            MaterialFile material = new();
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

            material.Metadata = Metadata.ReadFrom(src, encoding, endianness);

            return material;
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

            Metadata.Write(dst, encoding, endianness);
        }

        /// <summary>
        /// Deep clones a <see cref="MaterialFile"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="MaterialFile"/> instance.</returns>
        public MaterialFile Clone()
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
    }
}