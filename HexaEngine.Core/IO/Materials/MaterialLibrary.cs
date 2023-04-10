namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Mathematics;
    using System.Text;

    public class MaterialLibrary
    {
        public string Name;

        public MaterialLibraryHeader Header;
        public MaterialData[] Materials;

        public MaterialLibrary(string path)
        {
            Name = path;
            var fs = FileSystem.Open(path);
            Header.Read(fs);

            var stream = fs;

            Materials = new MaterialData[Header.MaterialCount];
            for (int i = 0; i < Header.MaterialCount; i++)
            {
                Materials[i] = MaterialData.Read(stream, Header.Encoding, Header.Endianness);
            }
            fs.Close();
        }

        public MaterialLibrary(string path, Stream fs)
        {
            Name = path;
            Header.Read(fs);

            var stream = fs;

            Materials = new MaterialData[Header.MaterialCount];
            for (int i = 0; i < Header.MaterialCount; i++)
            {
                Materials[i] = MaterialData.Read(stream, Header.Encoding, Header.Endianness);
            }
        }

        public MaterialLibrary(string name, MaterialData[] materials)
        {
            Name = name;
            Header.MaterialCount = (uint)materials.LongLength;
            Materials = materials;
        }

        public void Save(string dir, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.None)
        {
            Directory.CreateDirectory(dir);
            Stream fs = File.Create(Path.Combine(dir, Path.GetFileNameWithoutExtension(Name) + ".matlib"));

            var stream = fs;

            Header.Encoding = encoding;
            Header.Endianness = endianness;
            Header.Compression = compression;
            Header.MaterialCount = (uint)Materials.LongLength;
            Header.Write(stream);

            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i].Write(stream, Header.Encoding, Header.Endianness);
            }

            fs.Close();
        }
    }
}