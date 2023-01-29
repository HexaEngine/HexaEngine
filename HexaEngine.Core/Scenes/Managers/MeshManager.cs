namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.IO.Meshes;
    using System.Collections.Generic;

    public class MeshManager
    {
        private readonly Dictionary<string, MeshSource> pathToMeshes = new();
        private readonly List<MeshSource> meshes = new();

        public IReadOnlyList<MeshSource> Meshes => meshes;

        public int Count => meshes.Count;

        public void Clear()
        {
            lock (meshes)
            {
                meshes.Clear();
                pathToMeshes.Clear();
            }
        }

        public MeshSource Load(string path)
        {
            lock (meshes)
            {
                if (!pathToMeshes.TryGetValue(path, out var value))
                {
                    value = MeshSource.Load(path);
                    pathToMeshes.Add(path, value);
                    meshes.Add(value);
                }
                return value;
            }
        }

        public void Unload(MeshSource source)
        {
            lock (meshes)
            {
                if (meshes.Contains(source))
                {
                    meshes.Remove(source);
                    pathToMeshes.Remove(source.Name);
                }
            }
        }
    }
}