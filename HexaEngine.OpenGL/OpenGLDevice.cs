namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using HexaEngine.Mathematics;
    using Silk.NET.Core.Contexts;
    using Silk.NET.OpenGL;
    using System;
    using System.Runtime.CompilerServices;

    public class OpenGLDevice : IGraphicsDevice
    {
        private readonly IGLContext glContext;
        public readonly GL GL;

        public OpenGLDevice(IWindow window)
        {
            glContext = window.OpenGLCreateContext();
            GL = GL.GetApi(glContext);
        }

        public GraphicsBackend Backend => GraphicsBackend.OpenGL;

        public IGraphicsContext Context { get; }

        public ITextureLoader TextureLoader { get; }

        public string? DebugName { get; set; }

        public bool IsDisposed { get; }

        public nint NativePointer { get; }

        public event EventHandler? OnDisposed;

        public IBuffer CreateBuffer(BufferDescription description)
        {
            throw new NotImplementedException();
        }

        public unsafe IBuffer CreateBuffer(void* src, uint length, BufferDescription description)
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : struct
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : struct
        {
            throw new NotImplementedException();
        }

        public unsafe IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public unsafe IBuffer CreateBuffer<T>(T* values, uint count, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc)
        {
            throw new NotImplementedException();
        }

        public IGraphicsContext CreateDeferredContext()
        {
            throw new NotImplementedException();
        }

        public IDepthStencilView CreateDepthStencilView(IResource resource, DepthStencilViewDescription description)
        {
            throw new NotImplementedException();
        }

        public IDepthStencilView CreateDepthStencilView(IResource resource)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery()
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery(Core.Graphics.Query type)
        {
            throw new NotImplementedException();
        }

        public IRenderTargetView CreateRenderTargetView(IResource resource, Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description, Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public ISamplerState CreateSamplerState(SamplerDescription sampler)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IResource resource)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IBuffer buffer, ShaderResourceViewDescription description)
        {
            throw new NotImplementedException();
        }

        public ISwapChain CreateSwapChain(SdlWindow window)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Texture1DDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Texture1DDescription description, SubresourceData[]? subresources)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            throw new NotImplementedException();
        }

        public ITexture1D CreateTexture1D(Format format, int width, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Texture2DDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Texture2DDescription description, SubresourceData[]? subresources)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, int sampleCount = 1, int sampleQuality = 0, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            throw new NotImplementedException();
        }

        public ITexture2D CreateTexture2D(Format format, int width, int height, int arraySize, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Texture3DDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Texture3DDescription description, SubresourceData[]? subresources)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags = BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag misc = ResourceMiscFlag.None)
        {
            throw new NotImplementedException();
        }

        public ITexture3D CreateTexture3D(Format format, int width, int height, int depth, int mipLevels, SubresourceData[]? subresources, BindFlags bindFlags, ResourceMiscFlag none)
        {
            throw new NotImplementedException();
        }

        public IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}