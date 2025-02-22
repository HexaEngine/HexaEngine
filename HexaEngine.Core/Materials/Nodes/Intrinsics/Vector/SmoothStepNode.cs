﻿namespace HexaEngine.Core.Materials.Nodes.Intrinsics.Vector
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class SmoothStepNode : FuncCallNodeBase
    {
        [JsonConstructor]
        public SmoothStepNode(int id, bool removable, bool isStatic) : base(id, "SmoothStep", removable, isStatic)
        {
            AddAllowedMode(PinType.AnyFloat);
        }

        public SmoothStepNode() : this(0, true, false)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Left", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "Right", PinShape.QuadFilled, PinKind.Input, Mode, 1, flags: PinFlags.InferType));
            AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "V", PinShape.QuadFilled, PinKind.Input, Mode, 1));
            UpdateInferState();
            UpdateMode();
        }

        public override string Op { get; } = "smoothstep";
    }
}