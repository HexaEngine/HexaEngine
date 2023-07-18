namespace HexaEngine.Core.Unsafes
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents an interface for objects that wrap native resources.
    /// </summary>
    public interface INative : IDisposable
    {
        /// <summary>
        /// Gets the native pointer associated with the object.
        /// </summary>
        IntPtr NativePointer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    }
}