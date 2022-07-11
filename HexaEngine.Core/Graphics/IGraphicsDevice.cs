﻿namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;

    public interface IGraphicsDevice : IDeviceChild
    {
        public IGraphicsContext Context { get; }

        public ISwapChain SwapChain { get; }

        public IBlendState CreateBlendState(BlendDescription blendDescription);

        public IBuffer CreateBuffer(BufferDescription description);

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged;

        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged;

        public IBuffer CreateBuffer<T>(T[] values, BufferDescription description) where T : unmanaged;

        public IBuffer CreateBuffer<T>(T[] values, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged;

        public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description);

        public IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description);

        public IDepthStencilView CreateDepthStencilView(IResource resource);

        public IRasterizerState CreateRasterizerState(RasterizerDescription description);

        public IRenderTargetView CreateRenderTargetView(IResource resource, Viewport viewport);

        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport);

        public IShaderResourceView CreateShaderResourceView(IResource resource);

        public IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description);

        public ISamplerState CreateSamplerState(SamplerDescription sampler);

        public ITexture1D CreateTexture1D(Texture1DDescription description);

        public ITexture2D CreateTexture2D(Texture2DDescription description);

        public ITexture3D CreateTexture3D(Texture3DDescription description);

        public ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[] subresources);

        public ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[] subresources);

        public ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[] subresources);

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[] subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[] subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None);

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[] subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None);

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, object value, BindFlags bindFlags, ResourceMiscFlag none);

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, object value, BindFlags bindFlags, ResourceMiscFlag none);

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, object value, BindFlags bindFlags, ResourceMiscFlag none);

        public ITexture1D LoadTexture1D(string path);

        public ITexture2D LoadTexture2D(string path);

        public ITexture3D LoadTexture3D(string path);

        public ITexture2D LoadTextureCube(string path);

        public IVertexShader CreateVertexShader(byte[] bytecode);

        public IHullShader CreateHullShader(byte[] bytecode);

        public IDomainShader CreateDomainShader(byte[] bytecode);

        public IGeometryShader CreateGeometryShader(byte[] bytecode);

        public IPixelShader CreatePixelShader(byte[] bytecode);

        public IComputeShader CreateComputeShader(byte[] bytecode);

        public IVertexShader CreateVertexShader(Span<byte> bytecode) => CreateVertexShader(bytecode.ToArray());

        public IHullShader CreateHullShader(Span<byte> bytecode) => CreateHullShader(bytecode.ToArray());

        public IDomainShader CreateDomainShader(Span<byte> bytecode) => CreateDomainShader(bytecode.ToArray());

        public IGeometryShader CreateGeometryShader(Span<byte> bytecode) => CreateGeometryShader(bytecode.ToArray());

        public IPixelShader CreatePixelShader(Span<byte> bytecode) => CreatePixelShader(bytecode.ToArray());

        public IComputeShader CreateComputeShader(Span<byte> bytecode) => CreateComputeShader(bytecode.ToArray());

        public IVertexShader CreateVertexShader(Blob bytecode) => CreateVertexShader(bytecode.AsBytes());

        public IHullShader CreateHullShader(Blob bytecode) => CreateHullShader(bytecode.AsBytes());

        public IDomainShader CreateDomainShader(Blob bytecode) => CreateDomainShader(bytecode.AsBytes());

        public IGeometryShader CreateGeometryShader(Blob bytecode) => CreateGeometryShader(bytecode.AsBytes());

        public IPixelShader CreatePixelShader(Blob bytecode) => CreatePixelShader(bytecode.AsBytes());

        public IComputeShader CreateComputeShader(Blob bytecode) => CreateComputeShader(bytecode.AsBytes());

        public IInputLayout CreateInputLayout(InputElementDescription[] inputElements, Blob vertexShaderBlob);

        public IInputLayout CreateInputLayout(byte[] data);

        public IInputLayout CreateInputLayout(Span<byte> data) => CreateInputLayout(data.ToArray());

        public IInputLayout CreateInputLayout(Blob vBlob) => CreateInputLayout(vBlob.AsBytes());

        public void Compile(string code, string entry, string sourceName, string profile, out Blob shaderBlob, out Blob errorBlob);

        public void Compile(string code, string entry, string sourceName, string profile, out Blob shaderBlob);

        public void CompileFromFile(string path, string entry, string profile, out Blob shaderBlob, out Blob errorBlob);

        public void CompileFromFile(string path, string entry, string profile, out Blob shaderBlob);
    }
}