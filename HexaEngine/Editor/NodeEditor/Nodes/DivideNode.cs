namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public class DivideNode : MathBaseNode
    {
        public DivideNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Divide", removable, isStatic)
        {
        }

        public override string Op { get; } = "/";
    }
}