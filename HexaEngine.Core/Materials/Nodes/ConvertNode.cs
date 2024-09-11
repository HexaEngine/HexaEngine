namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using Newtonsoft.Json;

    public class ConvertNode : Node
    {
        public ConvertNode(int id, bool removable, bool isStatic) : base(id, "Convert", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            In = CreateOrGetPin(editor, "in", PinKind.Input, PinType.Float3, PinShape.CircleFilled);
            Out = CreateOrGetPin(editor, "out", PinKind.Output, PinType.Float4, PinShape.CircleFilled);
            base.Initialize(editor);
        }

        [JsonIgnore]
        public Pin In;

        [JsonIgnore]
        public Pin Out;

        public float Value;
    }
}