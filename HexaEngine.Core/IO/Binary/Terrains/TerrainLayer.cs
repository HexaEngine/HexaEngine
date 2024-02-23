namespace HexaEngine.Core.IO.Binary.Terrains
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Mathematics;
    using System.Text;
    using YamlDotNet.Core.Tokens;

    /// <summary>
    /// Represents a layer in a terrain.
    /// </summary>
    public class TerrainLayer
    {
        private string name;
        private AssetRef material;
        private MaterialData? data;

        /// <summary>
        /// The name of the terrain layer.
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// The material associated with the terrain layer.
        /// </summary>
        public AssetRef Material
        {
            get => material;
            set
            {
                data = MaterialData.GetMaterial(value);
                material = value;
            }
        }

        /// <summary>
        /// The material data associated with the terrain layer.
        /// </summary>
        public MaterialData? Data
        {
            get
            {
                return data ??= MaterialData.GetMaterial(material);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainLayer"/> class with specified parameters.
        /// </summary>
        /// <param name="name">The name of the terrain layer.</param>
        /// <param name="material">The material associated with the terrain layer.</param>
        public TerrainLayer(string name, AssetRef material)
        {
            this.name = name;
            this.material = material;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainLayer"/> class with specified parameters.
        /// </summary>
        /// <param name="name">The name of the terrain layer.</param>
        public TerrainLayer(string name)
        {
            this.name = name;
            material = Guid.Empty;
        }

        /// <summary>
        /// Writes the terrain layer data to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="encoding">The encoding used for string data.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(name, encoding, endianness);
            stream.WriteGuid(material, endianness);
        }

        /// <summary>
        /// Reads terrain layer data from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding used for string data.</param>
        /// <param name="endianness">The endianness used for binary data.</param>
        /// <returns>A new instance of <see cref="TerrainLayer"/> containing the read data.</returns>
        public static TerrainLayer Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            string name = stream.ReadString(encoding, endianness) ?? string.Empty;
            Guid material = stream.ReadGuid(endianness);
            return new TerrainLayer(name, material);
        }
    }
}