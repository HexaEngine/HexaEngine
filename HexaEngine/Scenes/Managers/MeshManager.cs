namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Collections.Generic;

    public class ModelManager : IDisposable
    {
        private readonly Dictionary<string, ModelFile> pathToMeshes = new();
        private readonly List<ModelFile> meshes = new();
        private bool disposedValue;

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
                    value = ModelFile.Load(path, MeshLoadMode.Immediate);
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
                    var key = pathToMeshes.FirstOrDefault(x => x.Value == source);
                    if (key.Key != null)
                    {
                        pathToMeshes.Remove(key.Key);
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                lock (meshes)
                {
                    meshes.Clear();
                    pathToMeshes.Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}