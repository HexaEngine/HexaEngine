namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class MaterialManager : IDisposable
    {
        private readonly List<MaterialFile> materials = [];
        private readonly Dictionary<Guid, MaterialFile> guidToMaterial = [];
        private readonly object _lock = new();
        private bool disposedValue;

        public MaterialManager(string? path)
        {
            if (path == null)
            {
                return;
            }

            foreach (var asset in ArtifactDatabase.GetArtifactsFromType(AssetType.Material))
            {
                Stream? stream = null;

                try
                {
                    stream = asset.OpenRead();
                    MaterialFile material = MaterialFile.Read(stream);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    Logger.Error($"Failed to load material file, {asset.Path}");
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        public MaterialManager(Scene scene)
        {
            var path = scene.Path;
            if (path == null)
            {
                return;
            }

            foreach (var asset in ArtifactDatabase.GetArtifactsFromType(AssetType.Material))
            {
                Stream? stream = null;

                try
                {
                    stream = asset.OpenRead();
                    MaterialFile material = MaterialFile.Read(stream);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    Logger.Error($"Failed to load material file, {asset.Path}");
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        public IReadOnlyList<MaterialFile> Materials => materials;

        public int Count => materials.Count;

        public object SyncObject => _lock;

        public event Action<MaterialData, string, string>? Renamed;

        public static MaterialManager? Current => SceneManager.Current?.MaterialManager;

        public void Clear()
        {
            lock (_lock)
            {
                materials.Clear();
                guidToMaterial.Clear();
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

        public bool TryGetMaterialFile(Guid name, [NotNullWhen(true)] out MaterialFile? material)
        {
            lock (_lock)
            {
                return guidToMaterial.TryGetValue(name, out material);
            }
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

            ResourceManager.Shared.UpdateMaterial<Model>(desc);
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

            await ResourceManager.Shared.UpdateMaterialAsync<Model>(desc);
        }

        public MaterialData GetMaterial(Guid name)
        {
            if (name == Guid.Empty)
                return MaterialData.Empty;

            lock (_lock)
            {
                if (guidToMaterial.TryGetValue(name, out var material))
                {
                    return material;
                }
            }

            return MaterialData.Empty;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                lock (_lock)
                {
                    materials.Clear();
                    guidToMaterial.Clear();
                }
                disposedValue = true;
            }
        }

        ~MaterialManager()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}