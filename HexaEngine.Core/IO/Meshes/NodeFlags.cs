namespace HexaEngine.Core.IO.Meshes
{
    /// <summary>
    /// Represents flags that define the type and properties of a node.
    /// </summary>
    public enum NodeFlags
    {
        /// <summary>
        /// No specific type or properties assigned to the node.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the node is a drawable object.
        /// </summary>
        Drawable,

        /// <summary>
        /// Indicates that the node is a bone in a skeletal animation system.
        /// </summary>
        Bone,

        /// <summary>
        /// Indicates that the node is a reference to another node or file.
        /// </summary>
        Reference,
    }
}