namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using System.Collections.Concurrent;

    public static class ResourceManager
    {
        private static readonly ConcurrentDictionary<string, ResourceInstance<Mesh>> meshes = new();
        private static readonly ConcurrentDictionary<string, Material> materials = new();
        private static readonly ConcurrentDictionary<string, ResourceInstance<MaterialTexture>> textures = new();
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

            // TODO: Handle different vertex types.
            Mesh model = new(device, mesh);
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

        public static Material LoadMaterial(MeshData mesh, MaterialData desc)
        {
            Material? modelMaterial;
            lock (materials)
            {
                if (materials.TryGetValue(desc.Name, out var model))
                {
                    model.AddRef();
                    return model;
                }

                modelMaterial = new(desc);
                modelMaterial.AddRef();
                materials.TryAdd(desc.Name, modelMaterial);
            }

            modelMaterial.Shader = LoadMaterialShader(mesh, desc);
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(LoadTexture(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();

            return modelMaterial;
        }

        public static async Task<Material> LoadMaterialAsync(MeshData mesh, MaterialData desc)
        {
            Material? modelMaterial;
            lock (materials)
            {
                if (materials.TryGetValue(desc.Name, out modelMaterial))
                {
                    modelMaterial.AddRef();
                    return modelMaterial;
                }

                modelMaterial = new(desc);
                modelMaterial.AddRef();
                materials.TryAdd(desc.Name, modelMaterial);
            }

            modelMaterial.Shader = await LoadMaterialShaderAsync(mesh, desc);

            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(await LoadTextureAsync(desc.Textures[i]));
            }

            modelMaterial.EndUpdate();

            return modelMaterial;
        }

        public static void UpdateMaterial(MeshData mesh, MaterialData desc)
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
            UpdateMaterialShader(ref modelMaterial.Shader, mesh, desc);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                UnloadTexture(modelMaterial.TextureList[i]);
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(LoadTexture(desc.Textures[i]));
            }

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

        public static async Task UpdateMaterialAsync(MeshData mesh, MaterialData desc)
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

            modelMaterial.Shader = await UpdateMaterialShaderAsync(modelMaterial.Shader, mesh, desc);

            for (int i = 0; i < modelMaterial.TextureList.Count; i++)
            {
                UnloadTexture(modelMaterial.TextureList[i]);
            }
            modelMaterial.TextureList.Clear();
            for (int i = 0; i < desc.Textures.Length; i++)
            {
                modelMaterial.TextureList.Add(await LoadTextureAsync(desc.Textures[i]));
            }

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
            for (int i = 0; i < desc.TextureList.Count; i++)
            {
                UnloadTexture(desc.TextureList[i]);
            }
        }

        public static ResourceInstance<MaterialTexture>? LoadTexture(MaterialTextureDesc desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (string.IsNullOrWhiteSpace(desc.File)) return null;
            ResourceInstance<MaterialTexture>? texture;
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
            var srv = device.CreateShaderResourceView(tex);
            var sampler = device.CreateSamplerState(desc.GetSamplerDesc());
            texture.EndLoad(new(srv, sampler, desc));
            tex.Dispose();

            return texture;
        }

        public static Task<ResourceInstance<MaterialTexture>?> LoadTextureAsync(MaterialTextureDesc desc)
        {
            return Task.Factory.StartNew(() => LoadTexture(desc));
        }

        public static void UnloadTexture(ResourceInstance<MaterialTexture>? texture)
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

        public static void UpdateTexture(ref ResourceInstance<MaterialTexture>? texture, MaterialTextureDesc desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (texture?.Name == fullname) return;
            UnloadTexture(texture);
            texture = LoadTexture(desc);
        }

        public static async Task<ResourceInstance<MaterialTexture>?> UpdateTextureAsync(ResourceInstance<MaterialTexture>? texture, MaterialTextureDesc desc)
        {
            string fullname = Paths.CurrentTexturePath + desc.File;
            if (texture?.Name == fullname) return texture;
            UnloadTexture(texture);
            return await LoadTextureAsync(desc);
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

        public static Task<ResourceInstance<IGraphicsPipeline>?> LoadPipelineAsync(string name, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
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

        public static ResourceInstance<MaterialShader>? LoadMaterialShader(MeshData mesh, MaterialData material)
        {
            ResourceInstance<MaterialShader>? shader;
            lock (shaders)
            {
                if (shaders.TryGetValue(material.Name, out shader))
                {
                    shader.Wait();
                    shader.AddRef();
                    return shader;
                }

                shader = new(material.Name, 1);
                shaders.TryAdd(material.Name, shader);
            }

            var shad = new MaterialShader();
            shad.Initialize(device, mesh, material);
            shader.EndLoad(shad);

            return shader;
        }

        public static async Task<ResourceInstance<MaterialShader>?> LoadMaterialShaderAsync(MeshData mesh, MaterialData material)
        {
            ResourceInstance<MaterialShader>? shader;
            lock (shaders)
            {
                if (shaders.TryGetValue(material.Name, out shader))
                {
                    shader.Wait();
                    shader.AddRef();
                    return shader;
                }

                shader = new(material.Name, 1);
                shaders.TryAdd(material.Name, shader);
            }

            var shad = new MaterialShader();
            await shad.InitializeAsync(device, mesh, material);
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

        public static void UpdateMaterialShader(ref ResourceInstance<MaterialShader>? shader, MeshData mesh, MaterialData material)
        {
            if (shader == null)
            {
                shader = LoadMaterialShader(mesh, material);
                return;
            }

            var shad = new MaterialShader();
            shad.Initialize(device, mesh, material);
            shader.BeginLoad();
            shader.EndLoad(shad);
        }

        public static async Task<ResourceInstance<MaterialShader>?> UpdateMaterialShaderAsync(ResourceInstance<MaterialShader>? pipeline, MeshData mesh, MaterialData material)
        {
            if (pipeline == null)
            {
                return await LoadMaterialShaderAsync(mesh, material);
            }

            var shad = new MaterialShader();
            await shad.InitializeAsync(device, mesh, material);
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