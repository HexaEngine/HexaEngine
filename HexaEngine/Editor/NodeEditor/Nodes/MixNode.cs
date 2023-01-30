namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public class MixNode : Node
    {
        public MixNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Mix", removable, isStatic)
        {
            Out = CreatePin("out", PinKind.Output, PinType.VectorAny, ImNodesNET.PinShape.QuadFilled);
            InLeft = CreatePin("Left", PinKind.Input, PinType.VectorAny, ImNodesNET.PinShape.QuadFilled);
            InRight = CreatePin("Right", PinKind.Input, PinType.VectorAny, ImNodesNET.PinShape.QuadFilled);
            InMix = CreatePin("Mix", PinKind.Input, PinType.Float, ImNodesNET.PinShape.QuadFilled);
        }

        public Pin Out;
        public Pin InLeft;
        public Pin InRight;
        public Pin InMix;
    }
}