namespace HexaEngine.Materials.Nodes.Operations
{
    public class MultiplyNode : FuncOperatorBaseNode
    {
        public MultiplyNode(int id, bool removable, bool isStatic) : base(id, "Multiply", removable, isStatic)
        {
        }

        public override string Op { get; } = "*";
    }
}