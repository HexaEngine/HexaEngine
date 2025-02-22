namespace HexaEngine.Materials.Nodes.Operations
{
    public class DivideNode : FuncOperatorBaseNode
    {
        [JsonConstructor]
        public DivideNode(int id, bool removable, bool isStatic) : base(id, "Divide", removable, isStatic)
        {
        }

        public DivideNode() : this(0, true, false)
        {
        }

        public override string Op { get; } = "/";
    }
}