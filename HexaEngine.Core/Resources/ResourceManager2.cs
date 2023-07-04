namespace HexaEngine.Core.Resources
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Resources;

    public class ResourceManager2
    {
        private readonly List<ResourceRef> resources = new();
        private readonly Dictionary<string, ResourceRef> sharedResources = new();
        private readonly IGraphicsDevice device;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ResourceManager2 Shared { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public ResourceManager2(IGraphicsDevice device)
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

        public ResourceRef<ISamplerState> AddSamplerState(string name, SamplerDescription description)
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

        public ResourceRefNotNull<ISamplerState> GetOrAddSamplerState(string name, SamplerDescription description)
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

        public ResourceRef<Texture> AddTexture(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                Texture texture = new(device, description);
                return AddResource(name, texture);
            }
        }

        public ResourceRef<Texture> AddTexture(string name, TextureDescription description, Span<byte> rawPixelData, int rowPitch)
        {
            lock (sharedResources)
            {
                Texture texture = new(device, rawPixelData, rowPitch, description);
                return AddResource(name, texture);
            }
        }

        public ResourceRef<Texture> AddTexture(string name, TextureDescription description, Span<byte> rawPixelData, int rowPitch, int slicePitch)
        {
            lock (sharedResources)
            {
                Texture texture = new(device, rawPixelData, rowPitch, slicePitch, description);
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

        public ResourceRef<Texture> AddTextureColor(string name, TextureDimension dimension, Vector4 color)
        {
            lock (sharedResources)
            {
                Texture texture = new(device, dimension, color);
                return AddResource(name, texture);
            }
        }

        public ResourceRef<Texture> AddOrUpdateTextureColor(string name, TextureDimension dimension, Vector4 color)
        {
            lock (sharedResources)
            {
                if (TryGetResource<Texture>(name, out var resourceRef))
                {
                    var old = resourceRef.Value;
                    resourceRef.Value = new Texture(device, dimension, color);
                    old?.Dispose();
                    return new(resourceRef.Resource);
                }

                return AddResource(name, new Texture(device, dimension, color));
            }
        }

        public ResourceRef<Texture> GetTexture(string name)
        {
            lock (sharedResources)
            {
                return GetResource<Texture>(name);
            }
        }

        public ResourceRef<Texture> UpdateTexture(string name, TextureDescription description)
        {
            lock (sharedResources)
            {
                var resource = GetResource<Texture>(name);
                var old = resource.Value;
                resource.Value = new(device, description);
                old?.Dispose();
                return resource;
            }
        }

        public ResourceRef<Texture> UpdateTexture(string name, TextureFileDescription description)
        {
            lock (sharedResources)
            {
                var resource = GetResource<Texture>(name);
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