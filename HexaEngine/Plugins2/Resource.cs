namespace HexaEngine.Plugins2
{
    public abstract class Resource
    {
        protected Resource(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract ResourceType Type { get; }

        public abstract int Read(ReadOnlySpan<byte> source);

        public abstract int Write(Span<byte> source);

        public abstract int Size();
    }
}