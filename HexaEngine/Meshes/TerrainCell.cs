namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Jobs;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System.Numerics;

    public class TerrainCell : IDisposable
    {
        private readonly List<TerrainDrawLayer> drawLayers = new();
        private readonly bool isDynamic;

        private readonly TerrainFile terrain;
        private readonly TerrainCellData cellData;
        private readonly int lodLevels;
        private readonly ReusableFileStream stream;
        private TerrainCellLODData lodData;
        private Mesh mesh;
        private int currentLODLevel = 0;

        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;
        public Point2 ID;
        public Vector3 Offset;
        public Matrix4x4 Transform;
        public Matrix4x4 TransformInv;

        public TerrainCell? Left;
        public TerrainCell? Right;
        public TerrainCell? Top;
        public TerrainCell? Bottom;

        private bool disposedValue;
        public uint TypeId;
        public uint DrawIndirectOffset;

        public unsafe TerrainCell(TerrainFile terrain, TerrainCellData cellData, ReusableFileStream stream, bool isDynamic)
        {
            ID = cellData.Position;
            Offset = new Vector3(32, 0, 32) * new Vector3(ID.X, 0, ID.Y);
            this.terrain = terrain;
            this.cellData = cellData;
            this.stream = stream;
            this.isDynamic = isDynamic;
            lodLevels = cellData.SeekTable.Entries.Count;

            lodData = cellData.LoadLODData(0, stream);
            MeshDesc meshDesc = new(cellData, lodData, false, isDynamic);
            mesh = ResourceManager.Shared.LoadMesh(meshDesc);

            for (int i = 0; i < cellData.LayerGroups.Count; i++)
            {
                drawLayers.Add(new(cellData.LayerGroups[i], stream, isDynamic));
            }

            BoundingBox = mesh.BoundingBox;
            BoundingSphere = mesh.BoundingSphere;
        }

        public IReadOnlyList<TerrainDrawLayer> DrawLayers => drawLayers;

        public uint VertexCount => mesh.VertexCount;

        public uint IndexCount => mesh.IndexCount;

        public Mesh Mesh => mesh;

        public TerrainCellData CellData => cellData;

        public TerrainCellLODData LODData => lodData;

        public int CurrentLODLevel => currentLODLevel;

        public int LODLevels => lodLevels;

        public void SetLOD(int level)
        {
            if (!stream.CanReOpen)
            {
                return;
            }

            Job.Run("LOD-Streaming Job", this, state =>
            {
                if (state is not TerrainCell cell)
                {
                    return;
                }

                var stream = cell.stream;
                lock (stream)
                {
                    stream.ReOpen();

                    var tmp = mesh;
                    lodData = cellData.LoadLODData(level, stream);
                    MeshDesc meshDesc = new(cellData, lodData, false, isDynamic);
                    mesh = ResourceManager.Shared.LoadMesh(meshDesc);
                    tmp.Dispose();

                    cell.BoundingBox = lodData.Box;
                    stream.Close();
                    cell.currentLODLevel = level;
                }
            }, ComputeLODJobPriority(level));
        }

        public JobPriority ComputeLODJobPriority(int level)
        {
            float s = level / (float)lodLevels;
            s = MathUtil.Clamp01(1 - s);
            return (JobPriority)(int)MathUtil.Lerp((float)JobPriority.Low, (float)JobPriority.Highest, s);
        }

        public TerrainDrawLayer? GetLayerMask(TerrainLayer terrainLayer, out ChannelMask mask)
        {
            for (int i = 0; i < drawLayers.Count; i++)
            {
                var layer = drawLayers[i];
                mask = layer.GetChannelMask(terrainLayer);
                if (mask != ChannelMask.None)
                {
                    return layer;
                }
            }

            mask = default;
            return null;
        }

        public bool ContainsLayer(TerrainLayer layer)
        {
            for (int i = 0; i < drawLayers.Count; i++)
            {
                var drawLayer = drawLayers[i];
                if (drawLayer.ContainsLayer(layer))
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryAddLayer(TerrainLayer layer)
        {
            return TryAddLayer(layer, out _, out _);
        }

        public bool TryAddLayer(TerrainLayer layer, out TerrainDrawLayer? drawLayer)
        {
            return TryAddLayer(layer, out drawLayer, out _);
        }

        public bool TryAddLayer(TerrainLayer layer, out TerrainDrawLayer? drawLayer, out ChannelMask channelMask)
        {
            drawLayer = GetLayerMask(layer, out channelMask);
            if (drawLayer != null)
            {
                return false;
            }

            drawLayer = AddLayer(layer, out channelMask);
            return true;
        }

        public TerrainDrawLayer AddLayer(TerrainLayer terrainLayer)
        {
            return AddLayer(terrainLayer, out _);
        }

        public TerrainDrawLayer AddLayer(TerrainLayer terrainLayer, out ChannelMask mask)
        {
            if (ContainsLayer(terrainLayer))
            {
                throw new Exception();
            }

            for (int i = 0; i < drawLayers.Count; i++)
            {
                var layer = drawLayers[i];
                if (layer.AddLayer(terrainLayer))
                {
                    mask = layer.GetChannelMask(terrainLayer);
                    return layer;
                }
            }

            TerrainLayerGroup group = new();
            group.Add(terrainLayer);
            cellData.LayerGroups.Add(group);
            terrain.LayerGroups.Add(group);

            TerrainDrawLayer drawLayer = new(group, stream, isDynamic, terrainLayer.Name == "Default");
            drawLayer.AddLayer(terrainLayer);
            drawLayers.Add(drawLayer);

            UpdateLayer(terrainLayer);

            mask = ChannelMask.R;
            return drawLayer;
        }

        public void RemoveLayer(TerrainLayer terrainLayer)
        {
            for (int i = 0; i < drawLayers.Count; i++)
            {
                var layer = drawLayers[i];
                if (layer.RemoveLayer(terrainLayer))
                {
                    return;
                }
            }
        }

        public bool RemoveDrawLayer(TerrainDrawLayer drawLayer)
        {
            if (drawLayers.Remove(drawLayer))
            {
                cellData.LayerGroups.Remove(drawLayer.LayerGroup);
                terrain.LayerGroups.Remove(drawLayer.LayerGroup);
                drawLayer.Dispose();
                return true;
            }
            return false;
        }

        public void UpdateLayer(TerrainLayer terrainLayer)
        {
            for (int i = 0; i < drawLayers.Count; i++)
            {
                var layer = drawLayers[i];
                if (layer.ContainsLayer(terrainLayer))
                {
                    // return after update because a layer can only be in one draw layer
                    layer.UpdateLayer();
                    return;
                }
            }
        }

        public void Bind(IGraphicsContext context)
        {
            mesh.BeginDraw(context);
        }

        public void Unbind(IGraphicsContext context)
        {
            mesh.EndDraw(context);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                mesh?.Dispose();
                for (int i = 0; i < drawLayers.Count; i++)
                {
                    drawLayers[i].Dispose();
                }
                drawLayers.Clear();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public bool IntersectRay(Ray ray, out Vector3 pointInTerrain)
        {
            return lodData.IntersectRay(ray, out pointInTerrain);
        }

        public bool IntersectRay(Ray ray, Matrix4x4 transform, out Vector3 pointInTerrain)
        {
            return lodData.IntersectRay(ray, transform, out pointInTerrain);
        }

        public void GenerateLOD()
        {
            Parallel.For(0, 4, i =>
            {
                cellData.GenerateLODLevel(i);
            });
        }

        public void GenerateIndicesAndUVs()
        {
            Parallel.For(0, 4, i =>
            {
                cellData.GenerateIndicesAndUVsLevel(i);
            });
        }

        public void AverageEdges()
        {
            Parallel.For(0, 4, i =>
            {
                Top?.cellData.AverageEdgeLevel(i, Edge.ZNeg, cellData);
                Bottom?.cellData.AverageEdgeLevel(i, Edge.ZPos, cellData);
                Right?.cellData.AverageEdgeLevel(i, Edge.XNeg, cellData);
                Left?.cellData.AverageEdgeLevel(i, Edge.XPos, cellData);
            });
        }

        public void Generate()
        {
            Parallel.For(0, 4, i =>
            {
                cellData.GenerateLODLevel(i);
                Top?.cellData.AverageEdgeLevel(i, Edge.ZNeg, cellData);
                Bottom?.cellData.AverageEdgeLevel(i, Edge.ZPos, cellData);
                Right?.cellData.AverageEdgeLevel(i, Edge.XNeg, cellData);
                Left?.cellData.AverageEdgeLevel(i, Edge.XPos, cellData);
            });
        }

        public void GenerateLevel(int i)
        {
            cellData.GenerateLODLevel(i);
            Top?.cellData.AverageEdgeLevel(i, Edge.ZNeg, cellData);
            Bottom?.cellData.AverageEdgeLevel(i, Edge.ZPos, cellData);
            Right?.cellData.AverageEdgeLevel(i, Edge.XNeg, cellData);
            Left?.cellData.AverageEdgeLevel(i, Edge.XPos, cellData);
        }

        public void UpdateVertexBuffer(IGraphicsContext context)
        {
            IVertexBuffer vb = mesh.VertexBuffer;
            lodData.WriteVertexBuffer(context, vb);
            vb.Update(context);
        }
    }
}