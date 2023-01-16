namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.Meshes.IO;
    using System.Collections.Generic;

    public class MeshManager
    {
        private readonly List<MeshSource> meshes = new();

        public IReadOnlyList<MeshSource> Meshes => meshes;

        public int Count => meshes.Count;

        public void Clear()
        {
            lock (meshes)
            {
                meshes.Clear();
            }
        }

        public void Add(MeshSource data)
        {
            lock (meshes)
            {
                meshes.Add(data);
            }
        }

        public void Remove(MeshSource data)
        {
            lock (meshes)
            {
                meshes.Remove(data);
            }
        }
    }
}