namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Debugging.Device;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using Silk.NET.Core.Contexts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class OpenGLAdapter : IGraphicsAdapter
    {
        private readonly IWindow source;
        private bool debug;

        public OpenGLAdapter(IWindow source, bool debug)
        {
            this.source = source;
            this.debug = debug;
        }

        public GraphicsBackend Backend => GraphicsBackend.OpenGL;

        public int PlatformScore => 100;

        public IReadOnlyList<GPU> GPUs { get; }

        public static void Init(IWindow source, bool debug)
        {
            GraphicsAdapter.Adapters.Add(new OpenGLAdapter(source, debug));
        }

        public IGraphicsDevice CreateGraphicsDevice(bool debug)
        {
            return new OpenGLGraphicsDevice(source, debug);
        }

        public ulong GetMemoryAvailableForReservation()
        {
            return 0;
        }

        public ulong GetMemoryBudget()
        {
            return 0;
        }

        public ulong GetMemoryCurrentUsage()
        {
            return 0;
        }

        public void PumpDebugMessages()
        {
            return;
        }
    }
}