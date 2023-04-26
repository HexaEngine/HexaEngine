using HexaEngine.Core.Resources;

namespace HexaEngine.Core.Scenes.Managers
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    public class MaterialManager
    {
        private readonly List<MaterialData> materials = new();
        private readonly Dictionary<MaterialData, MaterialLibrary> matToLib = new();
        private readonly Dictionary<string, MaterialLibrary> cache = new();
        private readonly object lockObject = new();

        public IReadOnlyList<MaterialData> Materials => materials;

        public int Count => materials.Count;

        public event Action<MaterialData, string, string>? Renamed;

        public string GetFreeName(string name)
        {
            if (!Exists(name))
            {
                return name;
            }

            int i = 1;
            while (true)
            {
                string newName = $"{name} {i++}";
                if (!Exists(newName))
                {
                    return newName;
                }
            }
        }

        public bool Exists(string name)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetMaterial(string name, [NotNullWhen(true)] out MaterialData? material)
        {
            lock (lockObject)
            {
                material = materials.FirstOrDefault(x => x.Name == name);
            }

            return material != null;
        }

        public bool Rename(string oldName, string newName)
        {
            lock (lockObject)
            {
                if (TryGetMaterial(oldName, out MaterialData? desc))
                {
                    if (!Exists(newName))
                    {
                        var mat = desc;
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

        public void Update(MaterialData desc)
        {
            lock (lockObject)
            {
                if (!materials.Contains(desc))
                {
                    return;
                }
            }

            ResourceManager.UpdateMaterial(desc);
        }

        public async Task UpdateAsync(MaterialData desc)
        {
            lock (lockObject)
            {
                if (!materials.Contains(desc))
                {
                    return;
                }
            }

            await ResourceManager.UpdateMaterialAsync(desc);
        }

        public MaterialLibrary Load(string path)
        {
            if (cache.TryGetValue(path, out var library))
            {
                return library;
            }

            if (FileSystem.Exists(path))
            {
                library = MaterialLibrary.Load(path);
            }
            else
            {
                library = MaterialLibrary.Empty;
                Trace.WriteLine($"Warning couldn't find material library {path}");
            }

            cache.Add(path, library);

            for (int i = 0; i < library.Materials.Length; i++)
            {
                materials.Add(library.Materials[i]);
                matToLib.Add(library.Materials[i], library);
            }

            return library;
        }
    }
}