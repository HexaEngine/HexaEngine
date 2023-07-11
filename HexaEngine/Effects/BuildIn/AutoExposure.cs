#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System;
    using System.Numerics;

    public class AutoExposure : IPostFx
    {
        private int width;
        private int height;
        private bool dirty;
        private IComputePipeline lumaCompute;
        private ConstantBuffer<LumaParams> lumaParams;
        private UavBuffer<uint> histogram;

        private IComputePipeline lumaAvgCompute;
        private ConstantBuffer<LumaAvgParams> lumaAvgParams;
        private UavTexture2D luma;

        public IShaderResourceView Input;
        public IRenderTargetView Output;
        private bool enabled = true;
        private float minLogLuminance = -8;
        private float maxLogLuminance = 3;
        private float tau = 1.1f;
        private int priority = 100;

        public event Action<bool> OnEnabledChanged;

        public event Action<int> OnPriorityChanged;

        public string Name => "Auto Exposure";

        public PostFxFlags Flags => PostFxFlags.NoOutput;

        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
                OnPriorityChanged?.Invoke(value);
            }
        }

        public unsafe bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                dirty = true;
                OnEnabledChanged?.Invoke(value);
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

            public LumaParams(uint width, uint height)
            {
                InputWidth = width;
                InputHeight = height;
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

            public LumaAvgParams(uint pixelCount)
            {
                PixelCount = pixelCount;
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

            lumaParams = new(device, new LumaParams((uint)width, (uint)height), CpuAccessFlags.Write);
            lumaAvgParams = new(device, new LumaAvgParams((uint)(width * height)), CpuAccessFlags.Write);

            lumaCompute = await device.CreateComputePipelineAsync(new("compute/luma/shader.hlsl"));
            lumaAvgCompute = await device.CreateComputePipelineAsync(new("compute/lumaAvg/shader.hlsl"));

            histogram = new(device, 256, CpuAccessFlags.None, Format.R32Typeless, BufferUnorderedAccessViewFlags.Raw);

            luma = new(device, Format.R32Float, 1, 1, 1, 1, true, false, ResourceMiscFlag.None);
            ResourceManager2.Shared.AddShaderResourceView("Luma", luma.SRV);
        }

        public unsafe void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            lumaParams.Local->InputWidth = (uint)width;
            lumaParams.Local->InputHeight = (uint)height;
            lumaAvgParams.Local->PixelCount = (uint)(width * height);
            dirty = true;
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public unsafe void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                dirty = false;
                lumaParams.Update(context);
            }

            lumaAvgParams.Local->TimeDelta = Time.Delta;
            lumaAvgParams.Update(context);
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            context.CSSetShaderResource(0, Input);
            context.CSSetConstantBuffer(0, lumaParams);
            context.CSSetUnorderedAccessView((void*)histogram.UAV.NativePointer);
            lumaCompute.Dispatch(context, (uint)width / 16, (uint)height / 16, 1);
            nint* emptyUAVs = stackalloc nint[1];
            context.CSSetUnorderedAccessView(null);
            context.CSSetConstantBuffer(0, null);
            context.CSSetShaderResource(0, null);

            nint* lumaAvgUAVs = stackalloc nint[] { histogram.UAV.NativePointer, luma.UAV.NativePointer };
            uint* initialCount = stackalloc uint[] { uint.MaxValue, uint.MaxValue };
            context.CSSetConstantBuffer(0, lumaAvgParams);
            context.CSSetUnorderedAccessViews(2, (void**)lumaAvgUAVs, initialCount);
            lumaAvgCompute.Dispatch(context, 1, 1, 1);
            nint* emptyUAV2s = stackalloc nint[2];
            context.CSSetUnorderedAccessViews(2, (void**)emptyUAV2s, null);
            context.CSSetConstantBuffer(0, null);
        }

        public unsafe void Dispose()
        {
            lumaCompute.Dispose();
            lumaParams.Dispose();
            histogram.Dispose();

            lumaAvgCompute.Dispose();
            lumaAvgParams.Dispose();
            luma.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}