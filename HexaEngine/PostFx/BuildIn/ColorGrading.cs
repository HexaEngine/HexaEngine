namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;

    /// <summary>
    /// A post-processing effect for color grading adjustments.
    /// </summary>
    public class ColorGrading : PostFxBase
    {
#nullable disable
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<ColorGradingParams> paramsBuffer;
        private ISamplerState samplerState;
#nullable restore
        private float blackIn = 0.02f;
        private float whiteIn = 10;
        private float blackOut = 0;
        private float whiteOut = 10;
        private float whiteLevel = 5.3f;
        private float whiteClip = 10;
        private float postExposure = 0.93f;
        private float temperature = 0;
        private float tint = 0;
        private float hueShift = 0;
        private float saturation = 1;
        private float contrast = 1;

        public struct ColorGradingParams
        {
            public float BlackIn;      // Inner control point for the black point.
            public float WhiteIn;      // Inner control point for the white point.
            public float BlackOut;     // Outer control point for the black point.
            public float WhiteOut;     // Outer control point for the white point.
            public float WhiteLevel;   // Pre - curve white point adjustment.
            public float WhiteClip;	    // Post - curve white point adjustment.
            public float PostExposure;  // Adjusts overall exposure in EV units.
            public float Temperature;   // Sets the white balance to a custom color temperature.
            public float Tint;          // Sets the white balance to compensate for tint (green or magenta).
            public float HueShift;      // Shift the hue of all colors.
            public float Saturation;    // Adjusts saturation (color intensity).
            public float Contrast;      // Adjusts the contrast.

            public ColorGradingParams()
            {
                BlackIn = 0.02f;
                WhiteIn = 10;
                BlackOut = 0;
                WhiteOut = 10;
                WhiteLevel = 5.3f;
                WhiteClip = 10;
                PostExposure = 0.93f;
                Temperature = 0;
                Tint = 0;
                HueShift = 0;
                Saturation = 1;
                Contrast = 1;
            }

            public ColorGradingParams(float blackIn, float whiteIn, float blackOut, float whiteOut, float whiteLevel, float whiteClip, float postExposure, float temperature, float tint, float hueShift, float saturation, float contrast)
            {
                BlackIn = blackIn;
                WhiteIn = whiteIn;
                BlackOut = blackOut;
                WhiteOut = whiteOut;
                WhiteLevel = whiteLevel;
                WhiteClip = whiteClip;
                PostExposure = postExposure;
                Temperature = temperature;
                Tint = tint;
                HueShift = hueShift;
                Saturation = saturation;
                Contrast = contrast;
            }
        }

        /// <inheritdoc/>
        public override string Name { get; } = "ColorGrading";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; }

        /// <summary>
        /// Inner control point for the black point.
        /// </summary>
        public float BlackIn
        {
            get => blackIn;
            set => NotifyPropertyChangedAndSet(ref blackIn, value);
        }

        /// <summary>
        /// Inner control point for the white point.
        /// </summary>
        public float WhiteIn
        {
            get => whiteIn;
            set => NotifyPropertyChangedAndSet(ref whiteIn, value);
        }

        /// <summary>
        /// Outer control point for the black point.
        /// </summary>
        public float BlackOut
        {
            get => blackOut;
            set => NotifyPropertyChangedAndSet(ref blackOut, value);
        }

        /// <summary>
        /// Outer control point for the white point.
        /// </summary>
        public float WhiteOut
        {
            get => whiteOut;
            set => NotifyPropertyChangedAndSet(ref whiteOut, value);
        }

        /// <summary>
        /// Pre - curve white point adjustment.
        /// </summary>
        public float WhiteLevel
        {
            get => whiteLevel;
            set => NotifyPropertyChangedAndSet(ref whiteLevel, value);
        }

        /// <summary>
        /// Post - curve white point adjustment.
        /// </summary>
        public float WhiteClip
        {
            get => whiteClip;
            set => NotifyPropertyChangedAndSet(ref whiteClip, value);
        }

        /// <summary>
        /// Adjusts overall exposure in EV units.
        /// </summary>
        public float PostExposure
        {
            get => postExposure;
            set => NotifyPropertyChangedAndSet(ref postExposure, value);
        }

        /// <summary>
        /// Sets the white balance to a custom color temperature.
        /// </summary>
        public float Temperature
        {
            get => temperature;
            set => NotifyPropertyChangedAndSet(ref temperature, value);
        }

        /// <summary>
        /// Sets the white balance to compensate for tint (green or magenta).
        /// </summary>
        public float Tint
        {
            get => tint;
            set => NotifyPropertyChangedAndSet(ref tint, value);
        }

        /// <summary>
        /// Shift the hue of all colors.
        /// </summary>
        public float HueShift
        {
            get => hueShift;
            set => NotifyPropertyChangedAndSet(ref hueShift, value);
        }

        /// <summary>
        /// Adjusts saturation (color intensity).
        /// </summary>
        public float Saturation
        {
            get => saturation;
            set => NotifyPropertyChangedAndSet(ref saturation, value);
        }

        /// <summary>
        /// Adjusts the contrast.
        /// </summary>
        public float Contrast
        {
            get => contrast;
            set => NotifyPropertyChangedAndSet(ref contrast, value);
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                 .RunAfter("Vignette")
                 .RunAfter("!AllNotReferenced")
                 .RunBefore("UserLUT");
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            paramsBuffer = new(device, CpuAccessFlags.Write);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/colorgrading/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen
            });

            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        /// <inheritdoc/>
        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                ColorGradingParams colorGradingParams;
                colorGradingParams.BlackIn = blackIn;
                colorGradingParams.WhiteIn = whiteIn;
                colorGradingParams.BlackOut = blackOut;
                colorGradingParams.WhiteOut = whiteOut;
                colorGradingParams.WhiteLevel = whiteLevel;
                colorGradingParams.WhiteClip = whiteClip;
                colorGradingParams.PostExposure = postExposure;
                colorGradingParams.Temperature = temperature;
                colorGradingParams.Tint = tint;
                colorGradingParams.HueShift = hueShift;
                colorGradingParams.Saturation = saturation;
                colorGradingParams.Contrast = contrast;
                paramsBuffer.Update(context, colorGradingParams);
                dirty = false;
            }
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, samplerState);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            samplerState.Dispose();
        }
    }
}