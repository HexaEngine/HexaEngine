namespace HexaEngine.Graphics.Batching
{
    public interface IBatch
    {
        IBatchInstance this[int index] { get; set; }

        public int Count { get; }

        public BatchMode Mode { get; }

        void AddInstance(IBatchInstance instance);

        void RemoveInstance(IBatchInstance instance);
    }

    public interface IBatch<T> : IBatch, IEnumerable<T> where T : IBatchInstance
    {
        new T this[int index] { get; set; }

        void AddInstance(T instance);

        void RemoveInstance(T instance);
    }
}