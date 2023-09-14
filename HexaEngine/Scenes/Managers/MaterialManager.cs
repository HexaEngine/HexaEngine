namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class MaterialManager
    {
        private readonly List<MaterialLibrary> libraries = new();
        private readonly Dictionary<string, MaterialLibrary> pathToLib = new();

        private readonly List<MaterialData> materials = new();
        private readonly Dictionary<MaterialData, MaterialLibrary> matToLib = new();

        private readonly object _lock = new();

        public IReadOnlyList<MaterialLibrary> Libraries => libraries;

        public IReadOnlyList<MaterialData> Materials => materials;

        public int Count => materials.Count;

        public event Action<MaterialData, string, string>? Renamed;

        public MaterialLibrary GetMaterialLibraryForm(MaterialData materialData)
        {
            lock (_lock)
            {
                return matToLib[materialData];
            }
        }

        public string? GetPathToMaterialLibrary(MaterialLibrary library)
        {
            lock (_lock)
            {
                var result = pathToLib.FirstOrDefault(x => x.Value == library);
                return result.Key;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                libraries.Clear();
                pathToLib.Clear();
                materials.Clear();
                matToLib.Clear();
            }
        }

        public string GetFreeName(string name)
        {
            lock (_lock)
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
        }

        public bool Exists(string name)
        {
            lock (_lock)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i].Name == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetMaterial(string name, [NotNullWhen(true)] out MaterialData? material)
        {
            lock (_lock)
            {
                material = materials.FirstOrDefault(x => x.Name == name);
            }

            return material != null;
        }

        public bool Rename(string oldName, string newName)
        {
            lock (_lock)
            {
                if (TryGetMaterial(oldName, out MaterialData? desc))
                {
                    if (!Exists(newName))
                    {
                        var mat = desc;
                        int index = materials.IndexOf(mat);
                        mat.Name = newName;
                        //ResourceManager.RenameMaterial(oldName, newName);
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
            lock (_lock)
            {
                if (!materials.Contains(desc))
                {
                    return;
                }
            }

            ResourceManager.Shared.UpdateMaterial(desc);
        }

        public async Task UpdateAsync(MaterialData desc)
        {
            lock (_lock)
            {
                if (!materials.Contains(desc))
                {
                    return;
                }
            }

            await ResourceManager.Shared.UpdateMaterialAsync(desc);
        }

        public MaterialLibrary Load(string path)
        {
            MaterialLibrary? library;
            lock (_lock)
            {
                if (pathToLib.TryGetValue(path, out library))
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
                    Logger.Warn($"Warning couldn't find material library {path}");
                }

                libraries.Add(library);
                pathToLib.Add(path, library);

                for (int i = 0; i < library.Materials.Count; i++)
                {
                    materials.Add(library.Materials[i]);
                    matToLib.Add(library.Materials[i], library);
                }
            }

            return library;
        }
    }
}