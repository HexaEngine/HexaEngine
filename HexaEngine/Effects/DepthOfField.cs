#nullable disable

namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class DepthOfField : IPostFx
    {
        private IGraphicsDevice device;
        private bool enabled = true;
        private bool bokehEnabled = true;
        private Quad quad;
        private ConstantBuffer<BlurParams> cbBlur;
        private ConstantBuffer<BokehParams> cbBokeh;
        private ConstantBuffer<DofParams> cbDof;
        private ISamplerState pointSampler;
        private IGraphicsPipeline pipelineBlur;
        private IGraphicsPipeline pipelineBokeh;
        private IGraphicsPipeline pipelineDof;
        private Texture2D outOfFocusTex;
        private Texture2D bokehTex;
        private int autoFocusSamples;
        private int bokehRadius;
        private float bokehSeparation;
        private float bokehMaxThreshold;
        private float focusRange;
        private bool dirty;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public ResourceRef<IShaderResourceView> Position;
        public ResourceRef<IBuffer> Camera;
        private int priority = 300;
        private bool autoFocusEnabled;
        private float autoFocusRadius;
        private int blurRadius;
        private float bokehMinThreshold;
        private Vector2 focusPoint;

        public event Action<bool> OnEnabledChanged;

        public event Action<int> OnPriorityChanged;

        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                dirty = true;
                OnEnabledChanged?.Invoke(value);
            }
        }

        public string Name => "Dof";

        public PostFxFlags Flags => PostFxFlags.None;

        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
                OnPriorityChanged?.Invoke(value);
            }
        }

        public bool AutoFocusEnabled
        {
            get => autoFocusEnabled;
            set
            {
                autoFocusEnabled = value;
                dirty = true;
            }
        }

        public int AutoFocusSamples
        {
            get => autoFocusSamples;
            set
            {
                autoFocusSamples = value;
                dirty = true;
            }
        }

        public float AutoFocusRadius
        {
            get => autoFocusRadius;
            set
            {
                autoFocusRadius = value;
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
            get => blurRadius;
            set
            {
                blurRadius = value;
                dirty = true;
            }
        }

        public int BokehRadius
        {
            get => bokehRadius;
            set
            {
                bokehRadius = value;
                dirty = true;
            }
        }

        public float BokehSeparation
        {
            get => bokehSeparation;
            set
            {
                bokehSeparation = value;
                dirty = true;
            }
        }

        public float BokehMinThreshold
        {
            get => bokehMinThreshold;
            set
            {
                bokehMinThreshold = value;
                dirty = true;
            }
        }

        public float BokehMaxThreshold
        {
            get => bokehMaxThreshold;
            set
            {
                bokehMaxThreshold = value;
                dirty = true;
            }
        }

        public float FocusRange
        {
            get => focusRange;
            set
            {
                focusRange = value;
                dirty = true;
            }
        }

        public Vector2 FocusPoint
        {
            get => focusPoint;
            set
            {
                focusPoint = value;
                dirty = true;
            }
        }

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

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            this.device = device;
            quad = new(device);

            pipelineBlur = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/blur/vs.hlsl",
                PixelShader = "effects/blur/box.hlsl"
            }, macros);
            pipelineBokeh = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/bokeh/vs.hlsl",
                PixelShader = "effects/bokeh/ps.hlsl"
            }, macros);
            pipelineDof = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/dof/vs.hlsl",
                PixelShader = "effects/dof/ps.hlsl"
            }, macros);

            outOfFocusTex = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            bokehTex = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            cbBlur = new(device, CpuAccessFlags.Write);
            cbBokeh = new(device, CpuAccessFlags.Write);
            cbDof = new(device, CpuAccessFlags.Write);

            pointSampler = device.CreateSamplerState(SamplerDescription.PointClamp);

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            Position = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Depth");
        }

        public void Resize(int width, int height)
        {
            outOfFocusTex.Resize(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            bokehTex.Resize(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                dirty = false;

                BlurParams blurParams = default;
                blurParams.Size = blurRadius;
                cbBlur.Update(context, blurParams);

                BokehParams bokehParams = default;
                bokehParams.Separation = bokehSeparation;
                bokehParams.MinThreshold = bokehMinThreshold;
                bokehParams.MaxThreshold = bokehMaxThreshold;
                bokehParams.Size = bokehRadius;
                cbBokeh.Update(context, bokehParams);

                DofParams dofParams = default;
                dofParams.Enabled = enabled ? 1 : 0;
                dofParams.AutoFocusEnabled = autoFocusEnabled ? 1 : 0;
                dofParams.AutoFocusRadius = autoFocusRadius;
                dofParams.AutoFocusSamples = autoFocusSamples;
                dofParams.FocusPoint = focusPoint;
                dofParams.FocusRange = focusRange;
                cbDof.Update(context, dofParams);
            }
        }

        public void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            if (enabled && bokehEnabled)
            {
                context.ClearRenderTargetView(bokehTex.RTV, default);
                context.SetRenderTarget(bokehTex.RTV, null);
                context.SetViewport(bokehTex.RTV.Viewport);
                context.PSSetShaderResource(0, Input);
                context.PSSetConstantBuffer(0, cbBlur);
                context.PSSetSampler(0, pointSampler);
                quad.DrawAuto(context, pipelineBlur);
                context.ClearState();

                context.ClearRenderTargetView(outOfFocusTex.RTV, default);
                context.SetRenderTarget(outOfFocusTex.RTV, null);
                context.SetViewport(outOfFocusTex.RTV.Viewport);
                context.PSSetShaderResource(0, bokehTex.SRV);
                context.PSSetConstantBuffer(0, cbBokeh);
                context.PSSetSampler(0, pointSampler);
                quad.DrawAuto(context, pipelineBokeh);
                context.ClearState();
            }
            else if (enabled)
            {
                context.ClearRenderTargetView(outOfFocusTex.RTV, default);
                context.SetRenderTarget(outOfFocusTex.RTV, null);
                context.SetViewport(outOfFocusTex.RTV.Viewport);
                context.PSSetShaderResource(0, Input);
                context.PSSetConstantBuffer(0, cbBlur);
                context.PSSetSampler(0, pointSampler);
                quad.DrawAuto(context, pipelineBlur);
                context.ClearState();
            }

            context.SetRenderTarget(Output, null);
            context.SetViewport(Output.Viewport);
            context.PSSetShaderResource(0, Position.Value);
            context.PSSetShaderResource(2, Input);
            context.PSSetShaderResource(3, outOfFocusTex.SRV);
            context.PSSetSampler(0, pointSampler);
            context.PSSetConstantBuffer(0, cbDof);
            context.PSSetConstantBuffer(1, Camera.Value);
            quad.DrawAuto(context, pipelineDof);
            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();

            cbBlur.Dispose();
            cbBokeh.Dispose();
            cbDof.Dispose();

            pointSampler.Dispose();

            pipelineBlur.Dispose();
            pipelineBokeh.Dispose();
            pipelineDof.Dispose();

            outOfFocusTex.Dispose();
            bokehTex.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}