namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Meshes;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    public static class ResourceManager
    {
        private static readonly ConcurrentDictionary<string, Mesh> meshes = new();
        private static readonly ConcurrentDictionary<string, Material> materials = new();
        private static readonly ConcurrentDictionary<string, Texture> textures = new();
        private static readonly List<IDisposable> resources = new();
        private static readonly Dictionary<string, IDisposable> sharedResources = new();
        private static readonly Dictionary<string, TaskCompletionSource> waitingHandles = new();
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

        public static bool GetMesh(Meshes.MeshData mesh, [NotNullWhen(true)] out Mesh? model)
        {
            return meshes.TryGetValue(mesh.Name, out model);
        }

        public static unsafe Mesh LoadMesh(MeshSource mesh)
        {
            lock (meshes)
            {
                if (meshes.TryGetValue(mesh.Path, out var value))
                    return value;
                var source = mesh.ReadMesh();
                Mesh model = new(device, mesh.Path, source.Vertices, source.Indices, mesh.Header.BoundingBox, mesh.Header.BoundingSphere);
                meshes.TryAdd(mesh.Path, model);
                return model;
            }
        }

        public static unsafe Mesh LoadMesh(string path)
        {
            lock (meshes)
            {
                if (meshes.TryGetValue(path, out var value))
                    return value;
                Mesh mesh = new(device, path);
                meshes.TryAdd(path, mesh);
                return mesh;
            }
        }

        public static async Task<Mesh> LoadMeshAsync(string mesh)
        {
            return await Task.Factory.StartNew(() => LoadMesh(mesh));
        }

        public static async Task<Mesh> LoadMeshAsync(MeshSource mesh)
        {
            return await Task.Factory.StartNew(() => LoadMesh(mesh));
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
                modelMaterial.AddRef();
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
                modelMaterial.AddRef();
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

        private static void SignalWaitHandle(string name)
        {
#if VERBOSE
            Debug.WriteLine($"Signal {name}");
#endif
            GetWaitHandle(name).SetResult();
        }

        private static void ResetWaitHandle(string name)
        {
#if VERBOSE
            Debug.WriteLine($"Reset {name}");
#endif
            lock (waitingHandles)
            {
                waitingHandles.Remove(name);
                waitingHandles.Add(name, new(TaskCreationOptions.None));
            }
        }

        private static void WaitForWaitHandle(string name)
        {
#if VERBOSE
            Debug.WriteLine($"Wait {name}");
#endif
            GetWaitHandle(name).Task.Wait();
        }

        private static Task WaitForWaitHandleAsync(string name)
        {
            return GetWaitHandle(name).Task;
        }

        public static Graphics.Texture GetOrWait(string name)
        {
            // Wait for the resource creation.
            WaitForWaitHandle(name);

            lock (sharedResources)
            {
                if (sharedResources[name] is Graphics.Texture texture)
                    return texture;
                else
                    throw new InvalidCastException();
            }
        }

        public static async Task<Graphics.Texture> GetOrWaitAsync(string name)
        {
            // Wait for the resource creation.
            await WaitForWaitHandleAsync(name);

            lock (sharedResources)
            {
                if (sharedResources[name] is Graphics.Texture texture)
                    return texture;
                else
                    throw new InvalidCastException();
            }
        }

        public static void RequireUpdate(string name)
        {
            ResetWaitHandle(name);
        }

        public static object GetResource(string name)
        {
            lock (sharedResources)
            {
                WaitForWaitHandle(name);
                return sharedResources[name];
            }
        }

        public static void SetResource(string name, IDisposable resource)
        {
            lock (sharedResources)
            {
                sharedResources[name] = resource;
                SignalWaitHandle(name);
            }
        }

        public static void SetResource<T>(string name, T resource) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                sharedResources[name] = resource;
                SignalWaitHandle(name);
            }
        }

        public static void SetOrAddResource<T>(string name, T resource) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                if (sharedResources.ContainsKey(name))
                    sharedResources[name] = resource;
                else
                    sharedResources.Add(name, resource);
                SignalWaitHandle(name);
            }
        }

        public static T? GetResource<T>(string name) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                WaitForWaitHandle(name);
                sharedResources.TryGetValue(name, out IDisposable? value);
                return value is T t ? t : null;
            }
        }

        public static async Task<T?> GetResourceAsync<T>(string name) where T : class, IDisposable
        {
            await WaitForWaitHandleAsync(name);
            lock (sharedResources)
            {
                sharedResources.TryGetValue(name, out IDisposable? value);
                return value is T t ? t : null;
            }
        }

        public static bool TryGetResource<T>(string name, [NotNullWhen(true)] out T? value, bool dontWait = false) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                if (!dontWait)
                    WaitForWaitHandle(name);
                sharedResources.TryGetValue(name, out IDisposable? val);
                value = val is T t ? t : null;
                return value != null;
            }
        }

        public static async Task<(bool, T?)> TryGetResourceAsync<T>(string name, bool dontWait = false) where T : class, IDisposable
        {
            if (!dontWait)
                await WaitForWaitHandleAsync(name);
            lock (sharedResources)
            {
                sharedResources.TryGetValue(name, out IDisposable? val);
                if (val is T t)
                {
                    return (true, t);
                }
                return (false, null);
            }
        }

        private static void AddResource(string name, IDisposable resource)
        {
            lock (sharedResources)
            {
                resources.Add(resource);
                sharedResources.Add(name, resource);
                SignalWaitHandle(name);
            }
        }

        public static void RemoveResource(string name)
        {
            lock (sharedResources)
            {
                var resource = sharedResources[name];
                resources.Remove(resource);
                sharedResources.Remove(name);
                waitingHandles.Remove(name);
                resource.Dispose();
            }
        }

        public static void TryRemoveResource(string name)
        {
            lock (sharedResources)
            {
                if (sharedResources.TryGetValue(name, out IDisposable? value))
                {
                    var resource = value;
                    resources.Remove(resource);
                    sharedResources.Remove(name);
                    waitingHandles.Remove(name);
                    resource.Dispose();
                }
            }
        }

        private static TaskCompletionSource GetWaitHandle(string name)
        {
            // Create an wait handle if there is none
            TaskCompletionSource? handle;
            lock (waitingHandles)
            {
                if (!waitingHandles.TryGetValue(name, out handle))
                {
                    handle = new();
                    waitingHandles.Add(name, handle);
                }
            }
            return handle;
        }

        public static IBuffer? GetBuffer(string name)
        {
            return GetResource<IBuffer>(name);
        }

        public static Task<IBuffer?> GetBufferAsync(string name)
        {
            return GetResourceAsync<IBuffer>(name);
        }

        public static IBuffer? GetConstantBuffer(string name)
        {
            return GetResource<IConstantBuffer>(name)?.Buffer;
        }

        public static async Task<IBuffer?> GetConstantBufferAsync(string name)
        {
            return (await GetResourceAsync<IConstantBuffer>(name))?.Buffer;
        }

        public static void AddConstantBuffer(string name, IConstantBuffer buffer)
        {
            AddResource(name, buffer);
        }

        public static ConstantBuffer<T> AddConstantBuffer<T>(string name, CpuAccessFlags cpuAccessFlags) where T : unmanaged
        {
            lock (sharedResources)
            {
                ConstantBuffer<T> buffer = new(device, cpuAccessFlags);
                AddResource(name, buffer);
                return buffer;
            }
        }

        public static ConstantBuffer<T> AddConstantBuffer<T>(string name, T[] values, CpuAccessFlags cpuAccessFlags) where T : unmanaged
        {
            lock (sharedResources)
            {
                ConstantBuffer<T> buffer = new(device, values, cpuAccessFlags);
                AddResource(name, buffer);
                return buffer;
            }
        }

        public static ConstantBuffer<T> AddConstantBuffer<T>(string name, uint length, CpuAccessFlags cpuAccessFlags) where T : unmanaged
        {
            lock (sharedResources)
            {
                ConstantBuffer<T> buffer = new(device, length, cpuAccessFlags);
                AddResource(name, buffer);
                return buffer;
            }
        }

        public static void AddSamplerState(string name, ISamplerState samplerState)
        {
            AddResource(name, samplerState);
        }

        public static ISamplerState AddSamplerState(string name, SamplerDescription description)
        {
            lock (sharedResources)
            {
                ISamplerState samplerState = device.CreateSamplerState(description);
                AddResource(name, samplerState);
                return samplerState;
            }
        }

        public static ISamplerState? GetSamplerState(string name)
        {
            return GetResource<ISamplerState>(name);
        }

        public static Task<ISamplerState?> GetSamplerStateAsync(string name)
        {
            return GetResourceAsync<ISamplerState>(name);
        }

        public static ISamplerState GetOrAddSamplerState(string name, SamplerDescription description)
        {
            if (TryGetResource<ISamplerState>(name, out var samplerState, true))
            {
                return samplerState;
            }

            lock (sharedResources)
            {
                samplerState = device.CreateSamplerState(description);
                AddResource(name, samplerState);
                return samplerState;
            }
        }

        public static void AddShaderResourceView(string name, IShaderResourceView srv)
        {
            AddResource(name, srv);
        }

        public static void AddDepthStencilView(string name, IDepthStencilView srv)
        {
            AddResource(name, srv);
        }

        public static IDepthStencilView? GetDepthStencilView(string name)
        {
            return GetResource<IDepthStencilView>(name);
        }

        public static Task<IDepthStencilView?> GetDepthStencilViewAsync(string name)
        {
            return GetResourceAsync<IDepthStencilView>(name);
        }

        public static void AddTextureArray(string name, TextureArray srv)
        {
            AddResource(name, srv);
        }

        public static TextureArray AddTextureArray(string name, int width, int height, uint count, Format format)
        {
            lock (sharedResources)
            {
                TextureArray textureArray = new(device, width, height, count, format);
                AddResource(name, textureArray);
                return textureArray;
            }
        }

        public static TextureArray? GetTextureArray(string name)
        {
            return GetResource<TextureArray>(name);
        }

        public static Task<TextureArray?> GetTextureArrayAsync(string name)
        {
            return GetResourceAsync<TextureArray>(name);
        }

        public static IRenderTargetView[]? GetTextureArrayRTV(string name)
        {
            return GetResource<TextureArray>(name)?.RTVs;
        }

        public static async Task<IRenderTargetView[]?> GetTextureArrayRTVAsync(string name)
        {
            return (await GetResourceAsync<TextureArray>(name))?.RTVs;
        }

        public static IShaderResourceView[]? GetTextureArraySRV(string name)
        {
            return GetResource<TextureArray>(name)?.SRVs;
        }

        public static async Task<IShaderResourceView[]?> GetTextureArraySRVAsync(string name)
        {
            return (await GetResourceAsync<TextureArray>(name))?.SRVs;
        }

        public static Graphics.Texture AddTexture(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture;
            }
        }

        public static Graphics.Texture AddTexture(string name, TextureDescription description, Span<byte> rawPixelData, int rowPitch)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, rawPixelData, rowPitch, description);
                AddResource(name, texture);
                return texture;
            }
        }

        public static Graphics.Texture AddTexture(string name, TextureDescription description, Span<byte> rawPixelData, int rowPitch, int slicePitch)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, rawPixelData, rowPitch, slicePitch, description);
                AddResource(name, texture);
                return texture;
            }
        }

        public static IRenderTargetView? AddTextureRTV(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture.RenderTargetView;
            }
        }

        public static IShaderResourceView? AddTextureSRV(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture.ShaderResourceView;
            }
        }

        public static IDepthStencilView? AddTextureDSV(string name, TextureDescription description, DepthStencilDesc depthStencilDesc)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, description, depthStencilDesc);
                AddResource(name, texture);
                return texture.DepthStencilView;
            }
        }

        public static Graphics.Texture AddTextureFile(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture;
            }
        }

        public static Graphics.Texture AddOrUpdateTextureFile(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                TryRemoveResource(name);
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture;
            }
        }

        public static Graphics.Texture AddTextureColor(string name, TextureDimension dimension, Vector4 color)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, dimension, color);
                AddResource(name, texture);
                return texture;
            }
        }

        public static Graphics.Texture AddOrUpdateTextureColor(string name, TextureDimension dimension, Vector4 color)
        {
            lock (sharedResources)
            {
                TryRemoveResource(name);
                Graphics.Texture texture = new(device, dimension, color);
                AddResource(name, texture);
                return texture;
            }
        }

        public static IRenderTargetView? AddTextureFileRTV(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture.RenderTargetView;
            }
        }

        public static IShaderResourceView? AddTextureFileSRV(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture.ShaderResourceView;
            }
        }

        public static IRenderTargetView? UpdateTextureRTV(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture, true))
                {
                    if (texture.Description != description)
                    {
                        texture = UpdateTexture(name, description);
                    }
                    return texture.RenderTargetView;
                }
                texture = new(device, description);
                AddResource(name, texture);
                return texture.RenderTargetView;
            }
        }

        public static IShaderResourceView? UpdateTextureSRV(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture, true))
                {
                    if (texture.Description != description)
                    {
                        texture = UpdateTexture(name, description);
                    }
                    return texture.ShaderResourceView;
                }
                texture = new(device, description);
                AddResource(name, texture);
                return texture.ShaderResourceView;
            }
        }

        public static IRenderTargetView? GetTextureRTV(string name)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture))
                {
                    return texture.RenderTargetView;
                }

                return null;
            }
        }

        public static IShaderResourceView? GetTextureSRV(string name)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture))
                {
                    return texture.ShaderResourceView;
                }

                return null;
            }
        }

        public static IDepthStencilView? GetTextureDSV(string name)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture))
                {
                    return texture.DepthStencilView;
                }

                return null;
            }
        }

        public static Graphics.Texture? GetTexture(string name)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture))
                {
                    return texture;
                }
                return null;
            }
        }

        public static IRenderTargetView? GetTextureFileRTV(string name)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture))
                {
                    return texture.RenderTargetView;
                }

                return null;
            }
        }

        public static IShaderResourceView? GetTextureFileSRV(string name)
        {
            lock (sharedResources)
            {
                if (TryGetResource(name, out Graphics.Texture? texture))
                {
                    return texture.ShaderResourceView;
                }

                return null;
            }
        }

        public static async Task<IRenderTargetView?> GetTextureRTVAsync(string name)
        {
            var result = await TryGetResourceAsync<Graphics.Texture>(name, false);
            if (result.Item1)
            {
                return result.Item2?.RenderTargetView;
            }
            return null;
        }

        public static async Task<IShaderResourceView?> GetTextureSRVAsync(string name)
        {
            var result = await TryGetResourceAsync<Graphics.Texture>(name, false);
            if (result.Item1)
            {
                return result.Item2?.ShaderResourceView;
            }
            return null;
        }

        public static async Task<IDepthStencilView?> GetTextureDSVAsync(string name)
        {
            var result = await TryGetResourceAsync<Graphics.Texture>(name, false);
            if (result.Item1)
            {
                return result.Item2?.DepthStencilView;
            }
            return null;
        }

        public static async Task<Graphics.Texture?> GetTextureAsync(string name)
        {
            var result = await TryGetResourceAsync<Graphics.Texture>(name, false);
            if (result.Item1)
            {
                return result.Item2;
            }
            return null;
        }

        public static Graphics.Texture UpdateTexture(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                RemoveResource(name);
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture;
            }
        }

        public static Graphics.Texture UpdateTexture(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                RemoveResource(name);
                Graphics.Texture texture = new(device, description);
                AddResource(name, texture);
                return texture;
            }
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

        public static void ReleaseShared()
        {
            foreach (var resource in resources)
            {
                resource.Dispose();
            }
            resources.Clear();
            sharedResources.Clear();
        }
    }
}