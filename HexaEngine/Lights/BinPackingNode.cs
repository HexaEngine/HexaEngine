namespace HexaEngine.Lights
{
    using HexaEngine.Mathematics;

    public class BinPackingNode
    {
        public Rect32 Rect;

        public int X => Rect.Left;

        public int Y => Rect.Top;

        public int Width => Rect.Right - Rect.Left;

        public int Height => Rect.Bottom - Rect.Top;

        public BinPackingNode? Parent;
        public BinPackingNode? Right;
        public BinPackingNode? Bottom;
        public bool Leaf;
        public bool Used;

        public BinPackingNode()
        {
            Leaf = true;
        }
    }
}