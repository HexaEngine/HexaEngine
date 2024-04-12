namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Collections;

    public class MaterialAssetMappingCollection : ICollection<MaterialAssetMapping>
    {
        private readonly List<MaterialAssetMapping> mappings = [];

        public int Count => mappings.Count;

        public bool IsReadOnly => false;

        public event Action<MaterialAssetMapping>? OnChanged;

        public MaterialAssetMapping this[int index]
        {
            get => mappings[index];
            set
            {
                mappings[index] = value;
                OnChanged?.Invoke(value);
            }
        }

        public MaterialAssetMapping Find(string mesh)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (mapping.Mesh == mesh)
                {
                    return mapping;
                }
            }

            return default;
        }

        public MaterialAssetMapping Find(AssetRef material)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (mapping.Material == material)
                {
                    return mapping;
                }
            }

            return default;
        }

        public MaterialData GetMaterial(string data, ILogger logger)
        {
            var mapping = Find(data);
            if (mapping.Material == AssetRef.Empty)
            {
                return MaterialData.Empty;
            }
            else
            {
                Artifact? artifact = ArtifactDatabase.GetArtifact(mapping.Material);
                if (artifact == null)
                {
                    logger.Warn($"Failed to load material {mapping.Material}");
                    return MaterialData.Empty;
                }
                if (artifact.Type != AssetType.Material)
                {
                    logger.Warn($"Failed to load material {mapping.Material}, asset was {artifact.Type} but needs to be {AssetType.Material}");
                    return MaterialData.Empty;
                }

                Stream? stream = null;

                try
                {
                    stream = artifact.OpenRead();
                    MaterialFile materialFile = MaterialFile.Read(stream);
                    return materialFile;
                }
                catch (Exception e)
                {
                    logger.Log(e);
                    logger.Warn($"Failed to load material {mapping.Material}");
                    return MaterialData.Empty;
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        public void Add(MaterialAssetMapping item)
        {
            mappings.Add(item);
            OnChanged?.Invoke(item);
        }

        public void Clear()
        {
            mappings.Clear();
        }

        public bool Contains(MaterialAssetMapping item)
        {
            return mappings.Contains(item);
        }

        public void CopyTo(MaterialAssetMapping[] array, int arrayIndex)
        {
            mappings.CopyTo(array, arrayIndex);
        }

        public IEnumerator<MaterialAssetMapping> GetEnumerator()
        {
            return mappings.GetEnumerator();
        }

        public bool Remove(MaterialAssetMapping item)
        {
            OnChanged?.Invoke(item);
            return mappings.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mappings.GetEnumerator();
        }

        public void Update(ModelFile modelFile)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (!ModelContainsMesh(modelFile, mapping.Mesh))
                {
                    mappings.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var mesh = modelFile.Meshes[i];
                if (!ContainsMesh(mappings, mesh.Name))
                {
                    mappings.Add(new(mesh, mesh.MaterialId));
                }
            }
        }

        private static bool ContainsMesh(List<MaterialAssetMapping> assetMappings, string meshName)
        {
            for (int i = 0; i < assetMappings.Count; i++)
            {
                var mapping = assetMappings[i];
                if (mapping.Mesh == meshName)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ModelContainsMesh(ModelFile modelFile, string meshName)
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var mesh = modelFile.Meshes[i];
                if (mesh.Name == meshName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}