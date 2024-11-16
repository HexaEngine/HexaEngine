namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public abstract class FuncCallNodeBase : TypedNodeBase, IFuncCallNode
    {
        private readonly List<FloatPin> pins = new();

        protected FuncCallNodeBase(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
        }

        [JsonIgnore]
        public abstract string Op { get; }

        [JsonIgnore]
        public IReadOnlyList<FloatPin> Params => pins;

        [JsonIgnore]
        public Pin Out { get; private set; } = null!;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new FloatPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, mode));
            base.Initialize(editor);
        }

        public override T AddPin<T>(T pin)
        {
            if (pin is FloatPin floatPin)
            {
                if (floatPin.Kind == PinKind.Input && !pins.Contains(floatPin))
                {
                    pins.Add(floatPin);
                }
            }
            return base.AddPin(pin);
        }

        public override T AddOrGetPin<T>(T pin)
        {
            var e = base.AddOrGetPin(pin);
            if (e is FloatPin floatPin)
            {
                if (floatPin.Kind == PinKind.Input && !pins.Contains(floatPin))
                {
                    pins.Add(floatPin);
                }
            }
            return e;
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

        public override void UpdateMode()
        {
            if (lockType)
            {
                return;
            }

            for (int i = 0; i < pins.Count; i++)
            {
                pins[i].Type = mode;
            }
            base.UpdateMode();
        }
    }
}