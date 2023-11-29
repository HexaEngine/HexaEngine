namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Terrains;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class StaticTerrainCell : IDisposable
    {
        private readonly List<TerrainDrawLayer> layers = new();
        private readonly bool writeable;
        private readonly IGraphicsDevice device;
        public HeightMap HeightMap;
        public TerrainCellData Terrain;
        public VertexBuffer<TerrainVertex> VertexBuffer;
        public IndexBuffer<uint> IndexBuffer;
        public uint Stride;
        public BoundingBox BoundingBox;
        public Point2 ID;
        public Vector3 Offset;
        public Matrix4x4 Transform;
        public Matrix4x4 TransformInv;

        public StaticTerrainCell? Left;
        public StaticTerrainCell? Right;
        public StaticTerrainCell? Top;
        public StaticTerrainCell? Bottom;

        private bool disposedValue;

        public unsafe StaticTerrainCell(IGraphicsDevice device, HeightMap heightMap, bool writeable)
        {
            this.writeable = writeable;
            this.device = device;
            HeightMap = heightMap;
            heightMap = new(32, 32);
            heightMap.GenerateEmpty();
            Terrain = new(heightMap, 32, 32);

            BoundingBox = Terrain.Box;
            VertexBuffer = Terrain.CreateVertexBuffer(device, writeable ? CpuAccessFlags.Write : CpuAccessFlags.None);
            IndexBuffer = Terrain.CreateIndexBuffer(device);
            Stride = (uint)sizeof(TerrainVertex);
        }

        public IReadOnlyList<TerrainDrawLayer> DrawLayers => layers;

        public uint VertexCount => VertexBuffer.Count;

        public uint IndexCount => IndexBuffer.Count;

        public (ChannelMask, UavTexture2D)? GetLayerMask(StaticTerrainLayer terrainLayer)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                var mask = layer.GetChannelMask(terrainLayer);
                if (mask != ChannelMask.None)
                {
                    return (mask, layer.GetMask());
                }
            }

            return null;
        }

        public (ChannelMask, UavTexture2D) AddLayer(StaticTerrainLayer terrainLayer)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer.AddLayer(terrainLayer))
                {
                    var mask = layer.GetChannelMask(terrainLayer);
                    return (mask, layer.GetMask());
                }
            }

            TerrainDrawLayer drawLayer = new(device);
            drawLayer.AddLayer(terrainLayer);
            layers.Add(drawLayer);

            return (ChannelMask.R, drawLayer.GetMask());
        }

        public void RemoveLayer(StaticTerrainLayer terrainLayer)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer.RemoveLayer(terrainLayer))
                {
                    return;
                }
            }
        }

        public void UpdateLayer(StaticTerrainLayer terrainLayer)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer.ContainsLayer(terrainLayer))
                {
                    // return after update because a layer can only be in one draw layer
                    layer.UpdateLayerMaterials(device);
                    return;
                }
            }
        }

        public void Bind(IGraphicsContext context)
        {
            context.SetVertexBuffer(VertexBuffer, Stride);
            context.SetIndexBuffer(IndexBuffer, Format.R32UInt, 0);
        }

        public void Unbind(IGraphicsContext context)
        {
            context.SetVertexBuffer(null, 0);
            context.SetIndexBuffer(null, Format.Unknown, 0);
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

        ~StaticTerrainCell()
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