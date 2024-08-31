using Hexa.NET.Daxa;
using Hexa.NET.SDL2;
using HexaEngine.Core.Graphics;
using HexaEngine.Core.Windows;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HexaEngine.Daxa
{
    public unsafe class DaxaGraphicsDevice : IGraphicsDevice
    {
        public DaxaDevice Device;

        public DaxaGraphicsDevice(DaxaInstance instance, bool debug)
        {
            DaxaDeviceInfo deviceInfo = new();
            deviceInfo.Flags = (uint)DaxaDeviceFlagBits.BufferDeviceAddressCaptureReplayBit;
            deviceInfo.Selector = (void*)Marshal.GetFunctionPointerForDelegate<Selector>(OnSelector);
            deviceInfo.Name = "my device";
            deviceInfo.MaxAllowedImages = 10000;
            deviceInfo.MaxAllowedSamplers = 400;
            deviceInfo.MaxAllowedBuffers = 10000;
            deviceInfo.MaxAllowedAccelerationStructures = 10000;
            instance.CreateDevice(ref deviceInfo, ref Device).CheckError();
        }

        private static unsafe int OnSelector(DaxaDeviceProperties* properties)
        {
            return DaxaDefaultDeviceScore(properties);
        }

        public GraphicsBackend Backend { get; }

        public IGraphicsContext Context { get; }

        public ITextureLoader TextureLoader { get; }

        public IGPUProfiler Profiler { get; }

        public GraphicsDeviceCapabilities Capabilities { get; }

        public string? DebugName { get; set; }

        public bool IsDisposed { get; }

        public nint NativePointer { get; }

        public event EventHandler? OnDisposed;

        public IBuffer CreateBuffer(BufferDescription description)
        {
            DaxaBufferInfo info;
            info.Size = (ulong)description.ByteWidth;
            info.AllocateInfo = 0;
            info.Name = string.Empty;
            DaxaBufferId bufferId;
            Device.DvcCreateBuffer(ref info, &bufferId);

            return new DaxaBuffer(Device, bufferId);
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged
        {
            return CreateBuffer(description);
        }

        public unsafe IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipeline(ComputePipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
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

        public IFence CreateFence(ulong initialValue, FenceFlags flags)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipelineState CreateGraphicsPipelineState(IGraphicsPipeline pipeline, GraphicsPipelineStateDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery()
        {
            throw new NotImplementedException();
        }

        public IQuery CreateQuery(Query type)
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

        public IResourceBindingList CreateResourceBindingList(IGraphicsPipeline pipeline)
        {
            throw new NotImplementedException();
        }

        public IResourceBindingList CreateResourceBindingList(IComputePipeline pipeline)
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

        public unsafe ISwapChain CreateSwapChain(SDLWindow* window)
        {
            throw new NotImplementedException();
        }

        public ISwapChain CreateSwapChain(SdlWindow window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
        {
            throw new NotImplementedException();
        }

        public unsafe ISwapChain CreateSwapChain(SDLWindow* window, SwapChainDescription swapChainDescription, SwapChainFullscreenDescription fullscreenDescription)
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

        public IUnorderedAccessView CreateUnorderedAccessView(IResource resource, UnorderedAccessViewDescription description)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Device.DvcDecRefcnt();
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
    }
}