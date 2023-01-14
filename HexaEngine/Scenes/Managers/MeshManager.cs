namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Objects;
    using System.Collections.Generic;

    public class MeshManager
    {
        private readonly List<MeshData> meshes = new();

        public IReadOnlyList<MeshData> Meshes => meshes;

        public int Count => meshes.Count;

        public void Clear()
        {
            lock (meshes)
            {
                meshes.Clear();
            }
        }

        public void Add(MeshData data)
        {
            lock (meshes)
            {
                meshes.Add(data);
            }
        }

        public void Remove(MeshData data)
        {
            lock (meshes)
            {
                meshes.Remove(data);
            }
        }
    }
}