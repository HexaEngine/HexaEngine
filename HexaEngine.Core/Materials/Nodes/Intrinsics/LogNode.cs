﻿namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class LogNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public LogNode(int id, bool removable, bool isStatic) : base(id, "Log", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public LogNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "value", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "log";
    }
}