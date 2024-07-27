namespace HexaEngine.Graphics.Graph
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
        private readonly Dictionary<ResourceRef, IResourceDescriptor> resourceToDescriptor = new();
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
        private readonly List<ResourceDescriptor<ShadowAtlasDescription>> shadowAtlasDescriptors = new();

        private readonly List<DepthMipChain> depthMipChains = new();
        private readonly List<ResourceDescriptor<DepthStencilBufferDescription>> depthMipChainDescriptors = new();

        private readonly List<DepthStencil> depthStencilBuffers = new();
        private readonly List<ResourceDescriptor<DepthStencilBufferDescription>> depthStencilBufferDescriptors = new();

        private readonly List<GBuffer> gBuffers = new();
        private readonly List<ResourceDescriptor<GBufferDescription>> gBufferDescriptors = new();

        private readonly List<Texture1D> textures1d = new();
        private readonly List<ResourceDescriptor<Texture1DDescription>> textures1dDescriptors = new();

        private readonly List<Texture2D> textures2d = new();
        private readonly List<ResourceDescriptor<Texture2DDescription>> textures2dDescriptors = new();

        private readonly List<Texture3D> textures3d = new();
        private readonly List<ResourceDescriptor<Texture3DDescription>> textures3dDescriptors = new();

        private readonly List<ISamplerState> samplerStates = new();
        private readonly List<ResourceDescriptor<SamplerStateDescription>> samplerStateDescriptors = new();

        private readonly List<IGraphicsPipelineState> graphicsPipelineStates = new();
        private readonly List<ResourceDescriptor<GraphicsPipelineStateDescEx>> graphicsPipelineStateDescriptors = new();

        private readonly List<IComputePipeline> computePipelines = new();
        private readonly List<ResourceDescriptor<ComputePipelineDesc>> computePipelineDescriptors = new();

        public IGraphicsDevice Device => device;

        public IReadOnlyList<Texture2D> Textures => textures2d;

        public IReadOnlyList<DepthStencil> DepthStencilBuffers => depthStencilBuffers;

        public IReadOnlyList<DepthMipChain> DepthMipChains => depthMipChains;

        public IReadOnlyList<ShadowAtlas> ShadowAtlas => shadowAtlas;

        public IReadOnlyList<GBuffer> GBuffers => gBuffers;

        public GraphResourceContainer? Container { get => container; set => container = value; }

        private void ConstructList<TType, TDesc>(List<ResourceDescriptor<TDesc>> descriptors, List<TType> group) where TDesc : struct where TType : class, IDisposable
        {
            for (int i = 0; i < descriptors.Count; i++)
            {
                var descriptor = descriptors[i];

                if (descriptor.IsCreated || (descriptor.Flags & ResourceCreationFlags.LazyInit) == 0)
                {
                    continue;
                }

                var shared = FindMatching<TType, TDesc>(descriptor, descriptors);
                descriptor.IsCreated = true;
                descriptor.ShareSource = shared;
                descriptor.Ref.Value = shared?.Ref.Value;
                if ((shared?.Ref.Value) == null)
                {
                    descriptor.IsCreated = true;
                    descriptors[i].Construct(device, group);
                }
            }
        }

        private static ResourceDescriptor<TDesc>? FindMatching<TType, TDesc>(ResourceDescriptor<TDesc> descriptor, IList<ResourceDescriptor<TDesc>> descriptors) where TDesc : struct where TType : class, IDisposable
        {
            if ((descriptor.Flags & ResourceCreationFlags.Shared) == 0)
            {
                return null;
            }

            for (int j = 0; j < descriptors.Count; j++)
            {
                var descShared = descriptors[j];

                if (!descShared.IsCreated || descShared == descriptor || (descShared.Flags & ResourceCreationFlags.Shared) == 0)
                {
                    continue;
                }

                if (descShared.Container != null && descShared.Container == descriptor.Container && ((descriptor.Flags & ResourceCreationFlags.GroupShared) == 0 || (descShared.Flags & ResourceCreationFlags.GroupShared) == 0))
                {
                    continue;
                }

                if (descriptor.Container?.HasSharedResource(descShared.Ref) ?? true)
                {
                    continue;
                }

                if (descShared.Desc.Equals(descriptor.Desc))
                {
                    return descShared;
                }
            }
            return null;
        }

        internal void CreateResources()
        {
            ConstructList(shadowAtlasDescriptors, shadowAtlas);
            ConstructList(depthMipChainDescriptors, depthMipChains);
            ConstructList(depthStencilBufferDescriptors, depthStencilBuffers);
            ConstructList(gBufferDescriptors, gBuffers);
            ConstructList(textures1dDescriptors, textures1d);
            ConstructList(textures2dDescriptors, textures2d);
            ConstructList(textures3dDescriptors, textures3d);
            ConstructList(samplerStateDescriptors, samplerStates);
            ConstructList(graphicsPipelineStateDescriptors, graphicsPipelineStates);
            ConstructList(computePipelineDescriptors, computePipelines);
        }

        internal void ReleaseResources()
        {
            for (int i = 0; i < resources.Count; i++)
            {
                resources[i].Dispose();
            }

            resources.Clear();
            nameToResource.Clear();
            resourceToDescriptor.Clear();

            structuredUavBuffers.Clear();
            structuredBuffers.Clear();
            constantBuffers.Clear();
            depthMipChains.Clear();
            depthStencilBuffers.Clear();
            gBuffers.Clear();
            textures1d.Clear();
            textures2d.Clear();
            textures3d.Clear();
            samplerStates.Clear();
            graphicsPipelineStates.Clear();
            computePipelines.Clear();

            shadowAtlasDescriptors.Clear();
            depthMipChainDescriptors.Clear();
            depthStencilBufferDescriptors.Clear();
            gBufferDescriptors.Clear();
            textures1dDescriptors.Clear();
            textures2dDescriptors.Clear();
            textures3dDescriptors.Clear();
            samplerStateDescriptors.Clear();
            graphicsPipelineStateDescriptors.Clear();
            computePipelineDescriptors.Clear();
        }

        public ResourceRef AddResource(string name)
        {
            if (nameToResource.ContainsKey(name))
            {
                throw new Exception($"Duplicate resource, the resource named {name} already exists!");
            }

            ResourceRef resource = new(this, name);
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

            ResourceRef resource = new(this, name);
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

            resource = new(this, name);
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

            resource = new(this, name);
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
                return RemoveResource(resource);
            }
            return false;
        }

        public bool RemoveResource(ResourceRef resource)
        {
            if (resources.Remove(resource))
            {
                var descriptor = resourceToDescriptor[resource];
                ((IList)shadowAtlasDescriptors).Remove(descriptor);
                ((IList)depthMipChainDescriptors).Remove(descriptor);
                ((IList)depthStencilBufferDescriptors).Remove(descriptor);
                ((IList)gBufferDescriptors).Remove(descriptor);
                ((IList)textures1dDescriptors).Remove(descriptor);
                ((IList)textures2dDescriptors).Remove(descriptor);
                ((IList)textures3dDescriptors).Remove(descriptor);
                ((IList)samplerStateDescriptors).Remove(descriptor);
                ((IList)graphicsPipelineStateDescriptors).Remove(descriptor);
                ((IList)computePipelineDescriptors).Remove(descriptor);
                resourceToDescriptor.Remove(resource);

                nameToResource.Remove(resource.Name);
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
            UpdateResource(name, description, (dev, desc) => new ShadowAtlas(description), shadowAtlas);
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
            UpdateResource(name, description, (dev, desc) => new DepthMipChain(description), depthMipChains);
        }

        public ResourceRef<DepthMipChain> GetDepthMipChain(string name)
        {
            return GetOrAddResource<DepthMipChain>(name);
        }

        public void UpdateDepthStencilBuffer(string name, DepthStencilBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new DepthStencil(description, name), depthStencilBuffers);
        }

        public ResourceRef<DepthStencil> GetDepthStencilBuffer(string name)
        {
            return GetOrAddResource<DepthStencil>(name);
        }

        public void UpdateGBuffer(string name, GBufferDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new GBuffer(description, name), gBuffers);
        }

        public ResourceRef<GBuffer> GetGBuffer(string name)
        {
            return GetOrAddResource<GBuffer>(name);
        }

        public void UpdateTexture1D(string name, Texture1DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture1D(description, name), textures1d);
        }

        public ResourceRef<Texture1D> GetTexture1D(string name)
        {
            return GetOrAddResource<Texture1D>(name);
        }

        public void UpdateTexture2D(string name, Texture2DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture2D(description, name), textures2d);
        }

        public ResourceRef<Texture2D> GetTexture2D(string name)
        {
            return GetOrAddResource<Texture2D>(name);
        }

        public void UpdateTexture3D(string name, Texture3DDescription description)
        {
            UpdateResource(name, description, (dev, desc) => new Texture3D(description, name), textures3d);
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
            return CreateResource(description.GetHashCode().ToString(), description, (dev, desc) => dev.CreateComputePipeline(desc), computePipelines, computePipelineDescriptors, flags);
        }

        public ResourceRef<IComputePipeline> CreateComputePipeline(string name, ComputePipelineDesc description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => dev.CreateComputePipeline(desc), computePipelines, computePipelineDescriptors, flags);
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            ConstantBuffer<T> constantBuffer = new(accessFlags, name);
            constantBuffers.Add(constantBuffer);

            var resource = GetOrAddResource<ConstantBuffer<T>>(name);
            resource.Value = constantBuffer;

            return resource;
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, T value, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            ConstantBuffer<T> constantBuffer = new(value, accessFlags, name);
            constantBuffers.Add(constantBuffer);

            var resource = GetOrAddResource<ConstantBuffer<T>>(name);
            resource.Value = constantBuffer;

            return resource;
        }

        public ResourceRef<DepthMipChain> CreateDepthMipChain(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new DepthMipChain(description), depthMipChains, depthMipChainDescriptors, flags);
        }

        public ResourceRef<DepthStencil> CreateDepthStencilBuffer(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new DepthStencil(description, name), depthStencilBuffers, depthStencilBufferDescriptors, flags);
        }

        public ResourceRef<GBuffer> CreateGBuffer(string name, GBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new GBuffer(description, name), gBuffers, gBufferDescriptors, flags);
        }

        public ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            string name = description.GetHashCode().ToString();
            return CreateResource(name, description, (dev, desc) => dev.CreateGraphicsPipelineState(desc), graphicsPipelineStates, graphicsPipelineStateDescriptors, flags);
        }

        public ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(string name, GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => dev.CreateGraphicsPipelineState(desc), graphicsPipelineStates, graphicsPipelineStateDescriptors, flags);
        }

        public ResourceRef<ISamplerState> CreateSamplerState(string name, SamplerStateDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All | ResourceCreationFlags.GroupShared)
        {
            return CreateResource(name, description, (dev, desc) => dev.CreateSamplerState(desc), samplerStates, samplerStateDescriptors, flags);
        }

        public ResourceRef<ShadowAtlas> CreateShadowAtlas(string name, ShadowAtlasDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new ShadowAtlas(description), shadowAtlas, shadowAtlasDescriptors, flags);
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(accessFlags, name);
            structuredBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredBuffer<T> structuredBuffer = new(initialCapacity, accessFlags, name);
            structuredBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(accessFlags, uavFlags, srvFlags, name);
            structuredUavBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredUavBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged
        {
            // creation of constant buffers cannot be deferred, because of generics.
            StructuredUavBuffer<T> structuredBuffer = new(initialCapacity, accessFlags, uavFlags, srvFlags, name);
            structuredUavBuffers.Add(structuredBuffer);

            var resource = GetOrAddResource<StructuredUavBuffer<T>>(name);
            resource.Value = structuredBuffer;

            return resource;
        }

        public ResourceRef<Texture1D> CreateTexture1D(string name, Texture1DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new Texture1D(description, name), textures1d, textures1dDescriptors, flags);
        }

        public ResourceRef<Texture2D> CreateTexture2D(string name, Texture2DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new Texture2D(description, name), textures2d, textures2dDescriptors, flags);
        }

        public ResourceRef<Texture3D> CreateTexture3D(string name, Texture3DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All)
        {
            return CreateResource(name, description, (dev, desc) => new Texture3D(description, name), textures3d, textures3dDescriptors, flags);
        }

        public ResourceRef<TType> CreateResource<TType, TDesc>(string name, TDesc description, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group, IList<ResourceDescriptor<TDesc>> descriptors, ResourceCreationFlags flags) where TDesc : struct where TType : class, IDisposable
        {
            if (TryGetResource<TType>(name, out var resourceRef) && resourceRef.Value != null)
            {
                return resourceRef;
            }

            resourceRef ??= AddResource<TType>(name);

            var contains = resourceToDescriptor.TryGetValue(resourceRef.Resource, out var resourceDescriptor);

            if ((flags & ResourceCreationFlags.LazyInit) == 0)
            {
                ResourceDescriptor<TDesc> descriptor = resourceDescriptor != null ? (ResourceDescriptor<TDesc>)resourceDescriptor : new ResourceDescriptor<TDesc>(description, resourceRef.Resource, container, null, flags);
                if (!contains)
                {
                    descriptors.Add(descriptor);
                    resourceToDescriptor.Add(resourceRef.Resource, descriptor);
                }

                ResourceDescriptor<TDesc>? shared = FindMatching<TType, TDesc>(descriptor, descriptors);

                TType resource = (TType)(shared?.Ref.Value ?? constructor(device, description));

                descriptor.Flags &= ~ResourceCreationFlags.LazyInit;
                descriptor.IsCreated = true;
                descriptor.ShareSource = shared;
                resourceRef.Value = resource;

                if (shared == null)
                {
                    group.Add(resource);
                }
            }
            else if (!contains)
            {
                ResourceDescriptor<TDesc> descriptor = new(description, resourceRef.Resource, container, constructor, flags);
                descriptors.Add(descriptor);
                resourceToDescriptor.Add(resourceRef.Resource, descriptor);
            }

            return resourceRef;
        }
    }
}