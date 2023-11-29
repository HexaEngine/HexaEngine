namespace HexaEngine.Configuration
{
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;

    public enum ConfigPreset
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public class Helper
    {
    }

    public static class GraphicsSettings
    {
        private static readonly ConfigKey config = Config.Global.GetOrCreateKey("Renderer");
        private static readonly ConfigKey shadows = config.GetOrCreateKey("Shadows");

        static GraphicsSettings()
        {
            shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Low)}", 512);
            shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Medium)}", 1024);
            shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.High)}", 2048);
            shadows.GetOrAddValue($"{nameof(DirectionalLight)}.{nameof(ShadowResolution.Ultra)}", 4096);

            shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.Low)}", 64);
            shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.Medium)}", 256);
            shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.High)}", 512);
            shadows.GetOrAddValue($"{nameof(PointLight)}.{nameof(ShadowResolution.Ultra)}", 1024);

            shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.Low)}", 256);
            shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.Medium)}", 512);
            shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.High)}", 1024);
            shadows.GetOrAddValue($"{nameof(Spotlight)}.{nameof(ShadowResolution.Ultra)}", 2048);
        }

        public static int ShadowAtlasSize => shadows.GetOrAddValue("Shadow Atlas Size", 8192);

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
    }
}