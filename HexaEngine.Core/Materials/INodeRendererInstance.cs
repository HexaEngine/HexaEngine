namespace HexaEngine.Materials
{
    public interface INodeRendererInstance<TNode> : INodeRenderer where TNode : Node
    {
        public TNode Node { get; set; }
    }
}