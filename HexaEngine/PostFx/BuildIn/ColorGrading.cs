﻿namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using System.Numerics;

    public enum Tonemapper
    {
        ACESFilm,
        Neutral,
        Custom,
    }

    /// <summary>
    /// A post-processing effect for color grading adjustments.
    /// </summary>
    public class ColorGrading : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<ColorGradingParams> paramsBuffer;
        private ISamplerState samplerState;
#nullable restore
        private float shoulderStrength = 0.2f;
        private float linearStrength = 0.29f;
        private float linearAngle = 0.24f;
        private float toeStrength = 0.272f;
        private float whiteLevel = 5.3f;
        private float whiteClip = 1;
        private float postExposure = 0.93f;
        private float temperature = 0;
        private float tint = 0;
        private float hueShift = 0;
        private float saturation = 1;
        private float contrast = 1;
        private float contrastMidpoint = 0.5f;
        private float lift = 0;
        private float gamma = 2.2f;
        private float gain = 1;
        private Vector3 channelMaskRed = new(1, 0, 0);
        private Vector3 channelMaskGreen = new(0, 1, 0);
        private Vector3 channelMaskBlue = new(0, 0, 1);
        private Tonemapper tonemapper;

        public struct ColorGradingParams
        {
            public float ShoulderStrength;
            public float LinearStrength;
            public float LinearAngle;
            public float ToeStrength;

            public float WhiteLevel;   // Pre - curve white point adjustment.
            public float WhiteClip;	    // Post - curve white point adjustment.
            public float PostExposure;  // Adjusts overall exposure in EV units.
            public float HueShift;      // Shift the hue of all colors.

            public float Saturation;    // Adjusts saturation (color intensity).
            public float Contrast;      // Adjusts the contrast.
            public float ContrastMidpoint;
            public float _padd0;

            public Vector3 WhiteBalance;
            public float _padd1;

            public Vector3 ChannelMaskRed;
            public float _padd2;
            public Vector3 ChannelMaskGreen;
            public float _padd3;
            public Vector3 ChannelMaskBlue;
            public float _padd4;

            public float Lift;
            public float GammaInv;
            public float Gain;
            public float _padd5;

            public ColorGradingParams()
            {
                WhiteLevel = 5.3f;
                WhiteClip = 10;
                PostExposure = 0.93f;
                HueShift = 0;
                Saturation = 1;
                Contrast = 1;
                ChannelMaskRed = new(1, 0, 0);
                ChannelMaskGreen = new(0, 1, 0);
                ChannelMaskBlue = new(0, 0, 1);
            }
        }

        /// <inheritdoc/>
        public override string Name { get; } = "ColorGrading";

        /// <inheritdoc/>
        public override PostFxFlags Flags { get; } = PostFxFlags.AlwaysEnabled;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        [EditorCategory("Tonemapping")]
        [EditorProperty<Tonemapper>("Tonemapper")]
        [Tooltip("Selects the tonemapping algorithm to be used.")]
        public Tonemapper Tonemapper
        {
            get => tonemapper;
            set => NotifyPropertyChangedAndSet(ref tonemapper, value);
        }

        [EditorCategory("Tonemapping")]
        [EditorProperty("Shoulder Strength")]
        [Tooltip("Determines the strength of the tonemapping shoulder.")]
        public float ShoulderStrength
        {
            get => shoulderStrength;
            set => NotifyPropertyChangedAndSet(ref shoulderStrength, value);
        }

        [EditorCategory("Tonemapping")]
        [EditorProperty("Linear Strength")]
        [Tooltip("Determines the strength of the tonemapping linear component.")]
        public float LinearStrength
        {
            get => linearStrength;
            set => NotifyPropertyChangedAndSet(ref linearStrength, value);
        }

        [EditorCategory("Tonemapping")]
        [EditorProperty("Linear Angle")]
        [Tooltip("Determines the angle of the tonemapping linear component.")]
        public float LinearAngle
        {
            get => linearAngle;
            set => NotifyPropertyChangedAndSet(ref linearAngle, value);
        }

        [EditorCategory("Tonemapping")]
        [EditorProperty("Toe Strength")]
        [Tooltip("Determines the strength of the tonemapping toe.")]
        public float ToeStrength
        {
            get => toeStrength;
            set => NotifyPropertyChangedAndSet(ref toeStrength, value);
        }

        /// <summary>
        /// Pre - curve white point adjustment.
        /// </summary>
        [EditorCategory("Tonemapping")]
        [EditorProperty("White Level")]
        [Tooltip("Determines the white level.")]
        public float WhiteLevel
        {
            get => whiteLevel;
            set => NotifyPropertyChangedAndSet(ref whiteLevel, value);
        }

        /// <summary>
        /// Post - curve white point adjustment.
        /// </summary>
        [EditorCategory("Tonemapping")]
        [EditorProperty("White Clip")]
        [Tooltip("Determines the white clip.")]
        public float WhiteClip
        {
            get => whiteClip;
            set => NotifyPropertyChangedAndSet(ref whiteClip, value);
        }

        /// <summary>
        /// Adjusts overall exposure in EV units.
        /// </summary>
        [EditorCategory("Basic")]
        [EditorProperty("Post Exposure")]
        [Tooltip("Adjusts overall exposure in EV units.")]
        public float PostExposure
        {
            get => postExposure;
            set => NotifyPropertyChangedAndSet(ref postExposure, value);
        }

        /// <summary>
        /// Sets the white balance to a custom color temperature.
        /// </summary>
        [EditorCategory("Basic")]
        [EditorProperty("Temperature")]
        [Tooltip("Sets the white balance to a custom color temperature.")]
        public float Temperature
        {
            get => temperature;
            set => NotifyPropertyChangedAndSet(ref temperature, value);
        }

        /// <summary>
        /// Sets the white balance to compensate for tint (green or magenta).
        /// </summary>
        [EditorCategory("Basic")]
        [EditorProperty("Tint")]
        [Tooltip("Sets the white balance to compensate for tint (green or magenta).")]
        public float Tint
        {
            get => tint;
            set => NotifyPropertyChangedAndSet(ref tint, value);
        }

        /// <summary>
        /// Shift the hue of all colors.
        /// </summary>
        [EditorCategory("Basic")]
        [EditorProperty("Hue Shift")]
        [Tooltip("Shift the hue of all colors.")]
        public float HueShift
        {
            get => hueShift;
            set => NotifyPropertyChangedAndSet(ref hueShift, value);
        }

        /// <summary>
        /// Adjusts saturation (color intensity).
        /// </summary>
        [EditorCategory("Basic")]
        [EditorProperty("Saturation")]
        [Tooltip("Adjusts saturation (color intensity).")]
        public float Saturation
        {
            get => saturation;
            set => NotifyPropertyChangedAndSet(ref saturation, value);
        }

        /// <summary>
        /// Adjusts the contrast.
        /// </summary>
        [EditorCategory("Basic")]
        [EditorProperty("Contrast")]
        [Tooltip("Adjusts the contrast.")]
        public float Contrast
        {
            get => contrast;
            set => NotifyPropertyChangedAndSet(ref contrast, value);
        }

        /// <summary>
        /// Adjusts the contrast mid-point.
        /// </summary>
        [EditorCategory("Basic")]
        [EditorProperty("Contrast Midpoint")]
        [Tooltip("Adjusts the contrast mid-point.")]
        public float ContrastMidpoint
        {
            get => contrastMidpoint;
            set => NotifyPropertyChangedAndSet(ref contrastMidpoint, value);
        }

        [EditorCategory("Channel Mixer")]
        [EditorProperty("Red")]
        [Tooltip("Specifies the channel mask for the red channel.")]
        public Vector3 ChannelMaskRed
        {
            get => channelMaskRed;
            set => NotifyPropertyChangedAndSet(ref channelMaskRed, value);
        }

        [EditorCategory("Channel Mixer")]
        [EditorProperty("Green")]
        [Tooltip("Specifies the channel mask for the green channel.")]
        public Vector3 ChannelMaskGreen
        {
            get => channelMaskGreen;
            set => NotifyPropertyChangedAndSet(ref channelMaskGreen, value);
        }

        [EditorCategory("Channel Mixer")]
        [EditorProperty("Blue")]
        [Tooltip("Specifies the channel mask for the blue channel.")]
        public Vector3 ChannelMaskBlue
        {
            get => channelMaskBlue;
            set => NotifyPropertyChangedAndSet(ref channelMaskBlue, value);
        }

        /// <summary>
        /// Adjusts the lift of the color grading.
        /// </summary>
        [EditorCategory("Color Grading")]
        [EditorProperty("Lift")]
        [Tooltip("Adjusts the lift of the color grading.")]
        public float Lift
        {
            get => lift;
            set => NotifyPropertyChangedAndSet(ref lift, value);
        }

        /// <summary>
        /// Adjusts the gamma of the color grading.
        /// </summary>
        [EditorCategory("Color Grading")]
        [EditorProperty("Gamma")]
        [Tooltip("Adjusts the gamma of the color grading.")]
        public float Gamma
        {
            get => gamma;
            set => NotifyPropertyChangedAndSet(ref gamma, value);
        }

        /// <summary>
        /// Adjusts the gain of the color grading.
        /// </summary>
        [EditorCategory("Color Grading")]
        [EditorProperty("Gain")]
        [Tooltip("Adjusts the gain of the color grading.")]
        public float Gain
        {
            get => gain;
            set => NotifyPropertyChangedAndSet(ref gain, value);
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                 .RunAfter<Vignette>()
                 .RunAfterAllNotReferenced()
                 .RunBefore<UserLUT>();
        }

        /// <inheritdoc/>
        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            paramsBuffer = new(device, CpuAccessFlags.Write);

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/colorgrading/ps.hlsl",
            },
            GraphicsPipelineStateDesc.DefaultFullscreen
           );

            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        /// <inheritdoc/>
        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                ColorGradingParams colorGradingParams = default;
                colorGradingParams.ShoulderStrength = shoulderStrength;
                colorGradingParams.LinearStrength = linearStrength;
                colorGradingParams.LinearAngle = linearAngle;
                colorGradingParams.ToeStrength = toeStrength;
                colorGradingParams.WhiteLevel = whiteLevel;
                colorGradingParams.WhiteClip = whiteClip;
                colorGradingParams.PostExposure = postExposure;

                float t1 = Temperature * 10 / 6;
                float t2 = Tint * 10 / 6;

                // Get the CIE xy chromaticity of the reference white point.
                // Note: 0.31271 = x value on the D65 white point
                float x = 0.31271f - t1 * (t1 < 0 ? 0.1f : 0.05f);
                float standardIlluminantY = 2.87f * x - 3 * x * x - 0.27509507f;
                float y = standardIlluminantY + t2 * 0.05f;

                // Calculate the coefficients in the LMS space.
                Vector3 w1 = new(0.949237f, 1.03542f, 1.08728f); // D65 white point

                // CIExyToLMS
                float Y = 1;
                float X = Y * x / y;
                float Z = Y * (1 - x - y) / y;
                float L = 0.7328f * X + 0.4296f * Y - 0.1624f * Z;
                float M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
                float S = 0.0030f * X + 0.0136f * Y + 0.9834f * Z;
                Vector3 w2 = new(L, M, S);

                Vector3 balance = new(w1.X / w2.X, w1.Y / w2.Y, w1.Z / w2.Z);

                colorGradingParams.WhiteBalance = balance;
                colorGradingParams.HueShift = hueShift;
                colorGradingParams.Saturation = saturation;
                colorGradingParams.Contrast = contrast;
                colorGradingParams.ContrastMidpoint = contrastMidpoint;
                colorGradingParams.ChannelMaskRed = channelMaskRed;
                colorGradingParams.ChannelMaskGreen = channelMaskGreen;
                colorGradingParams.ChannelMaskBlue = channelMaskBlue;
                colorGradingParams.Lift = lift;
                colorGradingParams.GammaInv = 1 / gamma;
                colorGradingParams.Gain = gain;
                paramsBuffer.Update(context, colorGradingParams);
                dirty = false;
            }
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, samplerState);
            context.SetPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
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