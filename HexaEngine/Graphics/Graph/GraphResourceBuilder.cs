namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Unsafes;
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
        private GraphResourceContainer? container;

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

        private readonly List<IGraphicsPipelineState> graphicsPipelineStates = new();
        private readonly List<LazyInitDesc<GraphicsPipelineStateDescEx>> lazyGraphicsPipelineStates = new();

        private readonly List<IComputePipeline> computePipelines = new();
        private readonly List<LazyInitDesc<ComputePipelineDesc>> lazyComputePipelines = new();

        public IGraphicsDevice Device => device;

        public IReadOnlyList<Texture2D> Textures => textures2d;

        public IReadOnlyList<ShadowAtlas> ShadowAtlas => shadowAtlas;

        public IReadOnlyList<GBuffer> GBuffers => gBuffers;

        public GraphResourceContainer? Container { get => container; set => container = value; }

        internal void CreateResources()
        {
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

            for (int i = 0; i < lazyGraphicsPipelineStates.Count; i++)
            {
                lazyGraphicsPipelineStates[i].Construct(device, graphicsPipelineStates);
            }

            for (int i = 0; i < lazyComputePipelines.Count; i++)
            {
                lazyComputePipelines[i].Construct(device, computePipelines);
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

            graphicsPipelineStates.Clear();
            lazyGraphicsPipelineStates.Clear();

            computePipelines.Clear();
            lazyComputePipelines.Clear();
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
            container?.AddResource(resource);
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
            container?.AddResource(resource);
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
            container?.AddResource(resource);
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
            container?.AddResource(resource);
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

        /// <summary>
        /// Removes a resource with the specified name from the manager.
        /// </summary>
        /// <param name="name">The name of the resource to remove.</param>
        /// <returns>
        ///   <c>true</c> if the resource was successfully removed; otherwise, <c>false</c> if the resource was not found.
        /// </returns>
        public bool RemoveResource(string name)
        {
            if (TryGetResource(name, out ResourceRef? resource))
            {
                nameToResource.Remove(name);
                resources.Remove(resource);
                container?.RemoveResource(resource);
                resource.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Releases the resource with the specified name from the manager.
        /// </summary>
        /// <param name="name">The name of the resource to release.</param>
        /// <returns>
        ///   <c>true</c> if the resource was successfully released; otherwise, <c>false</c> if the resource was not found.
        /// </returns>
        public bool DisposeResource(string name)
        {
            if (TryGetResource(name, out ResourceRef? resource))
            {
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

        public ResourceRef<StructuredUavBuffer<T>> GetStructuredUavBuffer<T>(string name) where T : unmanaged
        {
            return GetOrAddResource<StructuredUavBuffer<T>>(name);
        }

        public ResourceRef<StructuredBuffer<T>> GetStructuredBuffer<T>(string name) where T : unmanaged
        {
            return GetOrAddResource<StructuredBuffer<T>>(name);
        }

        public ResourceRef<ConstantBuffer<T>> GetConstantBuffer<T>(string name) where T : unmanaged
        {
            return GetOrAddResource<ConstantBuffer<T>>(name);
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

        public void UpdateDepthMipChain(string name, DepthStencilBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new DepthMipChain(device, description), depthMipChains);
        }

        public ResourceRef<DepthMipChain> GetDepthMipChain(string name)
        {
            return GetOrAddResource<DepthMipChain>(name);
        }

        public void UpdateDepthStencilBuffer(string name, DepthStencilBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new DepthStencil(device, description, name), depthStencilBuffers);
        }

        public ResourceRef<DepthStencil> GetDepthStencilBuffer(string name)
        {
            return GetOrAddResource<DepthStencil>(name);
        }

        public void UpdateGBuffer(string name, GBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new GBuffer(device, description, name), gBuffers);
        }

        public ResourceRef<GBuffer> GetGBuffer(string name)
        {
            return GetOrAddResource<GBuffer>(name);
        }

        public void UpdateTexture1D(string name, Texture1DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture1D(device, description, name), textures1d);
        }

        public ResourceRef<Texture1D> GetTexture1D(string name)
        {
            return GetOrAddResource<Texture1D>(name);
        }

        public void UpdateTexture2D(string name, Texture2DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture2D(device, description, name), textures2d);
        }

        public ResourceRef<Texture2D> GetTexture2D(string name)
        {
            return GetOrAddResource<Texture2D>(name);
        }

        public void UpdateTexture3D(string name, Texture3DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture3D(device, description, name), textures3d);
        }

        public ResourceRef<Texture3D> GetTexture3D(string name)
        {
            return GetOrAddResource<Texture3D>(name);
        }

        public void UpdateSamplerState(string name, SamplerStateDescription desc)
        {
            UpdateResource(name, desc, (dev, desc) => dev.CreateSamplerState(desc), samplerStates);
        }

        public ResourceRef<ISamplerState> GetSamplerState(string name)
        {
            return GetOrAddResource<ISamplerState>(name);
        }

        public void UpdateGraphicsPipelineState(string name, GraphicsPipelineStateDescEx desc)
        {
            UpdateResource(name, desc, (dev, desc) => dev.CreateGraphicsPipelineState(desc), graphicsPipelineStates);
        }

        public ResourceRef<IGraphicsPipelineState> GetGraphicsPipelineState(string name)
        {
            return GetOrAddResource<IGraphicsPipelineState>(name);
        }

        public void UpdateComputePipeline(string name, ComputePipelineDesc desc)
        {
            UpdateResource(name, desc, (dev, desc) => dev.CreateComputePipeline(desc), computePipelines);
        }

        public ResourceRef<IComputePipeline> GetComputePipeline(string name)
        {
            return GetOrAddResource<IComputePipeline>(name);
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

        public ResourceRef<IComputePipeline> CreateComputePipeline(ComputePipelineDesc description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(description.GetHashCode().ToString(), description, (dev, desc) => dev.CreateComputePipeline(desc), computePipelines, lazyComputePipelines, flags);
        }

        public ResourceRef<IComputePipeline> CreateComputePipeline(string name, ComputePipelineDesc description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => dev.CreateComputePipeline(desc), computePipelines, lazyComputePipelines, flags);
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            ConstantBuffer<T> constantBuffer = new(device, accessFlags, name);
            constantBuffers.Add(constantBuffer);

            var resource = GetOrAddResource<ConstantBuffer<T>>(name);
            resource.Value = constantBuffer;

            return resource;
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, T value, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            ConstantBuffer<T> constantBuffer = new(device, value, accessFlags, name);
            constantBuffers.Add(constantBuffer);

            var resource = GetOrAddResource<ConstantBuffer<T>>(name);
            resource.Value = constantBuffer;

            return resource;
        }

        public ResourceRef<DepthMipChain> CreateDepthMipChain(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new DepthMipChain(device, description), depthMipChains, lazyDepthMipChains, flags);
        }

        public ResourceRef<DepthStencil> CreateDepthStencilBuffer(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new DepthStencil(device, description, name), depthStencilBuffers, lazyDepthStencilBuffers, flags);
        }

        public ResourceRef<GBuffer> CreateGBuffer(string name, GBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new GBuffer(device, description, name), gBuffers, lazyGBuffers, flags);
        }

        public ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            string name = description.GetHashCode().ToString();
            return CreateResource(name, description, (dev, desc) => dev.CreateGraphicsPipelineState(desc), graphicsPipelineStates, lazyGraphicsPipelineStates, flags);
        }

        public ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(string name, GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => dev.CreateGraphicsPipelineState(desc), graphicsPipelineStates, lazyGraphicsPipelineStates, flags);
        }

        public ResourceRef<ISamplerState> CreateSamplerState(string name, SamplerStateDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => dev.CreateSamplerState(desc), samplerStates, lazySamplerStates, flags);
        }

        public ResourceRef<ShadowAtlas> CreateShadowAtlas(string name, ShadowAtlasDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new ShadowAtlas(device, description), shadowAtlas, lazyShadowAtlas, flags);
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(device, accessFlags, name);
            structuredBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(device, initialCapacity, accessFlags, name);
            structuredBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(device, accessFlags, uavFlags, srvFlags, name);
            structuredUavBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredUavBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(device, initialCapacity, accessFlags, uavFlags, srvFlags, name);
            structuredUavBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredUavBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<Texture1D> CreateTexture1D(string name, Texture1DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new Texture1D(device, description, name), textures1d, lazyTextures1d, flags);
        }

        public ResourceRef<Texture2D> CreateTexture2D(string name, Texture2DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new Texture2D(device, description, name), textures2d, lazyTextures2d, flags);
        }

        public ResourceRef<Texture3D> CreateTexture3D(string name, Texture3DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new Texture3D(device, description, name), textures3d, lazyTextures3d, flags);
        }

        public ResourceRef<TType> CreateResource<TType, TDesc>(string name, TDesc description, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group, IList<LazyInitDesc<TDesc>> lazyDescs, ResourceCreationFlags flags) where TType : class, IDisposable
        {
            if (TryGetResource<TType>(name, out var resourceRef) && resourceRef.Value != null)
            {
                return resourceRef;
                //throw new InvalidOperationException($"Resource {name} is already created ({resourceRef.Value})");
            }

            resourceRef ??= AddResource<TType>(name);

            var idx = lazyDescs.IndexOf(new LazyInitDesc<TDesc>(default, resourceRef.Resource, null, 0));
            var contains = idx != -1;

            if ((flags & ResourceCreationFlags.LazyInit) == 0)
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
                lazyDescs.Add(new(description, resourceRef.Resource, constructor, flags));
            }

            return resourceRef;
        }
    }
}