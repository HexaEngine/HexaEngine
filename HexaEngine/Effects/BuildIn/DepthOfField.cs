#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.Resources;
    using HexaEngine.Effects.Blur;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System.Numerics;

    public class DepthOfField : IPostFx
    {
        private IGraphicsDevice device;
        private bool enabled = true;
        private bool bokehEnabled = true;
        private Quad quad;

        private ConstantBuffer<BokehParams> cbBokeh;
        private ConstantBuffer<DofParams> cbDof;
        private ISamplerState linearWrapSampler;

        private IComputePipeline bokehGenerate;
        private GaussianBlur gaussianBlur;
        private IGraphicsPipeline dof;
        private IGraphicsPipeline bokehDraw;

        private Texture2D blurTex;
        private StructuredUavBuffer<Bokeh> bokehBuffer;
        private DrawIndirectArgsBuffer<DrawInstancedIndirectArgs> bokehIndirectBuffer;

        private Texture2D bokehTex;

        private UPoint3 DispatchArgs;
        private int width;
        private int height;

        private int autoFocusSamples;
        private int bokehRadius;
        private float bokehSeparation;
        private float bokehMaxThreshold;
        private float focusRange;
        private bool dirty;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;
        public ResourceRef<IShaderResourceView> Depth;
        public ResourceRef<IBuffer> Camera;
        private int priority = 500;
        private bool autoFocusEnabled;
        private float autoFocusRadius;
        private int blurRadius;
        private float bokehMinThreshold;
        private Vector2 focusPoint;

        #region Props

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

        public string Name => "DoF";

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

        #endregion Props

        #region Structs

        private struct Bokeh
        {
            public Vector3 Position;
            public Vector2 Size;
            public Vector3 Color;
        };

        private struct BokehParams
        {
            public DofParams DofParams;
            public float Fallout;
            public float RadiusScale;
            public float ColorScale;
            public float BlurThreshold;
            public float LumThreshold;
            public Vector3 padd;
        }

        private struct DofParams
        {
            public float FocusRange;
            public Vector2 FocusPoint;
            public int AutoFocusEnabled;
            public int AutoFocusSamples;
            public float AutoFocusRadius;
            public Vector2 padd;

            public DofParams()
            {
                FocusRange = 20.0f;
                FocusPoint = new(0.5f, 0.5f);
                AutoFocusEnabled = 1;
                AutoFocusSamples = 1;
                AutoFocusRadius = 30;
            }
        }

        #endregion Structs

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            this.width = width;
            this.height = height;
            this.device = device;
            quad = new(device);

            bokehGenerate = await device.CreateComputePipelineAsync(new()
            {
                Path = "compute/bokeh/shader.hlsl",
            }, macros);
            gaussianBlur = new(device, Format.R16G16B16A16Float, width, height);
            DispatchArgs = new((uint)MathF.Ceiling(width / 32f), (uint)MathF.Ceiling(height / 32f), 1);

            dof = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/dof/vs.hlsl",
                PixelShader = "effects/dof/ps.hlsl"
            }, macros);

            bokehDraw = await device.CreateGraphicsPipelineAsync(new()
            {
                PixelShader = "effects/bokeh/mask.hlsl",
                GeometryShader = "effects/bokeh/gs.hlsl",
                VertexShader = "effects/bokeh/vs.hlsl",
            }, new GraphicsPipelineState()
            {
                Topology = PrimitiveTopology.PointList,
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One
            });

            bokehBuffer = new(device, (uint)(width * height), CpuAccessFlags.None, BufferUnorderedAccessViewFlags.Append);
            blurTex = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            bokehIndirectBuffer = new(device, new DrawInstancedIndirectArgs(0, 1, 0, 0), CpuAccessFlags.None);

            cbBokeh = new(device, CpuAccessFlags.Write);
            cbDof = new(device, CpuAccessFlags.Write);

            bokehTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/bokeh/hex.dds"));

            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            Depth = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Depth");
        }

        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            bokehBuffer.Capacity = (uint)(width * height);
            blurTex.Resize(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
            DofParams dofParams = default;
            dofParams.FocusRange = 20.0f;
            dofParams.FocusPoint = new(0.5f, 0.5f);
            dofParams.AutoFocusEnabled = 1;
            dofParams.AutoFocusSamples = 1;
            dofParams.AutoFocusRadius = 30;
            cbDof.Update(context, dofParams);

            BokehParams bokehParams = default;
            bokehParams.DofParams = dofParams;
            bokehParams.BlurThreshold = 0.9f;
            bokehParams.LumThreshold = 1.0f;
            bokehParams.RadiusScale = 25.0f;
            bokehParams.ColorScale = 1.0f;
            bokehParams.Fallout = 0.9f;
            cbBokeh.Update(context, bokehParams);
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            if (bokehEnabled)
            {
                context.SetComputePipeline(bokehGenerate);
                nint* srvs_bokeh = stackalloc nint[] { Input.NativePointer, Depth.Value.NativePointer };
                context.CSSetShaderResources(0, 2, (void**)srvs_bokeh);
                context.CSSetUnorderedAccessView(0, (void*)bokehBuffer.UAV.NativePointer, 0);
                context.CSSetConstantBuffer(0, cbBokeh);
                context.Dispatch(DispatchArgs.X, DispatchArgs.Y, DispatchArgs.Z);
                context.CSSetConstantBuffer(0, null);
                context.CSSetUnorderedAccessView(0, null);
                Memset(srvs_bokeh, 0, 2);
                context.CSSetShaderResources(0, 2, (void**)srvs_bokeh);
                context.SetComputePipeline(null);
            }

            gaussianBlur.Blur(context, Input, blurTex.RTV, width, height);

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetSampler(0, linearWrapSampler);
            nint* srvs_dof = stackalloc nint[] { Input.NativePointer, blurTex.SRV.NativePointer, Depth.Value.NativePointer };
            context.PSSetShaderResources(0, 3, (void**)srvs_dof);
            context.PSSetConstantBuffer(0, cbDof);
            context.PSSetConstantBuffer(1, Camera.Value);
            context.SetGraphicsPipeline(dof);
            quad.DrawAuto(context);
            context.SetGraphicsPipeline(null);
            context.PSSetConstantBuffer(1, null);
            context.PSSetConstantBuffer(0, null);
            Memset(srvs_dof, 0, 3);
            context.PSSetShaderResources(0, 3, (void**)srvs_dof);

            if (bokehEnabled)
            {
                context.CopyStructureCount(bokehIndirectBuffer, 0, bokehBuffer.UAV);
                context.VSSetShaderResource(0, bokehBuffer.SRV);
                context.PSSetShaderResource(0, bokehTex.SRV);
                context.SetGraphicsPipeline(bokehDraw);
                context.DrawInstancedIndirect(bokehIndirectBuffer, 0);
                context.SetGraphicsPipeline(null);
                context.PSSetShaderResource(0, null);
                context.VSSetShaderResource(0, null);
            }
            context.PSSetSampler(0, null);
            context.SetRenderTarget(null, null);
        }

        public void Dispose()
        {
            quad.Dispose();

            cbBokeh.Dispose();
            cbDof.Dispose();

            linearWrapSampler.Dispose();

            bokehGenerate.Dispose();
            gaussianBlur.Dispose();
            dof.Dispose();
            bokehDraw.Dispose();

            blurTex.Dispose();
            bokehBuffer.Dispose();
            bokehIndirectBuffer.Dispose();

            bokehTex.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}