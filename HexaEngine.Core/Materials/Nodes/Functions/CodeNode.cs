namespace HexaEngine.Core.Materials.Nodes.Functions
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Structs;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;
    using System.Text;

    public class CodeNode : FuncCallDeclarationBaseNode
    {
        private SType type;
        private string codeBody = string.Empty;

        [JsonConstructor]
        public CodeNode(int id, bool removable, bool isStatic) : base(id, "Code", removable, isStatic)
        {
        }

        public CodeNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            Rebuild();
            base.Initialize(editor);
        }

        [JsonIgnore]
        public override string MethodName => Name;

        [JsonIgnore]
        public override SType Type => type;

        [JsonIgnore]
        public List<PrimitivePin> ParameterPins { get; set; } = [];

        [JsonIgnore]
        public override PrimitivePin Out { get; protected set; } = null!;

        public string CodeBody
        {
            get => codeBody;
            set
            {
                OnValueChanging();
                codeBody = value;
                OnValueChanged();
            }
        }

        public SType ReturnType
        {
            get => type;
            set
            {
                OnValueChanging();
                type = value;
                OnValueChanged();
            }
        }

        public List<Parameter> Parameters { get; set; } = [];

        public void Rebuild()
        {
            if (editor == null)
            {
                return;
            }

            var returnPin = PinHelper.CreatePin(editor, type, "return", PinShape.CircleFilled, PinKind.Output);
            if (returnPin != null)
            {
                Out = returnPin;
            }
            ParameterPins.Clear();
            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameter param = Parameters[i];
                PrimitivePin? paramPin = PinHelper.CreatePin(editor, param.Type, param.Name, PinShape.CircleFilled, PinKind.Input);
                if (paramPin == null) continue;
                ParameterPins.Add(AddOrGetPin(paramPin));
            }

            for (int i = Pins.Count - 1; i >= 0; i--)
            {
                Pin pin = Pins[i];
                if (pin == Out || ParameterPins.Contains(pin))
                {
                    continue;
                }
                DestroyPin(pin);
            }
        }

        public override void DefineMethod(GenerationContext context, VariableTable table)
        {
            StringBuilder sb = new();
            bool first = true;
            foreach (var param in Parameters)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append(param.Type.Name);
                sb.Append(' ');
                sb.Append(param.Name);
            }
            table.AddMethod(MethodName, sb.ToString(), Type.Name, CodeBody);
        }
    }
}