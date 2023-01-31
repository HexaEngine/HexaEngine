namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public class SubtractNode : MathBaseNode
    {
        public SubtractNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Subtract", removable, isStatic)
        {
        }

        public override string Op { get; } = "-";
    }
}