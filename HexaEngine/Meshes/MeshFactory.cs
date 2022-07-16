namespace HexaEngine.Meshes
{
    using HexaEngine.Core.IO;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public class MeshFactory
    {
        public readonly List<MeshMaterial> Materials = new();

        public static MeshFactory Instance { get; } = new();

        private MeshFactory()
        {
        }

        public MeshMaterialLibrary LoadLib(string path) => LoadLib(FileSystem.Open(path));

        public MeshMaterialLibrary LoadLib(byte[] data)
        {
            MeshMaterialLibrary library = new();
            library.Read(data);
            Materials.AddRange(Materials);
            return library;
        }

        public MeshMaterialLibrary LoadLib(Stream stream)
        {
            MeshMaterialLibrary library = new();
            Span<byte> buffer = new byte[stream.Length];
            stream.Read(buffer);
            library.Read(buffer);
            if (library.Materials != null)
                Materials.AddRange(library.Materials);
            return library;
        }

        public MeshFile Load(string path) => Load(File.ReadAllBytes(path));

        public MeshFile Load(byte[] data)
        {
            MeshFile mesh = new();
            mesh.Read(data);
            for (int i = 0; i < mesh.Groups.Length; i++)
            {
                mesh.Groups[i].Material = GetMaterialByName(mesh.Groups[i].MaterialName);
            }
            return mesh;
        }

        public MeshFile Load(Stream stream)
        {
            MeshFile mesh = new();
            Span<byte> buffer = new byte[stream.Length];
            stream.Read(buffer);
            mesh.Read(buffer);
            for (int i = 0; i < mesh.Groups.Length; i++)
            {
                mesh.Groups[i].Material = GetMaterialByName(mesh.Groups[i].MaterialName);
            }
            return mesh;
        }

        public void Save(MeshFile mesh, Stream stream)
        {
            Span<byte> buffer = new byte[mesh.SizeOf()];
            Debug.Assert(mesh.Write(buffer) == buffer.Length);
            stream.Write(buffer);
        }

        public void Save(MeshMaterialLibrary library, Stream stream)
        {
            Span<byte> buffer = new byte[library.SizeOf()];
            Debug.Assert(library.Write(buffer) == buffer.Length);
            stream.Write(buffer);
        }

        internal MeshMaterial GetMaterialByName(string ma)
        {
            return Materials.FirstOrDefault(m => m.Name == ma);
        }
    }
}