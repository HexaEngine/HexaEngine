namespace HexaEngine.Core.Graphics
{
    public interface IGraphicsAdapter
    {
        GraphicsBackend Backend { get; }

        IGraphicsDevice CreateGraphicsDevice(bool debug);
        ulong GetMemoryBudget();
        ulong GetMemoryCurrentUsage();
        ulong GetMemoryAvailableForReservation();

        int PlatformScore { get; }
    }
}