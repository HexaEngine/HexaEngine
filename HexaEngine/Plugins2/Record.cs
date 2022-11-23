namespace HexaEngine.Plugins2
{
    public abstract class Record
    {
        protected Record(string? name)
        {
            Name = name;
        }

        public abstract RecordType Type { get; }

        public string? Name { get; }

        public abstract int Read(ReadOnlySpan<byte> source);

        public abstract int Write(Span<byte> source);

        public abstract int Size();
    }
}