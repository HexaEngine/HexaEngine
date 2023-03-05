namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public class ModelSource
    {
        public readonly MeshHeader Header;
        private MeshData[] Meshes;
        public string Name;

        private ModelSource(string path)
        {
            Name = path;
            var fs = FileSystem.Open(Path.Combine(Paths.CurrentMeshesPath, path + ".model"));
            Header.Read(fs);

            Meshes = new MeshData[Header.MeshCount];
            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i] = MeshData.Read(fs, Encoding.UTF8);
            }

            fs.Close();
        }

        public ModelSource(string path, Stream fs)
        {
            Name = path;
            Header.Read(fs);

            Meshes = new MeshData[Header.MeshCount];
            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i] = MeshData.Read(fs, Encoding.UTF8);
            }

            fs.Close();
        }

        public ModelSource(string path, MeshData[] meshes)
        {
            Name = path;
            Header.Type = MeshType.Default;
            Header.MeshCount = (ulong)meshes.LongLength;
            Meshes = meshes;
        }

        public void Save(string dir)
        {
            Directory.CreateDirectory(dir);
            var fs = File.Create(Path.Combine(dir, Name + ".model"));

            Header.Write(fs);

            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                Meshes[i].Write(fs, Encoding.UTF8);
            }

            fs.Close();
        }

        public static ModelSource Load(string path)
        {
            return new ModelSource(path);
        }

        public static ModelSource LoadExternal(string path)
        {
            return new ModelSource(path, File.OpenRead(path + ".model"));
        }

        public void SetMaterial(int index, MaterialData material)
        {
            Meshes[index].Material = material;
        }

        public void SetMaterial(ulong index, MaterialData material)
        {
            Meshes[index].Material = material;
        }

        public MeshData GetMesh(int index)
        {
            return Meshes[index];
        }

        public MeshData GetMesh(ulong index)
        {
            return Meshes[index];
        }

        public MaterialData GetMaterial(int index)
        {
            return Meshes[index].Material;
        }

        public MaterialData GetMaterial(ulong index)
        {
            return Meshes[index].Material;
        }

        public Vector3[] GetPoints(int index)
        {
            var data = GetMesh(index);
            Vector3[] points = new Vector3[data.Indices.Length];
            for (int i = 0; i < data.Indices.Length; i++)
            {
                points[i] = data.Vertices[data.Indices[i]].Position;
            }
            return points;
        }

        public Vector3[] GetAllPoints()
        {
            ulong count = 0;
            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                count += GetMesh(i).IndicesCount;
            }
            Vector3[] points = new Vector3[count];
            ulong m = 0;
            for (ulong i = 0; i < Header.MeshCount; i++)
            {
                var data = GetMesh(i);
                for (int j = 0; j < data.Indices.Length; j++)
                {
                    points[m++] = data.Vertices[data.Indices[j]].Position;
                }
            }
            return points;
        }
    }
}