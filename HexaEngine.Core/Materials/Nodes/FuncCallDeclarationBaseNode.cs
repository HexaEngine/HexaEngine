namespace HexaEngine.Materials.Nodes
{
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Nodes.Textures;
    using HexaEngine.Materials.Pins;
    using System.Collections.Generic;

    public abstract class FuncCallDeclarationBaseNode : Node, IFuncCallDeclarationNode
    {
        private readonly List<PrimitivePin> pins = new();

        public FuncCallDeclarationBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = 0x473874FF.RGBAToVec4();
            TitleHoveredColor = 0x685797FF.RGBAToVec4();
            TitleSelectedColor = 0x74679AFF.RGBAToVec4();
        }

        public abstract string MethodName { get; }

        public abstract SType Type { get; }

        [JsonIgnore]
        public abstract PrimitivePin Out { get; protected set; }

        public IReadOnlyList<PrimitivePin> Params => pins;

        public override T AddOrGetPin<T>(T pin)
        {
            Pin? old = Find(pin.Name);

            if (old != null)
            {
                if (old is PrimitivePin primitvePin)
                {
                    if (primitvePin.Kind == PinKind.Input && !pins.Contains(primitvePin))
                    {
                        pins.Add(primitvePin);
                    }
                }
                return (T)old;
            }
            else
            {
                AddPin(pin);
            }

            return pin;
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

        public abstract void DefineMethod(GenerationContext context, VariableTable table);
    }
}