namespace HexaEngine.Materials.Nodes.Operations
{
    using HexaEngine.Materials.Nodes;

    public class SubtractNode : FuncOperatorBaseNode
    {
        public SubtractNode(int id, bool removable, bool isStatic) : base(id, "Subtract", removable, isStatic)
        {
        }

        public override string Op { get; } = "-";
    }
}