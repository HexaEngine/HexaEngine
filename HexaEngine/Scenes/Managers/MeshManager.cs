namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Objects;
    using HexaEngine.Resources;
    using System.Collections.Generic;

    public static class MeshManager
    {
        private static List<MeshData> meshes = new();

        public static IReadOnlyList<MeshData> Meshes => meshes;

        public static int Count => meshes.Count;

        public static void Clear()
        {
            lock (meshes)
            {
                meshes.Clear();
            }
        }

        public static void Add(MeshData data)
        {
            lock (meshes)
            {
                meshes.Add(data);
            }
        }

        public static void Remove(MeshData data)
        {
            lock (meshes)
            {
                meshes.Remove(data);
            }
        }

        public static async void Update(MeshData data)
        {
            lock (meshes)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    var mesh = meshes[i];
                    if (mesh.Name == data.Name)
                    {
                        meshes[i] = mesh;
                    }
                }
            }

            await ResourceManager.UpdateMeshAsync(data);
        }
    }
}