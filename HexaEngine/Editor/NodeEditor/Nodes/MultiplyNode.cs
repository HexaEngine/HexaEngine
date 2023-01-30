namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public class MultiplyNode : Node, IMathOpNode
    {
        public MultiplyNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Multiply", removable, isStatic)
        {
            Out = CreatePin("out", PinKind.Output, PinType.VectorAny, ImNodesNET.PinShape.QuadFilled);
            InLeft = CreatePin("Left", PinKind.Input, PinType.VectorAny, ImNodesNET.PinShape.QuadFilled);
            InRight = CreatePin("Right", PinKind.Input, PinType.VectorAny, ImNodesNET.PinShape.QuadFilled);
        }

        public Pin Out { get; }
        public Pin InLeft { get; }
        public Pin InRight { get; }
        public string Op { get; } = "*";
    }
}