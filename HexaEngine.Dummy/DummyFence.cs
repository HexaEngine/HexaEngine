namespace HexaEngine.Dummy
{
    using HexaEngine.Core.Graphics;

    public class DummyFence : DummyObject, IFence
    {
        private ulong initialValue;
        private FenceFlags flags;

        public DummyFence(ulong initialValue, FenceFlags flags)
        {
            this.initialValue = initialValue;
            this.flags = flags;
        }

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