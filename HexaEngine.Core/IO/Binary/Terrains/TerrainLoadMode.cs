namespace HexaEngine.Core.IO.Binary.Terrains
{
    using System.IO;

    /// <summary>
    /// Specifies the mode of loading mesh data.
    /// </summary>
    public enum TerrainLoadMode
    {
        /// <summary>
        /// Loads the mesh data immediately. Suitable for editing or dynamic terrain purposes.
        /// </summary>
        Immediate,

        /// <summary>
        /// Delays loading the mesh data until explicitly requested. Suitable for static terrain LOD (Level of Detail) streaming.
        /// To load the data, you need to manually call <see cref="TerrainCellData.LoadLODData(int, Stream)"/>.
        /// </summary>
        Streaming,
    }
}