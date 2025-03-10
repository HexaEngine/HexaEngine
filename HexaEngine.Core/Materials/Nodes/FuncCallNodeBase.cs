﻿namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public abstract class FuncCallNodeBase : InferTypedNodeBase, IFuncCallNode, INodeDropConnector
    {
        private readonly List<PrimitivePin> pins = [];

        protected FuncCallNodeBase(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
        }

        [JsonIgnore]
        public abstract string Op { get; }

        [JsonIgnore]
        public IReadOnlyList<PrimitivePin> Params => pins;

        [JsonIgnore]
        public PrimitivePin Out { get; private set; } = null!;

        public override void Initialize(NodeEditor editor)
        {
            Out = AddOrGetPin(new UniversalPin(editor.GetUniqueId(), "out", PinShape.QuadFilled, PinKind.Output, Mode));
            base.Initialize(editor);
            UpdateInferState();
            UpdateMode();
        }

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

            Out.Type = Mode;
            base.UpdateMode();
        }

        void INodeDropConnector.Connect(Pin outputPin)
        {
            if (pins.Count > 0)
            {
                editor?.TryCreateLink(pins[0], outputPin);
            }
        }
    }
}