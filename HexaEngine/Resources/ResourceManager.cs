namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    public class ResourceManager : IDisposable
    {
        private readonly ConcurrentDictionary<SceneNode, ModelInstance> instances = new();
        private readonly ConcurrentDictionary<Mesh, ModelMesh> meshes = new();
        private readonly ConcurrentDictionary<Material, ModelMaterial> materials = new();
        private readonly ConcurrentDictionary<string, ModelTexture> textures = new();
        private bool suppressCleanup;
        private readonly IGraphicsDevice device;
        private bool disposedValue;

        public ResourceManager(IGraphicsDevice device)
        {
            this.device = device;
        }

        public void KeepAlive()
        {
            suppressCleanup = true;
        }

        public void ClearKeepAlive()
        {
            suppressCleanup = false;
        }

        public bool GetMesh(Mesh mesh, [NotNullWhen(true)] out ModelMesh? model)
        {
            return meshes.TryGetValue(mesh, out model);
        }

        public ModelInstance CreateInstance(Mesh mesh, SceneNode node)
        {
            if (!meshes.TryGetValue(mesh, out var modelMesh))
            {
                modelMesh = LoadMesh(mesh);
            }

            var instance = modelMesh.CreateInstance(device, node);
            instances.TryAdd(node, instance);

            return instance;
        }

        public async Task<ModelInstance> AsyncCreateInstance(Mesh mesh, SceneNode node)
        {
            if (!meshes.TryGetValue(mesh, out var modelMesh))
            {
                modelMesh = await AsyncLoadMesh(mesh);
            }

            var instance = modelMesh.CreateInstance(device, node);
            instances.TryAdd(node, instance);

            return instance;
        }

        public void DestroyInstance(Mesh mesh, SceneNode node)
        {
            if (!instances.TryGetValue(node, out var instance))
            {
                return;
            }

            if (!meshes.TryGetValue(mesh, out var modelMesh))
            {
                return;
            }

            modelMesh.DestroyInstance(device, instance);
            instances.TryRemove(new KeyValuePair<SceneNode, ModelInstance>(node, instance));

            if (modelMesh.InstanceCount == 0)
            {
                UnloadMesh(mesh);
            }
        }

        public Task AsyncDestroyInstance(Mesh mesh, SceneNode node)
        {
            return Task.Factory.StartNew(() => DestroyInstance(mesh, node));
        }

        public ModelMesh LoadMesh(Mesh mesh)
        {
            ModelMesh model = new(device, mesh.Vertices, mesh.Indices, mesh.AABB);

            if (mesh.Material != null)
            {
                if (!materials.TryGetValue(mesh.Material, out var modelMaterial))
                {
                    modelMaterial = LoadMaterial(mesh.Material);
                }
                modelMaterial.Instances++;
                model.Material = modelMaterial;
            }

            meshes.TryAdd(mesh, model);
            return model;
        }

        public async Task<ModelMesh> AsyncLoadMesh(Mesh mesh)
        {
            ModelMesh model = new(device, mesh.Vertices, mesh.Indices, mesh.AABB);

            if (mesh.Material != null)
            {
                if (!materials.TryGetValue(mesh.Material, out var modelMaterial))
                {
                    modelMaterial = await AsyncLoadMaterial(mesh.Material);
                }
                modelMaterial.Instances++;
                model.Material = modelMaterial;
            }

            meshes.TryAdd(mesh, model);
            return model;
        }

        public void UpdateMesh(Mesh mesh)
        {
            if (meshes.TryGetValue(mesh, out var model))
            {
                model.Update(device, mesh.Vertices, mesh.Indices, mesh.AABB);

                if (mesh.Material != null)
                {
                    if (!materials.TryGetValue(mesh.Material, out var modelMaterial))
                    {
                        modelMaterial = LoadMaterial(mesh.Material);
                    }
                    var before = model.Material;
                    modelMaterial.Instances++;
                    model.Material = modelMaterial;
                    if (before != null)
                    {
                        before.Instances--;
                        if (before.Instances == 0)
                        {
                            UnloadMaterial(before);
                        }
                    }
                }
            }
        }

        public async Task AsyncUpdateMesh(Mesh mesh)
        {
            if (meshes.TryGetValue(mesh, out var model))
            {
                model.Update(device, mesh.Vertices, mesh.Indices, mesh.AABB);

                if (mesh.Material != null)
                {
                    if (!materials.TryGetValue(mesh.Material, out var modelMaterial))
                    {
                        modelMaterial = await AsyncLoadMaterial(mesh.Material);
                    }
                    var before = model.Material;
                    modelMaterial.Instances++;
                    model.Material = modelMaterial;
                    if (before != null)
                    {
                        before.Instances--;
                        if (before.Instances == 0)
                        {
                            UnloadMaterial(before);
                        }
                    }
                }
            }
        }

        public void UnloadMesh(Mesh mesh)
        {
            if (suppressCleanup) return;
            if (meshes.TryGetValue(mesh, out var model))
            {
                meshes.Remove(mesh, out _);
                model.Dispose();
                var material = model.Material;
                if (material != null)
                {
                    material.Instances--;
                    if (material.Instances == 0)
                        UnloadMaterial(material);
                }
            }
        }

        public ModelMaterial LoadMaterial(Material material)
        {
            ModelMaterial modelMaterial = new(material,
                device.CreateBuffer(new CBMaterial(material), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write),
                device.CreateSamplerState(SamplerDescription.AnisotropicWrap));
            modelMaterial.AlbedoTexture = LoadTexture(material.AlbedoTextureMap);
            modelMaterial.NormalTexture = LoadTexture(material.NormalTextureMap);
            modelMaterial.DisplacementTexture = LoadTexture(material.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = LoadTexture(material.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = LoadTexture(material.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = LoadTexture(material.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = LoadTexture(material.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = LoadTexture(material.AoTextureMap);
            modelMaterial.Update();
            materials.TryAdd(material, modelMaterial);
            return modelMaterial;
        }

        public async Task<ModelMaterial> AsyncLoadMaterial(Material material)
        {
            ModelMaterial modelMaterial = new(material,
                device.CreateBuffer(new CBMaterial(material), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write),
                device.CreateSamplerState(SamplerDescription.AnisotropicWrap));
            modelMaterial.AlbedoTexture = await AsyncLoadTexture(material.AlbedoTextureMap);
            modelMaterial.NormalTexture = await AsyncLoadTexture(material.NormalTextureMap);
            modelMaterial.DisplacementTexture = await AsyncLoadTexture(material.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = await AsyncLoadTexture(material.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = await AsyncLoadTexture(material.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = await AsyncLoadTexture(material.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = await AsyncLoadTexture(material.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = await AsyncLoadTexture(material.AoTextureMap);
            modelMaterial.Update();
            materials.TryAdd(material, modelMaterial);
            return modelMaterial;
        }

        public void UpdateMaterial(Material material)
        {
            if (materials.TryGetValue(material, out var modelMaterial))
            {
                UpdateTexture(ref modelMaterial.AlbedoTexture, material.AlbedoTextureMap);
                UpdateTexture(ref modelMaterial.NormalTexture, material.NormalTextureMap);
                UpdateTexture(ref modelMaterial.DisplacementTexture, material.DisplacementTextureMap);
                UpdateTexture(ref modelMaterial.RoughnessTexture, material.RoughnessTextureMap);
                UpdateTexture(ref modelMaterial.MetalnessTexture, material.MetalnessTextureMap);
                UpdateTexture(ref modelMaterial.EmissiveTexture, material.EmissiveTextureMap);
                UpdateTexture(ref modelMaterial.RoughnessMetalnessTexture, material.RoughnessMetalnessTextureMap);
                UpdateTexture(ref modelMaterial.AoTexture, material.AoTextureMap);
                modelMaterial.Update();
            }
        }

        public async Task AsyncUpdateMaterial(Material material)
        {
            if (materials.TryGetValue(material, out var modelMaterial))
            {
                modelMaterial.AlbedoTexture = await AsyncUpdateTexture(modelMaterial.AlbedoTexture, material.AlbedoTextureMap);
                modelMaterial.NormalTexture = await AsyncUpdateTexture(modelMaterial.NormalTexture, material.NormalTextureMap);
                modelMaterial.DisplacementTexture = await AsyncUpdateTexture(modelMaterial.DisplacementTexture, material.DisplacementTextureMap);
                modelMaterial.RoughnessTexture = await AsyncUpdateTexture(modelMaterial.RoughnessTexture, material.RoughnessTextureMap);
                modelMaterial.MetalnessTexture = await AsyncUpdateTexture(modelMaterial.MetalnessTexture, material.MetalnessTextureMap);
                modelMaterial.EmissiveTexture = await AsyncUpdateTexture(modelMaterial.EmissiveTexture, material.EmissiveTextureMap);
                modelMaterial.RoughnessMetalnessTexture = await AsyncUpdateTexture(modelMaterial.RoughnessMetalnessTexture, material.RoughnessMetalnessTextureMap);
                modelMaterial.AoTexture = await AsyncUpdateTexture(modelMaterial.AoTexture, material.AoTextureMap);
                modelMaterial.Update();
            }
        }

        public void UnloadMaterial(ModelMaterial material)
        {
            if (suppressCleanup) return;
            materials.Remove(material.Material, out _);
            material.Dispose();
            UnloadTexture(material.AlbedoTexture);
            UnloadTexture(material.NormalTexture);
            UnloadTexture(material.DisplacementTexture);
            UnloadTexture(material.RoughnessTexture);
            UnloadTexture(material.MetalnessTexture);
            UnloadTexture(material.EmissiveTexture);
            UnloadTexture(material.RoughnessMetalnessTexture);
            UnloadTexture(material.AoTexture);
        }

        public ModelTexture? LoadTexture(string? name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (string.IsNullOrEmpty(name)) return null;
            if (textures.TryGetValue(fullname, out var texture))
            {
                texture.InstanceCount++;
                return texture;
            }
            var tex = device.LoadTexture2D(fullname);
            texture = new(fullname, device.CreateShaderResourceView(tex), 1);
            tex.Dispose();
            textures.TryAdd(fullname, texture);
            return texture;
        }

        public Task<ModelTexture?> AsyncLoadTexture(string? name)
        {
            return Task.Factory.StartNew(() => LoadTexture(name));
        }

        public void UnloadTexture(ModelTexture? texture)
        {
            if (texture == null) return;
            texture.InstanceCount--;
            if (texture.InstanceCount == 0)
            {
                textures.Remove(texture.Name, out _);
                texture.SRV.Dispose();
            }
        }

        public void UpdateTexture(ref ModelTexture? texture, string name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (texture?.Name == fullname) return;
            UnloadTexture(texture);
            texture = LoadTexture(name);
        }

        public async Task<ModelTexture?> AsyncUpdateTexture(ModelTexture? texture, string name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (texture?.Name == fullname) return texture;
            UnloadTexture(texture);
            return await AsyncLoadTexture(name);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    instances.Clear();
                    foreach (var mesh in meshes.Values)
                    {
                        mesh.Dispose();
                    }
                    meshes.Clear();
                    foreach (var material in materials.Values)
                    {
                        material.CB.Dispose();
                        material.SamplerState.Dispose();
                    }
                    materials.Clear();
                    foreach (var texture in textures.Values)
                    {
                        texture.SRV.Dispose();
                    }
                    textures.Clear();
                }

                disposedValue = true;
            }
        }

        ~ResourceManager()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}