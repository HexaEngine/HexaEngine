namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using ImGuiNET;
    using System.Numerics;

    public class Bloom : IEffect
    {
        private RenderTexture[] mipChain;
        private BloomDownsample downsample;
        private BloomUpsample upsample;
        private IBuffer downsampleCB;
        private IBuffer upsampleCB;
        private ISamplerState sampler;
        private float radius = 0.003f;
        private bool dirty;

        public IShaderResourceView Source;
        private bool disposedValue;

        public IShaderResourceView Output => mipChain[0].ResourceView;

        private struct ParamsDownsample
        {
            public Vector2 SrcResolution;
            public Vector2 Padd;

            public ParamsDownsample(Vector2 srcResolution, Vector2 padd)
            {
                SrcResolution = srcResolution;
                Padd = padd;
            }
        }

        private struct ParamsUpsample
        {
            public float FilterRadius;
            public Vector3 Padd;

            public ParamsUpsample(float filterRadius, Vector3 padd)
            {
                FilterRadius = filterRadius;
                Padd = padd;
            }
        }

        public Bloom(IGraphicsDevice device)
        {
            mipChain = new RenderTexture[2];

            downsampleCB = device.CreateBuffer(new ParamsDownsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            upsampleCB = device.CreateBuffer(new ParamsUpsample(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerDescription.LinearClamp);

            downsample = new(device);
            downsample.Samplers.Add(new(sampler, ShaderStage.Pixel, 0));
            downsample.Constants.Add(new(downsampleCB, ShaderStage.Pixel, 0));

            upsample = new(device);
            upsample.Samplers.Add(new(sampler, ShaderStage.Pixel, 0));
            upsample.Constants.Add(new(upsampleCB, ShaderStage.Pixel, 0));
        }

        public float Radius
        { get => radius; set { radius = value; dirty = true; } }

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            int currentWidth = width / 2;
            int currentHeight = height / 2;
            for (int i = 0; i < mipChain.Length; i++)
            {
                mipChain[i]?.Dispose();
                mipChain[i] = new(device, TextureDescription.CreateTexture2DWithRTV(currentWidth, currentHeight, 1, Format.RGBA32Float));
                currentWidth /= 2;
                currentHeight /= 2;
            }
            device.Context.Write(downsampleCB, new ParamsDownsample(new(width, height), default));
        }

        public void Draw(IGraphicsContext context)
        {
            if (dirty)
            {
                context.Write(upsampleCB, new ParamsUpsample(radius, default));
                dirty = false;
            }

            for (int i = 0; i < mipChain.Length; i++)
            {
                if (i > 0)
                {
                    context.SetShaderResource(mipChain[i - 1].ResourceView, ShaderStage.Pixel, 0);
                }
                else
                {
                    context.SetShaderResource(Source, ShaderStage.Pixel, 0);
                }

                context.SetRenderTarget(mipChain[i].RenderTargetView, null);
                downsample.Draw(context, mipChain[i].Viewport);
                context.ClearState();
            }

            for (int i = mipChain.Length - 1; i > 0; i--)
            {
                context.SetRenderTarget(mipChain[i - 1].RenderTargetView, null);
                context.SetShaderResource(mipChain[i].ResourceView, ShaderStage.Pixel, 0);
                upsample.Draw(context, mipChain[i - 1].Viewport);
            }
        }

        public void DrawSettings()
        {
            if (ImGui.CollapsingHeader("Bloom settings"))
            {
                if (ImGui.InputFloat("Radius", ref radius))
                {
                    dirty = true;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                for (int i = 0; i < mipChain.Length; i++)
                {
                    mipChain[i]?.Dispose();
                }
                downsample.Dispose();
                upsample.Dispose();
                downsampleCB.Dispose();
                upsampleCB.Dispose();
                disposedValue = true;
            }
        }

        ~Bloom()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}