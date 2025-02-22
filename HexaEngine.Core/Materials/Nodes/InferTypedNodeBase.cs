namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Pins;
    using Newtonsoft.Json;

    public class InferTypedNodeBase : TypedNodeBase
    {
        private readonly List<Pin> inputPins = [];

        public InferTypedNodeBase(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            Mode = DefaultMode;
        }

        [JsonIgnore]
        public virtual PinType DefaultMode { get; set; } = PinType.AnyFloat;

        [JsonIgnore]
        public bool InferredType { get; set; }

        [JsonIgnore]
        public bool EnableManualSelection { get; set; }

        /// <summary>
        /// Lazy initialized mode collection, use <see cref="AddAllowedMode(PinType)"/> and <see cref="RemoveAllowedMode(PinType)"/> for modifying.
        /// </summary>
        [JsonIgnore]
        public HashSet<PinType>? AllowedModes { get; private set; }

        public void AddAllowedMode(PinType type)
        {
            AllowedModes ??= [];
            foreach (var subType in type.GetSubTypes())
            {
                AllowedModes.Add(subType);
            }
        }

        public void RemoveAllowedMode(PinType type)
        {
            if (AllowedModes == null) return;
            foreach (var subType in type.GetSubTypes())
            {
                AllowedModes.Remove(subType);
            }
            if (AllowedModes.Count == 0) AllowedModes = null;
        }

        public bool IsAllowedMode(PinType type)
        {
            return AllowedModes?.Contains(type) ?? true;
        }

        public override void Initialize(NodeEditor editor)
        {
            base.Initialize(editor);
            for (int i = 0; i < Pins.Count; i++)
            {
                var pin = Pins[i];
                if (pin.Kind != PinKind.Input || (pin.Flags & PinFlags.InferType) == 0) continue;
                if (!inputPins.Contains(pin))
                {
                    inputPins.Add(pin);
                }
            }

            UpdateInferState();
        }

        public void UpdateInferState()
        {
            var pin = inputPins.FirstOrDefault(x => x.Links.Count > 0);
            if (pin == null)
            {
                foreach (var input in inputPins)
                {
                    input.Type = PinType.DontCare;
                }
                Mode = DefaultMode;
                InferredType = false;
            }
            else
            {
                var type = pin.Links[0].Output.Type;
                Mode = type;
                foreach (var input in inputPins)
                {
                    input.Type = type;
                }
                InferredType = true;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            for (int i = 0; i < Pins.Count; i++)
            {
                var pin = Pins[i];
                if (pin.Kind != PinKind.Input || (pin.Flags & PinFlags.InferType) == 0) continue;
                if (inputPins.Remove(pin))
                {
                }
            }
        }

        public override bool CanCreateLink(Pin self, Node parentOther, Pin other)
        {
            if (self.Kind != PinKind.Input || (self.Flags & PinFlags.InferType) == 0) return true;
            return IsAllowedMode(other.Type);
        }

        public override void AddLink(Link link)
        {
            base.AddLink(link);

            if (link.InputNode != this)
            {
                return;
            }
            var pin = link.Input;
            if (pin.Kind != PinKind.Input || (pin.Flags & PinFlags.InferType) == 0) return;

            var type = link.Output.Type;

            if (!IsAllowedMode(type))
            {
                link.Destroy();
                return;
            }

            Mode = type;
            foreach (var input in inputPins)
            {
                input.Type = type;
            }
            InferredType = true;
        }

        public override void RemoveLink(Link link)
        {
            base.RemoveLink(link);
            if (link.InputNode != this)
            {
                return;
            }
            var pin = link.Input;
            if (pin.Kind != PinKind.Input || (pin.Flags & PinFlags.InferType) == 0) return;
            UpdateInferState();
        }

        public override T AddPin<T>(T pin)
        {
            if (pin.Kind != PinKind.Input || (pin.Flags & PinFlags.InferType) == 0) return base.AddPin(pin);
            if (!inputPins.Contains(pin))
            {
                inputPins.Add(pin);
            }
            return base.AddPin(pin);
        }

        public override T AddOrGetPin<T>(T pin)
        {
            if (pin.Kind != PinKind.Input || (pin.Flags & PinFlags.InferType) == 0) return base.AddOrGetPin(pin);
            if (!inputPins.Contains(pin))
            {
                inputPins.Add(pin);
            }
            return base.AddOrGetPin(pin);
        }

        public override void DestroyPin<T>(T pin)
        {
            base.DestroyPin(pin);
            if (pin.Kind != PinKind.Input || (pin.Flags & PinFlags.InferType) == 0) return;
            if (inputPins.Remove(pin))
            {
            }
        }
    }
}