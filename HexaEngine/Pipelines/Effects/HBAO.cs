﻿namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class HBAO : IEffect
    {
        private readonly Quad quad;
        private readonly Pipeline hbaoPipeline;
        private readonly IBuffer cbHBAO;
        private HBAOParams hbaoParams = new();
        private ITexture2D hbaoBuffer;
        private readonly IShaderResourceView[] hbaoSRVs;
        private readonly IBuffer[] hbaoCBs;
        private IShaderResourceView hbaoSRV;
        private IRenderTargetView hbaoRTV;

        private readonly Pipeline blurPipeline;
        private readonly IBuffer cbBlur;
        private BlurParams blurParams = new();
        private readonly IShaderResourceView[] blurSRVs;
        private readonly IBuffer[] blurCBs;

        private readonly ISamplerState samplerLinear;

        public IRenderTargetView? Output;
        public IBuffer? Camera;
        public IShaderResourceView? Position;
        public IShaderResourceView? Normal;

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
            public float Sharpness;
            public Vector2 InvResolutionDirection;
            public float padd;

            public BlurParams()
            {
                Sharpness = 3;
            }
        }

        #endregion Structs

        public unsafe HBAO(IGraphicsDevice device, int width, int height)
        {
            quad = new Quad(device);

            hbaoPipeline = new(device, new()
            {
                VertexShader = "effects/hbao/vs.hlsl",
                PixelShader = "effects/hbao/ps.hlsl",
            });
            cbHBAO = device.CreateBuffer(hbaoParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            hbaoBuffer = device.CreateTexture2D(Format.RG32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
            hbaoRTV = device.CreateRenderTargetView(hbaoBuffer, new(width, height));
            hbaoSRV = device.CreateShaderResourceView(hbaoBuffer);
            hbaoSRVs = new IShaderResourceView[2];
            hbaoCBs = new IBuffer[2];
            hbaoCBs[0] = cbHBAO;

            blurPipeline = new(device, new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/hbao.hlsl",
            });
            cbBlur = device.CreateBuffer(blurParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            blurSRVs = new IShaderResourceView[1] { hbaoSRV };
            blurCBs = new IBuffer[] { cbBlur };

            samplerLinear = device.CreateSamplerState(SamplerDescription.LinearClamp);
        }

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

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            hbaoBuffer.Dispose();
            hbaoRTV.Dispose();
            hbaoSRV.Dispose();
            hbaoBuffer = device.CreateTexture2D(Format.RG32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
            hbaoRTV = device.CreateRenderTargetView(hbaoBuffer, new(width, height));
            hbaoSRV = device.CreateShaderResourceView(hbaoBuffer);
            blurParams.InvResolutionDirection = new(1 / width, 1 / height);
            blurSRVs[0] = hbaoSRV;

            hbaoParams.Res = new(width, height);
            hbaoParams.ResInv = new(1 / width, 1 / height);

#nullable disable
            hbaoSRVs[0] = Position;
            hbaoSRVs[1] = Normal;

            hbaoCBs[1] = Camera;
#nullable enable
            isDirty = true;
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output is null) return;
            if (isDirty)
            {
                context.Write(cbHBAO, hbaoParams);
                context.Write(cbBlur, blurParams);
                isDirty = false;
            }

            context.SetRenderTarget(hbaoRTV, null);
            context.PSSetShaderResources(hbaoSRVs, 0);
            context.PSSetConstantBuffers(hbaoCBs, 0);
            context.PSSetSampler(samplerLinear, 0);
            quad.DrawAuto(context, hbaoPipeline, hbaoRTV.Viewport);

            context.SetRenderTarget(Output, null);
            context.PSSetShaderResources(blurSRVs, 0);
            context.PSSetConstantBuffers(blurCBs, 0);
            context.PSSetSampler(samplerLinear, 0);
            quad.DrawAuto(context, blurPipeline, Output.Viewport);
        }

        public void DrawSettings()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
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