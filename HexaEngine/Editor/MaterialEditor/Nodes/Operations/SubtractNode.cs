namespace HexaEngine.Editor.MaterialEditor.Nodes.Operations
{
    using HexaEngine.Editor.MaterialEditor.Nodes;

    public class SubtractNode : FuncOperatorBaseNode
    {
        public SubtractNode(int id, bool removable, bool isStatic) : base(id, "Subtract", removable, isStatic)
        {
        }

        public override string Op { get; } = "-";
    }
}