namespace HexaEngine.Core.Graphics
{
    public interface IDeviceChild : IDisposable
    {
        public IntPtr NativePointer { get; }

        string? DebugName { get; set; }

        bool IsDisposed { get; }
    }
}