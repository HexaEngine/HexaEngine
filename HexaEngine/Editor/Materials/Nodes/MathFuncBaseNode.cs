namespace HexaEngine.Editor.Materials.Nodes
{
    using HexaEngine.Editor.NodeEditor;
    using HexaEngine.Editor.NodeEditor.Pins;
    using System.Collections.Generic;

    public abstract class MathFuncBaseNode : MathBaseNode, IMathFuncNode
    {
        private readonly List<FloatPin> pins = new();

        protected MathFuncBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
        }

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

        public override T AddOrGetPin<T>(T pin)
        {
            var e = base.AddOrGetPin(pin);
            if (e is FloatPin floatPin)
            {
                if (floatPin.Kind == PinKind.Input)
                    pins.Add(floatPin);
            }
            return e;
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

        protected override void UpdateMode()
        {
            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].Type = mode;
            }
            base.UpdateMode();
        }
    }
}