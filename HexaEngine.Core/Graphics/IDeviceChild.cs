namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Unsafes;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a graphics device child, which is an object associated with a graphics device.
    /// </summary>
    public interface IDeviceChild : INative
    {
        /// <summary>
        /// Gets or sets the debug name for the device child.
        /// </summary>
        string? DebugName { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] set; }

        /// <summary>
        /// Gets a value indicating whether the device child has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Occurs when the device child is disposed.
        /// </summary>
        event EventHandler? OnDisposed;
    }
}