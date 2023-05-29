namespace HexaEngine.Editor.MaterialEditor.Nodes.Operations
{
    public class MultiplyNode : MathOpBaseNode
    {
        public MultiplyNode(int id, bool removable, bool isStatic) : base(id, "Multiply", removable, isStatic)
        {
        }

        public override string Op { get; } = "*";
    }
}