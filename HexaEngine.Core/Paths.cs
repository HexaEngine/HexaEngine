namespace HexaEngine.Core
{
    /// <summary>
    /// Provides paths to various asset directories.
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// Gets or sets the path to the current shader directory.
        /// </summary>
        public static string CurrentShaderPath { get; set; } = "shaders/";

        /// <summary>
        /// Gets or sets the path to the current project folder.
        /// </summary>
        public static string CurrentProjectFolder { get; set; } = null!;
    }
}