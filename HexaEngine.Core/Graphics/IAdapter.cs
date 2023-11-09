namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Debugging.Device;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a graphics adapter that provides information about the graphics device and GPU(s).
    /// </summary>
    public interface IGraphicsAdapter
    {
        /// <summary>
        /// Gets the graphics backend type, representing the underlying graphics API (e.g., DirectX, Vulkan, OpenGL).
        /// </summary>
        GraphicsBackend Backend { get; }

        /// <summary>
        /// Creates and returns an instance of an <see cref="IGraphicsDevice"/>.
        /// </summary>
        /// <param name="debug">A boolean indicating whether to enable debugging features.</param>
        /// <returns>An <see cref="IGraphicsDevice"/> instance.</returns>
        IGraphicsDevice CreateGraphicsDevice(bool debug);

        /// <summary>
        /// Gets the memory budget of the graphics adapter.
        /// </summary>
        /// <returns>The memory budget in bytes.</returns>
        ulong GetMemoryBudget();

        /// <summary>
        /// Gets the current memory usage of the graphics adapter.
        /// </summary>
        /// <returns>The current memory usage in bytes.</returns>
        ulong GetMemoryCurrentUsage();

        /// <summary>
        /// Gets the available memory for reservation on the graphics adapter.
        /// </summary>
        /// <returns>The available memory for reservation in bytes.</returns>
        ulong GetMemoryAvailableForReservation();

        /// <summary>
        /// Processes or logs debug messages related to the graphics adapter.
        /// </summary>
        void PumpDebugMessages();

        /// <summary>
        /// Gets a score or rating for the graphics adapter's compatibility or performance on the current platform.
        /// </summary>
        int PlatformScore { get; }

        /// <summary>
        /// Gets a list of GPU objects, representing the graphics processing units available on the graphics adapter.
        /// </summary>
        IReadOnlyList<GPU> GPUs { get; }
    }
}