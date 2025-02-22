namespace HexaEngine.Materials.Pins
{
    using HexaEngine.Materials;

    public abstract class PrimitivePin : Pin, IDefaultValuePin
    {
        protected PrimitivePin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue) : base(id, name, shape, kind, type, maxLinks)
        {
        }

        public abstract string GetDefaultValue();
    }

    public abstract class NumericPin : PrimitivePin
    {
        protected NumericPin(int id, string name, PinShape shape, PinKind kind, PinType type, uint maxLinks = uint.MaxValue) : base(id, name, shape, kind, type, maxLinks)
        {
        }

        public override PinType Type
        {
            get => base.Type;
            set
            {
                if (!value.IsNumeric())
                {
                    throw new InvalidOperationException("Cannot set pin type to a non numeric value on a NumericPin.");
                }
                base.Type = value;
            }
        }

        public T As<T>() where T : NumericPin
        {
            return (T)this;
        }
    }
}