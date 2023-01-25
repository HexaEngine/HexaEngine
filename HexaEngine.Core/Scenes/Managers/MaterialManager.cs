namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class MaterialManager
    {
        private readonly List<MaterialDesc> materials = new();

        public MaterialManager()
        {
        }

        public MaterialManager(IEnumerable<MaterialDesc> materials)
        {
            this.materials.AddRange(materials);
        }

        public IReadOnlyList<MaterialDesc> Materials => materials;

        public int Count => materials.Count;

        public event Action<MaterialDesc, string, string>? Renamed;

        public void Clear()
        {
            lock (materials)
            {
                materials.Clear();
            }
        }

        public string GetFreeName(string name)
        {
            if (!Exists(name))
                return name;
            int i = 1;
            while (true)
            {
                string newName = $"{name} {i++}";
                if (!Exists(newName))
                    return newName;
            }
        }

        public bool Exists(string name)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].Name == name)
                    return true;
            }
            return false;
        }

        public void Add(MaterialDesc desc)
        {
            lock (materials)
            {
                if (Exists(desc.Name))
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
                }
            }
        }

        public MaterialDesc? GetMaterial(string name)
        {
            lock (materials)
            {
                return materials.FirstOrDefault(x => x.Name == name);
            }
        }

        public bool TryGetMaterial(string name, [NotNullWhen(true)] out MaterialDesc? material)
        {
            lock (materials)
            {
                material = materials.FirstOrDefault(x => x.Name == name);
            }

            return material != null;
        }

        public bool Rename(string oldName, string newName)
        {
            lock (materials)
            {
                if (TryGetMaterial(oldName, out MaterialDesc? desc))
                {
                    if (!Exists(newName))
                    {
                        desc.Name = newName;
                        ResourceManager.RenameMaterial(oldName, newName);
                        Renamed?.Invoke(desc, oldName, newName);
                        return true;
                    }
                }
            }

            return false;
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
                materials.Remove(desc);
            }
        }
    }
}