namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using Newtonsoft.Json;

    public class ComponentMaskNode : Node
    {
        [JsonIgnore] public string mask = "xyzw";

        public ComponentMaskNode(int id, bool removable, bool isStatic) : base(id, "Component Mask", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            In = CreateOrGetPin(editor, "in", PinKind.Input, PinType.AnyFloat, PinShape.CircleFilled);
            Out = CreateOrGetPin(editor, "out", PinKind.Output, PinType.Float4, PinShape.CircleFilled);
            base.Initialize(editor);
            UpdateOutput();
        }

        [JsonIgnore]
        public Pin In;

        [JsonIgnore]
        public Pin Out;

        [JsonIgnore]
        public SType Type;

        public string Mask { get => mask; set => mask = value; }

        public void UpdateOutput()
        {
            if (mask.Length == 0)
            {
                Out.Type = PinType.Float;
                Type = new(ScalarType.Float);
            }
            else if (mask.Length == 1)
            {
                Out.Type = PinType.Float;
                Type = new(ScalarType.Float);
            }
            else if (mask.Length == 2)
            {
                Out.Type = PinType.Float2;
                Type = new(VectorType.Float2);
            }
            else if (mask.Length == 3)
            {
                Out.Type = PinType.Float3;
                Type = new(VectorType.Float3);
            }
            else if (mask.Length == 4)
            {
                Out.Type = PinType.Float4;
                Type = new(VectorType.Float4);
            }
        }
    }
}