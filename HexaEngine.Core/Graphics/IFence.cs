namespace HexaEngine.Core.Graphics
{
    using Silk.NET.Core.Attributes;

    /// <summary>
    /// Represents a fence, an object used for synchronization of the CPU and one or more GPUs.
    /// </summary>
    public interface IFence : IDeviceChild
    {
        ulong GetCompletedValue();

        unsafe void SetEventOnCompletion(ulong value, void* hEvent);
    }
}