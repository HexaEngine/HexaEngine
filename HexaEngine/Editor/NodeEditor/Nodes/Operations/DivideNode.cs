namespace HexaEngine.Editor.NodeEditor.Nodes.Operations
{
    public class DivideNode : MathOpBaseNode
    {
        public DivideNode(int id, bool removable, bool isStatic) : base(id, "Divide", removable, isStatic)
        {
        }

        public override string Op { get; } = "/";
    }
}