namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.IO.Meshes;
    using System.Collections.Generic;

    public class MeshManager
    {
        private readonly Dictionary<string, ModelSource> pathToMeshes = new();
        private readonly List<ModelSource> meshes = new();

        public IReadOnlyList<ModelSource> Meshes => meshes;

        public int Count => meshes.Count;

        public void Clear()
        {
            lock (meshes)
            {
                meshes.Clear();
                pathToMeshes.Clear();
            }
        }

        public ModelSource Load(string path)
        {
            lock (meshes)
            {
                if (!pathToMeshes.TryGetValue(path, out var value))
                {
                    value = ModelSource.Load(path);
                    pathToMeshes.Add(path, value);
                    meshes.Add(value);
                }
                return value;
            }
        }

        public void Unload(ModelSource source)
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