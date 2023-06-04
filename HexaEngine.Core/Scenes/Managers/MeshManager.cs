namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.IO.Meshes;
    using System.Collections.Generic;

    public class ModelManager
    {
        private readonly Dictionary<string, Model> pathToMeshes = new();
        private readonly List<Model> meshes = new();

        public IReadOnlyList<Model> Meshes => meshes;

        public int Count => meshes.Count;

        public void Clear()
        {
            lock (meshes)
            {
                meshes.Clear();
                pathToMeshes.Clear();
            }
        }

        public Model Load(string path)
        {
            lock (meshes)
            {
                if (!pathToMeshes.TryGetValue(path, out var value))
                {
                    value = Model.Load(path);
                    pathToMeshes.Add(path, value);
                    meshes.Add(value);
                }
                return value;
            }
        }

        public void Unload(Model source)
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