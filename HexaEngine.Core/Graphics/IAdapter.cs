namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Debugging.Device;
    using System.Collections.Generic;

    public interface IGraphicsAdapter
    {
        GraphicsBackend Backend { get; }

        IGraphicsDevice CreateGraphicsDevice(bool debug);

        ulong GetMemoryBudget();

        ulong GetMemoryCurrentUsage();

        ulong GetMemoryAvailableForReservation();

        void PumpDebugMessages();

        int PlatformScore { get; }
        IReadOnlyList<GPU> GPUs { get; }
    }
}