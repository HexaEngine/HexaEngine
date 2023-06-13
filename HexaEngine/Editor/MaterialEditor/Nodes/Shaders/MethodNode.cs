namespace HexaEngine.Editor.MaterialEditor.Nodes.Shaders
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using HexaEngine.Editor.MaterialEditor.Nodes;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using HexaEngine.Mathematics;
    using System.Collections.Generic;

    public abstract class MethodNode : Node, ITypedNode
    {
        private readonly List<FloatPin> pins = new();

        public MethodNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = MathUtil.Pack(0xff, 0x0f, 0x9e, 0x00);
            TitleHoveredColor = MathUtil.Pack(0xff, 0x13, 0xc4, 0x00);
            TitleSelectedColor = MathUtil.Pack(0xff, 0x16, 0xe4, 0x00);
        }

        public abstract string MethodName { get; }

        public abstract SType Type { get; }

        public abstract FloatPin Out { get; protected set; }

        public IReadOnlyList<FloatPin> Params => pins;

        public override T AddPin<T>(T pin)
        {
            if (pin is FloatPin floatPin)
            {
                if (floatPin.Kind == PinKind.Input)
                {
                    pins.Add(floatPin);
                }
            }
            return base.AddPin(pin);
        }

        public override void DestroyPin<T>(T pin)
        {
            if (pin is FloatPin floatPin)
            {
                if (floatPin.Kind == PinKind.Input)
                {
                    pins.Remove(floatPin);
                }
            }
            base.DestroyPin(pin);
        }

        public abstract void DefineMethod(VariableTable table);
    }
}