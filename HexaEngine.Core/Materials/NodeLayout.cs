namespace HexaEngine.Materials
{
    using System.Numerics;

    public class NodeLayout
    {
        private const float HorizontalSpacing = 150;
        private const float VerticalSpacing = 20;

        private readonly Dictionary<Node, Vector2> _nodeSizes = [];

        public void Layout(Node root)
        {
            ComputeNodeSizes(root);
            float startX = _nodeSizes[root].X;
            ComputePositions(root, new Vector2(startX, 0));
        }

        private void ComputeNodeSizes(Node node)
        {
            if (!_nodeSizes.ContainsKey(node))
            {
                var size = node.Size;
                var width = size.X;
                var height = size.Y;

                float maxChildWidth = 0;
                float totalChildHeight = 0;

                foreach (var dep in node.Dependencies)
                {
                    ComputeNodeSizes(dep);

                    var depSize = _nodeSizes[dep];
                    maxChildWidth = Math.Max(maxChildWidth, depSize.X);
                    totalChildHeight += depSize.Y + VerticalSpacing;
                }

                if (maxChildWidth > 0)
                {
                    width += maxChildWidth + HorizontalSpacing;
                    height = Math.Max(height, totalChildHeight);
                }

                _nodeSizes[node] = new Vector2(width, height);
            }
        }

        private void ComputePositions(Node node, Vector2 position)
        {
            var size = _nodeSizes[node];

            node.Position = new Vector2(position.X, position.Y + (size.Y - node.Size.Y) / 2);

            float currentY = position.Y;
            foreach (var dep in node.Dependencies)
            {
                ComputePositions(dep, new Vector2(position.X - (dep.Size.X + HorizontalSpacing), currentY));
                currentY += _nodeSizes[dep].Y + VerticalSpacing;
            }
        }
    }
}