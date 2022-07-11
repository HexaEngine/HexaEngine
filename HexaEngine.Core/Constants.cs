namespace HexaEngine.Core
{
    public static class Constants
    {
        public const int MAX_LIGHTS_PER_SCENE = 32;

        public static int ShadowMapSize { get; set; } = 1024 * 8;

        public static int MipLevels { get; set; } = 8;
    }
}