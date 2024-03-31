namespace HexaEngine.Dummy
{
    using HexaEngine.Core.Graphics;

    public class DummyFence : DummyObject, IFence
    {
        public ulong GetCompletedValue()
        {
            throw new NotImplementedException();
        }

        public unsafe void SetEventOnCompletion(ulong value, void* hEvent)
        {
            throw new NotImplementedException();
        }
    }
}