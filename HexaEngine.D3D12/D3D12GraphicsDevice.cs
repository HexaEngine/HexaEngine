namespace HexaEngine.D3D12
{
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D12;
    using Silk.NET.DXGI;
    using System;
    using System.Runtime.CompilerServices;
    using Format = Core.Graphics.Format;
    using SubresourceData = Core.Graphics.SubresourceData;

    public static class Helper
    {
        internal static ResourceFlags Convert(ResourceMiscFlag miscFlags)
        {
            throw new NotImplementedException();
        }
    }

    public unsafe class D3D12GraphicsDevice : IGraphicsDevice
    {
        internal static readonly D3D12 D3D12 = D3D12.GetApi();
        internal readonly ComPtr<ID3D12Device10> Device;
        internal ComPtr<ID3D12CommandAllocator> CommandAllocator;
        internal ComPtr<ID3D12CommandQueue> CommandQueue;
        internal ComPtr<ID3D12Fence> Fence;
        internal ComPtr<ID3D12Debug> Debug;

        private bool disposedValue;

        public D3D12GraphicsDevice(DXGIAdapterD3D12 adapter, bool debug)
        {
            if (debug)
            {
                D3D12.GetDebugInterface(out Debug).ThrowHResult();
                Debug.EnableDebugLayer();
            }

            D3D12.CreateDevice(adapter.IDXGIAdapter, D3DFeatureLevel.Level122, out Device).ThrowHResult();

            CommandQueueDesc commandQueueDesc = new()
            {
                Type = CommandListType.Direct,
                Flags = CommandQueueFlags.None,
            };

            Device.CreateCommandQueue(&commandQueueDesc, out CommandQueue).ThrowHResult();

            Device.CreateCommandAllocator(CommandListType.Direct, out CommandAllocator).ThrowHResult();

            ComPtr<ID3D12GraphicsCommandList> list;
            Device.CreateCommandList(0, CommandListType.Direct, CommandAllocator, new ComPtr<ID3D12PipelineState>(), out list);
        }

        public IGraphicsContext Context { get; }

        public ITextureLoader TextureLoader { get; }

        public string? DebugName { get; set; }

        public bool IsDisposed { get; }

        public nint NativePointer { get; }

        public GraphicsBackend Backend => GraphicsBackend.D3D12;

        public IGPUProfiler Profiler { get; }

        public event EventHandler? OnDisposed;

        public IBuffer CreateBuffer(BufferDescription description)
        {
            var resourceDesc = new ResourceDesc
            {
                Dimension = Silk.NET.Direct3D12.ResourceDimension.Buffer,
                Alignment = 0,
                Width = (ulong)description.ByteWidth,
                Height = 1,
                DepthOrArraySize = 1,
                MipLevels = 1,
                Format = Silk.NET.DXGI.Format.FormatUnknown,
                SampleDesc = new SampleDesc(1, 0),
                Layout = TextureLayout.LayoutRowMajor,
                Flags = Helper.Convert(description.MiscFlags)
            };

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                resourceDesc.Flags |= ResourceFlags.AllowUnorderedAccess;
            }

            var heapProperties = description.Usage switch
            {
                Usage.Dynamic => new HeapProperties(HeapType.Upload),
                Usage.Staging => new HeapProperties(HeapType.Readback),
                _ => new HeapProperties(HeapType.Default)
            };

            ResourceStates resourceStates = ResourceStates.Common;
            if ((description.BindFlags & BindFlags.VertexBuffer) != 0)
            {
                resourceStates |= ResourceStates.VertexAndConstantBuffer;
            }

            if ((description.BindFlags & BindFlags.IndexBuffer) != 0)
            {
                resourceStates |= ResourceStates.IndexBuffer;
            }

            if ((description.BindFlags & BindFlags.ConstantBuffer) != 0)
            {
                resourceStates |= ResourceStates.VertexAndConstantBuffer;
            }

            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
            {
                resourceStates |= ResourceStates.PixelShaderResource | ResourceStates.NonPixelShaderResource;
            }

            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
            {
                resourceStates |= ResourceStates.UnorderedAccess;
            }

            Device.CreateCommittedResource(&heapProperties, HeapFlags.None, &resourceDesc, resourceStates, null, out ComPtr<ID3D12Resource2> buffer).ThrowHResult();

            return new D3D12Buffer(buffer, Device);
        }

        public IBuffer CreateBuffer(void* src, uint length, BufferDescription description)
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T value, BufferDescription description) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T value, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T* values, uint count, BufferDescription description) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public IBuffer CreateBuffer<T>(T* values, uint count, BindFlags bindFlags, Usage usage = Usage.Default, CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None, ResourceMiscFlag miscFlags = ResourceMiscFlag.None) where T : unmanaged
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

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, Core.Graphics.GraphicsPipelineStateDesc state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, Core.Graphics.GraphicsPipelineStateDesc state, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, Core.Graphics.GraphicsPipelineStateDesc state, InputElementDescription[] elementDescriptions, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipeline(GraphicsPipelineDesc desc, Core.Graphics.GraphicsPipelineStateDesc state, InputElementDescription[] inputElements, ShaderMacro[] macros, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
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

        public ITexture1D LoadTexture1D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture1D LoadTexture1D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path, BindFlags flags)
        {
            throw new NotImplementedException();
        }

        public ITexture2D LoadTexture2D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path)
        {
            throw new NotImplementedException();
        }

        public ITexture3D LoadTexture3D(string path, Usage usage, BindFlags bind, CpuAccessFlags cpuAccess, ResourceMiscFlag misc)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

        public void PumpDebugMessages()
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

        public IGraphicsPipeline CreateGraphicsPipelineFromBytecode(GraphicsPipelineBytecodeDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipelineFromBytecode(GraphicsPipelineBytecodeDesc desc, Core.Graphics.GraphicsPipelineStateDesc state, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipeline CreateGraphicsPipelineFromBytecode(GraphicsPipelineBytecodeDesc desc, Core.Graphics.GraphicsPipelineStateDesc state, InputElementDescription[] inputElements, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IComputePipeline CreateComputePipelineFromBytecode(ComputePipelineBytecodeDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IFence CreateFence(ulong initialValue, Core.Graphics.FenceFlags flags)
        {
            throw new NotImplementedException();
        }

        public ICombinedTex2D CreateTex2D(CombinedTex2DDesc desc)
        {
            throw new NotImplementedException();
        }

        public IGraphicsPipelineState CreateGraphicsPipelineState(IGraphicsPipeline pipeline, Core.Graphics.GraphicsPipelineStateDesc desc, [CallerFilePath] string filename = "", [CallerLineNumber] int line = 0)
        {
            throw new NotImplementedException();
        }

        public IResourceBindingList CreateRootDescriptorTable(IGraphicsPipeline pipeline)
        {
            throw new NotImplementedException();
        }
    }
}