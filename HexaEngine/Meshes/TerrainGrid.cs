namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using Hexa.NET.Mathematics;
    using Silk.NET.Assimp;
    using System.Numerics;

    public class TerrainGrid
    {
        private readonly TerrainFile terrain;
        private readonly List<TerrainCell> cells = new();
        private readonly TerrainLayer defaultLayer;
        private readonly ReusableFileStream stream;
        private readonly bool isDynamic;

        private BoundingBox boundingBox;

        public TerrainGrid(ReusableFileStream stream, bool isDynamic)
        {
            terrain = TerrainFile.Load(stream, isDynamic ? TerrainLoadMode.Immediate : TerrainLoadMode.Streaming);
            defaultLayer = TryAddOrGetLayer("Default");

            cells.Capacity = terrain.Cells.Count;
            for (int i = 0; i < terrain.Cells.Count; i++)
            {
                TerrainCell cell = new(terrain, terrain.Cells[i], stream, isDynamic);
                cells.Add(cell);
                cell.TryAddLayer(defaultLayer);
            }

            stream.Close();

            this.stream = stream;
            this.isDynamic = isDynamic;

            FindNeighbors();
            GenerateBoundingBox();
        }

        public TerrainCell this[int index]
        {
            get => cells[index];
            set => cells[index] = value;
        }

        public int Count => cells.Count;

        public List<TerrainLayer> Layers => terrain.Layers;

        public TerrainFile TerrainData => terrain;

        public BoundingBox BoundingBox => boundingBox;

        public bool LayerExists(string name)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                if (layer.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOfLayer(string name)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                if (layer.Name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public TerrainLayer? FindLayer(string name)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                if (layer.Name == name)
                {
                    return layer;
                }
            }
            return null;
        }

        public void AddLayer(TerrainLayer layer)
        {
            if (LayerExists(layer.Name))
            {
                throw new Exception($"Layer with the name \"{layer.Name}\" already exists.");
            }

            terrain.Layers.Add(layer);
        }

        public bool TryAddLayer(TerrainLayer layer)
        {
            if (LayerExists(layer.Name))
            {
                return false;
            }

            terrain.Layers.Add(layer);
            return true;
        }

        public TerrainLayer TryAddOrGetLayer(string name)
        {
            return TryAddOrGetLayer(name, default);
        }

        public TerrainLayer TryAddOrGetLayer(string name, AssetRef material)
        {
            TerrainLayer? layer = FindLayer(name);
            if (layer != null)
            {
                return layer;
            }
            layer = new(name, material);
            AddLayer(layer);
            return layer;
        }

        public TerrainCell CreateCell(Point2 pos, HeightMap heightMap)
        {
            TerrainCellData cellData = new(pos, heightMap);
            TerrainCell cell = new(terrain, cellData, stream, isDynamic);
            return cell;
        }

        public TerrainCell CreateCell(Point2 pos)
        {
            TerrainCellData cellData = new(pos, new HeightMap());
            cellData.GenerateLOD();
            TerrainCell cell = new(terrain, cellData, stream, isDynamic);
            return cell;
        }

        public void Add(TerrainCell cell)
        {
            cell.TryAddLayer(defaultLayer);
            cells.Add(cell);
            terrain.Cells.Add(cell.CellData);
            FindNeighbors(cell);
            GenerateBoundingBox();
        }

        public void Remove(TerrainCell cell)
        {
            terrain.Cells.Remove(cell.CellData);
            cells.Remove(cell);
        }

        public void Clear()
        {
            cells.Clear();
        }

        public TerrainCell? Find(Point2 id)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.ID == id)
                {
                    return cell;
                }
            }
            return null;
        }

        public void FindNeighbors(TerrainCell cell)
        {
            cell.Top = Find(cell.ID + new Point2(0, 1));
            cell.Bottom = Find(cell.ID + new Point2(0, -1));
            cell.Right = Find(cell.ID + new Point2(1, 0));
            cell.Left = Find(cell.ID + new Point2(-1, 0));
        }

        public void FindNeighbors()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                FindNeighbors(cells[i]);
            }
        }

        public void RegenerateAll(IGraphicsContext context)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].FixHeightMap();
            }

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].GenerateIndicesAndUVs();
            }

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].GenerateLOD();
            }

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].FixNormals();
                cells[i].UpdateVertexBuffer(context);
            }

            GenerateBoundingBox();
        }

        public void GenerateBoundingBox()
        {
            BoundingBox boundingBox = BoundingBox.Empty;

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];

                var local = cell.BoundingBox;
                var offset = cell.Offset;
                var min = local.Min + offset;
                var max = local.Max + offset;
                boundingBox.Min = Vector3.Min(boundingBox.Min, min);
                boundingBox.Max = Vector3.Max(boundingBox.Max, max);
            }
            this.boundingBox = boundingBox;
        }

        public void Dispose()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].Dispose();
            }
            Clear();
        }
    }
}