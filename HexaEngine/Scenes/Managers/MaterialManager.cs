namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Objects;
    using HexaEngine.Resources;
    using System.Collections.Generic;

    public static class MaterialManager
    {
        private static readonly List<MaterialDesc> materials = new();

        public static IReadOnlyList<MaterialDesc> Materials => materials;

        public static int Count => materials.Count;

        public static void Clear()
        {
            lock (materials)
            {
                materials.Clear();
            }
        }

        public static void Add(MaterialDesc desc)
        {
            lock (materials)
            {
                materials.Add(desc);
            }
        }

        public static async void Update(MaterialDesc desc)
        {
            lock (materials)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i].Name == desc.Name)
                    {
                        materials[i] = desc;
                    }
                }
            }

            await ResourceManager.AsyncUpdateMaterial(desc);
        }

        public static void Remove(MaterialDesc desc)
        {
            lock (materials)
            {
                materials.Remove(desc);
            }
        }
    }
}