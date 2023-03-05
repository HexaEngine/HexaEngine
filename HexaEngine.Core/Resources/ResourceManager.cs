namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Instances;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Pipelines.Deferred;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    public static class ResourceManager
    {
        private static readonly ConcurrentDictionary<string, ResourceInstance<Mesh>> meshes = new();
        private static readonly ConcurrentDictionary<string, Material> materials = new();
        private static readonly ConcurrentDictionary<string, ResourceInstance<IShaderResourceView>> textures = new();
        private static readonly ConcurrentDictionary<string, ResourceInstance<MaterialShader>> shaders = new();
        private static readonly ConcurrentDictionary<string, ResourceInstance<IGraphicsPipeline>> pipelines = new();
        private static bool suppressCleanup;
#nullable disable
        private static IGraphicsDevice device;
#nullable enable

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

        public static unsafe ResourceInstance<Mesh> LoadMesh(MeshData mesh)
        {
            ResourceInstance<Mesh> instance;
            lock (meshes)
            {
                if (meshes.TryGetValue(mesh.Name, out var value))
                {
                    value.AddRef();
                    return value;
                }

                instance = new(mesh.Name, 1);
                meshes.TryAdd(mesh.Name, instance);
            }

            Mesh model = new(device, mesh.Name, mesh.Vertices, mesh.Indices, mesh.Box, mesh.Sphere);
            instance.BeginLoad();
            instance.EndLoad(model);

            return instance;
        }

        public static async Task<ResourceInstance<Mesh>> LoadMeshAsync(MeshData mesh)
        {
            return await Task.Factory.StartNew(() => LoadMesh(mesh));
        }

        public static void UnloadMesh(ResourceInstance<Mesh> mesh)
        {
            mesh.RemoveRef();
            if (mesh.IsUsed) return;
            if (suppressCleanup) return;
            lock (meshes)
            {
                meshes.Remove(mesh.Name, out _);
                mesh.Dispose();
            }
        }

        public static Material LoadMaterial(MaterialData material)
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
                modelMaterial.AddRef();
                materials.TryAdd(material.Name, modelMaterial);
            }

            modelMaterial.Shader = LoadMaterialShader(material);
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

        public static async Task<Material> LoadMaterialAsync(MaterialData material)
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
                modelMaterial.AddRef();
                materials.TryAdd(material.Name, modelMaterial);
            }

            modelMaterial.Shader = await LoadMaterialShaderAsync(material);
            modelMaterial.AlbedoTexture = await LoadTextureAsync(material.BaseColorTextureMap);
            modelMaterial.NormalTexture = await LoadTextureAsync(material.NormalTextureMap);
            modelMaterial.DisplacementTexture = await LoadTextureAsync(material.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = await LoadTextureAsync(material.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = await LoadTextureAsync(material.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = await LoadTextureAsync(material.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = await LoadTextureAsync(material.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = await LoadTextureAsync(material.AoTextureMap);
            modelMaterial.EndUpdate();

            return modelMaterial;
        }

        public static void UpdateMaterial(MaterialData desc)
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
            UpdateMaterialShader(ref modelMaterial.Shader, desc);
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

        internal static void RenameMaterial(string oldName, string newName)
        {
            lock (materials)
            {
                if (materials.Remove(oldName, out var material))
                {
                    materials.TryAdd(newName, material);
                }
            }
        }

        public static async Task UpdateMaterialAsync(MaterialData desc)
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
            modelMaterial.Shader = await UpdateMaterialShaderAsync(modelMaterial.Shader, desc);
            modelMaterial.AlbedoTexture = await UpdateTextureAsync(modelMaterial.AlbedoTexture, desc.BaseColorTextureMap);
            modelMaterial.NormalTexture = await UpdateTextureAsync(modelMaterial.NormalTexture, desc.NormalTextureMap);
            modelMaterial.DisplacementTexture = await UpdateTextureAsync(modelMaterial.DisplacementTexture, desc.DisplacementTextureMap);
            modelMaterial.RoughnessTexture = await UpdateTextureAsync(modelMaterial.RoughnessTexture, desc.RoughnessTextureMap);
            modelMaterial.MetalnessTexture = await UpdateTextureAsync(modelMaterial.MetalnessTexture, desc.MetalnessTextureMap);
            modelMaterial.EmissiveTexture = await UpdateTextureAsync(modelMaterial.EmissiveTexture, desc.EmissiveTextureMap);
            modelMaterial.RoughnessMetalnessTexture = await UpdateTextureAsync(modelMaterial.RoughnessMetalnessTexture, desc.RoughnessMetalnessTextureMap);
            modelMaterial.AoTexture = await UpdateTextureAsync(modelMaterial.AoTexture, desc.AoTextureMap);
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

            UnloadMaterialShader(desc.Shader);
            UnloadTexture(desc.AlbedoTexture);
            UnloadTexture(desc.NormalTexture);
            UnloadTexture(desc.DisplacementTexture);
            UnloadTexture(desc.RoughnessTexture);
            UnloadTexture(desc.MetalnessTexture);
            UnloadTexture(desc.EmissiveTexture);
            UnloadTexture(desc.RoughnessMetalnessTexture);
            UnloadTexture(desc.AoTexture);
        }

        public static ResourceInstance<IShaderResourceView>? LoadTexture(string? name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (string.IsNullOrEmpty(name)) return null;
            ResourceInstance<IShaderResourceView>? texture;
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

        public static Task<ResourceInstance<IShaderResourceView>?> LoadTextureAsync(string? name)
        {
            return Task.Factory.StartNew(() => LoadTexture(name));
        }

        public static void UnloadTexture(ResourceInstance<IShaderResourceView>? texture)
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

        public static void UpdateTexture(ref ResourceInstance<IShaderResourceView>? texture, string name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (texture?.Name == fullname) return;
            UnloadTexture(texture);
            texture = LoadTexture(name);
        }

        public static async Task<ResourceInstance<IShaderResourceView>?> UpdateTextureAsync(ResourceInstance<IShaderResourceView>? texture, string name)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (texture?.Name == fullname) return texture;
            UnloadTexture(texture);
            return await LoadTextureAsync(name);
        }

        public static ResourceInstance<IGraphicsPipeline>? LoadPipeline(string name, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            string fullname = Paths.CurrentTexturePath + name;
            if (string.IsNullOrEmpty(name)) return null;
            ResourceInstance<IGraphicsPipeline>? pipeline;
            lock (pipelines)
            {
                if (pipelines.TryGetValue(fullname, out pipeline))
                {
                    pipeline.Wait();
                    pipeline.AddRef();
                    return pipeline;
                }

                pipeline = new(fullname, 1);
                pipelines.TryAdd(fullname, pipeline);
            }

            var pipe = device.CreateGraphicsPipeline(desc, state, macros);
            pipeline.EndLoad(pipe);

            return pipeline;
        }

        public static Task<ResourceInstance<IGraphicsPipeline>?> LoadPipelineAsync(string? name, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            return Task.Factory.StartNew(() => LoadPipeline(name, desc, state, macros));
        }

        public static void UnloadPipeline(ResourceInstance<IGraphicsPipeline>? pipeline)
        {
            if (pipeline == null) return;
            pipeline.RemoveRef();
            if (!pipeline.IsUsed)
            {
                lock (pipelines)
                {
                    pipelines.Remove(pipeline.Name, out _);
                    pipeline.Dispose();
                }
            }
        }

        public static void UpdatePipeline(ref ResourceInstance<IGraphicsPipeline>? pipeline, string name, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            if (pipeline == null)
            {
                pipeline = LoadPipeline(name, desc, state, macros);
                return;
            }

            var pipe = device.CreateGraphicsPipeline(desc, state);
            pipeline.BeginLoad();
            pipeline.EndLoad(pipe);
        }

        public static async Task<ResourceInstance<IGraphicsPipeline>?> UpdatePipelineAsync(ResourceInstance<IGraphicsPipeline>? pipeline, string name, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            if (pipeline == null)
            {
                return await LoadPipelineAsync(name, desc, state, macros);
            }

            var pipe = await device.CreateGraphicsPipelineAsync(desc, state, macros);
            pipeline.BeginLoad();
            pipeline.EndLoad(pipe);
            return pipeline;
        }

        #region Shader

        public static ResourceInstance<MaterialShader>? LoadMaterialShader(MaterialData data)
        {
            ResourceInstance<MaterialShader>? shader;
            lock (shaders)
            {
                if (shaders.TryGetValue(data.Name, out shader))
                {
                    shader.Wait();
                    shader.AddRef();
                    return shader;
                }

                shader = new(data.Name, 1);
                shaders.TryAdd(data.Name, shader);
            }

            var shad = new MaterialShader();
            shad.Initialize(device, data);
            shader.EndLoad(shad);

            return shader;
        }

        public static async Task<ResourceInstance<MaterialShader>?> LoadMaterialShaderAsync(MaterialData data)
        {
            ResourceInstance<MaterialShader>? shader;
            lock (shaders)
            {
                if (shaders.TryGetValue(data.Name, out shader))
                {
                    shader.Wait();
                    shader.AddRef();
                    return shader;
                }

                shader = new(data.Name, 1);
                shaders.TryAdd(data.Name, shader);
            }

            var shad = new MaterialShader();
            await shad.InitializeAsync(device, data);
            shader.EndLoad(shad);

            return shader;
        }

        public static void UnloadMaterialShader(ResourceInstance<MaterialShader>? shader)
        {
            if (shader == null) return;
            shader.RemoveRef();
            if (!shader.IsUsed)
            {
                lock (shaders)
                {
                    shaders.Remove(shader.Name, out _);
                    shader.Dispose();
                }
            }
        }

        public static void UpdateMaterialShader(ref ResourceInstance<MaterialShader>? shader, MaterialData data)
        {
            if (shader == null)
            {
                shader = LoadMaterialShader(data);
                return;
            }

            var shad = new MaterialShader();
            shad.Initialize(device, data);
            shader.BeginLoad();
            shader.EndLoad(shad);
        }

        public static async Task<ResourceInstance<MaterialShader>?> UpdateMaterialShaderAsync(ResourceInstance<MaterialShader>? pipeline, MaterialData data)
        {
            if (pipeline == null)
            {
                return await LoadMaterialShaderAsync(data);
            }

            var shad = new MaterialShader();
            await shad.InitializeAsync(device, data);
            pipeline.BeginLoad();
            pipeline.EndLoad(shad);
            return pipeline;
        }

        #endregion Shader

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

            foreach (var pipeline in pipelines.Values)
            {
                pipeline.Dispose();
            }
            pipelines.Clear();
        }

        public static void Dispose()
        {
            Release();
        }
    }
}