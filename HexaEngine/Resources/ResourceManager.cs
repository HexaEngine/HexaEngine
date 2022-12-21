namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.IO;
    using HexaEngine.Objects;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    public static class ResourceManager
    {
        private static readonly ConcurrentDictionary<string, ModelMesh> meshes = new();
        private static readonly ConcurrentDictionary<string, ModelMaterial> materials = new();
        private static readonly ConcurrentDictionary<string, ModelTexture> textures = new();
        private static bool suppressCleanup;
        private static IGraphicsDevice device;

        public static void Initialize(IGraphicsDevice device)
        {
            ResourceManager.device = device;
        }

        /// <summary>
        /// Begins to ignore cleanup
        /// </summary>
        public static void BeginPauseCleanup()
        {
            if (suppressCleanup) return;
            suppressCleanup = true;
        }

        /// <summary>
        /// Ends cleanup free region, this causes an full cleanup
        /// </summary>
        public static void EndPauseCleanup()
        {
            if (!suppressCleanup) return;
            suppressCleanup = false;
            foreach (var mesh in meshes.ToArray())
            {
                if (!mesh.Value.IsUsed)
                {
                    meshes.Remove(mesh.Key, out _);
                    mesh.Value.Dispose();
                }
            }
            foreach (var material in materials.ToArray())
            {
                if (!material.Value.IsUsed)
                {
                    materials.Remove(material.Key, out _);
                    material.Value.Dispose();
                }
            }
            foreach (var texture in textures.ToArray())
            {
                if (!texture.Value.IsUsed)
                {
                    textures.Remove(texture.Key, out _);
                    texture.Value.Dispose();
                }
            }
        }

        public static bool GetMesh(Mesh mesh, [NotNullWhen(true)] out ModelMesh? model)
        {
            return meshes.TryGetValue(mesh.Name, out model);
        }

        public static ModelMesh LoadMesh(Mesh mesh)
        {
            lock (meshes)
            {
                if (meshes.TryGetValue(mesh.Name, out var value))
                    return value;
                ModelMesh model = new(device, mesh.Name, mesh.Vertices, mesh.Indices, mesh.AABB);
                meshes.TryAdd(mesh.Name, model);
                return model;
            }
        }

        public static async Task<ModelMesh> LoadMeshAsync(Mesh mesh)
        {
            return await Task.Factory.StartNew(() => LoadMesh(mesh));
        }

        public static void UpdateMesh(Mesh mesh)
        {
            lock (meshes)
            {
                if (meshes.TryGetValue(mesh.Name, out var model))
                {
                    model.Update(device, mesh.Vertices, mesh.Indices, mesh.AABB);
                }
            }
        }

        public static async Task AsyncUpdateMesh(Mesh mesh)
        {
            await Task.Factory.StartNew(() => UpdateMesh(mesh));
        }

        public static void UnloadMesh(Mesh mesh)
        {
            if (suppressCleanup) return;
            lock (meshes)
            {
                if (meshes.TryGetValue(mesh.Name, out var model))
                {
                    meshes.Remove(mesh.Name, out _);
                    model.Dispose();
                }
            }
        }

        public static void UnloadMesh(ModelMesh model)
        {
            if (suppressCleanup) return;
            lock (meshes)
            {
                meshes.Remove(model.Name, out _);
                model.Dispose();
            }
        }

        public static ModelMaterial LoadMaterial(Material material)
        {
            ModelMaterial? modelMaterial;
            lock (materials)
            {
                if (materials.TryGetValue(material.Name, out var model))
                {
                    model.AddRef();
                    return model;
                }

                modelMaterial = new(material,
                    device.CreateBuffer(new CBMaterial(material), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write),
                    device.CreateSamplerState(SamplerDescription.AnisotropicWrap));

                materials.TryAdd(material.Name, modelMaterial);
            }

            modelMaterial.AlbedoTexture = LoadTexture(material.BaseColorTextureMap);
            modelMaterial.NormalTexture = LoadTexture(material.NormalTextureMap);
            modelMaterial.DisplacementTexture = LoadTexture(material.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = LoadTexture(material.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = LoadTexture(material.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = LoadTexture(material.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = LoadTexture(material.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = LoadTexture(material.AoTextureMap);
            modelMaterial.EndUpdate();

            return modelMaterial;
        }

        public static async Task<ModelMaterial> LoadMaterialAsync(Material material)
        {
            ModelMaterial? modelMaterial;
            lock (materials)
            {
                if (materials.TryGetValue(material.Name, out modelMaterial))
                {
                    modelMaterial.AddRef();
                    return modelMaterial;
                }

                modelMaterial = new(material,
                    device.CreateBuffer(new CBMaterial(material), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write),
                    device.CreateSamplerState(SamplerDescription.AnisotropicWrap));
                materials.TryAdd(material.Name, modelMaterial);
            }

            modelMaterial.AlbedoTexture = await AsyncLoadTexture(material.BaseColorTextureMap);
            modelMaterial.NormalTexture = await AsyncLoadTexture(material.NormalTextureMap);
            modelMaterial.DisplacementTexture = await AsyncLoadTexture(material.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = await AsyncLoadTexture(material.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = await AsyncLoadTexture(material.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = await AsyncLoadTexture(material.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = await AsyncLoadTexture(material.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = await AsyncLoadTexture(material.AoTextureMap);
            modelMaterial.EndUpdate();

            return modelMaterial;
        }

        public static void UpdateMaterial(Material material)
        {
            ModelMaterial? modelMaterial;
            lock (materials)
            {
                if (!materials.TryGetValue(material.Name, out modelMaterial))
                {
                    return;
                }
            }

            modelMaterial.BeginUpdate();
            UpdateTexture(ref modelMaterial.AlbedoTexture, material.BaseColorTextureMap);
            UpdateTexture(ref modelMaterial.NormalTexture, material.NormalTextureMap);
            UpdateTexture(ref modelMaterial.DisplacementTexture, material.DisplacementTextureMap);
            UpdateTexture(ref modelMaterial.RoughnessTexture, material.RoughnessTextureMap);
            UpdateTexture(ref modelMaterial.MetalnessTexture, material.MetalnessTextureMap);
            UpdateTexture(ref modelMaterial.EmissiveTexture, material.EmissiveTextureMap);
            UpdateTexture(ref modelMaterial.RoughnessMetalnessTexture, material.RoughnessMetalnessTextureMap);
            UpdateTexture(ref modelMaterial.AoTexture, material.AoTextureMap);
            modelMaterial.EndUpdate();
        }

        public static async Task AsyncUpdateMaterial(Material material)
        {
            ModelMaterial? modelMaterial;
            lock (materials)
            {
                if (!materials.TryGetValue(material.Name, out modelMaterial))
                {
                    return;
                }
            }

            modelMaterial.BeginUpdate();
            modelMaterial.AlbedoTexture = await AsyncUpdateTexture(modelMaterial.AlbedoTexture, material.BaseColorTextureMap);
            modelMaterial.NormalTexture = await AsyncUpdateTexture(modelMaterial.NormalTexture, material.NormalTextureMap);
            modelMaterial.DisplacementTexture = await AsyncUpdateTexture(modelMaterial.DisplacementTexture, material.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = await AsyncUpdateTexture(modelMaterial.RoughnessTexture, material.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = await AsyncUpdateTexture(modelMaterial.MetalnessTexture, material.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = await AsyncUpdateTexture(modelMaterial.EmissiveTexture, material.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = await AsyncUpdateTexture(modelMaterial.RoughnessMetalnessTexture, material.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = await AsyncUpdateTexture(modelMaterial.AoTexture, material.AoTextureMap);
            modelMaterial.EndUpdate();
        }

        public static void UnloadMaterial(ModelMaterial material)
        {
            material.RemoveRef();
            if (material.IsUsed) return;
            if (suppressCleanup) return;

            lock (materials)
            {
                materials.Remove(material.Name, out _);
                material.Dispose();
            }

            UnloadTexture(material.AlbedoTexture);
            UnloadTexture(material.NormalTexture);
            UnloadTexture(material.DisplacementTexture);
            UnloadTexture(material.RoughnessTexture);
            UnloadTexture(material.MetalnessTexture);
            UnloadTexture(material.EmissiveTexture);
            UnloadTexture(material.RoughnessMetalnessTexture);
            UnloadTexture(material.AoTexture);
        }

        public static ModelTexture? LoadTexture(string? name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (string.IsNullOrEmpty(name)) return null;
            ModelTexture? texture;
            lock (textures)
            {
                if (textures.TryGetValue(fullname, out texture))
                {
                    texture.Wait();
                    texture.AddRef();
                    return texture;
                }

                texture = new(fullname, 1);
                textures.TryAdd(fullname, texture);
            }

            var tex = device.LoadTexture2D(fullname);
            texture.EndLoad(device.CreateShaderResourceView(tex));
            tex.Dispose();

            return texture;
        }

        public static Task<ModelTexture?> AsyncLoadTexture(string? name)
        {
            return Task.Factory.StartNew(() => LoadTexture(name));
        }

        public static void UnloadTexture(ModelTexture? texture)
        {
            if (texture == null) return;
            texture.RemoveRef();
            if (!texture.IsUsed)
            {
                lock (textures)
                {
                    textures.Remove(texture.Name, out _);
                    texture.Dispose();
                }
            }
        }

        public static void UpdateTexture(ref ModelTexture? texture, string name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (texture?.Name == fullname) return;
            UnloadTexture(texture);
            texture = LoadTexture(name);
        }

        public static async Task<ModelTexture?> AsyncUpdateTexture(ModelTexture? texture, string name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (texture?.Name == fullname) return texture;
            UnloadTexture(texture);
            return await AsyncLoadTexture(name);
        }

        public static void Release()
        {
            foreach (var mesh in meshes.Values)
            {
                mesh.Dispose();
            }
            meshes.Clear();
            foreach (var material in materials.Values)
            {
                material.Dispose();
            }
            materials.Clear();
            foreach (var texture in textures.Values)
            {
                texture.Dispose();
            }
            textures.Clear();
        }
    }
}