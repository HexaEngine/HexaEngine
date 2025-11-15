namespace HexaEngine.PostFx.BuildIn
{
    using Hexa.NET.Mathematics;
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

    public unsafe struct ColorGradingCurves
    {
        public Curve RedCurve;
        public Curve GreenCurve;
        public Curve BlueCurve;
        public Curve ValueCurve;

        public Curve HueCurve;
        public Curve HueSatCurve;
        public Curve SatCurve;
        public Curve LumaSatCurve;

        public ColorGradingCurves()
        {
            RedCurve = new([new(0, 0, 0), new(1, 1, 0)]);
            GreenCurve = new([new(0, 0, 0), new(1, 1, 0)]);
            BlueCurve = new([new(0, 0, 0), new(1, 1, 0)]);
            ValueCurve = new([new(0, 0, 0), new(1, 1, 0)]);
            HueCurve = new([new(0, 0.5f, 0), new(1, 0.5f, 0)]);
            HueSatCurve = new([new(0, 0.5f, 0), new(1, 0.5f, 0)]);
            SatCurve = new([new(0, 0.5f, 0), new(1, 0.5f, 0)]);
            LumaSatCurve = new([new(0, 0.5f, 0), new(1, 0.5f, 0)]);
        }

        public readonly Curve this[int index]
        {
            get
            {
                return index switch
                {
                    0 => RedCurve,
                    1 => GreenCurve,
                    2 => BlueCurve,
                    3 => ValueCurve,
                    4 => HueCurve,
                    5 => HueSatCurve,
                    6 => SatCurve,
                    7 => LumaSatCurve,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }

        public Texture1D GetTexture(CpuAccessFlags cpuAccessFlags)
        {
            RedCurve.Compute();
            GreenCurve.Compute();
            BlueCurve.Compute();
            ValueCurve.Compute();
            HueCurve.Compute();
            HueSatCurve.Compute();
            SatCurve.Compute();
            LumaSatCurve.Compute();

            const int numCurves = 8;
            var samplesCount = RedCurve.Samples.Length;

            SubresourceData[] initialData = new SubresourceData[numCurves];

            for (int i = 0; i < numCurves; i++)
            {
                float* pixels = AllocT<float>(samplesCount);
                Curve curve = this[i];
                for (int j = 0; j < samplesCount; j++)
                {
                    pixels[j] = curve.Samples[j];
                }
                initialData[i] = new(pixels, samplesCount * sizeof(float));
            }

            Texture1D texture = new(new Texture1DDescription(Format.R32Float, samplesCount, numCurves, 1, GpuAccessFlags.Read, cpuAccessFlags), initialData);

            for (int i = 0; i < initialData.Length; i++)
            {
                Free((void*)initialData[i].DataPointer);
            }

            return texture;
        }

        public static Texture2D BakeCurvesTo3DLUT2DTex(ColorGradingCurves curves)
        {
            curves.RedCurve.Compute();
            curves.GreenCurve.Compute();
            curves.BlueCurve.Compute();
            curves.ValueCurve.Compute();
            curves.HueCurve.Compute();
            curves.HueSatCurve.Compute();
            curves.SatCurve.Compute();
            curves.LumaSatCurve.Compute();

            int lutSize = 32;
            int tileSize = lutSize;
            int tileAmount = lutSize;
            int lut2DSize = tileSize * tileAmount;

            Vector3* lut = AllocT<Vector3>(lut2DSize * lut2DSize);
            int width = lutSize * tileAmount;
            int height = lutSize;
            int rowPitch = width * 3 * sizeof(float);

            for (int b = 0; b < lutSize; b++)
            {
                float fb = (float)b / (lutSize - 1);
                float newB = InterpolateCurve(curves.BlueCurve.Samples, curves.BlueCurve.Samples.Length, fb);

                for (int g = 0; g < lutSize; g++)
                {
                    float fg = (float)g / (lutSize - 1);
                    float newG = InterpolateCurve(curves.GreenCurve.Samples, curves.GreenCurve.Samples.Length, fg);

                    for (int r = 0; r < lutSize; r++)
                    {
                        float fr = (float)r / (lutSize - 1);
                        float newR = InterpolateCurve(curves.RedCurve.Samples, curves.RedCurve.Samples.Length, fr);

                        Color color = new(newR, newG, newB, 1);
                        ColorHSVA hsv = color.ToHSVA();

                        hsv.V += InterpolateCurve(curves.ValueCurve.Samples, curves.ValueCurve.Samples.Length, hsv.V);
                        hsv.H *= InterpolateCurve(curves.HueCurve.Samples, curves.HueCurve.Samples.Length, hsv.H) * 2;
                        hsv.S *= InterpolateCurve(curves.HueSatCurve.Samples, curves.HueSatCurve.Samples.Length, hsv.H) * 2;
                        hsv.S *= InterpolateCurve(curves.SatCurve.Samples, curves.SatCurve.Samples.Length, hsv.S) * 2;

                        color = hsv.ToRGBA();
                        float luma = 0.299f * color.R + 0.587f * color.G + 0.114f * color.B;

                        hsv.S *= InterpolateCurve(curves.LumaSatCurve.Samples, curves.LumaSatCurve.Samples.Length, luma) * 2;

                        int tileIndex = b;
                        int x = r + tileIndex * lutSize;
                        int y = g;

                        int index = y * width + x;

                        color = hsv.ToRGBA();

                        lut[index] = new Vector3(MathUtil.Clamp01(color.R), MathUtil.Clamp01(color.G), MathUtil.Clamp01(color.B));
                    }
                }
            }

            Texture2DDescription desc = new(Format.R32G32B32Float, width, height, 1, 1, GpuAccessFlags.Read);

            int slicePitch = 0;
            FormatHelper.ComputePitch(Format.R32G32B32Float, width, height, ref rowPitch, ref slicePitch, 0);

            SubresourceData subresourceData = new(lut, rowPitch);

            Texture2D texture = new(desc, subresourceData);

            Free(lut);

            return texture;
        }

        private static float InterpolateCurve(float[] samples, int sampleCount, float value)
        {
            while (value > 1)
            {
                value--;
            }

            while (value < 0)
            {
                value++;
            }

            float position = value * (sampleCount - 1);
            int index = (int)Math.Floor(position);
            float fraction = position - index;

            if (index >= sampleCount - 1)
            {
                return samples[sampleCount - 1];
            }

            // we have to clamp again after interpolation.
            return MathUtil.Clamp01(samples[index] * (1 - fraction) + samples[index + 1] * fraction);
        }
    }

    /// <summary>
    /// A post-processing effect for color grading adjustments.
    /// </summary>
    [EditorDisplayName("Color Grading")]
    public class ColorGrading : PostFxBase
    {
#nullable disable
        private IGraphicsPipelineState pipeline;
        private ConstantBuffer<ColorGradingParams> paramsBuffer;
        private Texture1D curvesTex;
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
        private Vector3 offset = new(0f);
        private Vector3 power = new(1f);
        private Vector3 slope = new(1f);
        private Vector3 lift = new(0);
        private Vector3 gamma = new(1f);
        private Vector3 gain = new(1);
        private Vector3 channelMaskRed = new(1, 0, 0);
        private Vector3 channelMaskGreen = new(0, 1, 0);
        private Vector3 channelMaskBlue = new(0, 0, 1);
        private Tonemapper tonemapper;
        private ColorGradingCurves curves = new();
        private bool curvesDirty;

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
            public float _padd0;
            public float _padd1;

            public Vector3 WhiteBalance;
            public float _padd2;

            public Vector3 ChannelMaskRed;
            public float _padd3;
            public Vector3 ChannelMaskGreen;
            public float _padd4;
            public Vector3 ChannelMaskBlue;
            public float _padd5;

            public Vector3 Lift;
            public float _padd6;
            public Vector3 GammaInv;
            public float _padd7;
            public Vector3 Gain;
            public float _padd8;

            public Vector3 Offset;
            public float _padd9;
            public Vector3 Power;
            public float _padd10;
            public Vector3 Slope;
            public float _padd11;

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
            set => NotifyPropertyChangedAndSetAndReload(ref tonemapper, value);
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
        public Vector3 Lift
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
        public Vector3 Gamma
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
        public Vector3 Gain
        {
            get => gain;
            set => NotifyPropertyChangedAndSet(ref gain, value);
        }

        /// <summary>
        /// Adjusts the offset of the color grading.
        /// </summary>
        [EditorCategory("Color Grading")]
        [EditorProperty("Offset")]
        [Tooltip("Adjusts the offset of the color grading.")]
        public Vector3 Offset
        {
            get => offset;
            set => NotifyPropertyChangedAndSet(ref offset, value);
        }

        /// <summary>
        /// Adjusts the power of the color grading.
        /// </summary>
        [EditorCategory("Color Grading")]
        [EditorProperty("Power")]
        [Tooltip("Adjusts the power of the color grading.")]
        public Vector3 Power
        {
            get => power;
            set => NotifyPropertyChangedAndSet(ref power, value);
        }

        /// <summary>
        /// Adjusts the slope of the color grading.
        /// </summary>
        [EditorCategory("Color Grading")]
        [EditorProperty("Slope")]
        [Tooltip("Adjusts the slope of the color grading.")]
        public Vector3 Slope
        {
            get => slope;
            set => NotifyPropertyChangedAndSet(ref slope, value);
        }

        [EditorCategory("Curves")]
        [EditorProperty("")]
        [Tooltip("")]
        public ColorGradingCurves Curves
        {
            get => curves;
            set
            {
                curvesDirty = true;
                NotifyPropertyChangedAndSet(ref curves, value);
            }
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
            paramsBuffer = new(CpuAccessFlags.Write);

            pipeline = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                PixelShader = "HexaEngine.Core:shaders/effects/colorgrading/ps.hlsl",
                Macros = [.. macros, new ShaderMacro("TONEMAP", ((int)tonemapper).ToString())]
            },
            GraphicsPipelineStateDesc.DefaultFullscreen
           );

            curvesTex = curves.GetTexture(CpuAccessFlags.None);
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
                colorGradingParams.ChannelMaskRed = channelMaskRed;
                colorGradingParams.ChannelMaskGreen = channelMaskGreen;
                colorGradingParams.ChannelMaskBlue = channelMaskBlue;
                colorGradingParams.Lift = lift;
                colorGradingParams.GammaInv = new Vector3(1f) / gamma;
                colorGradingParams.Gain = gain;
                colorGradingParams.Offset = offset;
                colorGradingParams.Power = new Vector3(1f) / power;
                colorGradingParams.Slope = slope;
                paramsBuffer.Update(context, colorGradingParams);
                dirty = false;
            }

            if (curvesDirty)
            {
                var staging = curves.GetTexture(CpuAccessFlags.RW);
                context.CopyResource(curvesTex, staging);
                staging.Dispose();
                curvesDirty = false;
            }
        }

        public override void UpdateBindings()
        {
            pipeline.Bindings.SetSRV("inputTex", Input);
            pipeline.Bindings.SetSRV("curvesTex", curvesTex);
            pipeline.Bindings.SetCBV("TonemapParams", paramsBuffer);
        }

        /// <inheritdoc/>
        public override void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            curvesTex.Dispose();
        }
    }
}