namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class MaterialManager
    {
        private static readonly List<MaterialLibrary> libraries = new();
        private static readonly List<MaterialData> materials = new();
        private static readonly Dictionary<MaterialData, MaterialLibrary> matToLib = new();
        private static readonly object lockObject = new();

        static MaterialManager()
        {
            lock (lockObject)
            {
                var files = FileSystem.GetFiles(Paths.CurrentMaterialsPath);
                for (int i = 0; i < files.Length; i++)
                {
                    MaterialLibrary library = new(files[i]);
                    libraries.Add(library);
                    for (int j = 0; j < library.Materials.Length; j++)
                    {
                        var material = library.Materials[j];
                        materials.Add(library.Materials[j]);
                        matToLib.Add(material, library);
                    }
                }
            }
        }

        public static void Refresh()
        {
            lock (lockObject)
            {
                var files = FileSystem.GetFiles(Paths.CurrentMaterialsPath);
                for (int i = 0; i < files.Length; i++)
                {
                    MaterialLibrary library = new(files[i]);
                    libraries.Add(library);
                    for (int j = 0; j < library.Materials.Length; j++)
                    {
                        var material = library.Materials[j];
                        materials.Add(library.Materials[j]);
                        matToLib.Add(material, library);
                    }
                }
            }
        }

        public static MaterialData? GetMaterial(string name)
        {
            lock (lockObject)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i].Name == name)
                        return materials[i];
                }
                return null;
            }
        }

        public static IReadOnlyList<MaterialData> Materials => materials;

        public static int Count => materials.Count;

        public static event Action<MaterialData, string, string>? Renamed;

        public static string GetFreeName(string name)
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

        public static bool Exists(string name)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].Name == name)
                    return true;
            }
            return false;
        }

        public static void Add(MaterialData desc)
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

        public static MaterialData TryAddMaterial(MaterialData material)
        {
            lock (lockObject)
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

        public static bool TryGetMaterial(string name, [NotNullWhen(true)] out MaterialData? material)
        {
            lock (lockObject)
            {
                material = materials.FirstOrDefault(x => x.Name == name);
            }

            return material != null;
        }

        public static bool Rename(string oldName, string newName)
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

        public static void Update(MaterialData desc)
        {
            lock (lockObject)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i].Name == desc.Name)
                    {
                        materials[i] = desc;
                        ResourceManager.UpdateMaterialAsync(desc);
                    }
                }
            }
        }

        public static void Remove(MaterialData desc)
        {
            lock (lockObject)
            {
                materials.Remove(desc);
            }
        }
    }
}