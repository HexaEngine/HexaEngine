namespace HexaEngine.Materials.Nodes
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Materials;
    using HexaEngine.Materials.Generator;
    using HexaEngine.Materials.Pins;
    using System.Collections.Generic;

    public abstract class FuncCallDeclarationBaseNode : Node, IFuncCallDeclarationNode
    {
        private readonly List<FloatPin> pins = new();

        public FuncCallDeclarationBaseNode(int id, string name, bool removable, bool isStatic) : base(id, name, removable, isStatic)
        {
            TitleColor = MathUtil.PackARGB(0xff, 0x0f, 0x9e, 0x00);
            TitleHoveredColor = MathUtil.PackARGB(0xff, 0x13, 0xc4, 0x00);
            TitleSelectedColor = MathUtil.PackARGB(0xff, 0x16, 0xe4, 0x00);
        }

        public abstract string MethodName { get; }

        public abstract SType Type { get; }

        public abstract FloatPin Out { get; protected set; }

        public IReadOnlyList<FloatPin> Params => pins;

        public override T AddOrGetPin<T>(T pin)
        {
            Pin? old = Find(pin.Name);

            if (old != null)
            {
                if (old is FloatPin floatPin)
                {
                    if (floatPin.Kind == PinKind.Input && !pins.Contains(floatPin))
                    {
                        pins.Add(floatPin);
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
            if (pin is FloatPin floatPin)
            {
                if (floatPin.Kind == PinKind.Input && !pins.Contains(floatPin))
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