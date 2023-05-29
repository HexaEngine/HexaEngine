namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using ImGuiNET;
    using ImNodesNET;

    public abstract class MathBaseNode : Node
    {
        private bool initialized;
        protected PinType mode = PinType.Float;
        protected string[] names;
        protected PinType[] modes;
        protected int item;

#pragma warning disable CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.

        protected MathBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
#pragma warning restore CS8618 // Non-nullable property 'Out' must contain a non-null value when exiting constructor. Consider declaring the property as nullable.
        {
            TitleColor = 0x0069d5ff;
            TitleHoveredColor = 0x0078f3ff;
            TitleSelectedColor = 0x007effff;
            modes = new PinType[] { PinType.Float, PinType.Float2, PinType.Float3, PinType.Float4 };
            names = modes.Select(x => x.ToString()).ToArray();
        }

        public PinType Mode
        {
            get => mode;
            set
            {
                mode = value;
                if (initialized)
                {
                    UpdateMode();
                }
            }
        }

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", ImNodesPinShape.QuadFilled, PinKind.Output, mode));
            base.Initialize(editor);
            initialized = true;
        }

        protected virtual void UpdateMode()
        {
            item = Array.IndexOf(modes, mode);
            Out.Type = mode;

            switch (mode)
            {
                case PinType.Float:
                    Type = new(Generator.Enums.ScalarType.Float);
                    break;

                case PinType.Float2:
                    Type = new(Generator.Enums.VectorType.Float2);
                    break;

                case PinType.Float3:
                    Type = new(Generator.Enums.VectorType.Float3);
                    break;

                case PinType.Float4:
                    Type = new(Generator.Enums.VectorType.Float4);
                    break;
            }
        }

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

        [JsonIgnore]
        public SType Type { get; protected set; }

        [JsonIgnore]
        public abstract string Op { get; }

        [JsonIgnore]
        public Pin Out { get; private set; }
    }
}