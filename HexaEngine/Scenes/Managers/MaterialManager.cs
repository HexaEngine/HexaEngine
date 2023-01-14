namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Objects;
    using HexaEngine.Resources;
    using System;
    using System.Collections.Generic;

    public class MaterialManager
    {
        private readonly List<MaterialDesc> materials = new();
        private string[] names = Array.Empty<string>();

        public MaterialManager()
        {
        }

        public MaterialManager(IEnumerable<MaterialDesc> materials)
        {
            this.materials.AddRange(materials);
        }

        public string[] Names => names;

        public IReadOnlyList<MaterialDesc> Materials => materials;

        public int Count => materials.Count;

        public void Clear()
        {
            lock (materials)
            {
                materials.Clear();
            }
        }

        public void Add(MaterialDesc desc)
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
                    lock (names)
                    {
                        if (names.Length != materials.Capacity)
                        {
                            var old = names;
                            names = new string[materials.Capacity];
                            Array.Fill(names, string.Empty);
                            Array.Copy(old, names, old.Length);
                        }
                        names[materials.Count - 1] = desc.Name;
                    }
                }
            }
        }

        public async void Update(MaterialDesc desc)
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

        public void Remove(MaterialDesc desc)
        {
            lock (materials)
            {
                var index = materials.IndexOf(desc);
                materials.RemoveAt(index);
                lock (names)
                {
                    Array.Copy(names, index + 1, names, index, materials.Count - index);
                }
            }
        }
    }
}