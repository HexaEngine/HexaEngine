namespace HexaEngine.Editor.MaterialEditor.Nodes
{
    using Hexa.NET.ImGui;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.NodeEditor;
    using Newtonsoft.Json;

    public abstract class TypedNodeBase : Node, ITypedNode
    {
        private bool initialized;
        protected PinType mode = PinType.Float;
        protected string[] names;
        protected PinType[] modes;
        protected int item;
        protected bool lockOutputType;
        protected bool lockType;

        protected TypedNodeBase(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0x0069d5ff;
            TitleHoveredColor = 0x0078f3ff;
            TitleSelectedColor = 0x007effff;
            modes = new PinType[] { PinType.Float, PinType.Float2OrFloat, PinType.Float3OrFloat, PinType.Float4OrFloat };
            names = modes.Select(x => x.ToString()).ToArray();
        }

        [JsonIgnore]
        public SType Type { get; protected set; }

        public PinType Mode
        {
            get => mode;
            set
            {
                if (lockType)
                {
                    return;
                }
                mode = value;
                if (initialized)
                {
                    UpdateMode();
                }
            }
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            initialized = true;
        }

        protected virtual void UpdateMode()
        {
            item = Array.IndexOf(modes, mode);

            if (lockType || lockOutputType)
            {
                return;
            }

            if (this is IFuncCallNode funcNode)
            {
                funcNode.Out.Type = mode;
            }
            if (this is IFuncOperatorNode operatorNode)
            {
                operatorNode.Out.Type = mode;
            }
            if (this is IFuncCallDeclarationNode declarationNode)
            {
                declarationNode.Out.Type = mode;
            }

            switch (mode)
            {
                case PinType.Float:
                    Type = new(Generator.Enums.ScalarType.Float);
                    break;

                case PinType.Float2OrFloat:
                    Type = new(Generator.Enums.VectorType.Float2);
                    break;

                case PinType.Float3OrFloat:
                    Type = new(Generator.Enums.VectorType.Float3);
                    break;

                case PinType.Float4OrFloat:
                    Type = new(Generator.Enums.VectorType.Float4);
                    break;
            }
        }

        protected override void DrawContentBeforePins()
        {
            if (lockType)
            {
                return;
            }

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