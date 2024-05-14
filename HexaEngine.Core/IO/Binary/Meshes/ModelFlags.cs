namespace HexaEngine.Core.IO.Binary.Meshes
{
    public enum ModelFlags
    {
        None = 0,

        /// <summary>
        /// Allows files to be larger than usual, disables validation.
        /// </summary>
        AllowExtendedSize = 1,
    }
}