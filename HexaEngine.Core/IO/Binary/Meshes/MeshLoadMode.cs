namespace HexaEngine.Core.IO.Binary.Meshes
{
    /// <summary>
    /// Specifies the mode of loading mesh data.
    /// </summary>
    public enum MeshLoadMode
    {
        /// <summary>
        /// Loads the mesh data immediately. Suitable for editing purposes.
        /// </summary>
        Immediate,

        /// <summary>
        /// Delays loading the mesh data until explicitly requested. Suitable for LOD (Level of Detail) streaming.
        /// To load the data, you need to manually call <see cref="MeshData.LoadLODData(int, Stream)"/>.
        /// </summary>
        Streaming,
    }
}