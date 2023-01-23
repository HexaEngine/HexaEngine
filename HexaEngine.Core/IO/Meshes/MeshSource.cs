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
        private readonly Stream stream;
        private bool cached;

        public readonly MeshHeader Header;
        private MeshData Body;
        public string Path;

        public MeshSource(string path)
        {
            Path = path;
            stream = FileSystem.Open(System.IO.Path.Combine(Paths.CurrentMeshesPath, path + ".mesh"));
            Header.Read(stream, Encoding.UTF8);
        }

        public MeshSource(string path, Stream source)
        {
            Path = path;
            stream = source;
            Header.Read(stream, Encoding.UTF8);
        }

        public MeshSource(string path, MeshVertex[] vertices, uint[] indices, BoundingBox box, BoundingSphere sphere)
        {
            Path = path;
            cached = true;
            Header.Type = MeshType.Default;
            Header.BoundingBox = box;
            Header.BoundingSphere = sphere;
            Header.VerticesCount = (ulong)vertices.LongLength;
            Header.IndicesCount = (ulong)indices.LongLength;

            Body = new(vertices, indices);
        }

        private void Cache()
        {
            if (cached) return;
            switch (Header.Type)
            {
                case MeshType.Default:
                    Body = MeshData.Read(Header, stream, Encoding.UTF8);
                    break;

                case MeshType.Skinned:
                    break;

                case MeshType.Terrain:
                    break;
            }

            cached = true;
        }

        public void Save(string dir)
        {
            Directory.CreateDirectory(dir);
            var fs = File.Create(System.IO.Path.Combine(dir, Path + ".mesh"));
            Header.Write(fs, Encoding.UTF8);
            Body.Write(fs, Encoding.UTF8);
            fs.Close();
        }

        public MeshData ReadMesh()
        {
            Cache();
            return Body;
        }

        public Vector3[] GetPoints()
        {
            var data = ReadMesh();
            Vector3[] points = new Vector3[data.Vertices.Length];
            for (int i = 0; i < data.Vertices.Length; i++)
            {
                points[i] = data.Vertices[i].Position;
            }
            return points;
        }
    }
}