using HexaEngine.Core.Graphics;

namespace HexaEngine.Graphics
{
    using HexaEngine.D3D11;
    using System;
    using System.Linq;

    public static class Adapter
    {
        private static readonly IAdapter[] adapters;

        static Adapter()
        {
            List<IAdapter> adapterList = new();
            if (OperatingSystem.IsWindows())
            {
                adapterList.Add(new DXGIAdapter());
            }
            adapters = adapterList.ToArray();
        }

        public static IGraphicsDevice CreateGraphics(RenderBackend backend)
        {
            var adapter = adapters.FirstOrDefault(x => x.Backend == backend) ?? throw new PlatformNotSupportedException();
            return adapter.CreateGraphics();
        }
    }
}