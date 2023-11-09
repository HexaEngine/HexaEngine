#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public unsafe class DebugDrawData
    {
        public List<DebugDrawCommandList> CmdLists { get; } = new();

        public uint TotalVertices;
        public uint TotalIndices;
        public Viewport Viewport;
        public Matrix4x4 Camera;
    }
}