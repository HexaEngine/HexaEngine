namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Unsafes;

    public struct Scope
    {
        public uint Id;
        public byte Level;
        public ulong Start;
        public ulong End;
        public bool Finalized = false;
        public bool Used;
        public StdString Name;
        public UnsafeRingBuffer<float> StartSamples;
        public UnsafeRingBuffer<float> EndSamples;
        public double Duration;

        public Scope(uint id, string name, int sampleCount = 1000)
        {
            Id = id;
            Name = name;
            StartSamples = new(sampleCount);
            EndSamples = new(sampleCount);
        }
    }
}