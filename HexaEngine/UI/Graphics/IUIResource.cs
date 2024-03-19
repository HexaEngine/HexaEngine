namespace HexaEngine.UI.Graphics
{
    public interface IUIResource : IDisposable
    {
        void AddRef();

        internal long ReferenceCount { get; }
        bool IsDisposed { get; }
    }
}