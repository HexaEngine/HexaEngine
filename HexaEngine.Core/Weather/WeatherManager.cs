namespace HexaEngine.Core.Weather
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Mathematics.HosekWilkie;
    using System.Numerics;
    using System.Threading.Tasks;

    public class WeatherManager
    {
        private ConstantBuffer<CBWeather> weatherBuffer;

        public static WeatherManager Current;
        private Vector3 skyColor;
        private Vector3 ambientColor;
        private Vector2 windDirection;
        private float windSpeed;
        private float crispiness;
        private float curliness;
        private float coverage;
        private float lightAbsorption;
        private float cloudsBottomHeight;
        private float cloudsTopHeight;
        private float densityFactor;
        private float cloudType;
        private float turbidity;
        private float groundAlbedo;

        public Vector3 SkyColor { get => skyColor; set => skyColor = value; }

        public Vector3 AmbientColor { get => ambientColor; set => ambientColor = value; }

        public Vector2 WindDirection { get => windDirection; set => windDirection = value; }

        public float WindSpeed { get => windSpeed; set => windSpeed = value; }

        public float Crispiness { get => crispiness; set => crispiness = value; }

        public float Curliness { get => curliness; set => curliness = value; }

        public float Coverage { get => coverage; set => coverage = value; }

        public float LightAbsorption { get => lightAbsorption; set => lightAbsorption = value; }

        public float CloudsBottomHeight { get => cloudsBottomHeight; set => cloudsBottomHeight = value; }

        public float CloudsTopHeight { get => cloudsTopHeight; set => cloudsTopHeight = value; }

        public float DensityFactor { get => densityFactor; set => densityFactor = value; }

        public float CloudType { get => cloudType; set => cloudType = value; }

        public float Turbidity { get => turbidity; set => turbidity = value; }

        public float GroundAlbedo { get => groundAlbedo; set => groundAlbedo = value; }

        public Task InitializeAsync(IGraphicsDevice device)
        {
            weatherBuffer = new(device, CpuAccessFlags.Write);
            return Task.CompletedTask;
        }

        public void UpdateWeather(IGraphicsContext context)
        {
            var manager = LightManager.Current;

            if (manager == null)
            {
                return;
            }

            CBWeather weather = default;

            for (int i = 0; i < manager.ActiveCount; i++)
            {
                var light = manager.Active[i];
                if (light is DirectionalLight directional)
                {
                    weather.LightDir = new Vector4(directional.Transform.Forward, 1);
                    weather.LightColor = directional.Color * directional.Intensity;
                    break;
                }
            }

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

            Vector3 sunDir = Vector3.Normalize(new(weather.LightDir.X, weather.LightDir.Y, weather.LightDir.Z));
            SkyParameters skyParams = SkyModel.CalculateSkyParameters(turbidity, groundAlbedo, sunDir);

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

            weatherBuffer.Update(context, weather);
        }
    }
}