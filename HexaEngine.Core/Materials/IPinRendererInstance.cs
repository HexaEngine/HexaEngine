namespace HexaEngine.Materials
{
    public interface IPinRendererInstance<TPin> : IPinRenderer where TPin : Pin
    {
        public TPin Pin { get; set; }
    }
}