namespace HexaEngine.Core.IO.Binary.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using K4os.Compression.LZ4;
    using K4os.Compression.LZ4.Streams;
    using System.IO;
    using System.IO.Compression;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Represents a 3D model file containing header information, mesh data, and a scene hierarchy.
    /// </summary>
    public unsafe class ModelFile
    {
        private ModelHeader header;
        private Node root;
        private readonly List<MeshData> meshes;

        /// <summary>
        /// Default constructor for creating an empty model file.
        /// </summary>
        public ModelFile()
        {
            header = default;

            root = new("ROOT", Matrix4x4.Identity, NodeFlags.None, null);
            meshes = new();
        }

        /// <summary>
        /// Constructor for creating a model file with specified material library, meshes, and root node.
        /// </summary>
        /// <param name="materialLibrary">The name of the material library associated with the model.</param>
        /// <param name="meshes">The list of mesh data in the model.</param>
        /// <param name="root">The root node of the model's scene hierarchy.</param>
        public ModelFile(string materialLibrary, IList<MeshData> meshes, Node root)
        {
            header.MaterialLibrary = materialLibrary;
            header.MeshCount = (ulong)meshes.Count;
            this.meshes = new(meshes);
            this.root = root;
        }

        /// <summary>
        /// Gets the header information of the model file.
        /// </summary>
        public ModelHeader Header => header;

        /// <summary>
        /// Gets or sets the material library associated with the model.
        /// </summary>
        public string MaterialLibrary { get => header.MaterialLibrary; set => header.MaterialLibrary = value; }

        /// <summary>
        /// Gets the list of mesh data in the model.
        /// </summary>
        public List<MeshData> Meshes => meshes;

        /// <summary>
        /// Gets the root node of the model's scene hierarchy.
        /// </summary>
        public Node Root => root;

        /// <summary>
        /// Saves the model file to a specified path with the given encoding, endianness, and compression.
        /// </summary>
        /// <param name="path">The path where the model file will be saved.</param>
        /// <param name="encoding">The encoding used for text data in the model file.</param>
        /// <param name="endianness">The endianness of the binary data in the model file.</param>
        /// <param name="compression">The compression method used for the model data.</param>
        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.LZ4)
        {
            Stream fs = File.Create(path);

            header.Encoding = encoding;
            header.Endianness = endianness;
            header.Compression = compression;
            header.Write(fs);

            var stream = fs;
            if (compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionLevel.SmallestSize, true);
            }

            if (compression == Compression.LZ4)
            {
                stream = LZ4Stream.Encode(fs, LZ4Level.L12_MAX, 0, true);
            }

            for (int i = 0; i < (int)header.MeshCount; i++)
            {
                meshes[i].Write(stream, header.Encoding, header.Endianness);
            }

            root.Write(stream, header.Encoding, header.Endianness);

            stream.Close();
            fs.Close();
        }

        /// <summary>
        /// Loads a model file from the specified path.
        /// </summary>
        /// <param name="path">The path from which the model file will be loaded.</param>
        /// <returns>The loaded model file.</returns>
        public static ModelFile Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        /// <summary>
        /// Loads a model file from the specified path.
        /// </summary>
        /// <param name="path">The path from which the model file will be loaded.</param>
        /// <returns>The loaded model file.</returns>
        public static ModelFile LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        /// <summary>
        /// Loads a model file from the specified stream.
        /// </summary>
        /// <param name="fs">The stream from which the model file will be loaded.</param>
        /// <returns>The loaded model file.</returns>
        public static ModelFile Load(Stream fs)
        {
            ModelFile model = new();
            model.header.Read(fs);

            model.meshes.Clear();
            model.meshes.Capacity = (int)model.header.MeshCount;

            var stream = fs;
            if (model.header.Compression == Compression.Deflate)
            {
                stream = new DeflateStream(fs, CompressionMode.Decompress, true);
            }

            if (model.header.Compression == Compression.LZ4)
            {
                stream = LZ4Stream.Decode(fs, 0, true);
            }

            for (int i = 0; i < (int)model.header.MeshCount; i++)
            {
                model.meshes.Add(MeshData.Read(stream, model.Header.Encoding, model.header.Endianness));
            }

            model.root = Node.ReadFrom(stream, model.Header.Encoding, model.header.Endianness);

            stream.Close();
            fs.Close();

            return model;
        }

        /// <summary>
        /// Gets the mesh data at the specified index in the model.
        /// </summary>
        /// <param name="index">The index of the mesh in the model.</param>
        /// <returns>The mesh data at the specified index.</returns>
        public MeshData GetMesh(int index)
        {
            return meshes[index];
        }

        /// <summary>
        /// Gets the points (positions) of a mesh in the model at the specified index.
        /// </summary>
        /// <param name="index">The index of the mesh in the model.</param>
        /// <returns>The array of points representing the positions in the mesh.</returns>
        public Vector3[] GetPoints(int index)
        {
            var data = GetMesh(index);
            Vector3[] points = new Vector3[data.Indices.Length];
            for (int i = 0; i < data.Indices.Length; i++)
            {
                points[i] = data.Positions[data.Indices[i]];
            }
            return points;
        }

        /// <summary>
        /// Gets all points (positions) from all meshes in the model.
        /// </summary>
        /// <returns>The array of points representing positions from all meshes in the model.</returns>
        public Vector3[] GetAllPoints()
        {
            ulong count = 0;
            for (int i = 0; i < (int)header.MeshCount; i++)
            {
                count += GetMesh(i).IndicesCount;
            }
            Vector3[] points = new Vector3[count];
            ulong m = 0;
            for (int i = 0; i < (int)header.MeshCount; i++)
            {
                var data = GetMesh(i);
                for (int j = 0; j < data.Indices.Length; j++)
                {
                    points[m++] = data.Positions[data.Indices[j]];
                }
            }
            return points;
        }

        /// <summary>
        /// Removes bone information from all meshes in the model.
        /// </summary>
        public void Debone()
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].Debone();
            }
        }
    }
}