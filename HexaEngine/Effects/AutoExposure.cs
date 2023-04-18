#nullable disable


namespace HexaEngine.Effects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public class AutoExposure : IPostFx
    {
        private int width;
        private int height;
        private bool dirty;
        private IComputePipeline lumaCompute;
        private ConstantBuffer<LumaParams> lumaParams;
        private IBuffer histogram;
        private IUnorderedAccessView histogramUAV;
        private unsafe void** lumaUAVs;

        private IComputePipeline lumaAvgCompute;
        private ConstantBuffer<LumaAvgParams> lumaAvgParams;
        private ITexture2D luma;
        private IShaderResourceView lumaSRV;
        private IUnorderedAccessView lumaUAV;
        private unsafe void** lumaAvgUAVs;

        private IGraphicsPipeline exposurePipeline;
        private Quad quad;
        private ISamplerState samplerPoint;

        public IShaderResourceView Input;
        public IRenderTargetView Output;
        private bool enabled = true;
        private float minLogLuminance = -8;
        private float maxLogLuminance = 3;
        private float tau = 1.1f;

        public unsafe bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                dirty = true;
            }
        }

        public unsafe float MinLogLuminance
        {
            get => minLogLuminance;
            set
            {
                minLogLuminance = value;
                lumaAvgParams.Local->MinLogLuminance = value;
                lumaAvgParams.Local->LogLuminanceRange = Math.Abs(maxLogLuminance) + Math.Abs(minLogLuminance);
                lumaParams.Local->MinLogLuminance = value;
                lumaParams.Local->OneOverLogLuminanceRange = 1.0f / lumaAvgParams.Local->LogLuminanceRange;
                dirty = true;
            }
        }

        public unsafe float MaxLogLuminance
        {
            get => maxLogLuminance;
            set
            {
                maxLogLuminance = value;
                lumaAvgParams.Local->LogLuminanceRange = Math.Abs(maxLogLuminance) + Math.Abs(minLogLuminance);
                lumaParams.Local->OneOverLogLuminanceRange = 1.0f / lumaAvgParams.Local->LogLuminanceRange;
                dirty = true;
            }
        }

        public unsafe float Tau
        {
            get => tau;
            set
            {
                tau = value;
                lumaAvgParams.Local->Tau = value;
                dirty = true;
            }
        }

        public string Name => "Auto Exposure";
        public PostFxFlags Flags => PostFxFlags.None;
        public int Priority { get; set; } = 200;

        #region Structs

        private struct LumaParams
        {
            public uint InputWidth;
            public uint InputHeight;
            public float MinLogLuminance;
            public float OneOverLogLuminanceRange;

            public LumaParams()
            {
                InputWidth = 0;
                InputHeight = 0;
                MinLogLuminance = -8.0f;
                OneOverLogLuminanceRange = 1.0f / (3.0f + 8.0f);
            }
        }

        private struct LumaAvgParams
        {
            public uint PixelCount;
            public float MinLogLuminance;
            public float LogLuminanceRange;
            public float TimeDelta;
            public float Tau;
            public Vector3 Padd;

            public LumaAvgParams()
            {
                MinLogLuminance = -8.0f;
                LogLuminanceRange = 3.0f + 8.0f;
                Tau = 1.1f;
                Padd = default;
            }
        }

        #endregion Structs

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            this.width = width;
            this.height = height;
            lumaCompute = await device.CreateComputePipelineAsync(new("compute/luma/shader.hlsl"));

            BufferDescription description = new()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource | BindFlags.IndexBuffer | BindFlags.VertexBuffer,
                ByteWidth = 256 * sizeof(uint),
                MiscFlags = ResourceMiscFlag.BufferAllowRawViews
            };
            histogram = device.CreateBuffer(description);
            histogramUAV = device.CreateUnorderedAccessView(histogram, new(histogram, Format.R32Typeless, 0, 256, BufferUnorderedAccessViewFlags.Raw));

            luma = device.CreateTexture2D(Format.R32Float, 1, 1, 1, 1, null, BindFlags.ShaderResource | BindFlags.UnorderedAccess, ResourceMiscFlag.None);
            lumaUAV = device.CreateUnorderedAccessView(luma, new(luma, UnorderedAccessViewDimension.Texture2D));
            lumaSRV = device.CreateShaderResourceView(luma);

            exposurePipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/exposure/vs.hlsl",
                PixelShader = "effects/exposure/ps.hlsl",
            });

            samplerPoint = device.CreateSamplerState(SamplerDescription.PointClamp);

            quad = new(device);
            InitUnsafe(device);

            dirty = true;
        }

        private unsafe void InitUnsafe(IGraphicsDevice device)
        {
            lumaParams = new(device, new LumaParams(), CpuAccessFlags.Write);
            lumaParams.Local->InputWidth = (uint)width;
            lumaParams.Local->InputHeight = (uint)height;

            lumaUAVs = AllocArray(2);
            lumaUAVs[0] = (void*)histogramUAV.NativePointer;

            lumaAvgCompute = device.CreateComputePipeline(new("compute/lumaAvg/shader.hlsl"));

            lumaAvgParams = new(device, new LumaAvgParams(), CpuAccessFlags.Write);
            lumaAvgParams.Local->PixelCount = (uint)(width * height);

            lumaAvgUAVs = AllocArray(2);
            lumaAvgUAVs[0] = (void*)histogramUAV.NativePointer;
            lumaAvgUAVs[1] = (void*)lumaUAV.NativePointer;
        }

        private unsafe void ResizeUnsafe(int width, int height)
        {
            lumaParams.Local->InputWidth = (uint)width;
            lumaParams.Local->InputHeight = (uint)height;
            lumaAvgParams.Local->PixelCount = (uint)(width * height);
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.

        public async void Resize(int width, int height)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            this.width = width;
            this.height = height;
            ResizeUnsafe(width, height);
            dirty = true;
        }

        public void SetOutput(IRenderTargetView view, Viewport viewport)
        {
            Output = view;
        }

        public void SetInput(IShaderResourceView view)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                dirty = false;
                lumaParams.Update(context);
            }
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null) return;

            context.CSSetShaderResource(Input, 0);
            context.CSSetConstantBuffer(lumaParams, 0);
            context.CSSetUnorderedAccessViews(lumaUAVs, 1);
            lumaCompute.Dispatch(context, width / 16, height / 16, 1);

            lumaAvgParams.Local->TimeDelta = Time.Delta;
            lumaAvgParams.Update(context);
            context.CSSetConstantBuffer(lumaAvgParams, 0);
            context.CSSetUnorderedAccessViews(lumaAvgUAVs, 2);
            lumaAvgCompute.Dispatch(context, 1, 1, 1);

            context.PSSetShaderResource(Input, 0);
            context.PSSetShaderResource(lumaSRV, 1);
            context.PSSetSampler(samplerPoint, 0);
            context.SetRenderTarget(Output, null);
            context.SetViewport(Output.Viewport);
            quad.DrawAuto(context, exposurePipeline);
            context.ClearState();
        }

        public unsafe void Dispose()
        {
            lumaCompute.Dispose();
            lumaParams.Dispose();
            histogram.Dispose();
            histogramUAV.Dispose();

            lumaAvgCompute.Dispose();
            lumaAvgParams.Dispose();
            luma.Dispose();
            lumaSRV.Dispose();
            lumaUAV.Dispose();

            exposurePipeline.Dispose();
            samplerPoint.Dispose();
            quad.Dispose();
            Free(lumaUAVs);
            Free(lumaAvgUAVs);
            GC.SuppressFinalize(this);
        }
    }
}