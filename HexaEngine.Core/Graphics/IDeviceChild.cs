namespace HexaEngine.Core.Graphics
{
    using System.Runtime.CompilerServices;

    public interface IDeviceChild : IDisposable
    {
        public IntPtr NativePointer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        string? DebugName { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] set; }

        bool IsDisposed { get; }

        event EventHandler? OnDisposed;
    }
}