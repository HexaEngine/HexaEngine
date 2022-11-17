namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using System.Numerics;

    public class DepthOfField : IEffect
    {
        private bool enabled;
        private bool bokehEnabled;
        private readonly Quad quad;
        private readonly IBuffer cbBlur;
        private readonly IBuffer cbBokeh;
        private readonly IBuffer cbDof;
        private readonly ISamplerState pointSampler;
        private readonly Pipeline pipelineBlur;
        private readonly Pipeline pipelineBokeh;
        private readonly Pipeline pipelineDof;
        private ITexture2D outOfFocusTex;
        private IShaderResourceView outOfFocusSRV;
        private IRenderTargetView outOfFocusRTV;
        private ITexture2D bokehTex;
        private IShaderResourceView bokehSRV;
        private IRenderTargetView bokehRTV;

        private BlurParams blurParams;
        private BokehParams bokehParams;
        private DofParams dofParams;

        private bool dirty;

        public IRenderTargetView? Target;
        public IShaderResourceView? Color;
        public IShaderResourceView? Position;
        public IBuffer? Camera;

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
            public float MinDistance;
            public float MaxDistance;
            public Vector2 FocusPoint;
            public int Enabled;
            public int AutoFocusEnabled;
            public int AutoFocusSamples;
            public float AutoFocusRadius;

            public DofParams()
            {
                MinDistance = 5.0f;
                MaxDistance = 12.0f;
                FocusPoint = new(0.5f, 0.5f);
                Enabled = 0;
                AutoFocusEnabled = 1;
                AutoFocusSamples = 1;
                AutoFocusRadius = 30;
            }

            public DofParams(float minDistance, float maxDistance, Vector2 focusPoint)
            {
                MinDistance = minDistance;
                MaxDistance = maxDistance;
                FocusPoint = focusPoint;
            }
        }

        #endregion Structs

        public DepthOfField(IGraphicsDevice device, int width, int height)
        {
            quad = new(device);
            blurParams = new BlurParams();
            bokehParams = new BokehParams();
            dofParams = new DofParams();
            pipelineBlur = new(device, new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/box.hlsl"
            });
            pipelineBokeh = new(device, new()
            {
                VertexShader = "effects/bokeh/vs.hlsl",
                PixelShader = "effects/bokeh/ps.hlsl"
            });
            pipelineDof = new(device, new()
            {
                VertexShader = "effects/dof/vs.hlsl",
                PixelShader = "effects/dof/ps.hlsl"
            });

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
        }

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

        public float DofMinDistance
        {
            get => dofParams.MinDistance;
            set
            {
                dofParams.MinDistance = value;
                dirty = true;
            }
        }

        public float DofMaxDistance
        {
            get => dofParams.MaxDistance;
            set
            {
                dofParams.MaxDistance = value;
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

        #endregion Properties

        public void Resize(IGraphicsDevice device, int width, int height)
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

        public void Draw(IGraphicsContext context)
        {
            if (dirty)
            {
                dirty = false;
                context.Write(cbBlur, blurParams);
                context.Write(cbBokeh, bokehParams);
                context.Write(cbDof, dofParams);
            }

            if (enabled && bokehEnabled)
            {
                context.ClearRenderTargetView(bokehRTV, default);
                context.SetRenderTarget(bokehRTV, null);
                context.PSSetShaderResource(Color, 0);
                context.PSSetConstantBuffer(cbBlur, 0);
                context.PSSetSampler(pointSampler, 0);
                quad.DrawAuto(context, pipelineBlur, bokehRTV.Viewport);
                context.ClearState();

                context.ClearRenderTargetView(outOfFocusRTV, default);
                context.SetRenderTarget(outOfFocusRTV, null);
                context.PSSetShaderResource(bokehSRV, 0);
                context.PSSetConstantBuffer(cbBokeh, 0);
                context.PSSetSampler(pointSampler, 0);
                quad.DrawAuto(context, pipelineBokeh, outOfFocusRTV.Viewport);
                context.ClearState();
            }
            else if (enabled)
            {
                context.ClearRenderTargetView(outOfFocusRTV, default);
                context.SetRenderTarget(outOfFocusRTV, null);
                context.PSSetShaderResource(Color, 0);
                context.PSSetConstantBuffer(cbBlur, 0);
                context.PSSetSampler(pointSampler, 0);
                quad.DrawAuto(context, pipelineBlur, outOfFocusRTV.Viewport);
                context.ClearState();
            }

            context.SetRenderTarget(Target, null);
            context.PSSetShaderResource(Position, 0);
            context.PSSetShaderResource(Color, 2);
            context.PSSetShaderResource(outOfFocusSRV, 3);
            context.PSSetSampler(pointSampler, 0);
            context.PSSetConstantBuffer(cbDof, 0);
            context.PSSetConstantBuffer(Camera, 1);
#nullable disable
            quad.DrawAuto(context, pipelineDof, Target.Viewport);
#nullable enable
            context.ClearState();
        }

        public void DrawSettings()
        {
        }
    }
}