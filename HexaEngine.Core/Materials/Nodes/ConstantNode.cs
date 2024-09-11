namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class ConstantNode : Node
    {
        [JsonIgnore] public PinType mode = PinType.Float4;
        [JsonIgnore] public string[] names;
        [JsonIgnore] public PinType[] modes;
        [JsonIgnore] public int item = 3;

        public ConstantNode(int id, bool removable, bool isStatic) : base(id, "Constant", removable, isStatic)
        {
            modes = new PinType[] { PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4 };
            names = modes.Select(x => x.ToString()).ToArray();
            Type = new(VectorType.Float4);
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Const", PinShape.QuadFilled, PinKind.Output, mode, 1, PinFlags.AllowOutput));
            base.Initialize(editor);
        }

        public void UpdateMode()
        {
            Out.Type = mode;
            switch (mode)
            {
                case PinType.Float:
                    Type = new(ScalarType.Float);
                    break;

                case PinType.Float2:
                    Type = new(VectorType.Float2);
                    break;

                case PinType.Float3:
                    Type = new(VectorType.Float3);
                    break;

                case PinType.Float4:
                    Type = new(VectorType.Float4);
                    break;
            }
        }

        [JsonIgnore]
        public SType Type { get; private set; }

        [JsonIgnore]
        public Pin Out;
    }
}