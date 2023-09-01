namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public interface IGraphResourceBuilder : IGraphResources
    {
        ResourceRef AddResource(string name);

        ResourceRef<T> AddResource<T>(string name) where T : class, IDisposable;

        ResourceRef GetOrAddResource(string name);

        ResourceRef<T> GetOrAddResource<T>(string name) where T : class, IDisposable;

        ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged;

        ResourceRef<ConstantBuffer<T>> CreateConstantBuffer<T>(string name, T value, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged;

        ResourceRef<DepthMipChain> CreateDepthMipChain(string name, DepthStencilBufferDescription description, bool lazyInit = true);

        ResourceRef<DepthStencil> CreateDepthStencilBuffer(string name, DepthStencilBufferDescription description, bool lazyInit = true);

        ResourceRef<GBuffer> CreateGBuffer(string name, GBufferDescription description, bool lazyInit = true);

        ResourceRef<TType> CreateResource<TType, TDesc>(string name, TDesc description, Func<IGraphicsDevice, TDesc, TType> constructor, IList<TType> group, IList<LazyInitDesc<TDesc>> lazyDescs, bool lazyInit) where TType : class, IDisposable;

        ResourceRef<ISamplerState> CreateSamplerState(string name, SamplerStateDescription description, bool lazyInit = true);

        ResourceRef<ShadowAtlas> CreateShadowAtlas(string name, ShadowAtlasDescription description, bool lazyInit = true);

        ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged;

        ResourceRef<StructuredBuffer<T>> CreateStructuredBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, bool lazyInit = true) where T : unmanaged;

        ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, bool lazyInit = true) where T : unmanaged;

        ResourceRef<StructuredUavBuffer<T>> CreateStructuredUavBuffer<T>(string name, uint initialCapacity, CpuAccessFlags accessFlags, BufferUnorderedAccessViewFlags uavFlags = BufferUnorderedAccessViewFlags.None, BufferExtendedShaderResourceViewFlags srvFlags = BufferExtendedShaderResourceViewFlags.None, bool lazyInit = true) where T : unmanaged;

        ResourceRef<Texture1D> CreateTexture1D(string name, Texture1DDescription description, bool lazyInit = true);

        ResourceRef<Texture2D> CreateTexture2D(string name, Texture2DDescription description, bool lazyInit = true);

        ResourceRef<Texture3D> CreateTexture3D(string name, Texture3DDescription description, bool lazyInit = true);

        bool RemoveResource(string name);
    }

    public interface IGraphResources
    {
        IReadOnlyList<ShadowAtlas> ShadowAtlas { get; }

        IReadOnlyList<Texture2D> Textures { get; }

        IReadOnlyList<GBuffer> GBuffers { get; }

        IRenderTargetView? Output { get; }

        ITexture2D OutputTex { get; }

        Viewport OutputViewport { get; }

        Viewport Viewport { get; }

        ResourceRef<ConstantBuffer<T>> GetConstantBuffer<T>(string name) where T : unmanaged;

        ResourceRef<DepthMipChain> GetDepthMipChain(string name);

        ResourceRef<DepthStencil> GetDepthStencilBuffer(string name);

        ResourceRef<GBuffer> GetGBuffer(string name);

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

        bool TryGetResource(string name, [NotNullWhen(true)] out ResourceRef? resourceRef);

        bool TryGetResource<T>(string name, [NotNullWhen(true)] out ResourceRef<T>? resourceRef) where T : class, IDisposable;
    }
}