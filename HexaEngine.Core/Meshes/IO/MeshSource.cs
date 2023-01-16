namespace HexaEngine.Core.Meshes.IO
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics;
    using System.IO;
    using System.Text;

    public class MeshSource
    {
        private readonly Stream stream;
        private bool cached;

        public readonly MeshHeader Header;
        private MeshBody Body;
        public readonly string Path;

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
                    Body = MeshBody.Read(Header, stream, Encoding.UTF8);
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
            Body.Write(Header, fs, Encoding.UTF8);
            fs.Close();
        }

        public MeshBody ReadMesh()
        {
            Cache();
            return Body;
        }
    }
}