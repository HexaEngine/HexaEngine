namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Linq;

    public static class GraphicsAdapter
    {
        public static List<IGraphicsAdapter> Adapters { get; } = new();

        public static IGraphicsAdapter Current { get; private set; }

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