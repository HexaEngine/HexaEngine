namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Mathematics;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class MaterialLibrary
    {
        public static readonly MaterialLibrary Empty = new(string.Empty) { Header = default, Materials = Array.Empty<MaterialData>() };

        public string Name;

        public MaterialLibraryHeader Header;
        public MaterialData[] Materials;

        public MaterialLibrary()
        {
            Name = string.Empty;
            Header = default;
            Materials = Array.Empty<MaterialData>();
        }

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

        public void Save(string path, Encoding encoding, Endianness endianness = Endianness.LittleEndian, Compression compression = Compression.None)
        {
            Stream fs = File.Create(path);

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

        public static MaterialLibrary Load(string path)
        {
            return new MaterialLibrary(path);
        }

        public static MaterialLibrary LoadExternal(string path)
        {
            return new MaterialLibrary(path, File.OpenRead(path));
        }

        public MaterialData GetMaterial(string name)
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                if (Materials[i].Name == name)
                {
                    return Materials[i];
                }
            }

            Trace.WriteLine($"Warning couldn't find material {name} in library {Name}");

            return MaterialData.Empty;
        }
    }
}