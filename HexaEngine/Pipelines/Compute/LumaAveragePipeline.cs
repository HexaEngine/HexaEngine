namespace HexaEngine.Pipelines.Compute
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using System.Numerics;

    public class LumaAveragePipeline : ComputePipeline
    {
        private readonly IUnorderedAccessView lumaUAV;
        private readonly ITexture2D luma;
        private readonly IBuffer cbParams;
        private LumaAvgParams lumaAvgParams;

        public readonly IShaderResourceView Output;
        public IUnorderedAccessView? Histogram;

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

        public LumaAveragePipeline(IGraphicsDevice device, int width, int height)
            : base(device, new()
            {
                Path = "compute/lumaAvg/shader.hlsl"
            })
        {
            lumaAvgParams = new();
            lumaAvgParams.PixelCount = (uint)(width * height);
            cbParams = device.CreateBuffer(lumaAvgParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            luma = device.CreateTexture2D(Format.R32Float, 1, 1, 1, 1, null, BindFlags.ShaderResource | BindFlags.UnorderedAccess, ResourceMiscFlag.None);
            lumaUAV = device.CreateUnorderedAccessView(luma, new(luma, UnorderedAccessViewDimension.Texture2D));
            Output = device.CreateShaderResourceView(luma);
        }

        public void Resize(int width, int height)
        {
            lumaAvgParams.PixelCount = (uint)(width * height);
        }

        public override void BeginDispatch(IGraphicsContext context)
        {
            lumaAvgParams.TimeDelta = Time.Delta;
            context.Write(cbParams, lumaAvgParams);
            context.CSSetConstantBuffer(cbParams, 0);
#nullable disable
            context.CSSetUnorderedAccessViews(new IUnorderedAccessView[] { Histogram, lumaUAV });
#nullable enable
            base.BeginDispatch(context);
        }
    }
}