namespace HexaEngine.Materials
{
    public class InstancedPinRendererFactory<TPin, TRenderer> : PinRendererFactory where TPin : Pin where TRenderer : IPinRendererInstance<TPin>, new()
    {
        public override bool CanCreate(Pin pin)
        {
            return pin is TPin;
        }

        public override IPinRenderer Create(Pin pin)
        {
            var renderer = new TRenderer
            {
                Pin = (TPin)pin
            };
            return renderer;
        }
    }
}