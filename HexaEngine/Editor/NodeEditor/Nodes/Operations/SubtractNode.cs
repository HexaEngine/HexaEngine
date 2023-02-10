namespace HexaEngine.Editor.NodeEditor.Nodes.Operations
{
    public class SubtractNode : MathOpBaseNode
    {
        public SubtractNode(int id, bool removable, bool isStatic) : base(id, "Subtract", removable, isStatic)
        {
        }

        public override string Op { get; } = "-";
    }
}