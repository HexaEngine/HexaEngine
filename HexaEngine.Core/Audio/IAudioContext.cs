namespace HexaEngine.Core.Audio
{
    public interface IAudioContext : IDisposable
    {
        public nint NativePointer { get; }
    }
}