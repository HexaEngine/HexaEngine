namespace HexaEngine.Materials.Nodes.Operations
{
    public class MultiplyNode : FuncOperatorBaseNode
    {
        [JsonConstructor]
        public MultiplyNode(int id, bool removable, bool isStatic) : base(id, "Multiply", removable, isStatic)
        {
        }

        public MultiplyNode() : this(0, true, false)
        {
        }

        public override string Op { get; } = "*";
    }
}