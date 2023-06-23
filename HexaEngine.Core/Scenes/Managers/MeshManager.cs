namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.IO.Meshes;
    using System.Collections.Generic;

    public class ModelManager
    {
        private readonly Dictionary<string, ModelFile> pathToMeshes = new();
        private readonly List<ModelFile> meshes = new();

        public IReadOnlyList<ModelFile> Meshes => meshes;

        public int Count => meshes.Count;

        public void Clear()
        {
            lock (meshes)
            {
                meshes.Clear();
                pathToMeshes.Clear();
            }
        }

        public ModelFile Load(string path)
        {
            lock (meshes)
            {
                if (!pathToMeshes.TryGetValue(path, out var value))
                {
                    value = ModelFile.Load(path);
                    pathToMeshes.Add(path, value);
                    meshes.Add(value);
                }
                return value;
            }
        }

        public void Unload(ModelFile source)
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