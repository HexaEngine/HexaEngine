namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;
    using System.Buffers;
    using System.Net.NetworkInformation;

    public abstract class TypedNodeBase : Node, ITypedNode
    {
        private bool initialized;
        private PinType mode = PinType.Float;
        private int modeIndex;
        private bool lockOutputType;
        private bool lockType;

        protected TypedNodeBase(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0x3160A1FF.RGBAToVec4();
            TitleHoveredColor = 0x4371B0FF.RGBAToVec4();
            TitleSelectedColor = 0x5480BBFF.RGBAToVec4();
        }

        [JsonIgnore]
        public virtual string ModesComboString => PinTypeHelper.NumericVectorOrScalarTypesCombo;

        [JsonIgnore]
        public virtual PinType[] Modes => PinTypeHelper.NumericVectorOrScalarTypes;

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
                if (mode == value) return;
                mode = value;
                if (initialized)
                {
                    UpdateMode();
                }
            }
        }

        [JsonIgnore]
        public int ModeIndex => modeIndex;

        [JsonIgnore]
        public bool LockType { get => lockType; set => lockType = value; }

        [JsonIgnore]
        public bool LockOutputType { get => lockOutputType; set => lockOutputType = value; }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            initialized = true;
        }

        public override void Destroy()
        {
            initialized = false;
            base.Destroy();
        }

        public virtual void UpdateMode()
        {
            modeIndex = Array.IndexOf(Modes, mode);

            if (lockType || lockOutputType)
            {
                return;
            }

            if (this is IOutNode node)
            {
                node.Out.Type = mode;
            }

            Type = mode.ToSType();

            UpdateLinks();
        }

        protected void OverwriteMode(PinType type)
        {
            mode = type;

            modeIndex = Array.IndexOf(Modes, mode);

            if (lockType || lockOutputType)
            {
                return;
            }

            if (this is IOutNode node)
            {
                node.Out.Type = mode;
            }

            Type = mode.ToSType();

            UpdateLinks();
        }
    }
}