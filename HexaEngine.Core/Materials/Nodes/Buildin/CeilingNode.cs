﻿namespace HexaEngine.Materials.Nodes.Buildin
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Nodes;
    using HexaEngine.Materials.Pins;

    public class CeilingNode : FuncCallNodeBase
    {
        public CeilingNode(int id, bool removable, bool isStatic) : base(id, "Floor", removable, isStatic)
        {
        }

        public override void Initialize(NodeEditor editor)
        {
            AddOrGetPin(new FloatPin(editor.GetUniqueId(), "in", PinShape.QuadFilled, PinKind.Input, mode, 1));
            base.Initialize(editor);
            UpdateMode();
        }

        public override string Op { get; } = "ceil";
    }
}