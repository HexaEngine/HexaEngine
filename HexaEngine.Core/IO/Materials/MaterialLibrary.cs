namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a collection of materials with associated metadata.
    /// </summary>
    public class MaterialLibrary
    {
        /// <summary>
        /// An empty material library.
        /// </summary>
        public static readonly MaterialLibrary Empty = new();

        private readonly List<MaterialData> materials;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialLibrary"/> class.
        /// </summary>
        public MaterialLibrary()
        {
            materials = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialLibrary"/> class with the specified materials.
        /// </summary>
        /// <param name="materials">The list of materials to include in the library.</param>
        public MaterialLibrary(IList<MaterialData> materials)
        {
            this.materials = new(materials);
        }

        /// <summary>
        /// Gets the list of materials contained in the library.
        /// </summary>
        public List<MaterialData> Materials => materials;

        /// <summary>
        /// Saves the material library to a file at the specified path.
        /// </summary>
        /// <param name="path">The path to save the material library file.</param>
        /// <param name="encoding">The encoding to use for strings in the material library.</param>
        /// <param name="endianness">The endianness of the material library file.</param>
        /// <param name="compression">The compression method used for the material library data.</param>
        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.None)
        {
            Stream fs = File.Create(path);

            var stream = fs;

            MaterialLibraryHeader header;
            header.Encoding = encoding;
            header.Endianness = endianness;
            header.Compression = compression;
            header.MaterialCount = (uint)materials.Count;
            header.Write(stream);

            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].Write(stream, header.Encoding, header.Endianness);
            }

            fs.Close();
        }

        /// <summary>
        /// Loads a material library from the specified file path.
        /// </summary>
        /// <param name="path">The path to the material library file.</param>
        /// <returns>A new instance of the <see cref="MaterialLibrary"/> class loaded from the file.</returns>
        public static MaterialLibrary Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Loads a material library from the specified file path using the File class.
        /// </summary>
        /// <param name="path">The path to the material library file.</param>
        /// <returns>A new instance of the <see cref="MaterialLibrary"/> class loaded from the file.</returns>
        public static MaterialLibrary LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        /// <summary>
        /// Loads a material library from the specified stream.
        /// </summary>
        /// <param name="fs">The stream containing the material library data.</param>
        /// <returns>A new instance of the <see cref="MaterialLibrary"/> class loaded from the stream.</returns>
        public static MaterialLibrary Load(Stream fs)
        {
            MaterialLibrary library = new();

            MaterialLibraryHeader header = default;
            header.Read(fs);

            var stream = fs;

            library.materials.Clear();
            library.materials.Capacity = (int)header.MaterialCount;

            for (int i = 0; i < header.MaterialCount; i++)
            {
                library.materials.Add(MaterialData.Read(stream, header.Encoding, header.Endianness));
            }

            stream.Close();
            fs.Close();

            return library;
        }

        /// <summary>
        /// Gets the material with the specified name from the library.
        /// </summary>
        /// <param name="name">The name of the material to retrieve.</param>
        /// <returns>The material with the specified name or an empty material if not found.</returns>
        public MaterialData GetMaterial(string name)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].Name == name)
                {
                    return materials[i];
                }
            }

            Logger.Warn($"Warning couldn't find material {name} in library");

            return MaterialData.Empty;
        }
    }
}