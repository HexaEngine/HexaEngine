namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Objects;
    using HexaEngine.Resources;
    using System;
    using System.Collections.Generic;

    public static class MaterialManager
    {
        private static readonly List<MaterialDesc> materials = new();
        private static string[] names = Array.Empty<string>();

        public static string[] Names => names;

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
                if (names.Contains(desc.Name))
                {
                    for (int i = 0; i < materials.Count; i++)
                    {
                        if (materials[i].Name == desc.Name)
                        {
                            materials[i] = desc;
                        }
                    }
                }
                else
                {
                    materials.Add(desc);
                    if (names.Length != materials.Capacity)
                    {
                        var old = names;
                        names = new string[materials.Capacity];
                        Array.Copy(old, names, old.Length);
                    }
                    names[materials.Count - 1] = desc.Name;
                }
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
                var index = materials.IndexOf(desc);
                materials.RemoveAt(index);
                Array.Copy(names, index + 1, names, index, materials.Count - index);
            }
        }
    }
}