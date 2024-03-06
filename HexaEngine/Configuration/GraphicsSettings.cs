namespace HexaEngine.Configuration
{
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;

    public static class GraphicsSettings
    {
        private static readonly ConfigKey config = Config.Global.GetOrCreateKey("Renderer");
        private static readonly ConfigKey shadows = config.GetOrCreateKey("Shadows");

        static GraphicsSettings()
        {
            shadows.GetOrAddValueKey($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Low)}", 512).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Medium)}", 1024).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.High)}", 2048).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Ultra)}", 4096).ValueChanged += ValueChanged;

            shadows.GetOrAddValueKey($"{nameof(PointLight)}.{nameof(ShadowResolution.Low)}", 64).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(PointLight)}.{nameof(ShadowResolution.Medium)}", 256).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(PointLight)}.{nameof(ShadowResolution.High)}", 512).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(PointLight)}.{nameof(ShadowResolution.Ultra)}", 1024).ValueChanged += ValueChanged;

            shadows.GetOrAddValueKey($"{nameof(Spotlight)}.{nameof(ShadowResolution.Low)}", 256).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(Spotlight)}.{nameof(ShadowResolution.Medium)}", 512).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(Spotlight)}.{nameof(ShadowResolution.High)}", 1024).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(Spotlight)}.{nameof(ShadowResolution.Ultra)}", 2048).ValueChanged += ValueChanged;

            shadows.GetOrAddValueKey($"{nameof(SoftShadowMode)}", SoftShadowMode.VSM).ValueChanged += ValueChanged;
            shadows.GetOrAddValueKey($"{nameof(ShadowPreFilterMode)}", ShadowPreFilterMode.Gaussian).ValueChanged += ValueChanged;
        }

        private static void ValueChanged(ConfigValue valueKey, string? value)
        {
        }

        public static int ShadowAtlasSize => shadows.GetOrAddValue("Shadow Atlas Size", 8192);

        public static SoftShadowMode SoftShadowMode
        {
            get => shadows.GetOrAddValue($"{nameof(SoftShadowMode)}", SoftShadowMode.VSM);
            set => shadows.SetValue($"{nameof(SoftShadowMode)}", value);
        }

        public static ShadowPreFilterMode ShadowPreFilterMode
        {
            get => shadows.GetOrAddValue($"{nameof(ShadowPreFilterMode)}", ShadowPreFilterMode.Gaussian);
            set => shadows.SetValue($"{nameof(ShadowPreFilterMode)}", value);
        }

        public static int GetSMSizeDirectionalLight(ShadowResolution resolution)
        {
            return resolution switch
            {
                ShadowResolution.Low => shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Low)}", 512),
                ShadowResolution.Medium => shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Medium)}", 1024),
                ShadowResolution.High => shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.High)}", 2048),
                ShadowResolution.Ultra => shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Ultra)}", 4096),
                _ => -1,
            };
        }

        public static int GetSMSizePointLight(ShadowResolution resolution)
        {
            return resolution switch
            {
                ShadowResolution.Low => shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.Low)}", 64),
                ShadowResolution.Medium => shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.Medium)}", 256),
                ShadowResolution.High => shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.High)}", 512),
                ShadowResolution.Ultra => shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.Ultra)}", 1024),
                _ => -1,
            };
        }

        public static int GetSMSizeSpotlight(ShadowResolution resolution)
        {
            return resolution switch
            {
                ShadowResolution.Low => shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.Low)}", 256),
                ShadowResolution.Medium => shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.Medium)}", 512),
                ShadowResolution.High => shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.High)}", 1024),
                ShadowResolution.Ultra => shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.Ultra)}", 2048),
                _ => -1,
            };
        }

        public static Format ShadowMapFormat => SoftShadowMode switch
        {
            SoftShadowMode.None => Format.R32Float,
            SoftShadowMode.PCF => Format.R32Float,
            SoftShadowMode.PCSS => Format.R32Float,
            SoftShadowMode.VSM => Format.R32G32Float,
            SoftShadowMode.EVSM => Format.R16G16B16A16Float,
            SoftShadowMode.SAVSM => Format.R32G32Float,
            SoftShadowMode.MSM => Format.R16G16B16A16Float,
            _ => Format.Unknown,
        };
    }
}