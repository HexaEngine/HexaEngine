#nullable disable

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using System.Numerics;

    public class DDASSR : IEffect
    {
        private readonly Quad quad;
        private bool isDirty = true;
        private bool disposedValue;
        private bool enabled;

        private readonly IGraphicsPipeline ssrPipeline;
        private readonly IBuffer cbSSR;
        private DDASSRParams ssrParams = new();
        private ITexture2D ssrBuffer;
        private IRenderTargetView ssrRTV;
        public IShaderResourceView ssrSRV;

        private readonly IGraphicsPipeline blurPipeline;
        private readonly IBuffer cbBlur;
        private BlurParams blurParams = new();
        private ITexture2D blurBuffer;
        private IRenderTargetView blurRTV;
        private IShaderResourceView blurSRV;

        private readonly IGraphicsPipeline mixPipeline;
        private readonly IBuffer cbMix;
        private MixParams mixParams = new();

        private readonly ISamplerState samplerPoint;
        private readonly ISamplerState samplerLinear;

        public IShaderResourceView Color;
        public IShaderResourceView Position;
        public IShaderResourceView Normal;
        public IShaderResourceView Backdepth;
        public IBuffer Camera;
        public IRenderTargetView Output;
        private bool debug;

        #region Structs

        private struct DDASSRParams
        {
            public Vector2 TargetSize;
            public Vector2 InvTargetSize;
            public int MaxRayStep;
            public int Enabled;
#pragma warning disable CS0649 // Field 'DDASSR.DDASSRParams.padd' is never assigned to, and will always have its default value
            public Vector2 padd;
#pragma warning restore CS0649 // Field 'DDASSR.DDASSRParams.padd' is never assigned to, and will always have its default value
            public float Depthbias;
            public float RayStepScale;
            public float MaxThickness;
            public float MaxRayLength;

            public DDASSRParams()
            {
                Enabled = 0;
                MaxRayStep = 70;
                Depthbias = 0.01f;
                RayStepScale = 1.05f;
                MaxThickness = 1.8f;
                MaxRayLength = 200.0f;
            }
        }

        private struct BlurParams
        {
            public int Size;
            public float Reserved0;
            public float Reserved1;
            public float Reserved2;

            public BlurParams()
            {
                Size = 3;
                Reserved0 = 0;
                Reserved1 = 0;
                Reserved2 = 0;
            }

            public BlurParams(int size)
            {
                Size = size;
                Reserved0 = 0;
                Reserved1 = 0;
                Reserved2 = 0;
            }
        }

        public struct MixParams
        {
            public int Enabled;
            public float Reserved0;
            public float Reserved1;
            public float Reserved2;

            public MixParams()
            {
                Enabled = 0;
                Reserved0 = 0;
                Reserved1 = 0;
                Reserved2 = 0;
            }
        }

        #endregion Structs

        public DDASSR(IGraphicsDevice device, int width, int height)
        {
            quad = new(device);

            ssrPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/ddassr/vs.hlsl",
                PixelShader = "effects/ddassr/ps.hlsl",
            }, new GraphicsPipelineState()
            {
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.None,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleList
            });
            cbSSR = device.CreateBuffer(ssrParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            ssrBuffer = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            ssrRTV = device.CreateRenderTargetView(ssrBuffer, new(width, height));
            ssrSRV = device.CreateShaderResourceView(ssrBuffer);

            blurPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/ddassr/vs.hlsl",
                PixelShader = "effects/ddassr/box.hlsl",
            });
            cbBlur = device.CreateBuffer(blurParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            blurBuffer = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            blurRTV = device.CreateRenderTargetView(blurBuffer, new(width, height));
            blurSRV = device.CreateShaderResourceView(blurBuffer);

            mixPipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/ddassr/vs.hlsl",
                PixelShader = "effects/ddassr/blend.hlsl",
            });
            cbMix = device.CreateBuffer(mixParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            samplerPoint = device.CreateSamplerState(SamplerDescription.PointClamp);
            samplerLinear = device.CreateSamplerState(SamplerDescription.LinearClamp);

            ssrParams.TargetSize = new(width, height);
            isDirty = true;
        }

        public DDASSR()
        {
        }

        #region Properties

        public bool Enabled
        { get => enabled; set { enabled = value; isDirty = true; } }

        public int MaxRayStep
        { get => ssrParams.MaxRayStep; set { ssrParams.MaxRayStep = value; isDirty = true; } }

        public float Depthbias
        { get => ssrParams.Depthbias; set { ssrParams.Depthbias = value; isDirty = true; } }

        public float RayStepScale
        { get => ssrParams.RayStepScale; set { ssrParams.RayStepScale = value; isDirty = true; } }

        public float MaxThickness
        { get => ssrParams.MaxThickness; set { ssrParams.MaxThickness = value; isDirty = true; } }

        public float MaxRayLength
        { get => ssrParams.MaxRayLength; set { ssrParams.MaxRayLength = value; isDirty = true; } }

        public bool Debug { get => debug; set => debug = value; }

        #endregion Properties

        public void BeginResize()
        {
            throw new NotImplementedException();
        }

        public void EndResize(int width, int height)
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task Initialize(IGraphicsDevice device, int width, int height)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            throw new NotImplementedException();
        }

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            ssrBuffer.Dispose();
            ssrRTV.Dispose();
            ssrSRV.Dispose();
            ssrBuffer = device.CreateTexture2D(Format.RGBA32Float, width / 8, height / 8, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            ssrRTV = device.CreateRenderTargetView(ssrBuffer, new(width / 8, height / 8));
            ssrSRV = device.CreateShaderResourceView(ssrBuffer);

            blurBuffer.Dispose();
            blurRTV.Dispose();
            blurSRV.Dispose();
            blurBuffer = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            blurRTV = device.CreateRenderTargetView(blurBuffer, new(width, height));
            blurSRV = device.CreateShaderResourceView(blurBuffer);

            ssrParams.TargetSize = new(width / 8, height / 8);
            isDirty = true;
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null) return;
            if (isDirty)
            {
                mixParams.Enabled = enabled ? 1 : 0;
                ssrParams.InvTargetSize = new Vector2(1f / ssrParams.TargetSize.X, 1f / ssrParams.TargetSize.Y);
                context.Write(cbSSR, ssrParams);
                context.Write(cbBlur, blurParams);
                context.Write(cbMix, mixParams);
                isDirty = false;
            }

            if (enabled)
            {
                context.SetRenderTarget(ssrRTV, null);
                context.PSSetConstantBuffer(cbSSR, 0);
                context.PSSetConstantBuffer(Camera, 1);
                context.PSSetShaderResource(Color, 0);
                context.PSSetShaderResource(Position, 1);
                context.PSSetShaderResource(Normal, 2);
                context.PSSetShaderResource(Backdepth, 3);
                context.PSSetSampler(samplerLinear, 0);
                quad.DrawAuto(context, ssrPipeline, ssrRTV.Viewport);

                context.SetRenderTarget(blurRTV, null);
                context.PSSetConstantBuffer(cbBlur, 0);
                context.PSSetShaderResource(ssrSRV, 0);
                context.PSSetShaderResource(Normal, 1);
                context.PSSetSampler(samplerLinear, 0);
                quad.DrawAuto(context, blurPipeline, blurRTV.Viewport);
            }

            context.SetRenderTarget(Output, null);
            context.PSSetConstantBuffer(cbMix, 0);
            context.PSSetShaderResource(Color, 0);
            if (!debug)
                context.PSSetShaderResource(blurSRV, 1);
            else
                context.PSSetShaderResource(ssrSRV, 1);
            context.PSSetSampler(samplerPoint, 0);
            quad.DrawAuto(context, mixPipeline, Output.Viewport);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                quad.Dispose();
                ssrPipeline.Dispose();
                cbSSR.Dispose();
                ssrBuffer.Dispose();
                ssrRTV.Dispose();
                ssrSRV.Dispose();
                blurPipeline.Dispose();
                cbBlur.Dispose();
                blurBuffer.Dispose();
                blurRTV.Dispose();
                blurSRV.Dispose();
                mixPipeline.Dispose();
                cbMix.Dispose();
                samplerPoint.Dispose();
                samplerLinear.Dispose();

                disposedValue = true;
            }
        }

        ~DDASSR()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}