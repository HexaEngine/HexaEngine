#nullable disable

using HexaEngine;

namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    /// <summary>
    /// Represents debug draw data.
    /// </summary>
    public unsafe class DebugDrawData
    {
        /// <summary>
        /// Gets the list of debug draw command lists.
        /// </summary>
        public List<DebugDrawCommandList> CmdLists { get; } = new();

        /// <summary>
        /// Gets or sets the total number of vertices.
        /// </summary>
        public uint TotalVertices;

        /// <summary>
        /// Gets or sets the total number of indices.
        /// </summary>
        public uint TotalIndices;

        /// <summary>
        /// Gets or sets the viewport information.
        /// </summary>
        public Viewport Viewport;

        /// <summary>
        /// Gets or sets the camera matrix.
        /// </summary>
        public Matrix4x4 Camera;
    }
}