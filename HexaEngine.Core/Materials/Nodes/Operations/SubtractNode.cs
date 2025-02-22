namespace HexaEngine.Materials.Nodes.Operations
{
    using HexaEngine.Materials.Nodes;

    public class SubtractNode : FuncOperatorBaseNode
    {
        [JsonConstructor]
        public SubtractNode(int id, bool removable, bool isStatic) : base(id, "Subtract", removable, isStatic)
        {
        }

        public SubtractNode() : this(0, true, false)
        {
        }

        public override string Op { get; } = "-";
    }
}