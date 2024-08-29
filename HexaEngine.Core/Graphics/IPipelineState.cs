namespace HexaEngine.Core.Graphics
{
    public interface IPipelineState : IDisposable
    {
        IResourceBindingList Bindings { get; }

        string DebugName { get; }

        bool IsDisposed { get; }

        bool IsInitialized { get; }

        bool IsValid { get; }

        bool IsReady => IsInitialized && IsValid && !IsDisposed;
    }
}