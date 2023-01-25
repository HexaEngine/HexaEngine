#nullable disable

namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using System;
    using System.Numerics;

    public class AutoExposure : IEffect
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
        private ConstantBuffer<ExposureParams> exposureParams;
        private ISamplerState samplerPoint;

        public IShaderResourceView Color;
        public IRenderTargetView Output;
        private bool enabled;
        private float minLogLuminance = -8;
        private float maxLogLuminance = 3;
        private float tau = 1.1f;

        public unsafe bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                exposureParams.Local->Enabled = value ? 1 : 0;
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

        private struct ExposureParams
        {
            public int Enabled;
            public Vector3 Padd;

            public ExposureParams()
            {
                Enabled = 0;
                Padd = default;
            }
        }

        #endregion Structs

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            Color = ResourceManager.AddTextureSRV("AutoExposure", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            this.width = width;
            this.height = height;
            lumaCompute = device.CreateComputePipeline(new("compute/luma/shader.hlsl"));

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

            exposurePipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/exposure/vs.hlsl",
                PixelShader = "effects/exposure/ps.hlsl",
            });

            samplerPoint = device.CreateSamplerState(SamplerDescription.PointClamp);

            quad = new(device);
            InitUnsafe(device);

            Output = await ResourceManager.GetTextureRTVAsync("Tonemap");
        }

        private unsafe void InitUnsafe(IGraphicsDevice device)
        {
            lumaParams = new(device, CpuAccessFlags.Write);
            lumaParams.Local->InputWidth = (uint)width;
            lumaParams.Local->InputHeight = (uint)height;

            lumaUAVs = AllocArray(2);
            lumaUAVs[0] = (void*)histogramUAV.NativePointer;

            lumaAvgCompute = device.CreateComputePipeline(new("compute/lumaAvg/shader.hlsl"));

            lumaAvgParams = new(device, CpuAccessFlags.Write); //device.CreateBuffer(lumaAvgParams, 1, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            lumaAvgParams.Local->PixelCount = (uint)(width * height);

            lumaAvgUAVs = AllocArray(2);
            lumaAvgUAVs[0] = (void*)histogramUAV.NativePointer;
            lumaAvgUAVs[1] = (void*)lumaUAV.NativePointer;

            exposureParams = new(device, CpuAccessFlags.Write); //device.CreateBuffer(exposureParams, 1, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            dirty = true;
        }

        public void BeginResize()
        {
            ResourceManager.RequireUpdate("AutoExposure");
        }

        public async void EndResize(int width, int height)
        {
            Output = await ResourceManager.GetTextureRTVAsync("Tonemap");
            Color = ResourceManager.UpdateTextureSRV("AutoExposure", TextureDescription.CreateTexture2DWithRTV(width, height, 1));

            this.width = width;
            this.height = height;
            EndResizeUnsafe(width, height);
            dirty = true;
        }

        private unsafe void EndResizeUnsafe(int width, int height)
        {
            lumaParams.Local->InputWidth = (uint)width;
            lumaParams.Local->InputHeight = (uint)height;
            lumaAvgParams.Local->PixelCount = (uint)(width * height);
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            if (dirty)
            {
                dirty = false;
                lumaParams.Update(context);
                exposureParams.Update(context);
            }

            if (enabled)
            {
                context.CSSetShaderResource(Color, 0);
                context.CSSetConstantBuffer(lumaParams, 0);
                context.CSSetUnorderedAccessViews(lumaUAVs, 1);
                lumaCompute.Dispatch(context, width / 16, height / 16, 1);

                lumaAvgParams.Local->TimeDelta = Time.Delta;
                lumaAvgParams.Update(context);
                context.CSSetConstantBuffer(lumaAvgParams, 0);
                context.CSSetUnorderedAccessViews(lumaAvgUAVs, 2);
                lumaAvgCompute.Dispatch(context, 1, 1, 1);
            }

            context.PSSetConstantBuffer(exposureParams, 0);
            context.PSSetShaderResource(Color, 0);
            context.PSSetShaderResource(lumaSRV, 1);
            context.PSSetSampler(samplerPoint, 0);
            context.SetRenderTarget(Output, null);
            quad.DrawAuto(context, exposurePipeline, Output.Viewport);
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
            exposureParams.Dispose();
            samplerPoint.Dispose();
            quad.Dispose();
            Free(lumaUAVs);
            Free(lumaAvgUAVs);
            GC.SuppressFinalize(this);
        }
    }
}