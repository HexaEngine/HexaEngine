namespace HexaEngine.Lights
{
    public class BinaryTreeBinPacking
    {
        private readonly BinPackingNode root = new();

        public BinaryTreeBinPacking(int width, int height)
        {
            root.Rect.Top = 0;
            root.Rect.Left = 0;
            root.Rect.Bottom = height;
            root.Rect.Right = width;

            root.Leaf = true;
        }

        public BinPackingNode? AddBlock(int width, int height)
        {
            BinPackingNode? node = null;
            while (node == null)
            {
                node = AddBlock(root, width, height);
                width /= 2;
                height /= 2;
                if (width == 0)
                {
                    return null;
                }
            }

            node.Used = true;
            return node;
        }

        private static BinPackingNode? AddBlock(BinPackingNode? node, int width, int height)
        {
            if (node == null)
            {
                return null;
            }
            if (!node.Leaf)
            {
                var newNode = AddBlock(node.Right, width, height);
                if (newNode != null)
                {
                    return newNode;
                }

                return AddBlock(node.Bottom, width, height);
            }

            if (node.Used)
            {
                return null;
            }

            var rect = node.Rect;
            var size = rect.Size;

            if (size.X < width || size.Y < height)
            {
                return null;
            }

            if (size.X == width && size.Y == height)
            {
                return node;
            }

            node.Right = new();
            node.Bottom = new();
            node.Leaf = false;
            node.Right.Parent = node;
            node.Bottom.Parent = node;

            float dw = size.X - width;
            float dh = size.Y - height;

            if (dw > dh)
            {
                node.Right.Rect = new(rect.Left, rect.Top, rect.Left + width, rect.Bottom);
                node.Bottom.Rect = new(rect.Left + width, rect.Top, rect.Right, rect.Bottom);
            }
            else
            {
                node.Right.Rect = new(rect.Left, rect.Top, rect.Right, rect.Top + height);
                node.Bottom.Rect = new(rect.Left, rect.Top + height, rect.Right, rect.Bottom);
            }

            return AddBlock(node.Right, width, height);
        }

        public void RemoveNode(BinPackingNode node)
        {
            node.Used = false;
        }

        public void Clear()
        {
            root.Bottom = null;
            root.Right = null;
            root.Leaf = true;
            root.Used = false;
        }
    }
}