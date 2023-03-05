#nullable disable

using HexaEngine;

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class DepthOfField : IPostFx
    {
        private IGraphicsDevice device;
        private bool enabled = false;
        private bool bokehEnabled = true;
        private Quad quad;
        private IBuffer cbBlur;
        private IBuffer cbBokeh;
        private IBuffer cbDof;
        private ISamplerState pointSampler;
        private IGraphicsPipeline pipelineBlur;
        private IGraphicsPipeline pipelineBokeh;
        private IGraphicsPipeline pipelineDof;
        private ITexture2D outOfFocusTex;
        private IShaderResourceView outOfFocusSRV;
        private IRenderTargetView outOfFocusRTV;
        private ITexture2D bokehTex;
        private IShaderResourceView bokehSRV;
        private IRenderTargetView bokehRTV;

        private BlurParams blurParams = new();
        private BokehParams bokehParams = new();
        private DofParams dofParams = new();

        private bool dirty;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public ResourceRef<IShaderResourceView> Position;
        public ResourceRef<IBuffer> Camera;

        #region Structs

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

        private struct BokehParams
        {
            public int Size;
            public float Separation;
            public float MinThreshold;
            public float MaxThreshold;

            public BokehParams()
            {
                Size = 8;
                Separation = 1;
                MinThreshold = 0.1f;
                MaxThreshold = 0.3f;
            }

            public BokehParams(int size, float separation, float min, float max)
            {
                Size = size;
                Separation = separation;
                MinThreshold = min;
                MaxThreshold = max;
            }
        }

        private struct DofParams
        {
            public float FocusRange;
            public float Padding;
            public Vector2 FocusPoint;
            public int Enabled;
            public int AutoFocusEnabled;
            public int AutoFocusSamples;
            public float AutoFocusRadius;

            public DofParams()
            {
                FocusRange = 20.0f;
                Padding = 0;
                FocusPoint = new(0.5f, 0.5f);
                Enabled = 0;
                AutoFocusEnabled = 1;
                AutoFocusSamples = 1;
                AutoFocusRadius = 30;
            }

            public DofParams(float focusRange, Vector2 focusPoint)
            {
                FocusRange = focusRange;
                FocusPoint = focusPoint;
            }
        }

        #endregion Structs

        #region Properties

        public bool Enabled
        {
            get => dofParams.Enabled == 1;
            set
            {
                dofParams.Enabled = value ? 1 : 0;
                enabled = value;
                dirty = true;
            }
        }

        public bool AutoFocusEnabled
        {
            get => dofParams.AutoFocusEnabled == 1;
            set
            {
                dofParams.AutoFocusEnabled = value ? 1 : 0;
                dirty = true;
            }
        }

        public int AutoFocusSamples
        {
            get => dofParams.AutoFocusSamples;
            set
            {
                dofParams.AutoFocusSamples = value;
                dirty = true;
            }
        }

        public float AutoFocusRadius
        {
            get => dofParams.AutoFocusRadius;
            set
            {
                dofParams.AutoFocusRadius = value;
                dirty = true;
            }
        }

        public bool BokehEnabled
        {
            get => bokehEnabled;
            set
            {
                bokehEnabled = value;
                dirty = true;
            }
        }

        public int BlurRadius
        {
            get => blurParams.Size;
            set
            {
                blurParams.Size = value;
                dirty = true;
            }
        }

        public int BokehRadius
        {
            get => bokehParams.Size;
            set
            {
                bokehParams.Size = value;
                dirty = true;
            }
        }

        public float BokehSeparation
        {
            get => bokehParams.Separation;
            set
            {
                bokehParams.Separation = value;
                dirty = true;
            }
        }

        public float BokehMinThreshold
        {
            get => bokehParams.MinThreshold;
            set
            {
                bokehParams.MinThreshold = value;
                dirty = true;
            }
        }

        public float BokehMaxThreshold
        {
            get => bokehParams.MaxThreshold;
            set
            {
                bokehParams.MaxThreshold = value;
                dirty = true;
            }
        }

        public float FocusRange
        {
            get => dofParams.FocusRange;
            set
            {
                dofParams.FocusRange = value;
                dirty = true;
            }
        }

        public Vector2 FocusPoint
        {
            get => dofParams.FocusPoint;
            set
            {
                dofParams.FocusPoint = value;
                dirty = true;
            }
        }

        public string Name => "Dof";

        public PostFxFlags Flags => PostFxFlags.None;

        public int Priority { get; set; } = 300;

        #endregion Properties

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            this.device = device;
            quad = new(device);
            blurParams = new BlurParams();
            bokehParams = new BokehParams();
            dofParams = new DofParams();
            pipelineBlur = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/box.hlsl"
            }, macros);
            pipelineBokeh = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/bokeh/vs.hlsl",
                PixelShader = "effects/bokeh/ps.hlsl"
            }, macros);
            pipelineDof = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/dof/vs.hlsl",
                PixelShader = "effects/dof/ps.hlsl"
            }, macros);

            outOfFocusTex = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            outOfFocusSRV = device.CreateShaderResourceView(outOfFocusTex);
            outOfFocusRTV = device.CreateRenderTargetView(outOfFocusTex, new(width, height));

            bokehTex = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            bokehSRV = device.CreateShaderResourceView(bokehTex);
            bokehRTV = device.CreateRenderTargetView(bokehTex, new(width, height));

            cbBlur = device.CreateBuffer(blurParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            cbBokeh = device.CreateBuffer(bokehParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            cbDof = device.CreateBuffer(dofParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            pointSampler = device.CreateSamplerState(SamplerDescription.PointClamp);

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            Position = ResourceManager2.Shared.GetResource<IShaderResourceView>("SwapChain.SRV");
        }

        public async void Resize(int width, int height)
        {
            outOfFocusTex.Dispose();
            outOfFocusSRV.Dispose();
            outOfFocusRTV.Dispose();
            outOfFocusTex = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            outOfFocusSRV = device.CreateShaderResourceView(outOfFocusTex);
            outOfFocusRTV = device.CreateRenderTargetView(outOfFocusTex, new(width, height));
            bokehTex.Dispose();
            bokehSRV.Dispose();
            bokehRTV.Dispose();
            bokehTex = device.CreateTexture2D(Format.RGBA32Float, width, height, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget);
            bokehSRV = device.CreateShaderResourceView(bokehTex);
            bokehRTV = device.CreateRenderTargetView(bokehTex, new(width, height));
        }

        public void SetOutput(IRenderTargetView view, Viewport viewport)
        {
            Output = view;
        }

        public void SetInput(IShaderResourceView view)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                dirty = false;
                context.Write(cbBlur, blurParams);
                context.Write(cbBokeh, bokehParams);
                context.Write(cbDof, dofParams);
            }
        }

        public void Draw(IGraphicsContext context)
        {
            if (enabled && bokehEnabled)
            {
                context.ClearRenderTargetView(bokehRTV, default);
                context.SetRenderTarget(bokehRTV, null);
                context.SetViewport(bokehRTV.Viewport);
                context.PSSetShaderResource(Input, 0);
                context.PSSetConstantBuffer(cbBlur, 0);
                context.PSSetSampler(pointSampler, 0);
                quad.DrawAuto(context, pipelineBlur);
                context.ClearState();

                context.ClearRenderTargetView(outOfFocusRTV, default);
                context.SetRenderTarget(outOfFocusRTV, null);
                context.SetViewport(outOfFocusRTV.Viewport);
                context.PSSetShaderResource(bokehSRV, 0);
                context.PSSetConstantBuffer(cbBokeh, 0);
                context.PSSetSampler(pointSampler, 0);
                quad.DrawAuto(context, pipelineBokeh);
                context.ClearState();
            }
            else if (enabled)
            {
                context.ClearRenderTargetView(outOfFocusRTV, default);
                context.SetRenderTarget(outOfFocusRTV, null);
                context.SetViewport(outOfFocusRTV.Viewport);
                context.PSSetShaderResource(Input, 0);
                context.PSSetConstantBuffer(cbBlur, 0);
                context.PSSetSampler(pointSampler, 0);
                quad.DrawAuto(context, pipelineBlur);
                context.ClearState();
            }

            context.SetRenderTarget(Output, null);
            context.SetViewport(Output.Viewport);
            context.PSSetShaderResource(Position.Value, 0);
            context.PSSetShaderResource(Input, 2);
            context.PSSetShaderResource(outOfFocusSRV, 3);
            context.PSSetSampler(pointSampler, 0);
            context.PSSetConstantBuffer(cbDof, 0);
            context.PSSetConstantBuffer(Camera.Value, 1);
            quad.DrawAuto(context, pipelineDof);
            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();
            pipelineBlur.Dispose();
            pipelineBokeh.Dispose();
            pipelineDof.Dispose();
            pointSampler.Dispose();
            pipelineBokeh.Dispose();
            pipelineDof.Dispose();
            outOfFocusTex.Dispose();
            outOfFocusSRV.Dispose();
            outOfFocusRTV.Dispose();
            bokehTex.Dispose();
            bokehSRV.Dispose();
            bokehRTV.Dispose();
            cbBlur.Dispose();
            cbBokeh.Dispose();
            cbDof.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}