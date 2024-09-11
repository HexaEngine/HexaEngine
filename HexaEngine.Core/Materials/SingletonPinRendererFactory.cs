namespace HexaEngine.Materials
{
    public class SingletonPinRendererFactory<TPin, TRenderer> : PinRendererFactory where TPin : Pin where TRenderer : IPinRenderer, new()
    {
        private readonly TRenderer renderer = new();

        public override bool CanCreate(Pin pin)
        {
            return pin is TPin;
        }

        public override IPinRenderer Create(Pin pin)
        {
            renderer.AddRef(); // important, else the renderer instance gets prematurely disposed.
            return renderer;
        }
    }
}