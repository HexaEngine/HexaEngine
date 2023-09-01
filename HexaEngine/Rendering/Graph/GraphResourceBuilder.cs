namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using System;
    using System.Diagnostics.CodeAnalysis;

    public struct ResourceRefBinding<T> where T : class, INative, IDisposable
    {
        public ResourceRef<T> Ref;
        public uint Slot;
        public ShaderStage Stage;
    }

    public class ShaderResourceViewCollection
    {
        private List<ResourceRefBinding<IShaderResourceView>> bindings = new();
    }

    public class GraphResourceBuilder : IGraphResourceBuilder
    {
        private readonly List<ResourceRef> resources = new();
        private readonly Dictionary<string, ResourceRef> nameToResource = new();
        private readonly IGraphicsDevice device;
        private Viewport viewport;
        private Viewport outputViewport;

        public GraphResourceBuilder(IGraphicsDevice device)
        {
            this.device = device;
        }

        private readonly List<IStructuredUavBuffer> structuredUavBuffers = new();

        private readonly List<IStructuredBuffer> structuredBuffers = new();

        private readonly List<IConstantBuffer> constantBuffers = new();

        private readonly List<ShadowAtlas> shadowAtlas = new();
        private readonly List<LazyInitDesc<ShadowAtlasDescription>> lazyShadowAtlas = new();

        private readonly List<DepthMipChain> depthMipChains = new();
        private readonly List<LazyInitDesc<DepthStencilBufferDescription>> lazyDepthMipChains = new();

        private readonly List<DepthStencil> depthStencilBuffers = new();
        private readonly List<LazyInitDesc<DepthStencilBufferDescription>> lazyDepthStencilBuffers = new();

        private readonly List<GBuffer> gBuffers = new();
        private readonly List<LazyInitDesc<GBufferDescription>> lazyGBuffers = new();

        private readonly List<Texture1D> textures1d = new();
        private readonly List<LazyInitDesc<Texture1DDescription>> lazyTextures1d = new();

        private readonly List<Texture2D> textures2d = new();
        private readonly List<LazyInitDesc<Texture2DDescription>> lazyTextures2d = new();

        private readonly List<Texture3D> textures3d = new();
        private readonly List<LazyInitDesc<Texture3DDescription>> lazyTextures3d = new();

        private readonly List<ISamplerState> samplerStates = new();
        private readonly List<LazyInitDesc<SamplerStateDescription>> lazySamplerStates = new();

        public IReadOnlyList<Texture2D> Textures => textures2d;

        public IReadOnlyList<ShadowAtlas> ShadowAtlas => shadowAtlas;

        public IReadOnlyList<GBuffer> GBuffers => gBuffers;

        internal void CreateResources()
        {
            int gid = 0;
            for (int i = 0; i < lazyShadowAtlas.Count; i++)
            {
                lazyShadowAtlas[i].Construct(device, shadowAtlas);
            }

            for (int i = 0; i < lazyDepthMipChains.Count; i++)
            {
                lazyDepthMipChains[i].Construct(device, depthMipChains);
            }

            for (int i = 0; i < lazyDepthStencilBuffers.Count; i++)
            {
                lazyDepthStencilBuffers[i].Construct(device, depthStencilBuffers);
            }

            for (int i = 0; i < lazyGBuffers.Count; i++)
            {
                lazyGBuffers[i].Construct(device, gBuffers);
            }

            for (int i = 0; i < lazyTextures1d.Count; i++)
            {
                lazyTextures1d[i].Construct(device, textures1d);
            }

            for (int i = 0; i < lazyTextures2d.Count; i++)
            {
                lazyTextures2d[i].Construct(device, textures2d);
            }

            for (int i = 0; i < lazyTextures3d.Count; i++)
            {
                lazyTextures3d[i].Construct(device, textures3d);
            }

            for (int i = 0; i < lazySamplerStates.Count; i++)
            {
                lazySamplerStates[i].Construct(device, samplerStates);
            }
        }

        internal void ReleaseResources()
        {
            for (int i = 0; i < resources.Count; i++)
            {
                resources[i].Dispose();
            }

            resources.Clear();
            nameToResource.Clear();

            structuredUavBuffers.Clear();

            structuredBuffers.Clear();

            constantBuffers.Clear();

            shadowAtlas.Clear();
            lazyShadowAtlas.Clear();

            depthMipChains.Clear();
            lazyDepthMipChains.Clear();

            depthStencilBuffers.Clear();
            lazyDepthStencilBuffers.Clear();

            gBuffers.Clear();
            lazyGBuffers.Clear();

            textures1d.Clear();
            lazyTextures1d.Clear();

            textures2d.Clear();
            lazyTextures2d.Clear();

            textures3d.Clear();
            lazyTextures3d.Clear();

            samplerStates.Clear();
            lazySamplerStates.Clear();
        }

        public ResourceRef AddResource(string name)
        {
            if (nameToResource.ContainsKey(name))
            {
                throw new Exception($"Duplicate resource, the resource named {name} already exists!");
            }

            ResourceRef resource = new(name);
            nameToResource.Add(name, resource);
            resources.Add(resource);
            return resource;
        }

        public ResourceRef<T> AddResource<T>(string name) where T : class, IDisposable
        {
            if (nameToResource.ContainsKey(name))
            {
                throw new Exception($"Duplicate resource, the resource named {name} already exists!");
            }

            ResourceRef resource = new(name);
            nameToResource.Add(name, resource);
            resources.Add(resource);
            return new(resource);
        }

        public ResourceRef GetOrAddResource(string name)
        {
            if (nameToResource.TryGetValue(name, out var resource))
            {
                return resource;
            }

            resource = new(name);
            nameToResource.Add(name, resource);
            resources.Add(resource);
            return resource;
        }

        public ResourceRef<T> GetOrAddResource<T>(string name) where T : class, IDisposable
        {
            if (nameToResource.TryGetValue(name, out var resource))
            {
                return new(resource);
            }

            resource = new(name);
            nameToResource.Add(name, resource);
            resources.Add(resource);
            return new(resource);
        }

        public ResourceRef? GetResource(string name)
        {
            if (nameToResource.TryGetValue(name, out var resource))
            {
                return resource;
            }

            throw new KeyNotFoundException($"There is no resource named {name}");
        }

        public bool TryGetResource(string name, [NotNullWhen(true)] out ResourceRef? resourceRef)
        {
            if (nameToResource.TryGetValue(name, out var resource))
            {
                resourceRef = resource;
                return true;
            }

            resourceRef = null;
            return false;
        }

        public ResourceRef<T> GetResource<T>(string name) where T : class, IDisposable
        {
            if (nameToResource.TryGetValue(name, out var resource) && resource.Value is T)
            {
                return new(resource);
            }

            throw new KeyNotFoundException($"There is no resource named {name}");
        }

        public bool TryGetResource<T>(string name, [NotNullWhen(true)] out ResourceRef<T>? resourceRef) where T : class, IDisposable
        {
            if (nameToResource.TryGetValue(name, out var resource))
            {
                resourceRef = new(resource);
                return true;
            }

            resourceRef = null;
            return false;
        }

        public bool RemoveResource(string name)
        {
            if (TryGetResource(name, out ResourceRef? resource))
            {
                nameToResource.Remove(name);
                resources.Remove(resource);
                resource.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// The static renderer viewport
        /// </summary>
        public Viewport Viewport { get => viewport; internal set => viewport = value; }

        /// <summary>
        /// The dynamic window viewport
        /// </summary>
        public Viewport OutputViewport { get => outputViewport; internal set => outputViewport = value; }

        public IRenderTargetView? Output { get; internal set; }

        public ITexture2D OutputTex { get; internal set; }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, bool lazyInit = true) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(device, accessFlags, uavFlags, srvFlags, name);
            structuredUavBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredUavBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, bool lazyInit = true) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(device, initialCapacity, accessFlags, uavFlags, srvFlags, name);
            structuredUavBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredUavBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredUavBuffer<T>> GetStructuredUavBuffer<T>(string name) where T : unmanaged
        {
            return GetOrAddResource<StructuredUavBuffer<T>>(name);
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(device, accessFlags, name);
            structuredBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(device, initialCapacity, accessFlags, name);
            structuredBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredBuffer<T>> GetStructuredBuffer<T>(string name) where T : unmanaged
        {
            return GetOrAddResource<StructuredBuffer<T>>(name);
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            ConstantBuffer<T> constantBuffer = new(device, accessFlags, name);
            constantBuffers.Add(constantBuffer);

            var resource = GetOrAddResource<ConstantBuffer<T>>(name);
            resource.Value = constantBuffer;

            return resource;
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, T value, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            ConstantBuffer<T> constantBuffer = new(device, value, accessFlags, name);
            constantBuffers.Add(constantBuffer);

            var resource = GetOrAddResource<ConstantBuffer<T>>(name);
            resource.Value = constantBuffer;

            return resource;
        }

        public ResourceRef<ConstantBuffer<T>> GetConstantBuffer<T>(string name) where T : unmanaged
        {
            return GetOrAddResource<ConstantBuffer<T>>(name);
        }

        public ResourceRef<ShadowAtlas> CreateShadowAtlas(string name, ShadowAtlasDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => new ShadowAtlas(device, description), shadowAtlas, lazyShadowAtlas, lazyInit);
        }

        public void UpdateShadowAtlas(string name, ShadowAtlasDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new ShadowAtlas(device, description), shadowAtlas);
        }

        public ShadowAtlas GetShadowAtlas(int index)
        {
            return shadowAtlas[index];
        }

        public ResourceRef<ShadowAtlas> GetShadowAtlas(string name)
        {
            return GetOrAddResource<ShadowAtlas>(name);
        }

        public ResourceRef<DepthMipChain> CreateDepthMipChain(string name, DepthStencilBufferDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => new DepthMipChain(device, description), depthMipChains, lazyDepthMipChains, lazyInit);
        }

        public void UpdateDepthMipChain(string name, DepthStencilBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new DepthMipChain(device, description), depthMipChains);
        }

        public ResourceRef<DepthMipChain> GetDepthMipChain(string name)
        {
            return GetOrAddResource<DepthMipChain>(name);
        }

        public ResourceRef<DepthStencil> CreateDepthStencilBuffer(string name, DepthStencilBufferDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => new DepthStencil(device, description), depthStencilBuffers, lazyDepthStencilBuffers, lazyInit);
        }

        public void UpdateDepthStencilBuffer(string name, DepthStencilBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new DepthStencil(device, description), depthStencilBuffers);
        }

        public ResourceRef<DepthStencil> GetDepthStencilBuffer(string name)
        {
            return GetOrAddResource<DepthStencil>(name);
        }

        public ResourceRef<GBuffer> CreateGBuffer(string name, GBufferDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => new GBuffer(device, description), gBuffers, lazyGBuffers, lazyInit);
        }

        public void UpdateGBuffer(string name, GBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new GBuffer(device, description), gBuffers);
        }

        public ResourceRef<GBuffer> GetGBuffer(string name)
        {
            return GetOrAddResource<GBuffer>(name);
        }

        public ResourceRef<Texture1D> CreateTexture1D(string name, Texture1DDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => new Texture1D(device, description), textures1d, lazyTextures1d, lazyInit);
        }

        public void UpdateTexture1D(string name, Texture1DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture1D(device, description), textures1d);
        }

        public ResourceRef<Texture1D> GetTexture1D(string name)
        {
            return GetOrAddResource<Texture1D>(name);
        }

        public ResourceRef<Texture2D> CreateTexture2D(string name, Texture2DDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => new Texture2D(device, description), textures2d, lazyTextures2d, lazyInit);
        }

        public void UpdateTexture2D(string name, Texture2DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture2D(device, description), textures2d);
        }

        public ResourceRef<Texture2D> GetTexture2D(string name)
        {
            return GetOrAddResource<Texture2D>(name);
        }

        public ResourceRef<Texture3D> CreateTexture3D(string name, Texture3DDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => new Texture3D(device, description), textures3d, lazyTextures3d, lazyInit);
        }

        public void UpdateTexture3D(string name, Texture3DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture3D(device, description), textures3d);
        }

        public ResourceRef<Texture3D> GetTexture3D(string name)
        {
            return GetOrAddResource<Texture3D>(name);
        }

        public ResourceRef<ISamplerState> CreateSamplerState(string name, SamplerStateDescription description, bool lazyInit = true)
        {
            return CreateResource(name, description, (dev, desc) => dev.CreateSamplerState(desc), samplerStates, lazySamplerStates, lazyInit);
        }

        public void UpdateSamplerState(string name, SamplerStateDescription desc)
        {
            UpdateResource(name, desc, (dev, desc) => dev.CreateSamplerState(desc), samplerStates);
        }

        public ResourceRef<ISamplerState> GetSamplerState(string name)
        {
            return GetOrAddResource<ISamplerState>(name);
        }

        public ResourceRef<TType> CreateResource<TType, TDesc>(string name, TDesc description, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group, IList<LazyInitDesc<TDesc>> lazyDescs, bool lazyInit) where TType : class, IDisposable
        {
            if (TryGetResource<TType>(name, out var resourceRef) && resourceRef.Value != null)
            {
                throw new InvalidOperationException($"Resource {name} is already created ({resourceRef.Value})");
            }

            resourceRef ??= AddResource<TType>(name);

            var idx = lazyDescs.IndexOf(new LazyInitDesc<TDesc>(default, resourceRef.Resource, null));
            var contains = idx != -1;

            if (!lazyInit)
            {
                TType resource = constructor(device, description);
                resourceRef.Value = resource;
                group.Add(resource);
                if (contains)
                {
                    lazyDescs.RemoveAt(idx);
                }
            }
            else if (!contains)
            {
                lazyDescs.Add(new(description, resourceRef.Resource, constructor));
            }

            return resourceRef;
        }

        public void UpdateResource<TType, TDesc>(string name, TDesc desc, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group) where TType : class, IDisposable
        {
            if (!TryGetResource<TType>(name, out var resourceRef))
            {
                throw new InvalidOperationException($"Tried to update non existing resource ({name})");
            }

            if (resourceRef.Value != null)
            {
                group.Remove(resourceRef.Value);
                resourceRef.Resource.Dispose();
            }

            TType resource = resourceRef.Value = constructor(device, desc);

            group.Add(resource);
        }
    }
}