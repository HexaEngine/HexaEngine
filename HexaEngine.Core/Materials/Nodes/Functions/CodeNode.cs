namespace HexaEngine.Core.Materials.Nodes.Functions
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class CodeNode : FuncCallDeclarationBaseNode
    {
        private SType type;

        private string codeBody = string.Empty;

        public CodeNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
        }

        [JsonIgnore]
        public override string MethodName => Name;

        [JsonIgnore]
        public override SType Type => type;

        [JsonIgnore]
        public override FloatPin Out { get; protected set; }

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
                Rebuild();
                OnValueChanged();
            }
        }

        private void Rebuild()
        {
            if (editor == null)
            {
                return;
            }

            Out = (FloatPin)AddOrGetPin(PinHelper.CreatePin(editor, type, "out", PinShape.CircleFilled, PinKind.Output));
        }

        public override void DefineMethod(GenerationContext context, VariableTable table)
        {
            table.AddMethod(MethodName, "float2 uv, float rotation, float2 mid", Type.Name, CodeBody);
        }
    }
}