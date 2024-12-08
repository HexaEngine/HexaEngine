namespace HexaEngine.Materials.Nodes.Operations
{
    public class AddNode : FuncOperatorBaseNode
    {
        public AddNode(int id, bool removable, bool isStatic) : base(id, "Add", removable, isStatic)
        {
        }

        public override string Op { get; } = "+";
    }
}