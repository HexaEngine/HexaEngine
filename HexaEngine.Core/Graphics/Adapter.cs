using HexaEngine.Core.Graphics;

namespace HexaEngine.Graphics
{
    using System;
    using System.Linq;

    public static class Adapter
    {
        public static List<IAdapter> Adapters { get; } = new();

        public static IGraphicsDevice CreateGraphics(RenderBackend backend, bool debug)
        {
            var adapter = Adapters.FirstOrDefault(x => x.Backend == backend) ?? throw new PlatformNotSupportedException();
            return adapter.CreateGraphics(debug);
        }
    }
}