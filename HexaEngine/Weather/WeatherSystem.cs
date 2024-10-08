namespace HexaEngine.Weather
{
    using Hexa.NET.Mathematics;
    using Hexa.NET.Mathematics.Sky;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public enum SkyType
    {
        Skybox,
        UniformColor,
        HosekWilkie,
        Preetham,
        Custom,
    }

    [Flags]
    public enum FogMode
    {
        Linear = 0,
        Exp = 1,
        Exp2 = 2,
        HeightBased = 4,
    }

    /// <summary>
    /// Manages weather-related graphics rendering and effects.
    /// TODO: Weather system is broadly incomplete and needs refactoring.
    /// TODO: Volumetric Fog.
    /// TODO: Volumetric lightning inclusion.
    /// TODO: Volumetric Particles: rain, snow, dust, etc.
    /// TODO: Weather Id system with transitions or something like that.
    /// TODO: Volumes in general should be added.
    /// </summary>
    public class WeatherSystem : ISceneSystem
    {
#nullable disable
        private ResourceRef<ConstantBuffer<CBWeather>> weatherBuffer;
#nullable restore
        private bool isDirty = true;
        private bool hasSun = false;
        private bool hasAmbient = false;

        /// <summary>
        /// Gets the current instance of the WeatherManager if available in the current scene.
        /// </summary>
        public static WeatherSystem? Current => SceneManager.Current?.WeatherManager;

        private Vector3 skyColor = new(0.53f, 0.81f, 0.92f);
        private Vector3 ambientColor = new(15.0f / 255.0f, 15.0f / 255.0f, 15.0f / 255.0f);
        private Vector2 windDirection = new(10.0f, 10.0f);
        private Vector4 sunDir;
        private float windSpeed = 15f;
        private float crispiness = 43f;
        private float curliness = 3.6f;
        private float coverage = 0.505f;
        private float lightAbsorption = 0.003f;
        private float cloudsBottomHeight = 3000.0f;
        private float cloudsTopHeight = 10000.0f;
        private float densityFactor = 0.015f;
        private float cloudType = 1;
        private float turbidity = 3;
        private float groundAlbedo = 0.1f;
        private float phaseFunctionG = 0.2f;
        private SkyType skyModel;
        private float overcast;
        private Vector3 fogColor;
        private float fogIntensity;
        private float fogHeight;
        private float fogStart;
        private float fogEnd;
        private FogMode fogMode;
        private float fogDensity;
        private Vector3 sunColor = new(1, 1, 0.5f);
        private float sunIntensity = 10;
        private float sunRadius = 0.05f;
        private float sunFalloff = 2;

        /// <summary>
        /// Gets a value indicating whether the weather has a sun.
        /// </summary>
        public bool HasSun => hasSun;

        /// <summary>
        /// Gets or sets the color of the sky.
        /// </summary>
        /// <remarks>
        /// TODO: Add detailed description for the SkyColor property.
        /// </remarks>
        public Vector3 SkyColor
        {
            get => skyColor;
            set
            {
                skyColor = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the ambient color of the weather.
        /// </summary>
        public Vector3 AmbientColor
        {
            get => ambientColor;
            set
            {
                ambientColor = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the wind direction of the weather.
        /// </summary>
        public Vector2 WindDirection
        {
            get => windDirection;
            set
            {
                windDirection = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the wind speed of the weather.
        /// </summary>
        public float WindSpeed
        {
            get => windSpeed;
            set
            {
                windSpeed = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the crispiness of the weather.
        /// </summary>
        public float Crispiness
        {
            get => crispiness;
            set
            {
                crispiness = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the curliness of the weather.
        /// </summary>
        public float Curliness
        {
            get => curliness;
            set
            {
                curliness = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the coverage of the weather.
        /// </summary>
        public float Coverage
        {
            get => coverage;
            set
            {
                coverage = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the overcast of the weather.
        /// </summary>
        public float Overcast
        {
            get => overcast;
            set
            {
                overcast = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the light absorption of the weather.
        /// </summary>
        public float LightAbsorption
        {
            get => lightAbsorption;
            set
            {
                lightAbsorption = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the bottom height of the clouds in the weather.
        /// </summary>
        public float CloudsBottomHeight
        {
            get => cloudsBottomHeight;
            set
            {
                cloudsBottomHeight = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the top height of the clouds in the weather.
        /// </summary>
        public float CloudsTopHeight
        {
            get => cloudsTopHeight;
            set
            {
                cloudsTopHeight = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the density factor of the weather.
        /// </summary>
        public float DensityFactor
        {
            get => densityFactor;
            set
            {
                densityFactor = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the type of clouds in the weather.
        /// </summary>
        public float CloudType
        {
            get => cloudType;
            set
            {
                cloudType = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the turbidity of the weather.
        /// </summary>
        public float Turbidity
        {
            get => turbidity;
            set
            {
                turbidity = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the ground albedo of the weather.
        /// </summary>
        public float GroundAlbedo
        {
            get => groundAlbedo;
            set
            {
                groundAlbedo = value; isDirty = true;
            }
        }

        public float PhaseFunctionG
        {
            get => phaseFunctionG;
            set
            {
                phaseFunctionG = value; isDirty = true;
            }
        }

        public Vector3 FogColor
        {
            get => fogColor;
            set
            {
                fogColor = value; isDirty = true;
            }
        }

        public float FogIntensity
        {
            get => fogIntensity;
            set
            {
                fogIntensity = value; isDirty = true;
            }
        }

        public float FogStart
        {
            get => fogStart;
            set
            {
                fogStart = value; isDirty = true;
            }
        }

        public float FogEnd
        {
            get => fogEnd;
            set
            {
                fogEnd = value; isDirty = true;
            }
        }

        public float FogHeight
        {
            get => fogHeight;
            set
            {
                fogHeight = value; isDirty = true;
            }
        }

        public FogMode FogMode
        {
            get => fogMode;
            set
            {
                fogMode = value; isDirty = true;
            }
        }

        public float FogDensity
        {
            get => fogDensity;
            set
            {
                fogDensity = value; isDirty = true;
            }
        }

        public Vector3 SunColor
        {
            get => sunColor;
            set
            {
                sunColor = value; isDirty = true;
            }
        }

        public float SunIntensity
        {
            get => sunIntensity;
            set
            {
                sunIntensity = value; isDirty = true;
            }
        }

        public float SunRadius
        {
            get => sunRadius;
            set
            {
                sunRadius = value; isDirty = true;
            }
        }

        public float SunFalloff
        {
            get => sunFalloff;
            set
            {
                sunFalloff = value; isDirty = true;
            }
        }

        /// <summary>
        /// Gets the constant buffer containing weather information for graphics rendering.
        /// </summary>
        public ConstantBuffer<CBWeather> WeatherBuffer => weatherBuffer.Value;

        /// <inheritdoc/>
        public string Name { get; } = "Weather System";

        /// <inheritdoc/>
        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.GraphicsUpdate;

        public SkyType SkyModel { get => skyModel; set => skyModel = value; }

        public Vector4 SunDir => sunDir;

        /// <summary>
        /// Called when the system is awakened in a scene.
        /// </summary>
        /// <param name="scene">The scene where the system is awakened.</param>
        public void Awake(Scene scene)
        {
            if (!Application.GraphicsDisabled)
            {
                weatherBuffer = SceneRenderer.Current.ResourceBuilder.GetConstantBuffer<CBWeather>("CBWeather");
            }
        }

        /// <summary>
        /// Updates the WeatherManager and contributes weather-related information to graphics rendering.
        /// </summary>
        /// <param name="context">The graphics context for rendering updates.</param>
        [Profiling.Profile]
        public void Update(IGraphicsContext context)
        {
            var manager = LightManager.Current;

            if (manager == null)
            {
                return;
            }

            CBWeather weather = default;
            weather.LightDir = new(0, 1, 0, 1);

            hasSun = false;
            for (int i = 0; i < manager.ActiveCount; i++)
            {
                var light = manager.Active[i];
                if (light is DirectionalLight directional)
                {
                    weather.LightDir = new Vector4(-directional.Transform.Forward, 1);
                    weather.LightColor = directional.Color * directional.Intensity;
                    hasSun = true;
                }
                if (light is AmbientLight ambient)
                {
                    ambientColor = new Vector3(ambient.Color.X, ambient.Color.Y, ambient.Color.Z) * ambient.Intensity;
                }
            }

            this.sunDir = weather.LightDir;

            weather.SkyColor = new(skyColor, 1);
            weather.AmbientColor = new(ambientColor, 1);
            weather.WindDir = new(windDirection.X, 0, windDirection.Y, 0);
            weather.WindSpeed = windSpeed;
            weather.Time = Time.CumulativeFrameTime;
            weather.Crispiness = crispiness;
            weather.Curliness = curliness;
            weather.Coverage = coverage;
            weather.Absorption = lightAbsorption;
            weather.CloudsBottomHeight = cloudsBottomHeight;
            weather.CloudsTopHeight = cloudsTopHeight;
            weather.DensityFactor = densityFactor;
            weather.CloudType = cloudType;
            weather.PhaseFunctionG = phaseFunctionG;
            weather.FogColor = fogColor;
            weather.FogIntensity = fogIntensity;
            weather.FogStart = fogStart;
            weather.FogEnd = fogEnd;
            weather.FogHeight = fogHeight;
            weather.FogMode = (int)fogMode;
            weather.FogDensity = fogDensity;
            weather.SunColor = sunColor;
            weather.SunIntensity = sunIntensity;
            weather.SunRadius = sunRadius;
            weather.SunFalloff = sunFalloff;

            Vector3 sunDir = Vector3.Normalize(new Vector3(weather.LightDir.X, weather.LightDir.Y, weather.LightDir.Z));
            if (skyModel == SkyType.HosekWilkie)
            {
                SkyParameters skyParams = Hexa.NET.Mathematics.Sky.HosekWilkie.SkyModel.CalculateSkyParameters(turbidity, groundAlbedo, sunDir, overcast);

                weather.A = skyParams[(int)EnumSkyParams.A];
                weather.B = skyParams[(int)EnumSkyParams.B];
                weather.C = skyParams[(int)EnumSkyParams.C];
                weather.D = skyParams[(int)EnumSkyParams.D];
                weather.E = skyParams[(int)EnumSkyParams.E];
                weather.F = skyParams[(int)EnumSkyParams.F];
                weather.G = skyParams[(int)EnumSkyParams.G];
                weather.H = skyParams[(int)EnumSkyParams.H];
                weather.I = skyParams[(int)EnumSkyParams.I];
                weather.Z = skyParams[(int)EnumSkyParams.Z];
            }

            if (skyModel == SkyType.Preetham)
            {
                SkyParameters skyParams = Hexa.NET.Mathematics.Sky.Preetham.SkyModel.CalculateSkyParameters(turbidity, sunDir, overcast, 1);

                weather.A = skyParams[(int)EnumSkyParams.A];
                weather.B = skyParams[(int)EnumSkyParams.B];
                weather.C = skyParams[(int)EnumSkyParams.C];
                weather.D = skyParams[(int)EnumSkyParams.D];
                weather.E = skyParams[(int)EnumSkyParams.E];
                weather.F = skyParams[(int)EnumSkyParams.F];
            }

            weatherBuffer!.Value!.Update(context, weather);
            isDirty = false;
        }
    }

    public class WeatherRegistry
    {
        private Dictionary<string, long> nameToId = new();
        private Dictionary<long, Weather> idToWeather = new();
        private List<Weather> weathers = new();
        private Dictionary<string, WeatherParameterDefinition> nameToParam = new();
        private List<WeatherParameterDefinition> weatherParameters = new();

        private List<WeatherParameterData> state = new();

        public void Update()
        {
            for (int i = 0; i < weathers.Count; i++)
            {
            }
        }
    }

    public enum WeatherParameterType
    {
        Int,
        UInt,
        Float,
        Float2,
        Float3,
        Float4,
    }

    public unsafe class WeatherParameterDefinition
    {
        public string Name { get; }

        public WeatherParameterType Type { get; }

        public BezierCurve TransitionCurve { get; } = new(Vector2.Zero, Vector2.One);

        public WeatherParameterData Transition(WeatherParameterData a, WeatherParameterData b, float v)
        {
            WeatherParameterData data = default;
            switch (Type)
            {
                case WeatherParameterType.Int:
                    data.IntValue = (int)MathUtil.Lerp(a.IntValue, b.IntValue, TransitionCurve.ComputePoint(v).Y);
                    break;

                case WeatherParameterType.UInt:
                    data.UIntValue = (uint)MathUtil.Lerp(a.UIntValue, b.UIntValue, TransitionCurve.ComputePoint(v).Y);
                    break;

                case WeatherParameterType.Float:
                    data.FloatValue = MathUtil.Lerp(a.FloatValue, b.FloatValue, TransitionCurve.ComputePoint(v).Y);
                    break;

                case WeatherParameterType.Float2:
                    data.Vector2Value = Vector2.Lerp(a.Vector2Value, b.Vector2Value, TransitionCurve.ComputePoint(v).Y);
                    break;

                case WeatherParameterType.Float3:
                    data.Vector3Value = Vector3.Lerp(a.Vector3Value, b.Vector3Value, TransitionCurve.ComputePoint(v).Y);
                    break;

                case WeatherParameterType.Float4:
                    data.Vector4Value = Vector4.Lerp(a.Vector4Value, b.Vector4Value, TransitionCurve.ComputePoint(v).Y);
                    break;
            }
            return data;
        }
    }

    public struct WeatherParameter
    {
        public string Name;
        public WeatherParameterType Type;
        public WeatherParameterData Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct WeatherParameterData
    {
        [FieldOffset(0)]
        public int IntValue;

        [FieldOffset(0)]
        public uint UIntValue;

        [FieldOffset(0)]
        public float FloatValue;

        [FieldOffset(0)]
        public Vector2 Vector2Value;

        [FieldOffset(0)]
        public Vector3 Vector3Value;

        [FieldOffset(0)]
        public Vector4 Vector4Value;
    }

    public enum WeatherState
    {
        Inactive,
        TransitionToInactive,
        TransitionToActive,
        Active,
    }

    public class Weather
    {
        private float elapsedTime;

        public long Id { get; }

        public string Name { get; set; }

        public List<WeatherParameter> Parameters { get; } = new();

        public WeatherState State;

        public float TransitionValue;

        public BezierCurve TransitionCurve = new(Vector2.Zero, Vector2.One);

        public float TransitionTime;

        public void Tick(float dt)
        {
            if (State == WeatherState.Inactive || State == WeatherState.Active)
            {
                return;
            }

            elapsedTime += dt;
            TransitionValue = (elapsedTime / TransitionTime);

            if (TransitionValue >= 1)
            {
                // clamp to 1.
                TransitionValue = 1;
                switch (State)
                {
                    case WeatherState.TransitionToInactive:
                        State = WeatherState.Inactive;
                        break;

                    case WeatherState.TransitionToActive:
                        State = WeatherState.Active;
                        break;
                }
            }
        }

        public float GetTransitionFactor()
        {
            return ((State == WeatherState.TransitionToInactive || State == WeatherState.Inactive) ? 1 : 0) - TransitionCurve.ComputePoint(TransitionValue).Y;
        }

        public void Activate()
        {
            State = WeatherState.TransitionToActive;
            TransitionValue = 0;
            elapsedTime = 0;
        }

        public void Deactivate()
        {
            State = WeatherState.TransitionToInactive;
            TransitionValue = 0;
            elapsedTime = 0;
        }

        public void Toggle()
        {
            switch (State)
            {
                case WeatherState.Inactive:
                    State = WeatherState.TransitionToActive;
                    TransitionValue = 0;
                    elapsedTime = 0;
                    break;

                case WeatherState.TransitionToInactive:
                    State = WeatherState.TransitionToActive;
                    // Do not clear TransitionValue here to ensure smooth reverse transition.
                    // Reverse elapsed time
                    elapsedTime = TransitionTime - elapsedTime;
                    break;

                case WeatherState.TransitionToActive:
                    State = WeatherState.TransitionToInactive;
                    // Do not clear TransitionValue here to ensure smooth reverse transition.
                    // Reverse elapsed time
                    elapsedTime = TransitionTime - elapsedTime;
                    break;

                case WeatherState.Active:
                    State = WeatherState.TransitionToInactive;
                    TransitionValue = 0;
                    elapsedTime = 0;
                    break;
            }
        }
    }
}