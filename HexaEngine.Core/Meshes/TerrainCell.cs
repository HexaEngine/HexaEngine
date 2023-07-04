namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class TerrainCell : IDisposable
    {
        private readonly List<TerrainLayer> layers = new();
        private readonly bool writeable;
        private readonly IGraphicsDevice device;
        public HeightMap HeightMap;
        public Terrain Terrain;
        public VertexBuffer<TerrainVertex> VertexBuffer;
        public IndexBuffer IndexBuffer;
        public uint Stride;
        public BoundingBox BoundingBox;
        public Point2 ID;
        public Vector3 Offset;
        public Matrix4x4 Transform;

        public TerrainShader DefaultShader;

        public TerrainCell? Left;
        public TerrainCell? Right;
        public TerrainCell? Top;
        public TerrainCell? Bottom;

        private bool disposedValue;

        public unsafe TerrainCell(IGraphicsDevice device, HeightMap heightMap, bool writeable)
        {
            this.writeable = writeable;
            this.device = device;
            HeightMap = heightMap;
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            Terrain = new(heightMap);
            BoundingBox = Terrain.Box;
            VertexBuffer = Terrain.CreateVertexBuffer(device, writeable ? CpuAccessFlags.Write : CpuAccessFlags.None);
            IndexBuffer = Terrain.CreateIndexBuffer(device);
            Stride = (uint)sizeof(TerrainVertex);
            DefaultShader = new(device, Terrain, MaterialData.Empty.GetShaderMacros(), false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~TerrainCell()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}