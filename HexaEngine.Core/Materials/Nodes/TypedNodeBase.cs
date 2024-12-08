namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Nodes.Textures;
    using Newtonsoft.Json;

    public abstract class TypedNodeBase : Node, ITypedNode
    {
        private bool initialized;
        [JsonIgnore] public PinType mode = PinType.Float;
        [JsonIgnore] public string[] names;
        [JsonIgnore] public PinType[] modes;
        [JsonIgnore] public int item;
        [JsonIgnore] public bool lockOutputType;
        [JsonIgnore] public bool lockType;

        protected TypedNodeBase(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0x3160A1FF.RGBAToVec4();
            TitleHoveredColor = 0x4371B0FF.RGBAToVec4();
            TitleSelectedColor = 0x5480BBFF.RGBAToVec4();
            modes = [PinType.Float, PinType.Float2OrFloat, PinType.Float3OrFloat, PinType.Float4OrFloat];
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

        public virtual void UpdateMode()
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
                    Type = new(ScalarType.Float);
                    break;

                case PinType.Float2OrFloat:
                    Type = new(VectorType.Float2);
                    break;

                case PinType.Float3OrFloat:
                    Type = new(VectorType.Float3);
                    break;

                case PinType.Float4OrFloat:
                    Type = new(VectorType.Float4);
                    break;
            }
        }
    }
}