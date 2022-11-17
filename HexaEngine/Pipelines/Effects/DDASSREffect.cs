namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;
    using static HexaEngine.Pipelines.Effects.DDASSR;

    public class DDASSR : IEffect
    {
        private readonly Quad quad;
        private bool isDirty = true;
        private bool disposedValue;
        private bool enabled;

        private readonly Pipeline ssrPipeline;
        private readonly IBuffer cbSSR;
        private DDASSRParams ssrParams = new();
        private ITexture2D ssrBuffer;
        private IRenderTargetView ssrRTV;
        private IShaderResourceView ssrSRV;

        private readonly Pipeline blurPipeline;
        private readonly IBuffer cbBlur;
        private BlurParams blurParams = new();
        private ITexture2D blurBuffer;
        private IRenderTargetView blurRTV;
        private IShaderResourceView blurSRV;

        private readonly Pipeline mixPipeline;
        private readonly IBuffer cbMix;
        private MixParams mixParams = new();

        private ISamplerState samplerPoint;
        private ISamplerState samplerAnsio;

        public IShaderResourceView? Color;
        public IShaderResourceView? Position;
        public IShaderResourceView? Normal;
        public IShaderResourceView? Depth;
        public IBuffer? Camera;
        public IRenderTargetView? Output;

        #region Structs

        private struct DDASSRParams
        {
            public Vector2 TargetSize;
            public Vector2 InvTargetSize;
            public int MaxRayStep;
            public int Enabled;
            public Vector2 padd;
            public float Depthbias;
            public float RayStepScale;
            public float MaxThickness;
            public float MaxRayLength;

            public DDASSRParams()
            {
                Enabled = 0;
                TargetSize = new(200, 200);
                InvTargetSize = new(1 / 200, 1 / 200);
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

            ssrPipeline = new(device, new()
            {
                VertexShader = "effects/ddassr/vs.hlsl",
                PixelShader = "effects/ddassr/ps.hlsl",
            });
            cbSSR = device.CreateBuffer(ssrParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            ssrBuffer = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            ssrRTV = device.CreateRenderTargetView(ssrBuffer, new(width, height));
            ssrSRV = device.CreateShaderResourceView(ssrBuffer);

            blurPipeline = new(device, new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/box.hlsl",
            });
            cbBlur = device.CreateBuffer(blurParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            blurBuffer = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            blurRTV = device.CreateRenderTargetView(blurBuffer, new(width, height));
            blurSRV = device.CreateShaderResourceView(blurBuffer);

            mixPipeline = new(device, new()
            {
                VertexShader = "effects/mix/vs.hlsl",
                PixelShader = "effects/mix/ps.hlsl",
            });
            cbMix = device.CreateBuffer(mixParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            samplerPoint = device.CreateSamplerState(SamplerDescription.LinearClamp);
            samplerAnsio = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
        }

        #region Properties

        public bool Enabled
        { get => enabled; set { enabled = value; isDirty = true; } }

        public Vector2 TargetSize
        { get => ssrParams.TargetSize; set { ssrParams.TargetSize = value; isDirty = true; } }

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

        #endregion Properties

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            ssrBuffer.Dispose();
            ssrRTV.Dispose();
            ssrSRV.Dispose();
            ssrBuffer = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            ssrRTV = device.CreateRenderTargetView(ssrBuffer, new(width, height));
            ssrSRV = device.CreateShaderResourceView(ssrBuffer);

            blurBuffer.Dispose();
            blurRTV.Dispose();
            blurSRV.Dispose();
            blurBuffer = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            blurRTV = device.CreateRenderTargetView(blurBuffer, new(width, height));
            blurSRV = device.CreateShaderResourceView(blurBuffer);
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
                context.PSSetShaderResource(Depth, 3);
                context.PSSetSampler(samplerPoint, 0);
                quad.DrawAuto(context, ssrPipeline, ssrRTV.Viewport);

                context.SetRenderTarget(blurRTV, null);
                context.PSSetConstantBuffer(cbBlur, 0);
                context.PSSetShaderResource(ssrSRV, 0);
                context.PSSetSampler(samplerAnsio, 0);
                quad.DrawAuto(context, blurPipeline, blurRTV.Viewport);
            }

            context.SetRenderTarget(Output, null);
            context.PSSetConstantBuffer(cbMix, 0);
            context.PSSetShaderResource(Color, 0);
            context.PSSetShaderResource(ssrSRV, 1);
            context.PSSetShaderResource(blurSRV, 2);
            context.PSSetShaderResource(Normal, 3);
            context.PSSetSampler(samplerPoint, 0);
            quad.DrawAuto(context, mixPipeline, Output.Viewport);
        }

        public void DrawSettings()
        {
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DDASSR()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}