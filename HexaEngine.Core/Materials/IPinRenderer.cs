namespace HexaEngine.Materials
{
    public interface IPinRenderer : IDisposable
    {
        void Draw(Pin pin);

        void AddRef();
    }
}