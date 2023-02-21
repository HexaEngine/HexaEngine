namespace HexaEngine.Core.IO.Meshes
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Numerics;
    using System.Text;

    public class MeshSource
    {
        public readonly MeshHeader Header;
        private MaterialData Material;
        private MeshData Data;
        public string Name;

        private MeshSource(string path)
        {
            Name = path;
            var fs = FileSystem.Open(Path.Combine(Paths.CurrentMeshesPath, path + ".mesh"));
            Header.Read(fs, Encoding.UTF8);
            Material = MaterialData.Read(fs, Encoding.UTF8);
            Data = MeshData.Read(Header, fs, Encoding.UTF8);
            fs.Close();
        }

        public MeshSource(string path, Stream fs)
        {
            Name = path;
            Header.Read(fs, Encoding.UTF8);
            Material = MaterialData.Read(fs, Encoding.UTF8);
            Data = MeshData.Read(Header, fs, Encoding.UTF8);
            fs.Close();
        }

        public MeshSource(string path, MeshVertex[] vertices, uint[] indices, MeshBone[] bones, BoundingBox box, BoundingSphere sphere)
        {
            Name = path;
            Header.Type = MeshType.Default;
            Header.BoundingBox = box;
            Header.BoundingSphere = sphere;
            Header.VerticesCount = (ulong)vertices.LongLength;
            Header.IndicesCount = (ulong)indices.LongLength;

            Data = new(vertices, indices, bones);
        }

        public MeshSource(string path, MaterialData material, MeshVertex[] vertices, uint[] indices, MeshBone[] bones, BoundingBox box, BoundingSphere sphere)
        {
            Name = path;
            Header.Type = MeshType.Default;
            Header.BoundingBox = box;
            Header.BoundingSphere = sphere;
            Header.VerticesCount = (ulong)vertices.LongLength;
            Header.IndicesCount = (ulong)indices.LongLength;
            Material = material;
            Data = new(vertices, indices, bones);
        }

        public void Save(string dir)
        {
            Directory.CreateDirectory(dir);
            var fs = File.Create(Path.Combine(dir, Name + ".mesh"));
            Header.Write(fs, Encoding.UTF8);
            Material.Write(fs, Encoding.UTF8);
            Data.Write(fs, Encoding.UTF8);
            fs.Close();
        }

        public static MeshSource Load(string path)
        {
            return new MeshSource(path);
        }

        public static MeshSource LoadExternal(string path)
        {
            return new MeshSource(path, File.OpenRead(path + ".mesh"));
        }

        public void SetMaterial(MaterialData material)
        {
            Material = material;
        }

        public MeshData GetMesh()
        {
            return Data;
        }

        public MeshVertex[] GetVertices()
        {
            return Data.Vertices;
        }

        public uint[] GetIndices()
        {
            return Data.Indices;
        }

        public MeshBone[] GetBones()
        {
            return Data.Bones;
        }

        public MaterialData GetMaterial()
        {
            return Material;
        }

        public Vector3[] GetPoints()
        {
            var data = GetMesh();
            Vector3[] points = new Vector3[data.Indices.Length];
            for (int i = 0; i < data.Indices.Length; i++)
            {
                points[i] = data.Vertices[data.Indices[i]].Position;
            }
            return points;
        }
    }
}