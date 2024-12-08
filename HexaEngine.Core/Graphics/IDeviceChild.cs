namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Utilities;
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

    public static class IDeviceChildExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe T* GetAs<T>(this IDeviceChild? child) where T : unmanaged
        {
            if (child == null)
            {
                return null;
            }

            return (T*)child.NativePointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe nint GetPointer(this IDeviceChild? child)
        {
            if (child == null)
            {
                return 0;
            }

            return child.NativePointer;
        }
    }
}