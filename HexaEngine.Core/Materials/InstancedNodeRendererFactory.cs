namespace HexaEngine.Materials
{
    public class InstancedNodeRendererFactory<TNode, TRenderer> : NodeRendererFactory where TNode : Node where TRenderer : INodeRendererInstance<TNode>, new()
    {
        public override bool CanCreate(Node node)
        {
            return node is TNode;
        }

        public override INodeRenderer Create(Node node)
        {
            var renderer = new TRenderer
            {
                Node = (TNode)node
            };
            return renderer;
        }
    }
}