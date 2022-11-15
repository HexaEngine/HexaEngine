namespace HexaEngine.Pipelines.Compute
{
    using HexaEngine.Core.Graphics;

    public class LumaPipeline : ComputePipeline
    {
        private LumaParams lumaParams;
        private readonly IBuffer cbParams;
        private readonly IUnorderedAccessView[] uavs;
        private bool dirty;

        public readonly IBuffer Histogram;
        public readonly IUnorderedAccessView HistogramUAV;

        public float MinLogLuminance
        { get => lumaParams.MinLogLuminance; set { lumaParams.MinLogLuminance = value; dirty = true; } }

        public float OneOverLogLuminanceRange
        { get => lumaParams.OneOverLogLuminanceRange; set { lumaParams.OneOverLogLuminanceRange = value; dirty = true; } }

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

        public LumaPipeline(IGraphicsDevice device, int width, int height)
            : base(device, new()
            {
                Path = "compute/luma/shader.hlsl"
            })
        {
            lumaParams = new();
            lumaParams.InputWidth = (uint)width;
            lumaParams.InputHeight = (uint)height;
            cbParams = device.CreateBuffer(lumaParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            Histogram = CreateRawBuffer(device, 256 * sizeof(uint));
            HistogramUAV = device.CreateUnorderedAccessView(Histogram, new(Histogram, Format.R32Typeless, 0, 256, BufferUnorderedAccessViewFlags.Raw));
            uavs = new IUnorderedAccessView[1];
            uavs[0] = HistogramUAV;
        }

        public void Resize(int width, int height)
        {
            lumaParams.InputWidth = (uint)width;
            lumaParams.InputHeight = (uint)height;
            dirty = true;
        }

        public override void BeginDispatch(IGraphicsContext context)
        {
            if (dirty)
            {
                dirty = false;
                context.Write(cbParams, lumaParams);
            }

            context.CSSetConstantBuffer(cbParams, 0);
            context.CSSetUnorderedAccessViews(uavs);
            base.BeginDispatch(context);
        }
    }
}