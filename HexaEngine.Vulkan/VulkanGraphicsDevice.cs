namespace HexaEngine.Vulkan
{
    using Hexa.NET.SDL2;
    using Hexa.NET.Vulkan;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using System;
    using System.Runtime.CompilerServices;
    using Format = Core.Graphics.Format;

    public unsafe class VulkanGraphicsDevice : DeviceChildBase, IGraphicsDevice
    {
        private readonly VulkanAdapter vulkanAdapter;
        public VkDevice Device;
        public VkQueue Queue;

        public VulkanGraphicsDevice(VulkanAdapter vulkanAdapter, VkDevice device, VkQueue queue)
        {
            this.vulkanAdapter = vulkanAdapter;
            Device = device;
            Queue = queue;
        }

        public GraphicsBackend Backend => GraphicsBackend.Vulkan;

        public IGraphicsContext Context { get; }

        public ITextureLoader TextureLoader { get; }

        public IGPUProfiler Profiler { get; }

        public VkPhysicalDevice PhysicalDevice => vulkanAdapter.PhysicalDevice;

        public GraphicsDeviceCapabilities Capabilities { get; }

        public ISwapChain CreateSwapChain(SdlWindow window)
        {
            return CreateSwapChain(window.GetWindow());
        }

        public ISwapChain CreateSwapChain(SDLWindow* window)
        {
            return vulkanAdapter.CreateSwapChain(this, window);
        }

        public ISwapChain CreateSwapChain(SdlWindow window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            return CreateSwapChain(window.GetWindow(), swapChainDescription, fullscreenDescription);
        }

        public ISwapChain CreateSwapChain(SDLWindow* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            return vulkanAdapter.CreateSwapChain(this, window, swapChainDescription, fullscreenDescription);
        }

        public IBuffer CreateBuffer(BufferDescription description)
        {
            VkBufferCreateInfo info = default;
            info.SharingMode = VkSharingMode.Concurrent;
            info.Size = (ulong)description.ByteWidth;

            if ((description.BindFlags & BindFlags.VertexBuffer) != 0)
            {
                info.Usage |= (uint)VkBufferUsageFlagBits.VertexBufferBit;
            }

            if ((description.BindFlags & BindFlags.IndexBuffer) != 0)
            {
                info.Usage |= (uint)VkBufferUsageFlagBits.IndexBufferBit;
            }

            if ((description.BindFlags & BindFlags.ConstantBuffer) != 0)
            {
                info.Usage |= (uint)VkBufferUsageFlagBits.UniformBufferBit;
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                info.Usage |= (uint)VkBufferUsageFlagBits.ShaderBindingTableBitKhr;
            }

            if ((description.MiscFlags & ResourceMiscFlag.DrawIndirectArguments) == 0)
            {
                info.Usage |= (uint)VkBufferUsageFlagBits.IndirectBufferBit;
            }

            VkBuffer buffer;
            Vulkan.VkCreateBuffer(Device, &info, null, &buffer);

            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer(void* values, int stride, uint count, BufferDescription description)
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

        public IQuery CreateQuery()
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
            throw new NotImplementedException();
        }

        public IShaderResourceView CreateShaderResourceView(IResource texture, ShaderResourceViewDescription description)
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTextureCube(string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture1D(ITexture1D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture1D(ITexture1D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture2D(ITexture2D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture2D(ITexture2D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture3D(ITexture3D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTexture3D(ITexture3D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTextureCube(ITexture2D texture, string path)
        {
            throw new NotImplementedException();
        }

        public void SaveTextureCube(ITexture2D texture, Format format, string path)
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery(Query type)
        {
            throw new NotImplementedException();
        }

        public IGraphicsContext CreateDeferredContext()
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description)
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

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc)
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer(void* src, uint length, BufferDescription description)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path, BindFlags flags)
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, ShaderMacro[] macros)
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
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

        protected override void DisposeCore()
        {
            Device.DestroyDevice(null);
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