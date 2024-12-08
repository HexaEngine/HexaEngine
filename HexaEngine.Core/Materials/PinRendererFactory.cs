using System.Diagnostics.CodeAnalysis;

namespace HexaEngine.Materials
{
    public abstract class PinRendererFactory
    {
        public abstract bool CanCreate(Pin pin);

        public abstract IPinRenderer Create(Pin pin);

        public bool TryCreate(Pin pin, [NotNullWhen(true)] out IPinRenderer? renderer)
        {
            if (CanCreate(pin))
            {
                renderer = Create(pin);
                return true;
            }
            renderer = null;
            return false;
        }
    }
}