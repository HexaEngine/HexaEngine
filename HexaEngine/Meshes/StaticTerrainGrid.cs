namespace HexaEngine.Meshes
{
    using HexaEngine.Mathematics;

    public class StaticTerrainGrid
    {
        private readonly List<StaticTerrainCell> cells = new();
        private readonly List<StaticTerrainLayer> layers = new();
        private readonly StaticTerrainLayer def = new("Default");

        public StaticTerrainGrid()
        {
            layers.Add(def);
        }

        public StaticTerrainCell this[int index]
        {
            get => cells[index];
            set => cells[index] = value;
        }

        public int Count => cells.Count;

        public List<StaticTerrainLayer> Layers => layers;

        public void Add(StaticTerrainCell cell)
        {
            cell.AddLayer(def, out _);
            cells.Add(cell);
            FindNeighbors(cell);
        }

        public void Remove(StaticTerrainCell cell)
        {
            cells.Remove(cell);
        }

        public void Clear()
        {
            cells.Clear();
        }

        public StaticTerrainCell? Find(Point2 id)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.ID == id)
                    return cell;
            }
            return null;
        }

        public void FindNeighbors(StaticTerrainCell cell)
        {
            cell.Top = Find(cell.ID + new Point2(0, 1));
            cell.Bottom = Find(cell.ID + new Point2(0, -1));
            cell.Right = Find(cell.ID + new Point2(1, 0));
            cell.Left = Find(cell.ID + new Point2(-1, 0));
        }

        public void FindNeighbors()
        {
            for (int i = 0; i < cells.Count; ++i)
            {
                FindNeighbors(cells[i]);
            }
        }
    }
}