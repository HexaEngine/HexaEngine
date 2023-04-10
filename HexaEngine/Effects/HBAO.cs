#nullable disable

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using System.Numerics;

    public class HBAO : IEffect
    {
        private IGraphicsDevice device;
        private Quad quad;
        private IGraphicsPipeline hbaoPipeline;
        private IBuffer cbHBAO;
        private HBAOParams hbaoParams = new();
        private ITexture2D hbaoBuffer;
        private unsafe void** hbaoSRVs;
        private unsafe void** hbaoCBs;
        private IShaderResourceView hbaoSRV;
        private IRenderTargetView hbaoRTV;

        private IGraphicsPipeline blurPipeline;
        private IBuffer cbBlur;
        private BlurParams blurParams = new();
        private unsafe void** blurSRVs;
        private unsafe void** blurCBs;

        private ISamplerState samplerLinear;

        public ResourceRef<Texture> Output;
        public ResourceRef<IBuffer> Camera;
        public ResourceRef<IShaderResourceView> Position;
        public ResourceRef<IShaderResourceView> Normal;

        private bool isDirty = true;
        private bool disposedValue;

        #region Structs

        private struct HBAOParams
        {
            public int Enabled;
            public float SAMPLING_RADIUS;
            public uint NUM_SAMPLING_DIRECTIONS;
            public float SAMPLING_STEP;
            public uint NUM_SAMPLING_STEPS;
            public Vector3 padd;
            public Vector2 Res;
            public Vector2 ResInv;

            public HBAOParams()
            {
                Enabled = 0;
                SAMPLING_RADIUS = 0.5f;
                NUM_SAMPLING_DIRECTIONS = 8;
                SAMPLING_STEP = 0.004f;
                NUM_SAMPLING_STEPS = 4;
                padd = default;
            }
        }

        private struct BlurParams
        {
            public int Radius;
#pragma warning disable CS0649 // Field 'HBAO.BlurParams.Padding' is never assigned to, and will always have its default value
            public Vector3 Padding;
#pragma warning restore CS0649 // Field 'HBAO.BlurParams.Padding' is never assigned to, and will always have its default value

            public BlurParams()
            {
                Radius = 2;
            }
        }

        #endregion Structs

        #region Properties

        public bool Enabled
        { get => hbaoParams.Enabled == 1; set { hbaoParams.Enabled = value ? 1 : 0; isDirty = true; } }

        public float SamplingRadius
        { get => hbaoParams.SAMPLING_RADIUS; set { hbaoParams.SAMPLING_RADIUS = value; isDirty = true; } }

        public uint NumSamplingDirections
        { get => hbaoParams.NUM_SAMPLING_DIRECTIONS; set { hbaoParams.NUM_SAMPLING_DIRECTIONS = value; isDirty = true; } }

        public float SamplingStep
        { get => hbaoParams.SAMPLING_STEP; set { hbaoParams.SAMPLING_STEP = value; isDirty = true; } }

        public uint NumSamplingSteps
        { get => hbaoParams.NUM_SAMPLING_STEPS; set { hbaoParams.NUM_SAMPLING_STEPS = value; isDirty = true; } }

        #endregion Properties

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            this.device = device;
            Output = ResourceManager2.Shared.AddTexture("SSAOBuffer", TextureDescription.CreateTexture2DWithRTV(width / 2, height / 2, 1, Format.R32Float));

            quad = new Quad(device);

            hbaoPipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/hbao/vs.hlsl",
                PixelShader = "effects/hbao/ps.hlsl",
            });
            cbHBAO = device.CreateBuffer(hbaoParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            hbaoBuffer = device.CreateTexture2D(Format.RG32Float, width / 2, height / 2, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
            hbaoRTV = device.CreateRenderTargetView(hbaoBuffer, new(width / 2, height / 2));
            hbaoSRV = device.CreateShaderResourceView(hbaoBuffer);

            Position = ResourceManager2.Shared.GetResource<IShaderResourceView>("SwapChain.SRV");
            Normal = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Normal");
            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");

            Position.Resource.ValueChanged += ValueChanged;
            Normal.Resource.ValueChanged += ValueChanged;
            Camera.Resource.ValueChanged += ValueChanged;

            unsafe
            {
                hbaoSRVs = AllocArray(2);
                hbaoSRVs[0] = (void*)Position.Value.NativePointer;
                hbaoSRVs[1] = (void*)Normal.Value.NativePointer;
                hbaoCBs = AllocArray(2);
                hbaoCBs[0] = (void*)cbHBAO.NativePointer;
                hbaoCBs[1] = (void*)Camera.Value.NativePointer;
            }

            blurPipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/box.hlsl",
            });
            cbBlur = device.CreateBuffer(blurParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            unsafe
            {
                blurSRVs = AllocArray(1);
                blurSRVs[0] = (void*)hbaoSRV.NativePointer;
                blurCBs = AllocArray(1);
                blurCBs[0] = (void*)cbBlur.NativePointer;
            }

            samplerLinear = device.CreateSamplerState(SamplerDescription.LinearClamp);
        }

        private void ValueChanged(object sender, IDisposable e)
        {
            unsafe
            {
                hbaoSRVs[0] = (void*)Position.Value.NativePointer;
                hbaoSRVs[1] = (void*)Normal.Value.NativePointer;
                hbaoCBs[1] = (void*)Camera.Value.NativePointer;
            }
        }

        public void BeginResize()
        {
        }

        public void EndResize(int width, int height)
        {
            Output = ResourceManager2.Shared.UpdateTexture("SSAOBuffer", TextureDescription.CreateTexture2DWithRTV(width / 2, height / 2, 1, Format.R32Float));

            hbaoBuffer.Dispose();
            hbaoRTV.Dispose();
            hbaoSRV.Dispose();
            hbaoBuffer = device.CreateTexture2D(Format.RG32Float, width / 2, height / 2, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
            hbaoRTV = device.CreateRenderTargetView(hbaoBuffer, new(width / 2, height / 2));
            hbaoSRV = device.CreateShaderResourceView(hbaoBuffer);

            unsafe
            {
                blurSRVs[0] = (void*)hbaoSRV.NativePointer;
            }

            hbaoParams.Res = new(width, height);
            hbaoParams.ResInv = new(1 / width, 1 / height);

            isDirty = true;
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output is null) return;
            if (isDirty)
            {
                context.Write(cbHBAO, hbaoParams);
                context.Write(cbBlur, blurParams);
                isDirty = false;
            }

            context.SetRenderTarget(hbaoRTV, null);
            context.SetViewport(hbaoRTV.Viewport);
            context.PSSetShaderResources(hbaoSRVs, 2, 0);
            context.PSSetConstantBuffers(hbaoCBs, 2, 0);
            context.PSSetSampler(samplerLinear, 0);
            quad.DrawAuto(context, hbaoPipeline);

            context.SetRenderTarget(Output.Value.RenderTargetView, null);
            context.SetViewport(Output.Value.RenderTargetView.Viewport);
            context.PSSetShaderResources(blurSRVs, 1, 0);
            context.PSSetConstantBuffers(blurCBs, 1, 0);
            context.PSSetSampler(samplerLinear, 0);
            quad.DrawAuto(context, blurPipeline);
            context.ClearState();
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free(hbaoCBs);
                Free(blurCBs);
                quad.Dispose();
                hbaoPipeline.Dispose();
                cbHBAO.Dispose();
                hbaoBuffer.Dispose();
                hbaoRTV.Dispose();
                hbaoSRV.Dispose();
                blurPipeline.Dispose();
                cbBlur.Dispose();
                samplerLinear.Dispose();
                disposedValue = true;
            }
        }

        ~HBAO()
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