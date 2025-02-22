namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class Log10Node : FuncCallNodeBase
    {
        [JsonConstructor]
        public Log10Node(int id, bool removable, bool isStatic) : base(id, "Log10", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public Log10Node() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "log10";
    }
}