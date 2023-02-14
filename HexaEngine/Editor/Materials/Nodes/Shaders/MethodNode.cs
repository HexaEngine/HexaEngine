﻿namespace HexaEngine.Editor.Materials.Nodes.Shaders
{
    using HexaEngine.Editor.Materials.Generator;
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using System.Collections.Generic;

    public abstract class MethodNode : Node, ITypedNode
    {
        private readonly List<FloatPin> pins = new();

        public MethodNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = new(0x009e0fff);
            TitleHoveredColor = new(0x00c413ff);
            TitleSelectedColor = new(0x00e416ff);
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
                    pins.Add(floatPin);
            }
            return base.AddPin(pin);
        }

        public override void DestroyPin<T>(T pin)
        {
            if (pin is FloatPin floatPin)
            {
                if (floatPin.Kind == PinKind.Input)
                    pins.Remove(floatPin);
            }
            base.DestroyPin(pin);
        }

        public abstract void DefineMethod(VariableTable table);
    }
}