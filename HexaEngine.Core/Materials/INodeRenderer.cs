namespace HexaEngine.Materials
{
    public interface INodeRenderer : IDisposable
    {
        void Draw(Node node);

        void AddRef();
    }
}