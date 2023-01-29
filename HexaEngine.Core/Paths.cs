namespace HexaEngine.Core
{
    public static class Paths
    {
        public static string CurrentAssetsPath { get; set; } = "assets/";

        public static string CurrentTexturePath { get; set; } = "assets/textures/";

        public static string CurrentMeshesPath { get; set; } = "assets/meshes/";

        public static string CurrentAnimationsPath { get; set; } = "assets/animations/";

        public static string CurrentShaderPath { get; set; } = "assets/shaders/";

        public static string CurrentPDBShaderPath { get; set; } = "assets/pdbs";

        public static string CurrentFontPath { get; set; } = "assets/fonts/";

        public static string CurrentSoundPath { get; set; } = "assets/sounds/";

        public static string CurrentScriptFolder { get; set; } = "assets/scripts/";

        public static string CurrentProjectFolder { get; set; } = Path.GetFullPath(CurrentAssetsPath);
    }
}