namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.MaterialEditor.Generator.Enums;
    using HexaEngine.ImGuiNET;
    using HexaEngine.ImNodesNET;
    using Newtonsoft.Json;

    public class ConstantNode : Node
    {
        private PinType mode = PinType.Float4;
        private string[] names;
        private PinType[] modes;
        private int item = 3;

        public ConstantNode(int id, bool removable, bool isStatic) : base(id, "Constant", removable, isStatic)
        {
            modes = new PinType[] { PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4 };
            names = modes.Select(x => x.ToString()).ToArray();
            Type = new(VectorType.Float4);
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Const", ImNodesPinShape.QuadFilled, PinKind.Output, mode, 1, PinFlags.AllowOutput));
            base.Initialize(editor);
        }

        private void UpdateMode()
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

        protected override void DrawContentBeforePins()
        {
            ImGui.PushItemWidth(100);
            if (ImGui.Combo("##Mode", ref item, names, names.Length))
            {
                mode = modes[item];
                UpdateMode();
            }
            ImGui.PopItemWidth();
        }
    }
}