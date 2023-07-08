namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Mathematics;

    public class TerrainGrid
    {
        private readonly List<TerrainCell> cells = new();
        private readonly List<TerrainLayer> layers = new();

        public TerrainCell this[int index]
        {
            get => cells[index];
            set => cells[index] = value;
        }

        public int Count => cells.Count;

        public List<TerrainLayer> Layers => layers;

        public void Add(TerrainCell cell)
        {
            cells.Add(cell);
            FindNeighbors(cell);
        }

        public void Remove(TerrainCell cell)
        {
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
                    return cell;
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
            for (int i = 0; i < cells.Count; ++i)
            {
                FindNeighbors(cells[i]);
            }
        }
    }
}