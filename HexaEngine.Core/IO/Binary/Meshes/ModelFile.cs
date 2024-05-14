namespace HexaEngine.Core.IO.Binary.Meshes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.IO;
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
            meshes = [];
        }

        /// <summary>
        /// Constructor for creating a model file with specified material library, meshes, and root node.
        /// </summary>
        /// <param name="meshes">The list of mesh data in the model.</param>
        /// <param name="root">The root node of the model's scene hierarchy.</param>
        public ModelFile(IList<MeshData> meshes, Node root)
        {
            header.MeshCount = (ulong)meshes.Count;
            this.meshes = new(meshes);
            this.root = root;
        }

        /// <summary>
        /// Gets the header information of the model file.
        /// </summary>
        public ModelHeader Header => header;

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

            root.Write(fs, header.Encoding, header.Endianness);

            for (int i = 0; i < (int)header.MeshCount; i++)
            {
                meshes[i].Write(fs, header.Encoding, header.Endianness, compression);
            }

            fs.Close();
        }

        /// <summary>
        /// Loads a model file from the specified path.
        /// </summary>
        /// <param name="path">The path from which the model file will be loaded.</param>
        /// <param name="loadMode"></param>
        /// <param name="stream"></param>
        /// <returns>The loaded model file.</returns>
        public static ModelFile Load(string path, MeshLoadMode loadMode)
        {
            using var fs = File.OpenRead(path);
            try
            {
                return Load(fs, loadMode);
            }
            finally
            {
                fs.Dispose();
            }
        }

        /// <summary>
        /// Loads a model file from the specified stream.
        /// </summary>
        /// <param name="fs">The stream from which the model file will be loaded.</param>
        /// <param name="loadMode"></param>
        /// <returns>The loaded model file.</returns>
        public static ModelFile Load(Stream fs, MeshLoadMode loadMode)
        {
            ModelFile model = new();

            model.header.Read(fs);

            model.meshes.Clear();
            model.meshes.Capacity = (int)model.header.MeshCount;

            model.root = Node.ReadFrom(fs, model.Header.Encoding, model.header.Endianness);

            for (int i = 0; i < (int)model.header.MeshCount; i++)
            {
                model.meshes.Add(MeshData.Read(fs, model.Header.Encoding, model.header.Endianness, model.header.Compression, loadMode));
            }

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