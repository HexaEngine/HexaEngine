namespace HexaEngine.Core.Graphics
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a view that allows read and write access to a depth-stencil buffer.
    /// </summary>
    public interface IDepthStencilView : IDeviceChild
    {
        /// <summary>
        /// Gets the description of the depth-stencil view.
        /// </summary>
        DepthStencilViewDescription Description { get; }

        /// <summary>
        /// Gets or sets the debug name for the device child.
        /// </summary>
        new string? DebugName
        { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] set; }

        /// <summary>
        /// Gets a value indicating whether the device child has been disposed.
        /// </summary>
        new bool IsDisposed { get; }

        /// <summary>
        /// Gets the native pointer associated with the object.
        /// </summary>
        new IntPtr NativePointer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    }
}