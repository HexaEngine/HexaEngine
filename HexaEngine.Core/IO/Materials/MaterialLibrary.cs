namespace HexaEngine.Core.IO.Materials
{
    using System.Text;

    public class MaterialLibrary
    {
        private MaterialLibraryHeader header;
        public MaterialData[] Materials;

        public MaterialLibrary(string path)
        {
            var fs = FileSystem.Open(path);
            header.Read(fs);
            Materials = new MaterialData[header.MaterialCount];
            Encoding encoding = Encoding.UTF8;
            for (int i = 0; i < header.MaterialCount; i++)
            {
                Materials[i] = MaterialData.Read(fs, encoding, header.Endianness);
            }
        }

        public MaterialLibrary(Stream stream)
        {
            header.Read(stream);
            Materials = new MaterialData[header.MaterialCount];
        }

        public MaterialLibrary(MaterialData[] materials)
        {
            header.MaterialCount = (uint)materials.LongLength;
            Materials = materials;
        }

        public void Write(string path)
        {
            var fs = File.Create(path);
            Write(fs);
            fs.Close();
        }

        public void Write(Stream stream)
        {
            header.MaterialCount = (uint)Materials.LongLength;
            header.Write(stream);

            Encoding encoding = Encoding.UTF8;
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i].Write(stream, encoding, header.Endianness);
            }
        }
    }
}