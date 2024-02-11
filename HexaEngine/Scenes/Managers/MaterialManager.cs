namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.UI;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    public class MaterialManager
    {
        private readonly List<MaterialLibrary> libraries = [];
        private readonly Dictionary<string, MaterialLibrary> pathToLib = [];
        private readonly Dictionary<MaterialLibrary, string> libToPath = [];

        private readonly List<MaterialData> materials = [];
        private readonly Dictionary<MaterialData, MaterialLibrary> matToLib = [];

        private readonly object _lock = new();

        public MaterialManager(string? path)
        {
            if (path == null)
            {
                return;
            }

            var materialLibPaths = FileSystem.GetFiles("assets/", "*.matlib");

            for (var i = 0; i < materialLibPaths.Length; i++)
            {
                var materialLibPath = materialLibPaths[i];
                MaterialLibrary library = MaterialLibrary.Load(materialLibPath);
                libraries.Add(library);
                pathToLib.Add(materialLibPath, library);
                libToPath.Add(library, materialLibPath);
                for (var j = 0; j < library.Materials.Count; j++)
                {
                    var material = library.Materials[j];
                    materials.Add(material);
                    matToLib.Add(material, library);
                }
            }
        }

        public IReadOnlyList<MaterialLibrary> Libraries => libraries;

        public IReadOnlyList<MaterialData> Materials => materials;

        public int Count => materials.Count;

        public object SyncObject => _lock;

        public event Action<MaterialData, string, string>? Renamed;

        public static MaterialManager? Current => SceneManager.Current?.MaterialManager;

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
                libToPath.TryGetValue(library, out var value);
                return value;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                libraries.Clear();
                pathToLib.Clear();
                libToPath.Clear();
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
                libToPath.Add(library, path);

                for (int i = 0; i < library.Materials.Count; i++)
                {
                    materials.Add(library.Materials[i]);
                    matToLib.Add(library.Materials[i], library);
                }
            }

            return library;
        }

        public void SaveChanges(MaterialData desc)
        {
            SaveChanges(GetMaterialLibraryForm(desc));
        }

        public void SaveChanges(MaterialLibrary library)
        {
            lock (_lock)
            {
                var path = GetPathToMaterialLibrary(library);

                if (path == null)
                {
                    Logger.Error("Failed to save library, Path not found");
                    MessageBox.Show("Failed to save library", "Path not found");
                    return;
                }

                path = FileSystem.GetFullPath(path);

                try
                {
                    library.Save(path, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    Logger.Error($"Failed to save library, {e.Message}");
                    MessageBox.Show("Failed to save library", e.Message);
                }
            }
        }
    }
}