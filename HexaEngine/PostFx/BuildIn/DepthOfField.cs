#nullable disable

using HexaEngine;

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using System.Numerics;

    /// <summary>
    /// Post-processing effect for simulating depth of field in a graphics pipeline.
    /// </summary>
    public class DepthOfField : PostFxBase
    {
        private IGraphicsDevice device;

        private ConstantBuffer<BokehParams> cbBokeh;
        private ConstantBuffer<DofParams> cbDof;
        private ISamplerState linearWrapSampler;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
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

        private float focusRange = 20f;
        private Vector2 focusPoint = new(0.5f);
        private bool autoFocusEnabled = true;
        private float autoFocusRadius = 30;
        private int autoFocusSamples = 1;

        private bool bokehEnabled = true;
        private float bokehBlurThreshold = 0.9f;
        private float bokehLumThreshold = 1.0f;
        private float bokehRadiusScale = 25f;
        private float bokehColorScale = 1.0f;
        private float bokehFallout = 0.9f;

        /// <inheritdoc/>
        public override string Name => "DepthOfField";

        /// <inheritdoc/>
        public override PostFxFlags Flags => PostFxFlags.None;

        /// <summary>
        /// Gets or sets the focus range for the depth of field effect.
        /// </summary>
        public float FocusRange
        {
            get => focusRange;
            set => NotifyPropertyChangedAndSet(ref focusRange, value);
        }

        /// <summary>
        /// Gets or sets the focus point for the depth of field effect.
        /// </summary>
        public Vector2 FocusPoint
        {
            get => focusPoint;
            set => NotifyPropertyChangedAndSet(ref focusPoint, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto-focus is enabled.
        /// </summary>
        public bool AutoFocusEnabled
        {
            get => autoFocusEnabled;
            set => NotifyPropertyChangedAndSet(ref autoFocusEnabled, value);
        }

        /// <summary>
        /// Gets or sets the auto-focus radius.
        /// </summary>
        public float AutoFocusRadius
        {
            get => autoFocusRadius;
            set => NotifyPropertyChangedAndSet(ref autoFocusRadius, value);
        }

        /// <summary>
        /// Gets or sets the number of auto-focus samples.
        /// </summary>
        public int AutoFocusSamples
        {
            get => autoFocusSamples;
            set => NotifyPropertyChangedAndSet(ref autoFocusSamples, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the bokeh effect is enabled.
        /// </summary>
        public bool BokehEnabled
        {
            get => bokehEnabled;
            set => NotifyPropertyChangedAndSet(ref bokehEnabled, value);
        }

        /// <summary>
        /// Gets or sets the threshold for bokeh blur.
        /// </summary>
        public float BokehBlurThreshold
        {
            get => bokehBlurThreshold;
            set => NotifyPropertyChangedAndSet(ref bokehBlurThreshold, value);
        }

        /// <summary>
        /// Gets or sets the luminance threshold for bokeh.
        /// </summary>
        public float BokehLumThreshold
        {
            get => bokehLumThreshold;
            set => NotifyPropertyChangedAndSet(ref bokehLumThreshold, value);
        }

        /// <summary>
        /// Gets or sets the scale factor for bokeh radius.
        /// </summary>
        public float BokehRadiusScale
        {
            get => bokehRadiusScale;
            set => NotifyPropertyChangedAndSet(ref bokehRadiusScale, value);
        }

        /// <summary>
        /// Gets or sets the scale factor for bokeh color.
        /// </summary>
        public float BokehColorScale
        {
            get => bokehColorScale;
            set => NotifyPropertyChangedAndSet(ref bokehColorScale, value);
        }

        /// <summary>
        /// Gets or sets the falloff factor for bokeh.
        /// </summary>
        public float BokehFallout
        {
            get => bokehFallout;
            set => NotifyPropertyChangedAndSet(ref bokehFallout, value);
        }

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

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
               .RunBefore("ColorGrading")
               .RunAfter("HBAO")
               .RunAfter("SSGI")
               .RunAfter("SSR")
               .RunAfter("MotionBlur")
               .RunAfter("AutoExposure")
               .RunAfter("TAA")
               .RunBefore("ChromaticAberration")
               .RunBefore("Bloom");
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            this.width = width;
            this.height = height;
            this.device = device;

            bokehGenerate = device.CreateComputePipeline(new()
            {
                Path = "compute/bokeh/shader.hlsl",
                Macros = macros,
            });

            gaussianBlur = new(device, Format.R16G16B16A16Float, width, height);
            DispatchArgs = new((uint)MathF.Ceiling(width / 32f), (uint)MathF.Ceiling(height / 32f), 1);

            dof = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/dof/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
                Macros = macros
            });

            bokehDraw = device.CreateGraphicsPipeline(new()
            {
                PixelShader = "effects/bokeh/mask.hlsl",
                GeometryShader = "effects/bokeh/gs.hlsl",
                VertexShader = "effects/bokeh/vs.hlsl",
                State = new GraphicsPipelineState()
                {
                    Topology = PrimitiveTopology.PointList,
                    Blend = BlendDescription.Additive,
                    BlendFactor = Vector4.One
                },
            });

            bokehBuffer = new(device, (uint)(width * height), CpuAccessFlags.None, BufferUnorderedAccessViewFlags.Append);
            blurTex = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            bokehIndirectBuffer = new(device, new DrawInstancedIndirectArgs(0, 1, 0, 0), CpuAccessFlags.None);

            cbBokeh = new(device, CpuAccessFlags.Write);
            cbDof = new(device, CpuAccessFlags.Write);

            bokehTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/bokeh/hex.dds"));

            linearWrapSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);
        }

        /// <inheritdoc/>
        public override void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            bokehBuffer.Capacity = (uint)(width * height);
            blurTex.Resize(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
        }

        /// <inheritdoc/>
        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                DofParams dofParams = default;
                dofParams.FocusRange = focusRange;
                dofParams.FocusPoint = focusPoint;
                dofParams.AutoFocusEnabled = autoFocusEnabled ? 1 : 0;
                dofParams.AutoFocusSamples = autoFocusSamples;
                dofParams.AutoFocusRadius = autoFocusRadius;
                cbDof.Update(context, dofParams);

                BokehParams bokehParams = default;
                bokehParams.DofParams = dofParams;
                bokehParams.BlurThreshold = bokehBlurThreshold;
                bokehParams.LumThreshold = bokehLumThreshold;
                bokehParams.RadiusScale = bokehRadiusScale;
                bokehParams.ColorScale = bokehColorScale;
                bokehParams.Fallout = bokehFallout;
                cbBokeh.Update(context, bokehParams);

                dirty = false;
            }
        }

        /// <inheritdoc/>
        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
            {
                return;
            }

            if (bokehEnabled)
            {
                context.SetComputePipeline(bokehGenerate);
                nint* srvs_bokeh = stackalloc nint[] { Input.NativePointer, depth.Value.SRV.NativePointer };
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

            gaussianBlur.Blur(context, Input, blurTex, width, height);

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetSampler(0, linearWrapSampler);
            nint* srvs_dof = stackalloc nint[] { Input.NativePointer, blurTex.SRV.NativePointer, depth.Value.SRV.NativePointer };
            context.PSSetShaderResources(0, 3, (void**)srvs_dof);
            context.PSSetConstantBuffer(0, cbDof);
            context.PSSetConstantBuffer(1, camera.Value);
            context.SetGraphicsPipeline(dof);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.PSSetConstantBuffer(1, null);
            context.PSSetConstantBuffer(0, null);
            Memset(srvs_dof, 0, 3);
            context.PSSetShaderResources(0, 3, (void**)srvs_dof);

            if (bokehEnabled)
            {
                context.CopyStructureCount(bokehIndirectBuffer, 0, bokehBuffer.UAV);
                context.VSSetShaderResource(0, bokehBuffer.SRV);
                context.PSSetShaderResource(0, bokehTex);
                context.SetGraphicsPipeline(bokehDraw);
                context.DrawInstancedIndirect(bokehIndirectBuffer, 0);
                context.SetGraphicsPipeline(null);
                context.PSSetShaderResource(0, null);
                context.VSSetShaderResource(0, null);
            }
            context.PSSetSampler(0, null);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
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
        }
    }
}