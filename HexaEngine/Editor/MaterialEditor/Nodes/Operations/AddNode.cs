namespace HexaEngine.Editor.MaterialEditor.Nodes.Operations
{
    public class AddNode : MathOpBaseNode
    {
        public AddNode(int id, bool removable, bool isStatic) : base(id, "Add", removable, isStatic)
        {
        }

        public override string Op { get; } = "+";
    }
}