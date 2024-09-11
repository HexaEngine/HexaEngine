namespace HexaEngine.OpenGL
{
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using Silk.NET.Core.Contexts;
    using Silk.NET.OpenGL;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    public unsafe class OpenGLGraphicsDevice : IGraphicsDevice
    {
        private readonly IGLContext glContext;
        public static GL GL;

        public OpenGLGraphicsDevice(IWindow window, bool debug)
        {
            glContext = window.OpenGLCreateContext();
            GL = GL.GetApi(glContext);

            if (debug)
            {
                GL.Enable(EnableCap.DebugOutput);
                GL.DebugMessageCallback(DebugMsg, null);
            }

            Context = new OpenGLGraphicsContext(GL);
            ShaderCompiler = new(GL);
            TextureLoader = new OpenGLTextureLoader(this);
            Profiler = new OpenGLProfiler();

            Capabilities = new GraphicsDeviceCapabilities
            {
                Flags = InitializeCapabilitiesFlags()
            };
        }

        public void DebugMsg(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
        {
            var msg = ToStringFromUTF8((byte*)message);
            Trace.WriteLine(msg);
        }

        private GraphicsDeviceCapabilitiesFlags InitializeCapabilitiesFlags()
        {
            GraphicsDeviceCapabilitiesFlags flags = GraphicsDeviceCapabilitiesFlags.None;

            if (GL.IsExtensionPresent("GL_NV_command_list"))
            {
                flags |= GraphicsDeviceCapabilitiesFlags.SupportsCommandLists;
            }

            if (GL.IsExtensionPresent("GL_ARB_geometry_shader4"))
            {
                flags |= GraphicsDeviceCapabilitiesFlags.SupportsGeometryShaders;
            }

            if (GL.IsExtensionPresent("GL_ARB_tessellation_shader"))
            {
                flags |= GraphicsDeviceCapabilitiesFlags.SupportsTessellationShaders;
            }

            if (GL.IsExtensionPresent("GL_NV_conservative_raster") ||
                GL.IsExtensionPresent("GL_INTEL_conservative_rasterization") ||
                GL.IsExtensionPresent("GL_AMD_conservative_depth") ||
                GL.IsExtensionPresent("GL_ARB_conservative_depth"))
            {
                flags |= GraphicsDeviceCapabilitiesFlags.SupportsConservativeRasterization;
            }

            if (CheckForRayTracingSupport())
            {
                flags |= GraphicsDeviceCapabilitiesFlags.SupportsRayTracing;
            }

            return flags;
        }

        private static bool CheckForTessellationShaderSupport()
        {
            // Check for the presence of the geometry shader feature
            return GL.GetInteger(GetPName.MaxTessControlUniformBlocks) != 0;
        }

        private static bool CheckForGeometryShaderSupport()
        {
            // Check for the presence of the geometry shader feature
            return GL.GetInteger(GetPName.MaxGeometryUniformComponents) != 0;
        }

        private static bool CheckForConservativeRasterizationSupport()
        {
            // Check for the presence of the GL_NV_conservative_raster extension
            return GL.IsExtensionPresent("GL_NV_conservative_raster");
        }

        private static bool CheckForRayTracingSupport()
        {
            // Check for the presence of the ray tracing extension (e.g., GL_NV_ray_tracing)
            return GL.IsExtensionPresent("GL_NV_ray_tracing");
        }

        public GraphicsBackend Backend => GraphicsBackend.OpenGL;

        public IGraphicsContext Context { get; }

        public ITextureLoader TextureLoader { get; }

        public ShaderCompiler ShaderCompiler { get; }

        public string? DebugName { get; set; }

        public bool IsDisposed { get; }

        public nint NativePointer { get; }

        public IGPUProfiler Profiler { get; }
        public GraphicsDeviceCapabilities Capabilities { get; }

        public event EventHandler? OnDisposed;

        public IBuffer CreateBuffer(BufferDescription description)
        {
            var buffer = GL.GenBuffer();
            CheckError();

            GL.BufferData(Helper.ConvertBufferTarget(description.BindFlags, description.MiscFlags), (nuint)description.ByteWidth, null, Helper.Convert(description.Usage));

            return new OpenGLBuffer(buffer, description);
        }

        public unsafe IBuffer CreateBuffer(void* src, uint length, BufferDescription description)
        {
            var buffer = GL.GenBuffer();
            CheckError();
            description.ByteWidth = (int)length;

            var target = Helper.ConvertBufferTarget(description.BindFlags, description.MiscFlags);
            GL.BindBuffer(target, buffer);
            GL.BufferData(target, length, src, Helper.Convert(description.Usage));
            GL.BindBuffer(target, 0);
            CheckError();

            return new OpenGLBuffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged
        {
            if (description.ByteWidth == 0)
            {
                description.ByteWidth = sizeof(T);
            }

            var buffer = GL.GenBuffer();
            CheckError();

            var target = Helper.ConvertBufferTarget(description.BindFlags, description.MiscFlags);
            GL.BindBuffer(target, buffer);
            GL.BufferData(target, (nuint)description.ByteWidth, &value, Helper.Convert(description.Usage));
            GL.BindBuffer(target, 0);
            CheckError();

            return new OpenGLBuffer(buffer, description);
        }

        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            BufferDescription description = new(0, bindFlags, usage, cpuAccessFlags, miscFlags);
            return CreateBuffer(value, description);
        }

        public unsafe IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged
        {
            uint size = (uint)(sizeof(T) * count);
            description.ByteWidth = (int)size;
            var buffer = GL.GenBuffer();
            CheckError();

            var target = Helper.ConvertBufferTarget(description.BindFlags, description.MiscFlags);
            var usage = Helper.Convert(description.Usage);
            GL.BindBuffer(target, buffer);
            GL.BufferData(target, (nuint)description.ByteWidth, values, usage);
            GL.BindBuffer(target, 0);
            CheckError();

            return new OpenGLBuffer(buffer, description);
        }

        public unsafe IBuffer CreateBuffer(void* values, int stride, uint count, BufferDescription description)
        {
            uint size = (uint)(stride * count);
            description.ByteWidth = (int)size;
            var buffer = GL.GenBuffer();
            CheckError();

            var target = Helper.ConvertBufferTarget(description.BindFlags, description.MiscFlags);
            var usage = Helper.Convert(description.Usage);
            GL.BindBuffer(target, buffer);
            GL.BufferData(target, (nuint)description.ByteWidth, values, usage);
            GL.BindBuffer(target, 0);
            CheckError();

            return new OpenGLBuffer(buffer, description);
        }

        public unsafe IBuffer CreateBuffer<T>(T* values, uint count, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            BufferDescription description = new(0, bindFlags, usage, cpuAccessFlags, miscFlags);
            return CreateBuffer(values, count, description);
        }

        public IGraphicsContext CreateDeferredContext()
        {
            throw new NotSupportedException();
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
            return new OpenGLGraphicsPipeline(this, desc, macros, $"({nameof(OpenGLGraphicsPipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLGraphicsPipeline(this, desc, elementDescriptions, $"({nameof(OpenGLGraphicsPipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLGraphicsPipeline(this, desc, inputElements, macros, $"({nameof(OpenGLGraphicsPipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLGraphicsPipeline(this, desc, state, $"({nameof(OpenGLGraphicsPipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLGraphicsPipeline(this, desc, state, macros, $"({nameof(OpenGLGraphicsPipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLGraphicsPipeline(this, desc, state, elementDescriptions, $"({nameof(OpenGLGraphicsPipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLGraphicsPipeline(this, desc, state, inputElements, macros, $"({nameof(OpenGLGraphicsPipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLComputePipeline(this, desc, $"({nameof(OpenGLComputePipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            return new OpenGLComputePipeline(this, desc, macros, $"({nameof(OpenGLComputePipeline)} : {filename}, Line:{line.ToString(CultureInfo.InvariantCulture)})");
        }

        public IQuery CreateQuery()
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery(Core.Graphics.Query type)
        {
            throw new NotImplementedException();
        }

        public IRenderTargetView CreateRenderTargetView(IResource resource)
        {
            throw new NotImplementedException();
        }

        public IRenderTargetView CreateRenderTargetView(IResource resource, RenderTargetViewDescription description)
        {
            throw new NotImplementedException();
        }

        public ISamplerState CreateSamplerState(SamplerStateDescription sampler)
        {
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IResource resource)
        {
            ShaderResourceViewDescription description = default;
            if (resource is ITexture1D texture1D)
            {
                description = new(texture1D, texture1D.Description.ArraySize > 1);
            }

            if (resource is ITexture2D texture2D)
            {
                description = new(texture2D, ShaderResourceViewDimension.Texture2D);
            }

            if (resource is ITexture3D texture3D)
            {
                description = new(texture3D);
            }

            return new OpenGLShaderResourceView((uint)resource.NativePointer, description);
        }

        public IShaderResourceView CreateShaderResourceView(IResource resource, ShaderResourceViewDescription description)
        {
            return new OpenGLShaderResourceView((uint)resource.NativePointer, description);
        }

        public IShaderResourceView CreateShaderResourceView(IBuffer buffer)
        {
            ShaderResourceViewDescription description = new(ShaderResourceViewDimension.Buffer);
            return new OpenGLShaderResourceView((uint)buffer.NativePointer, description, ObjectIdentifier.Buffer);
        }

        public IShaderResourceView CreateShaderResourceView(IBuffer buffer, ShaderResourceViewDescription description)
        {
            return new OpenGLShaderResourceView((uint)buffer.NativePointer, description, ObjectIdentifier.Buffer);
        }

        public ISwapChain CreateSwapChain(SdlWindow window)
        {
            return new OpenGLSwapChain(GL, glContext, window);
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
            return new OpenGLUnorderedAccessView((uint)resource.NativePointer, description);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ISwapChain CreateSwapChain(SDLWindow* window)
        {
            throw new NotImplementedException();
        }

        public ISwapChain CreateSwapChain(SdlWindow window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            throw new NotImplementedException();
        }

        public ISwapChain CreateSwapChain(SDLWindow* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            throw new NotImplementedException();
        }

        public IFence CreateFence(ulong initialValue, FenceFlags flags)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipelineState CreateGraphicsPipelineState(IGraphicsPipeline pipeline, GraphicsPipelineStateDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IResourceBindingList CreateResourceBindingList(IGraphicsPipeline pipeline)
        {
            throw new NotImplementedException();
        }

        public IResourceBindingList CreateResourceBindingList(IComputePipeline pipeline)
        {
            throw new NotImplementedException();
        }

        public ICommandBuffer CreateCommandBuffer()
        {
            throw new NotImplementedException();
        }

        public IComputePipelineState CreateComputePipelineState(IComputePipeline pipeline, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public void SetGlobalSRV(string name, IShaderResourceView? srv)
        {
            throw new NotImplementedException();
        }

        public void SetGlobalCBV(string name, IBuffer? cbv)
        {
            throw new NotImplementedException();
        }

        public void SetGlobalSampler(string name, ISamplerState? sampler)
        {
            throw new NotImplementedException();
        }

        public void SetGlobalUAV(string name, IUnorderedAccessView? uav, uint initialCount = uint.MaxValue)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDescEx desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDescEx desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }
    }
}