using Hexa.NET.Daxa;
using HexaEngine.Core.Debugging.Device;
using HexaEngine.Core.Graphics;

namespace HexaEngine.Daxa
{
    public class DaxaAdapter : IGraphicsAdapter, IDisposable
    {
        private readonly DaxaInstance instance;

        public DaxaAdapter()
        {
            DaxaInstanceInfo info = new(0, "HexaEngine", "HexaEngine");
            DaxaCreateInstance(ref info, ref instance).CheckError();
        }

        public GraphicsBackend Backend { get; } = GraphicsBackend.Vulkan;

        public int PlatformScore { get; }

        public IReadOnlyList<GPU> GPUs { get; }

        public int AdapterIndex { get; }

        public IGraphicsDevice CreateGraphicsDevice(bool debug)
        {
            return new DaxaGraphicsDevice(instance, debug);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            instance.DecRefcnt();
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
        }
    }
}