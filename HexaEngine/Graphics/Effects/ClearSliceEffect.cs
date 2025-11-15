namespace HexaEngine.Graphics.Effects
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;

    public class ClearSliceEffect : DisposableBase
    {
        private readonly IComputePipelineState computePipelineState;
        private readonly ConstantBuffer<UPoint4> paramBuffer;

        public ClearSliceEffect(IGraphicsDevice device)
        {
            computePipelineState = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "HexaEngine.Core:shaders/filter/clear/cs.hlsl"
            });
            paramBuffer = new(CpuAccessFlags.Write);
            computePipelineState.Bindings.SetCBV("CBParams", paramBuffer);
        }

        public ClearSliceEffect(IGraphResourceBuilder builder)
        {
            var device = builder.Device;

            computePipelineState = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "HexaEngine.Core:shaders/filter/clear/cs.hlsl"
            });
            paramBuffer = new(CpuAccessFlags.Write);
            computePipelineState.Bindings.SetCBV("CBParams", paramBuffer);
        }

        public unsafe void Clear(IGraphicsContext context, IUnorderedAccessView uav, uint width, uint height, uint slices, uint mask)
        {
            UPoint4 maskParams = default;
            maskParams.X = mask;
            paramBuffer.Update(context, maskParams);

            computePipelineState.Bindings.SetUAV("inputTex", uav);
            context.SetComputePipelineState(computePipelineState);
            context.Dispatch((uint)MathF.Ceiling(width / 32f), (uint)MathF.Ceiling(height / 32f), slices);
            context.SetComputePipelineState(null);
        }

        protected override void DisposeCore()
        {
            computePipelineState.Dispose();
            paramBuffer.Dispose();
        }
    }
}