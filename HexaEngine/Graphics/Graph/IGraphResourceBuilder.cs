namespace HexaEngine.Graphics.Graph
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public interface IGraphResourceBuilder
    {
        IGraphicsDevice Device { get; }

        IReadOnlyList<GBuffer> GBuffers { get; }

        IRenderTargetView? Output { get; }

        Viewport OutputViewport { get; }

        IReadOnlyList<ShadowAtlas> ShadowAtlas { get; }

        IReadOnlyList<Texture2D> Textures { get; }

        Viewport Viewport { get; }
        GraphResourceContainer? Container { get; set; }

        ResourceRef AddResource(string name);

        ResourceRef<T> AddResource<T>(string name) where T : class, IDisposable;

        ResourceRef<IComputePipeline> CreateComputePipeline(ComputePipelineDesc description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<IComputePipeline> CreateComputePipeline(string name, ComputePipelineDesc description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged;

        ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, T value, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged;

        ResourceRef<DepthMipChain> CreateDepthMipChain(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<DepthStencil> CreateDepthStencilBuffer(string name, DepthStencilBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<GBuffer> CreateGBuffer(string name, GBufferDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<IGraphicsPipelineState> CreateGraphicsPipelineState(string name, GraphicsPipelineStateDescEx description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<TType> CreateResource<TType, TDesc>(string name, TDesc description, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group, IList<ResourceDescriptor<TDesc>> lazyDescs, ResourceCreationFlags flags) where TDesc : struct where TType : class, IDisposable;

        ResourceRef<ISamplerState> CreateSamplerState(string name, SamplerStateDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<ShadowAtlas> CreateShadowAtlas(string name, ShadowAtlasDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged;

        ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged;

        ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged;

        ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, ResourceCreationFlags flags = ResourceCreationFlags.All) where T : unmanaged;

        ResourceRef<Texture1D> CreateTexture1D(string name, Texture1DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<Texture2D> CreateTexture2D(string name, Texture2DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        ResourceRef<Texture3D> CreateTexture3D(string name, Texture3DDescription description, ResourceCreationFlags flags = ResourceCreationFlags.All);

        bool DisposeResource(string name);

        ResourceRef<IComputePipeline> GetComputePipeline(string name);

        ResourceRef<ConstantBuffer<T>> GetConstantBuffer<T>(string name) where T : unmanaged;

        ResourceRef<DepthMipChain> GetDepthMipChain(string name);

        ResourceRef<DepthStencil> GetDepthStencilBuffer(string name);

        ResourceRef<GBuffer> GetGBuffer(string name);

        ResourceRef<IGraphicsPipelineState> GetGraphicsPipelineState(string name);

        ResourceRef GetOrAddResource(string name);

        ResourceRef<T> GetOrAddResource<T>(string name) where T : class, IDisposable;

        ResourceRef? GetResource(string name);

        ResourceRef<T> GetResource<T>(string name) where T : class, IDisposable;

        ResourceRef<ISamplerState> GetSamplerState(string name);

        ShadowAtlas GetShadowAtlas(int index);

        ResourceRef<ShadowAtlas> GetShadowAtlas(string name);

        ResourceRef<StructuredBuffer<T>> GetStructuredBuffer<T>(string name) where T : unmanaged;

        ResourceRef<StructuredUavBuffer<T>> GetStructuredUavBuffer<T>(string name) where T : unmanaged;

        ResourceRef<Texture1D> GetTexture1D(string name);

        ResourceRef<Texture2D> GetTexture2D(string name);

        ResourceRef<Texture3D> GetTexture3D(string name);

        bool RemoveResource(string name);

        bool TryGetResource(string name, [NotNullWhen(true)] out ResourceRef? resourceRef);

        bool TryGetResource<T>(string name, [NotNullWhen(true)] out ResourceRef<T>? resourceRef) where T : class, IDisposable;

        void UpdateComputePipeline(string name, ComputePipelineDesc desc);

        void UpdateDepthMipChain(string name, DepthStencilBufferDescription description);

        void UpdateDepthStencilBuffer(string name, DepthStencilBufferDescription description);

        void UpdateGBuffer(string name, GBufferDescription description);

        void UpdateGraphicsPipelineState(string name, GraphicsPipelineStateDescEx desc);

        void UpdateResource<TType, TDesc>(string name, TDesc desc, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group) where TType : class, IDisposable;

        void UpdateSamplerState(string name, SamplerStateDescription desc);

        void UpdateShadowAtlas(string name, ShadowAtlasDescription description);

        void UpdateTexture1D(string name, Texture1DDescription description);

        void UpdateTexture2D(string name, Texture2DDescription description);

        void UpdateTexture3D(string name, Texture3DDescription description);
    }
}