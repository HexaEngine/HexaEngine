namespace HexaEngine.Core
{
    /// <summary>
    /// Provides paths to various asset directories.
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// Gets or sets the path to the current assets directory.
        /// </summary>
        public static string CurrentAssetsPath { get; set; } = "assets/";

        /// <summary>
        /// Gets or sets the path to the current textures directory.
        /// </summary>
        public static string CurrentTexturePath { get; set; } = "assets/textures/";

        /// <summary>
        /// Gets or sets the path to the current meshes directory.
        /// </summary>
        public static string CurrentMeshesPath { get; set; } = "assets/meshes/";

        /// <summary>
        /// Gets or sets the path to the current materials directory.
        /// </summary>
        public static string CurrentMaterialsPath { get; set; } = "assets/materials/";

        /// <summary>
        /// Gets or sets the path to the current animations directory.
        /// </summary>
        public static string CurrentAnimationsPath { get; set; } = "assets/animations/";

        /// <summary>
        /// Gets or sets the path to the current shader directory.
        /// </summary>
        public static string CurrentShaderPath { get; set; } = "assets/shaders/";

        /// <summary>
        /// Gets or sets the path to the current PDB shader directory.
        /// </summary>
        public static string CurrentPDBShaderPath { get; set; } = "assets/pdbs";

        /// <summary>
        /// Gets or sets the path to the current font directory.
        /// </summary>
        public static string CurrentFontPath { get; set; } = "assets/fonts/";

        /// <summary>
        /// Gets or sets the path to the current sound directory.
        /// </summary>
        public static string CurrentSoundPath { get; set; } = "assets/sounds/";

        /// <summary>
        /// Gets or sets the path to the current script folder.
        /// </summary>
        public static string CurrentScriptFolder { get; set; } = "assets/scripts/";

        /// <summary>
        /// Gets or sets the path to the current project folder.
        /// </summary>
        public static string CurrentProjectFolder { get; set; } = Path.GetFullPath(CurrentAssetsPath);
    }
}