namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public abstract class FuncCallVoidNodeBase : InferTypedNodeBase, IFuncCallVoidNode
    {
        private readonly List<PrimitivePin> pins = new();

        protected FuncCallVoidNodeBase(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
        }

        [JsonIgnore]
        public abstract string Op { get; }

        [JsonIgnore]
        public IReadOnlyList<PrimitivePin> Params => pins;

        public override T AddPin<T>(T pin)
        {
            if (pin is PrimitivePin primitvePin)
            {
                if (primitvePin.Kind == PinKind.Input && !pins.Contains(primitvePin))
                {
                    pins.Add(primitvePin);
                }
            }
            return base.AddPin(pin);
        }

        public override T AddOrGetPin<T>(T pin)
        {
            var e = base.AddOrGetPin(pin);
            if (e is PrimitivePin primitvePin)
            {
                if (primitvePin.Kind == PinKind.Input && !pins.Contains(primitvePin))
                {
                    pins.Add(primitvePin);
                }
            }
            return e;
        }

        public override void DestroyPin<T>(T pin)
        {
            if (pin is PrimitivePin primitvePin)
            {
                if (primitvePin.Kind == PinKind.Input)
                {
                    pins.Remove(primitvePin);
                }
            }
            base.DestroyPin(pin);
        }

        public override void UpdateMode()
        {
            if (LockType)
            {
                return;
            }

            for (int i = 0; i < pins.Count; i++)
            {
                //pins[i].Type = Mode;
            }
            base.UpdateMode();
        }
    }
}