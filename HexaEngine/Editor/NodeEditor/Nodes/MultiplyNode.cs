namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public class MultiplyNode : MathBaseNode
    {
        public MultiplyNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Multiply", removable, isStatic)
        {
        }

        public override string Op { get; } = "*";
    }
}