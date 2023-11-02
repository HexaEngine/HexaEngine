namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public class MaterialLibrary
    {
        public static readonly MaterialLibrary Empty = new();

        private MaterialLibraryHeader header;
        private readonly List<MaterialData> materials;

        public MaterialLibrary()
        {
            header = default;
            materials = new();
        }

        public MaterialLibrary(IList<MaterialData> materials)
        {
            header.MaterialCount = (uint)materials.Count;
            this.materials = new(materials);
        }

        public MaterialLibraryHeader Header => header;

        public List<MaterialData> Materials => materials;

        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.None)
        {
            Stream fs = File.Create(path);

            var stream = fs;

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

        public static MaterialLibrary Load(string path)
        {
            return Load(FileSystem.OpenRead(path));
        }

        public static MaterialLibrary LoadExternal(string path)
        {
            return Load(File.OpenRead(path));
        }

        public static MaterialLibrary Load(Stream fs)
        {
            MaterialLibrary library = new();

            library.header.Read(fs);

            var stream = fs;

            library.materials.Clear();
            library.materials.Capacity = (int)library.header.MaterialCount;

            for (int i = 0; i < library.header.MaterialCount; i++)
            {
                library.materials.Add(MaterialData.Read(stream, library.header.Encoding, library.header.Endianness));
            }

            stream.Close();
            fs.Close();

            return library;
        }

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