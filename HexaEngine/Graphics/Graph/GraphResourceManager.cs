namespace HexaEngine.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public class GraphResourceManager
    {
        private readonly List<ResourceRef> resources = new();
        private readonly Dictionary<string, ResourceRef> sharedResources = new();
        private readonly IGraphicsDevice device;

        public GraphResourceManager(IGraphicsDevice device)
        {
            this.device = device;
        }

        public IReadOnlyList<ResourceRef> Resources => resources;

        public ResourceRef GetResource(string name)
        {
            lock (sharedResources)
            {
                if (sharedResources.TryGetValue(name, out var resource))
                {
                    return resource;
                }
                return AddResource(name, null);
            }
        }

        public void SetResource(string name, IDisposable resource)
        {
            lock (sharedResources)
            {
                sharedResources[name].Value = resource;
            }
        }

        public void SetResource<T>(string name, T resource) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                sharedResources[name].Value = resource;
            }
        }

        public ResourceRefNotNull<T> SetOrAddResource<T>(string name, T resource) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                if (sharedResources.ContainsKey(name))
                {
                    sharedResources[name].Value = resource;
                    return new(sharedResources[name]);
                }
                else
                {
                    var resourceRef = AddResource(name, resource);
                    return new(resourceRef);
                }
            }
        }

        public ResourceRef<T> GetResource<T>(string name) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                sharedResources.TryGetValue(name, out var value);
                if (value == null)
                {
                    return new(AddResource(name, null));
                }
                return new(value);
            }
        }

        public bool TryGetResource<T>(string name, [NotNullWhen(true)] out ResourceRef<T>? value) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                sharedResources.TryGetValue(name, out ResourceRef? val);
                value = val != null ? new(val) : null;
                return val != null;
            }
        }

        private ResourceRef AddResource(string name, IDisposable? resource)
        {
            lock (sharedResources)
            {
                ResourceRef resourceRef = new(name) { Value = resource };
                resources.Add(resourceRef);
                sharedResources.Add(name, resourceRef);
                return resourceRef;
            }
        }

        private ResourceRefNotNull AddResourceNotNull(string name, IDisposable resource)
        {
            lock (sharedResources)
            {
                ResourceRef resourceRef = new(name) { Value = resource };
                resources.Add(resourceRef);
                sharedResources.Add(name, resourceRef);
                return new(resourceRef);
            }
        }

        private ResourceRef<T> AddResource<T>(string name, T? resource) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                if (sharedResources.TryGetValue(name, out var resourceRef))
                {
                    resourceRef.Value = resource;
                    return new(resourceRef);
                }
                else
                {
                    resourceRef = new(name) { Value = resource };
                    resources.Add(resourceRef);
                    sharedResources.Add(name, resourceRef);
                    return new(resourceRef);
                }
            }
        }

        private ResourceRefNotNull<T> AddResourceNotNull<T>(string name, T resource) where T : class, IDisposable
        {
            lock (sharedResources)
            {
                ResourceRef resourceRef = new(name) { Value = resource };
                resources.Add(resourceRef);
                sharedResources.Add(name, resourceRef);
                return new(resourceRef);
            }
        }

        public void RemoveResource(string name)
        {
            lock (sharedResources)
            {
                if (!sharedResources.ContainsKey(name))
                {
                    return;
                }

                var resource = sharedResources[name];
                resources.Remove(resource);
                sharedResources.Remove(name);
                resource.Value?.Dispose();
            }
        }

        public void TryRemoveResource(string name)
        {
            lock (sharedResources)
            {
                if (sharedResources.TryGetValue(name, out ResourceRef? value))
                {
                    var resource = value;
                    resources.Remove(resource);
                    sharedResources.Remove(name);
                    resource.Value?.Dispose();
                }
            }
        }

        public ResourceRef<IBuffer> GetBuffer(string name)
        {
            return GetResource<IBuffer>(name);
        }

        public ResourceRef<IConstantBuffer> AddConstantBuffer(string name, IConstantBuffer buffer)
        {
            return AddResource(name, buffer);
        }

        public ResourceRef<ConstantBuffer<T>> AddConstantBuffer<T>(string name, CpuAccessFlags cpuAccessFlags) where T : unmanaged
        {
            lock (sharedResources)
            {
                ConstantBuffer<T> buffer = new(device, cpuAccessFlags);
                return AddResource(name, buffer);
            }
        }

        public ResourceRef<ConstantBuffer<T>> SetOrAddConstantBuffer<T>(string name, CpuAccessFlags cpuAccessFlags) where T : unmanaged
        {
            lock (sharedResources)
            {
                if (TryGetResource<ConstantBuffer<T>>(name, out var resourceRef))
                {
                    var old = resourceRef.Value;
                    resourceRef.Value = new ConstantBuffer<T>(device, cpuAccessFlags);
                    old?.Dispose();
                    return resourceRef;
                }

                return AddResource(name, new ConstantBuffer<T>(device, cpuAccessFlags));
            }
        }

        public ResourceRef<ConstantBuffer<T>> AddConstantBuffer<T>(string name, T[] values, CpuAccessFlags cpuAccessFlags) where T : unmanaged
        {
            lock (sharedResources)
            {
                ConstantBuffer<T> buffer = new(device, values, cpuAccessFlags);
                return AddResource(name, buffer);
            }
        }

        public ResourceRef<ConstantBuffer<T>> AddConstantBuffer<T>(string name, uint length, CpuAccessFlags cpuAccessFlags) where T : unmanaged
        {
            lock (sharedResources)
            {
                ConstantBuffer<T> buffer = new(device, length, cpuAccessFlags);
                return AddResource(name, buffer);
            }
        }

        public ResourceRef<ISamplerState> AddSamplerState(string name, ISamplerState samplerState)
        {
            return AddResource(name, samplerState);
        }

        public ResourceRef<ISamplerState> AddSamplerState(string name, SamplerStateDescription description)
        {
            lock (sharedResources)
            {
                ISamplerState samplerState = device.CreateSamplerState(description);
                return AddResource(name, samplerState);
            }
        }

        public ResourceRef<ISamplerState> GetSamplerState(string name)
        {
            return GetResource<ISamplerState>(name);
        }

        public ResourceRefNotNull<ISamplerState> GetOrAddSamplerState(string name, SamplerStateDescription description)
        {
            if (TryGetResource<ISamplerState>(name, out var samplerState))
            {
                return new(samplerState);
            }

            lock (sharedResources)
            {
                var state = device.CreateSamplerState(description);
                return AddResourceNotNull(name, state);
            }
        }

        public ResourceRef<IShaderResourceView> AddShaderResourceView(string name, IShaderResourceView srv)
        {
            return AddResource(name, srv);
        }

        public ResourceRef<IShaderResourceView> GetShaderResourceView(string name)
        {
            return GetResource<IShaderResourceView>(name);
        }

        public ResourceRef<IDepthStencilView> AddDepthStencilView(string name, IDepthStencilView srv)
        {
            return AddResource(name, srv);
        }

        public ResourceRef<IDepthStencilView> GetDepthStencilView(string name)
        {
            return GetResource<IDepthStencilView>(name);
        }

        public ResourceRef<GBuffer> AddGBuffer(string name, GBuffer gbuffer)
        {
            return AddResource(name, gbuffer);
        }

        public ResourceRef<GBuffer> GetGBuffer(string name)
        {
            return GetResource<GBuffer>(name);
        }

        public ResourceRef<Texture2D> AddTexture(string name, Texture2DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock (sharedResources)
            {
                Texture2D texture = new(device, description, filename, lineNumber);
                return AddResource(name, texture);
            }
        }

        public ResourceRef<Texture2D> AddTextureFile(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                Texture2D texture = new(device, description);
                return AddResource(name, texture);
            }
        }

        public ResourceRef<Texture2D> AddOrUpdateTextureFile(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                if (TryGetResource<Texture2D>(name, out var resourceRef))
                {
                    var old = resourceRef.Value;
                    resourceRef.Value = new Texture2D(device, description);
                    old?.Dispose();
                    return new(resourceRef.Resource);
                }

                return AddResource(name, new Texture2D(device, description));
            }
        }

        public ResourceRef<Texture2D> GetTexture(string name)
        {
            lock (sharedResources)
            {
                return GetResource<Texture2D>(name);
            }
        }

        public ResourceRef<Texture2D> UpdateTexture(string name, Texture2DDescription description, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            lock (sharedResources)
            {
                var resource = GetResource<Texture2D>(name);
                var old = resource.Value;
                resource.Value = new(device, description, filename, lineNumber);
                old?.Dispose();
                return resource;
            }
        }

        public ResourceRef<Texture2D> UpdateTexture(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                var resource = GetResource<Texture2D>(name);
                var old = resource.Value;
                resource.Value = new(device, description);
                old?.Dispose();
                return resource;
            }
        }

        internal void Dispose()
        {
            foreach (var resource in resources)
            {
                resource.Value?.Dispose();
            }
            resources.Clear();
            sharedResources.Clear();
        }
    }
}