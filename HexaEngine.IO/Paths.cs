namespace HexaEngine.IO
{
    public static class Paths
    {
        public static string CurrentAssetsPath { get; set; } = "assets/";

        public static string CurrentTexturePath { get; set; } = "assets/textures/";

        public static string CurrentModelPath { get; set; } = "assets/models/";

        public static string CurrentShaderPath { get; set; } = "assets/shaders/";

        public static string CurrentPDBShaderPath { get; set; } = "assets/pdbs";

        public static string CurrentFontPath { get; set; } = "assets/fonts/";

        public static string CurrentSoundPath { get; set; } = "assets/sounds/";

        public static string CurrentScriptFolder { get; set; } = "assets/scripts/";

        public static string CurrentConsoleScriptFolder { get; set; } = "assets/console/";

        public static string GetRoot()
        {
            return Path.GetFullPath("assets");
        }
    }
}