namespace HexaEngine.Network
{
    public interface INetworkEntity
    {
        void Read(ReadOnlySpan<byte> data);

        int Write(Span<byte> data);

        int SizeOf();
    }

    public class NetworkEntityType
    {
        protected NetworkEntityType(int identifier, Type type)
        {
            Identifier = identifier;
            Type = type;
        }

        public int Identifier { get; set; }

        public Type Type { get; set; }
    }

    public class NetworkEntityType<T> : NetworkEntityType where T : INetworkEntity
    {
        protected NetworkEntityType(int identifier, Type type) : base(identifier, type)
        {
        }

        private static int id;

        public static NetworkEntityType<T> Register()
        {
            var entityId = Interlocked.Increment(ref id);
            NetworkEntityType<T> entity = new(entityId, typeof(T));
            return entity;
        }
    }
}