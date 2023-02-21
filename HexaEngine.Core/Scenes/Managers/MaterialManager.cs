namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class MaterialManager
    {
        private readonly List<MaterialData> materials = new();

        public MaterialManager()
        {
        }

        public MaterialManager(IEnumerable<MaterialData> materials)
        {
            this.materials.AddRange(materials);
        }

        public IReadOnlyList<MaterialData> Materials => materials;

        public int Count => materials.Count;

        public event Action<MaterialData, string, string>? Renamed;

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

        public void Add(MaterialData desc)
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

        public MaterialData? GetMaterial(string name)
        {
            lock (materials)
            {
                return materials.FirstOrDefault(x => x.Name == name);
            }
        }

        public MaterialData TryAddMaterial(MaterialData material)
        {
            lock (materials)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i].Name == material.Name)
                    {
                        return materials[i];
                    }
                }
                materials.Add(material);
                return material;
            }
        }

        public bool TryGetMaterial(string name, [NotNullWhen(true)] out MaterialData? material)
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
                if (TryGetMaterial(oldName, out MaterialData? desc))
                {
                    if (!Exists(newName))
                    {
                        var mat = desc.Value;
                        int index = materials.IndexOf(mat);
                        mat.Name = newName;
                        ResourceManager.RenameMaterial(oldName, newName);
                        Renamed?.Invoke(mat, oldName, newName);
                        materials[index] = mat;
                        return true;
                    }
                }
            }

            return false;
        }

        public async void Update(MaterialData desc)
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

        public void Remove(MaterialData desc)
        {
            lock (materials)
            {
                materials.Remove(desc);
            }
        }
    }
}