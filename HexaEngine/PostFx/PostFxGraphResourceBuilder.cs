namespace HexaEngine.PostFx
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using System.Diagnostics.CodeAnalysis;

    public class PostFxGraphResourceBuilder : IGraphResourceBuilder
    {
        private readonly GraphResourceBuilder resourceBuilder;
        private readonly IGraphicsDevice device;

        public PostFxGraphResourceBuilder(GraphResourceBuilder resourceBuilder, IGraphicsDevice device, Format format, int width, int height)
        {
            this.resourceBuilder = resourceBuilder;
            this.device = device;
            Format = format;
            Width = width;
            Height = height;
        }

        public Format Format { get; }

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<ShadowAtlas> ShadowAtlas => resourceBuilder.ShadowAtlas;

        public IReadOnlyList<Texture2D> Textures => resourceBuilder.Textures;

        public IReadOnlyList<GBuffer> GBuffers => resourceBuilder.GBuffers;

        public IRenderTargetView? Output => resourceBuilder.Output;

        public Viewport OutputViewport => resourceBuilder.OutputViewport;

        public Viewport Viewport => resourceBuilder.Viewport;

        public Viewport ViewportHalf => new(resourceBuilder.Viewport.Offset, resourceBuilder.Viewport.Size / 2f);

        public IGraphicsDevice Device => device;

        public GraphResourceContainer? Container { get => resourceBuilder.Container; set => resourceBuilder.Container = value; }

        public ResourceRef AddResource(string name)
        {
            return resourceBuilder.AddResource(name);
        }

        public ResourceRef<Texture2D> CreateBuffer(string name, int arraySize = 1, int mipLevels = 1, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.RW, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceMiscFlag = ResourceMiscFlag.None, ResourceCreationFlags creationFlags = ResourceCreationFlags.Shared)
        {
            var texRef = CreateTexture2D(name, new(Format, Width, Height, arraySize, mipLevels, gpuAccessFlags, cpuAccessFlags, 1, 0, resourceMiscFlag), creationFlags);
            return texRef;
        }

        public ResourceRef<Texture2D> CreateBufferHalfRes(string name, int arraySize = 1, int mipLevels = 1, ResourceCreationFlags creationFlags = ResourceCreationFlags.Shared)
        {
            var texRef = CreateTexture2D(name, new(Format, Width / 2, Height / 2, arraySize, mipLevels, GpuAccessFlags.RW), creationFlags);
            return texRef;
        }

        public ResourceRef<Texture2D> CreateBufferQuarterRes(string name, int arraySize = 1, int mipLevels = 1, ResourceCreationFlags creationFlags = ResourceCreationFlags.Shared)
        {
            var texRef = CreateTexture2D(name, new(Format, Width / 4, Height / 4, arraySize, mipLevels, GpuAccessFlags.RW), creationFlags);
            return texRef;
        }

        public ResourceRef<Texture2D> CreateBuffer(string name, Format format, int arraySize = 1, int mipLevels = 1, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.RW, ResourceCreationFlags creationFlags = ResourceCreationFlags.Shared)
        {
            var texRef = CreateTexture2D(name, new(format, Width, Height, arraySize, mipLevels, gpuAccessFlags), creationFlags);
            return texRef;
        }

        public ResourceRef<Texture2D> CreateBuffer(string name, Format format, int arraySize = 1, int mipLevels = 1, GpuAccessFlags gpuAccessFlags = GpuAccessFlags.RW, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag resourceMiscFlag = ResourceMiscFlag.None, ResourceCreationFlags creationFlags = ResourceCreationFlags.Shared)
        {
            var texRef = CreateTexture2D(name, new(format, Width, Height, arraySize, mipLevels, gpuAccessFlags, cpuAccessFlags, 1, 0, resourceMiscFlag), creationFlags);
            return texRef;
        }

        public ResourceRef<Texture2D> CreateBufferHalfRes(string name, Format format, int arraySize = 1, int mipLevels = 1, ResourceCreationFlags creationFlags = ResourceCreationFlags.Shared)
        {
            var texRef = CreateTexture2D(name, new(format, Width / 2, Height / 2, arraySize, mipLevels, GpuAccessFlags.RW), creationFlags);
            return texRef;
        }

        public ResourceRef<Texture2D> CreateBufferQuarterRes(string name, Format format, int arraySize = 1, int mipLevels = 1, ResourceCreationFlags creationFlags = ResourceCreationFlags.Shared)
        {
            var texRef = CreateTexture2D(name, new(format, Width / 4, Height / 4, arraySize, mipLevels, GpuAccessFlags.RW), creationFlags);
            return texRef;
        }

        public bool DisposeResource(string name)
        {
            return resourceBuilder.DisposeResource(name);
        }

        public ResourceRef<IComputePipeline> GetComputePipeline(string name)
        {
            return resourceBuilder.GetComputePipeline(name);
        }

        public ResourceRef<ConstantBuffer<T>> GetConstantBuffer<T>(string name) where T : unmanaged
        {
            return resourceBuilder.GetConstantBuffer<T>(name);
        }

        public ResourceRef<DepthMipChain> GetDepthMipChain(string name)
        {
            return resourceBuilder.GetDepthMipChain(name);
        }

        public ResourceRef<DepthStencil> GetDepthStencilBuffer(string name)
        {
            return resourceBuilder.GetDepthStencilBuffer(name);
        }

        public ResourceRef<GBuffer> GetGBuffer(string name)
        {
            return resourceBuilder.GetGBuffer(name);
        }

        public ResourceRef<IGraphicsPipelineState> GetGraphicsPipelineState(string name)
        {
            return resourceBuilder.GetGraphicsPipelineState(name);
        }

        public ResourceRef GetOrAddResource(string name)
        {
            return resourceBuilder.GetOrAddResource(name);
        }

        public ResourceRef? GetResource(string name)
        {
            return resourceBuilder.GetResource(name);
        }

        public ResourceRef<ISamplerState> GetSamplerState(string name)
        {
            return resourceBuilder.GetSamplerState(name);
        }

        public ShadowAtlas GetShadowAtlas(int index)
        {
            return resourceBuilder.GetShadowAtlas(index);
        }

        public ResourceRef<ShadowAtlas> GetShadowAtlas(string name)
        {
            return resourceBuilder.GetShadowAtlas(name);
        }

        public ResourceRef<StructuredBuffer<T>> GetStructuredBuffer<T>(string name) where T : unmanaged
        {
            return resourceBuilder.GetStructuredBuffer<T>(name);
        }

        public ResourceRef<StructuredUavBuffer<T>> GetStructuredUavBuffer<T>(string name) where T : unmanaged
        {
            return resourceBuilder.GetStructuredUavBuffer<T>(name);
        }

        public ResourceRef<Texture1D> GetTexture1D(string name)
        {
            return resourceBuilder.GetTexture1D(name);
        }

        public ResourceRef<Texture2D> GetTexture2D(string name)
        {
            return resourceBuilder.GetTexture2D(name);
        }

        public ResourceRef<Texture3D> GetTexture3D(string name)
        {
            return resourceBuilder.GetTexture3D(name);
        }

        public bool RemoveResource(string name)
        {
            return resourceBuilder.RemoveResource(name);
        }

        public bool TryGetResource(string name, [NotNullWhen(true)] out ResourceRef? resourceRef)
        {
            return resourceBuilder.TryGetResource(name, out resourceRef);
        }

        public void UpdateComputePipelineState(string name, ComputePipelineStateDescEx desc)
        {
            resourceBuilder.UpdateComputePipelineState(name, desc);
        }

        public void UpdateDepthMipChain(string name, DepthStencilBufferDescription description)
        {
            resourceBuilder.UpdateDepthMipChain(name, description);
        }

        public void UpdateDepthStencilBuffer(string name, DepthStencilBufferDescription description)
        {
            resourceBuilder.UpdateDepthStencilBuffer(name, description);
        }

        public void UpdateGBuffer(string name, GBufferDescription description)
        {
            resourceBuilder.UpdateGBuffer(name, description);
        }

        public void UpdateGraphicsPipelineState(string name, GraphicsPipelineStateDescEx desc)
        {
            resourceBuilder.UpdateGraphicsPipelineState(name, desc);
        }

        public void UpdateSamplerState(string name, SamplerStateDescription desc)
        {
            resourceBuilder.UpdateSamplerState(name, desc);
        }

        public void UpdateShadowAtlas(string name, ShadowAtlasDescription description)
        {
            resourceBuilder.UpdateShadowAtlas(name, description);
        }

        public void UpdateTexture1D(string name, Texture1DDescription description)
        {
            resourceBuilder.UpdateTexture1D(name, description);
        }

        public void UpdateTexture2D(string name, Texture2DDescription description)
        {
            resourceBuilder.UpdateTexture2D(name, description);
        }

        public void UpdateTexture3D(string name, Texture3DDescription description)
        {
            resourceBuilder.UpdateTexture3D(name, description);
        }

        public ResourceRef<T> AddResource<T>(string name) where T : class, IDisposable
        {
            return resourceBuilder.AddResource<T>(name);
        }

        public ResourceRef<T> GetOrAddResource<T>(string name) where T : class, IDisposable
        {
            return resourceBuilder.GetOrAddResource<T>(name);
        }

        public ResourceRef<T> GetResource<T>(string name) where T : class, IDisposable
        {
            return resourceBuilder.GetResource<T>(name);
        }

        public bool TryGetResource<T>(string name, [NotNullWhen(true)] out ResourceRef<T>? resourceRef) where T : class, IDisposable
        {
            return resourceBuilder.TryGetResource(name, out resourceRef);
        }

        public void UpdateResource<TType, TDesc>(string name, TDesc desc, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group) where TType : class, IDisposable
        {
            resourceBuilder.UpdateResource(name, desc, constructor, group);
        }

        public ResourceRef<IComputePipelineState> CreateComputePipelineState(ComputePipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateComputePipelineState(description, flags);
        }

        public ResourceRef<IComputePipelineState> CreateComputePipelineState(string name, ComputePipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateComputePipelineState(name, description, flags);
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.Default) where T : unmanaged
        {
            return resourceBuilder.CreateConstantBuffer<T>(name, accessFlags, flags);
        }

        public ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, T value, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.Default) where T : unmanaged
        {
            return resourceBuilder.CreateConstantBuffer(name, value, accessFlags, flags);
        }

        public ResourceRef<DepthMipChain> CreateDepthMipChain(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateDepthMipChain(name, description, flags);
        }

        public ResourceRef<DepthStencil> CreateDepthStencilBuffer(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateDepthStencilBuffer(name, description, flags);
        }

        public ResourceRef<GBuffer> CreateGBuffer(string name, GBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateGBuffer(name, description, flags);
        }

        public ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateGraphicsPipelineState(description, flags);
        }

        public ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(string name, GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateGraphicsPipelineState(name, description, flags);
        }

        ResourceRef<TType> IGraphResourceBuilder.CreateResource<TType, TDesc>(string name, TDesc description, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group, IList<ResourceDescriptor<TDesc>> lazyDescs, ResourceCreationFlags flags)
        {
            return resourceBuilder.CreateResource(name, description, constructor, group, lazyDescs, flags);
        }

        public ResourceRef<ISamplerState> CreateSamplerState(string name, SamplerStateDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateSamplerState(name, description, flags);
        }

        public ResourceRef<ShadowAtlas> CreateShadowAtlas(string name, ShadowAtlasDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateShadowAtlas(name, description, flags);
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.Default) where T : unmanaged
        {
            return resourceBuilder.CreateStructuredBuffer<T>(name, accessFlags, flags);
        }

        public ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.Default) where T : unmanaged
        {
            return resourceBuilder.CreateStructuredBuffer<T>(name, initialCapacity, accessFlags, flags);
        }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.Default) where T : unmanaged
        {
            return resourceBuilder.CreateStructuredUavBuffer<T>(name, accessFlags, uavFlags, srvFlags, flags);
        }

        public ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.Default) where T : unmanaged
        {
            return resourceBuilder.CreateStructuredUavBuffer<T>(name, initialCapacity, accessFlags, uavFlags, srvFlags, flags);
        }

        public ResourceRef<Texture1D> CreateTexture1D(string name, Texture1DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateTexture1D(name, description, flags);
        }

        public ResourceRef<Texture2D> CreateTexture2D(string name, Texture2DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateTexture2D(name, description, flags);
        }

        public ResourceRef<Texture3D> CreateTexture3D(string name, Texture3DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.Default)
        {
            return resourceBuilder.CreateTexture3D(name, description, flags);
        }
    }
}