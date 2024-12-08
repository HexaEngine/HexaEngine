namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class Log10Node : FuncCallNodeBase
    {
        public Log10Node(int id, bool removable, bool isStatic) : base(id, "Log10", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "log10";
    }
}