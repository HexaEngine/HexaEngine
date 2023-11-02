namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using Hexa.NET.ImGui;
    using Hexa.NET.ImNodes;
    using Newtonsoft.Json;

    public class ComponentMaskNode : Node
    {
        private string mask = "xyzw";

        public ComponentMaskNode(int id, bool removable, bool isStatic) : base(id, "Component Mask", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            In = CreateOrGetPin(editor, "in", PinKind.Input, PinType.AnyFloat, ImNodesPinShape.CircleFilled);
            Out = CreateOrGetPin(editor, "out", PinKind.Output, PinType.Float4, ImNodesPinShape.CircleFilled);
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

        private void UpdateOutput()
        {
            if (mask.Length == 0)
            {
                Out.Type = PinType.Float;
                Type = new(Generator.Enums.ScalarType.Float);
            }
            else if (mask.Length == 1)
            {
                Out.Type = PinType.Float;
                Type = new(Generator.Enums.ScalarType.Float);
            }
            else if (mask.Length == 2)
            {
                Out.Type = PinType.Float2;
                Type = new(Generator.Enums.VectorType.Float2);
            }
            else if (mask.Length == 3)
            {
                Out.Type = PinType.Float3;
                Type = new(Generator.Enums.VectorType.Float3);
            }
            else if (mask.Length == 4)
            {
                Out.Type = PinType.Float4;
                Type = new(Generator.Enums.VectorType.Float4);
            }
        }

        protected override void DrawContent()
        {
            ImGui.PushItemWidth(100);
            ImGui.Text("Component Mask");
            if (ImGui.InputText("##MaskEdit", ref mask, 5))
            {
                UpdateOutput();
            }
            ImGui.PopItemWidth();
        }
    }
}