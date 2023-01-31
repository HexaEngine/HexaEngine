namespace HexaEngine.Editor.NodeEditor.Nodes
{
    public class InputNode : Node
    {
        public InputNode(NodeEditor graph, bool removable, bool isStatic) : base(graph, "Geometry", removable, isStatic)
        {
            ClipPos = CreatePin("position", PinKind.Output, PinType.Float4, ImNodesNET.PinShape.CircleFilled);
            WorldPos = CreatePin("pos", PinKind.Output, PinType.Float4, ImNodesNET.PinShape.CircleFilled);
            TexCoord = CreatePin("tex", PinKind.Output, PinType.Float2, ImNodesNET.PinShape.CircleFilled);
            Normal = CreatePin("normal", PinKind.Output, PinType.Float3, ImNodesNET.PinShape.CircleFilled);
            Tangent = CreatePin("tangent", PinKind.Output, PinType.Float3, ImNodesNET.PinShape.CircleFilled);
        }

        public Pin ClipPos;
        public Pin WorldPos;
        public Pin TexCoord;
        public Pin Normal;
        public Pin Tangent;
    }
}