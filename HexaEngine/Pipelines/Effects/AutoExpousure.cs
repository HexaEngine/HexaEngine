namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Specialized;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Pipelines.Compute;
    using System;
    using System.Numerics;

    public class AutoExposure : IEffect
    {
        private int width;
        private int height;
        private bool dirty;
        private readonly ComputePipeline lumaCompute;
        private LumaParams lumaParams;
        private readonly IBuffer cbLumaParams;
        private readonly IBuffer histogram;
        private readonly IUnorderedAccessView histogramUAV;
        private readonly IUnorderedAccessView[] lumaUAVs;

        private readonly ComputePipeline lumaAvgCompute;
        private LumaAvgParams lumaAvgParams;
        private readonly IBuffer cbLumaAvgParams;
        private readonly ITexture2D luma;
        private readonly IShaderResourceView lumaSRV;
        private readonly IUnorderedAccessView lumaUAV;
        private readonly IUnorderedAccessView[] lumaAvgUAVs;

        private readonly Pipeline exposurePipeline;
        private readonly Quad quad;
        private ExposureParams exposureParams = new();
        private readonly IBuffer cbExposureParams;
        private readonly ISamplerState samplerPoint;

        public IShaderResourceView? Color;
        public IRenderTargetView? Output;
        private bool enabled;

        public bool Enabled
        { get => enabled; set { exposureParams.Enabled = value ? 1 : 0; enabled = value; dirty = true; } }

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
            public Vector3 padd;

            public LumaAvgParams()
            {
                MinLogLuminance = -8.0f;
                LogLuminanceRange = (3.0f + 8.0f);
                Tau = 1.1f;
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

        public AutoExposure(IGraphicsDevice device, int width, int height)
        {
            this.width = width;
            this.height = height;
            lumaCompute = new(device, new("compute/luma/shader.hlsl"));
            lumaParams = new();
            lumaParams.InputWidth = (uint)width;
            lumaParams.InputHeight = (uint)height;
            cbLumaParams = device.CreateBuffer(lumaParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            histogram = ComputePipeline.CreateRawBuffer(device, 256 * sizeof(uint));
            histogramUAV = device.CreateUnorderedAccessView(histogram, new(histogram, Format.R32Typeless, 0, 256, BufferUnorderedAccessViewFlags.Raw));
            lumaUAVs = new IUnorderedAccessView[] { histogramUAV };

            lumaAvgCompute = new(device, new("compute/lumaAvg/shader.hlsl"));
            lumaAvgParams = new();
            lumaAvgParams.PixelCount = (uint)(width * height);
            cbLumaAvgParams = device.CreateBuffer(lumaAvgParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            luma = device.CreateTexture2D(Format.R32Float, 1, 1, 1, 1, null, BindFlags.ShaderResource | BindFlags.UnorderedAccess, ResourceMiscFlag.None);
            lumaUAV = device.CreateUnorderedAccessView(luma, new(luma, UnorderedAccessViewDimension.Texture2D));
            lumaSRV = device.CreateShaderResourceView(luma);
            lumaAvgUAVs = new IUnorderedAccessView[] { histogramUAV, lumaUAV };

            exposurePipeline = new(device, new()
            {
                VertexShader = "effects/exposure/vs.hlsl",
                PixelShader = "effects/exposure/ps.hlsl",
            });
            cbExposureParams = device.CreateBuffer(exposureParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            samplerPoint = device.CreateSamplerState(SamplerDescription.PointClamp);

            quad = new(device);
        }

        public void Dispose()
        {
            lumaCompute.Dispose();
            cbLumaParams.Dispose();
            histogram.Dispose();
            histogramUAV.Dispose();

            lumaAvgCompute.Dispose();
            cbLumaAvgParams.Dispose();
            luma.Dispose();
            lumaSRV.Dispose();
            lumaUAV.Dispose();

            exposurePipeline.Dispose();
            cbExposureParams.Dispose();
            quad.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            lumaParams.InputWidth = (uint)width;
            lumaParams.InputHeight = (uint)height;
            lumaAvgParams.PixelCount = (uint)(width * height);
            dirty = true;
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            if (dirty)
            {
                dirty = false;
                context.Write(cbLumaParams, lumaParams);
                context.Write(cbExposureParams, exposureParams);
            }

            if (enabled)
            {
                context.CSSetShaderResource(Color, 0);
                context.CSSetConstantBuffer(cbLumaParams, 0);
                context.CSSetUnorderedAccessViews(lumaUAVs);
                lumaCompute.Dispatch(context, width / 16, height / 16, 1);

                lumaAvgParams.TimeDelta = Time.Delta;
                context.Write(cbLumaAvgParams, lumaAvgParams);
                context.CSSetConstantBuffer(cbLumaAvgParams, 0);
                context.CSSetUnorderedAccessViews(lumaAvgUAVs);
                lumaAvgCompute.Dispatch(context, 1, 1, 1);
            }

            context.PSSetConstantBuffer(cbExposureParams, 0);
            context.PSSetShaderResource(Color, 0);
            context.PSSetShaderResource(lumaSRV, 1);
            context.PSSetSampler(samplerPoint, 0);
            context.SetRenderTarget(Output, null);
            quad.DrawAuto(context, exposurePipeline, Output.Viewport);
            context.ClearState();
        }

        public void DrawSettings()
        {
        }
    }
}