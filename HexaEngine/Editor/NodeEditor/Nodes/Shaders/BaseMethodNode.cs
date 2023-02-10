namespace HexaEngine.Editor.NodeEditor.Nodes.Shaders
{
    using HexaEngine.Editor.NodeEditor.Pins;
    using System.Collections.Generic;

    public abstract class BaseMethodNode : Node
    {
        private readonly List<FloatPin> pins = new();

        public BaseMethodNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = new(0x009e0fff);
            TitleHoveredColor = new(0x00c413ff);
            TitleSelectedColor = new(0x00e416ff);
        }

        public abstract string GetMethod();

        public abstract string MethodName { get; }

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
    }
}