namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public abstract class FuncOperatorBaseNode : TypedNodeBase, IFuncOperatorNode
    {
        protected FuncOperatorBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0x293580FF.RGBAToVec4();
            TitleHoveredColor = 0x34418CFF.RGBAToVec4();
            TitleSelectedColor = 0x414D96FF.RGBAToVec4();
        }

        [JsonIgnore]
        public FloatPin InLeft { get; private set; } = null!;

        [JsonIgnore]
        public FloatPin InRight { get; private set; } = null!;

        [JsonIgnore]
        public abstract string Op { get; }

        [JsonIgnore]
        public Pin Out { get; private set; } = null!;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, mode));
            InLeft = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, mode, 1));
            InRight = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override void UpdateMode()
        {
            if (lockType)
            {
                return;
            }

            InLeft.Type = mode;
            InRight.Type = mode;
            base.UpdateMode();
        }
    }
}