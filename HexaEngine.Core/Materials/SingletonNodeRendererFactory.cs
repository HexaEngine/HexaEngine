namespace HexaEngine.Materials
{
    public class SingletonNodeRendererFactory<TNode, TRenderer> : NodeRendererFactory where TNode : Node where TRenderer : INodeRenderer, new()
    {
        private readonly TRenderer renderer = new();

        public override bool CanCreate(Node node)
        {
            return node is TNode;
        }

        public override INodeRenderer Create(Node node)
        {
            renderer.AddRef(); // important, else the renderer instance gets prematurely disposed.
            return renderer;
        }
    }
}