namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public abstract class FuncOperatorBaseNode : InferTypedNodeBase, IFuncOperatorNode, INodeDropConnector
    {
        protected FuncOperatorBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0x293580FF.RGBAToVec4();
            TitleHoveredColor = 0x34418CFF.RGBAToVec4();
            TitleSelectedColor = 0x414D96FF.RGBAToVec4();
        }

        [JsonIgnore]
        public PrimitivePin InLeft { get; private set; } = null!;

        [JsonIgnore]
        public PrimitivePin InRight { get; private set; } = null!;

        [JsonIgnore]
        public abstract string Op { get; }

        [JsonIgnore]
        public PrimitivePin Out { get; private set; } = null!;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, Mode));
            InLeft = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, PinType.DontCare, 1, flags: PinFlags.InferType));
            InRight = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, PinType.DontCare, 1, flags: PinFlags.InferType));
            base.Initialize(editor);
            UpdateInferState();
            UpdateMode();
        }

        public override void UpdateMode()
        {
            if (LockType)
            {
                return;
            }

            //InLeft.Type = Mode;
            //InRight.Type = Mode;
            Out.Type = Mode;
            base.UpdateMode();
        }

        void INodeDropConnector.Connect(Pin outputPin)
        {
            editor?.TryCreateLink(InLeft, outputPin);
        }
    }
}