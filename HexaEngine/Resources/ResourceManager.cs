namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.IO;
    using HexaEngine.Objects;
    using HexaEngine.Rendering.ConstantBuffers;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    public static class ResourceManager
    {
        private static readonly ConcurrentDictionary<string, Mesh> meshes = new();
        private static readonly ConcurrentDictionary<string, Material> materials = new();
        private static readonly ConcurrentDictionary<string, Texture> textures = new();
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

        public static bool GetMesh(MeshData mesh, [NotNullWhen(true)] out Mesh? model)
        {
            return meshes.TryGetValue(mesh.Name, out model);
        }

        public static Mesh LoadMesh(MeshData mesh)
        {
            lock (meshes)
            {
                if (meshes.TryGetValue(mesh.Name, out var value))
                    return value;
                Mesh model = new(device, mesh.Name, mesh.Vertices, mesh.Indices, mesh.BoundingBox, mesh.BoundingSphere);
                meshes.TryAdd(mesh.Name, model);
                return model;
            }
        }

        public static async Task<Mesh> LoadMeshAsync(MeshData mesh)
        {
            return await Task.Factory.StartNew(() => LoadMesh(mesh));
        }

        public static void UnloadMesh(MeshData mesh)
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

        public static void UnloadMesh(Mesh model)
        {
            if (suppressCleanup) return;
            lock (meshes)
            {
                meshes.Remove(model.Name, out _);
                model.Dispose();
            }
        }

        public static Material LoadMaterial(MaterialDesc material)
        {
            Material? modelMaterial;
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

        public static async Task<Material> LoadMaterialAsync(MaterialDesc material)
        {
            Material? modelMaterial;
            lock (materials)
            {
                if (materials.TryGetValue(material.Name, out modelMaterial))
                {
                    modelMaterial.AddRef();
                    return modelMaterial;
                }

                modelMaterial = new(material,
                    device.CreateBuffer((CBMaterial)material, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write),
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

        public static void UpdateMaterial(MaterialDesc desc)
        {
            Material? modelMaterial;
            lock (materials)
            {
                if (!materials.TryGetValue(desc.Name, out modelMaterial))
                {
                    return;
                }
            }

            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();
            UpdateTexture(ref modelMaterial.AlbedoTexture, desc.BaseColorTextureMap);
            UpdateTexture(ref modelMaterial.NormalTexture, desc.NormalTextureMap);
            UpdateTexture(ref modelMaterial.DisplacementTexture, desc.DisplacementTextureMap);
            UpdateTexture(ref modelMaterial.RoughnessTexture, desc.RoughnessTextureMap);
            UpdateTexture(ref modelMaterial.MetalnessTexture, desc.MetalnessTextureMap);
            UpdateTexture(ref modelMaterial.EmissiveTexture, desc.EmissiveTextureMap);
            UpdateTexture(ref modelMaterial.RoughnessMetalnessTexture, desc.RoughnessMetalnessTextureMap);
            UpdateTexture(ref modelMaterial.AoTexture, desc.AoTextureMap);
            modelMaterial.EndUpdate();
        }

        public static async Task AsyncUpdateMaterial(MaterialDesc desc)
        {
            Material? modelMaterial;
            lock (materials)
            {
                if (!materials.TryGetValue(desc.Name, out modelMaterial))
                {
                    return;
                }
            }

            modelMaterial.Update(desc);
            modelMaterial.BeginUpdate();
            modelMaterial.AlbedoTexture = await AsyncUpdateTexture(modelMaterial.AlbedoTexture, desc.BaseColorTextureMap);
            modelMaterial.NormalTexture = await AsyncUpdateTexture(modelMaterial.NormalTexture, desc.NormalTextureMap);
            modelMaterial.DisplacementTexture = await AsyncUpdateTexture(modelMaterial.DisplacementTexture, desc.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = await AsyncUpdateTexture(modelMaterial.RoughnessTexture, desc.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = await AsyncUpdateTexture(modelMaterial.MetalnessTexture, desc.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = await AsyncUpdateTexture(modelMaterial.EmissiveTexture, desc.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = await AsyncUpdateTexture(modelMaterial.RoughnessMetalnessTexture, desc.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = await AsyncUpdateTexture(modelMaterial.AoTexture, desc.AoTextureMap);
            modelMaterial.EndUpdate();
        }

        public static void UnloadMaterial(Material desc)
        {
            desc.RemoveRef();
            if (desc.IsUsed) return;
            if (suppressCleanup) return;

            lock (materials)
            {
                materials.Remove(desc.Name, out _);
                desc.Dispose();
            }

            UnloadTexture(desc.AlbedoTexture);
            UnloadTexture(desc.NormalTexture);
            UnloadTexture(desc.DisplacementTexture);
            UnloadTexture(desc.RoughnessTexture);
            UnloadTexture(desc.MetalnessTexture);
            UnloadTexture(desc.EmissiveTexture);
            UnloadTexture(desc.RoughnessMetalnessTexture);
            UnloadTexture(desc.AoTexture);
        }

        public static Texture? LoadTexture(string? name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (string.IsNullOrEmpty(name)) return null;
            Texture? texture;
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

        public static Task<Texture?> AsyncLoadTexture(string? name)
        {
            return Task.Factory.StartNew(() => LoadTexture(name));
        }

        public static void UnloadTexture(Texture? texture)
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

        public static void UpdateTexture(ref Texture? texture, string name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (texture?.Name == fullname) return;
            UnloadTexture(texture);
            texture = LoadTexture(name);
        }

        public static async Task<Texture?> AsyncUpdateTexture(Texture? texture, string name)
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