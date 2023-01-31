namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public class AddNode : MathBaseNode
    {
        public AddNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Add", removable, isStatic)
        {
        }

        public override string Op { get; } = "+";
    }
}