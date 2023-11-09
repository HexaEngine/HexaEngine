namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Linq;

    /// <summary>
    /// Provides functionality for managing graphics adapters and creating graphics devices.
    /// </summary>
    public static class GraphicsAdapter
    {
        /// <summary>
        /// Gets a list of available graphics adapters.
        /// </summary>
        public static List<IGraphicsAdapter> Adapters { get; } = new();

#nullable disable

        /// <summary>
        /// Gets or sets the current graphics adapter in use.
        /// </summary>
        public static IGraphicsAdapter Current { get; private set; }

#nullable restore

        /// <summary>
        /// Creates a graphics device using the specified graphics backend.
        /// </summary>
        /// <param name="backend">The graphics backend to use.</param>
        /// <param name="debug">Indicates whether debug mode should be enabled.</param>
        /// <returns>A new instance of an <see cref="IGraphicsDevice"/>.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown when the specified backend is not supported by any available graphics adapter.</exception>
        public static IGraphicsDevice CreateGraphicsDevice(GraphicsBackend backend, bool debug)
        {
            if (backend == GraphicsBackend.Auto)
            {
                if (Adapters.Count == 1)
                {
                    Current = Adapters[0];
                    return Adapters[0].CreateGraphicsDevice(debug);
                }
                else
                {
                    IGraphicsAdapter graphicsAdapter = Adapters[0];
                    for (int i = 0; i < Adapters.Count; i++)
                    {
                        if (Adapters[i].PlatformScore > graphicsAdapter.PlatformScore)
                        {
                            graphicsAdapter = Adapters[i];
                        }
                    }
                    Current = graphicsAdapter;
                    return graphicsAdapter.CreateGraphicsDevice(debug);
                }
            }
            var adapter = Adapters.FirstOrDefault(x => x.Backend == backend) ?? throw new PlatformNotSupportedException();
            Current = adapter;
            return adapter.CreateGraphicsDevice(debug);
        }
    }
}