namespace HexaEngine.Graphics.Effects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public enum ReprojectFlags
    {
        None,
        VSM
    }

    /// <summary>
    /// Reprojects a cascade slice using a constant per-cascade texel shift and depth offset.
    /// This is valid because a cascade's orthographic frustum only translates between updates
    /// (rotation/scale are stable), so the shift is identical for every pixel and doesn't need
    /// a per-pixel matrix round-trip. Renders into a graph-managed scratch texture rather than the
    /// slice itself, since reading a shifted location while writing the same slice in-place
    /// would be a data race between invocations.
    /// </summary>
    public class ReprojectEffect : DisposableBase
    {
        private readonly IGraphicsPipelineState pipeline;
        private readonly ConstantBuffer<ReprojectParams> paramBuffer;
        private readonly ISamplerState linearClampSampler;
        private readonly ResourceRef<Texture2D> scratch;

        private struct ReprojectParams
        {
            public Vector2 TexelShift;
            public Vector2 TexelSize;
            public float DepthDelta;
            public uint VSM;
            public Vector2 Padding;
        }

        public ReprojectEffect(IGraphResourceBuilder builder, [CallerFilePath] string filename = "", [CallerLineNumber] int lineNumber = 0)
        {
            var device = builder.Device;
            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/filter/reproject/ps.hlsl"
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
            paramBuffer = new(CpuAccessFlags.Write);
            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            scratch = builder.CreateTexture2D($"{filename}_REPROJECT_SCRATCH_{lineNumber}", new(Format.R32G32Float, 1, 1, 1, 1, GpuAccessFlags.RW), ResourceCreationFlags.Shared);
            SetupState();
        }

        private void SetupState()
        {
            pipeline.Bindings.SetCBV("Params", paramBuffer);
            pipeline.Bindings.SetSampler("linearClampSampler", linearClampSampler);
        }

        /// <summary>
        /// Reprojects <paramref name="source"/> into the graph-managed scratch texture, exposed via <see cref="Scratch"/>
        /// for the caller to blit back into the actual destination slice. <paramref name="texelShift"/> is the shift
        /// (in texels) of a fixed world point from the old cascade frame to the new one, and <paramref name="depthDelta"/>
        /// is the constant depth offset between those two frames.
        /// </summary>
        public unsafe void Reproject(IGraphicsContext context, IShaderResourceView source, uint width, uint height, Format format, Vector2 texelShift, float depthDelta, ReprojectFlags flags)
        {
            var tex = scratch.Value!;
            if (tex.Width != (int)width || tex.Height != (int)height || tex.Format != format)
            {
                tex.Resize(format, (int)width, (int)height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            }

            ReprojectParams reprojectParams = default;
            reprojectParams.TexelShift = texelShift;
            reprojectParams.TexelSize = new(1f / width, 1f / height);
            reprojectParams.DepthDelta = depthDelta;
            reprojectParams.VSM = ((flags & ReprojectFlags.VSM) != 0) ? 1u : 0u;

            paramBuffer.Update(context, reprojectParams);
            pipeline.Bindings.SetSRV("inputTex", source);
            context.SetRenderTarget(tex.RTV, null);
            context.SetViewport(tex.Viewport);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        public Texture2D Scratch => scratch.Value!;

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramBuffer.Dispose();
            linearClampSampler.Dispose();
        }
    }
}