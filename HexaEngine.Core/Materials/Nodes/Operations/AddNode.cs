namespace HexaEngine.Materials.Nodes.Operations
{
    public class AddNode : FuncOperatorBaseNode
    {
        [JsonConstructor]
        public AddNode(int id, bool removable, bool isStatic) : base(id, "Add", removable, isStatic)
        {
        }

        public AddNode() : this(0, true, false)
        {
        }

        public override string Op { get; } = "+";
    }
}