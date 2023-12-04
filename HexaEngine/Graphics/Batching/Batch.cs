namespace HexaEngine.Graphics.Batching
{
    using System.Collections;

    public class Batch<T> : IBatch<T> where T : IBatchInstance
    {
        private readonly List<T> instances = new();
        private readonly BatchMode mode;

        public Batch(T instance, BatchMode mode)
        {
            this.mode = mode;
            instances.Add(instance);
        }

        public BatchMode Mode => mode;

        public T this[int index]
        {
            get => instances[index];
            set => instances[index] = value;
        }

        public int Count => instances.Count;

        IBatchInstance IBatch.this[int index] { get => instances[index]; set => instances[index] = (T)value; }

        public void AddInstance(T instance)
        {
        }

        public void RemoveInstance(T instance)
        {
        }

        public void AddInstance(IBatchInstance instance)
        {
            if (instance is T t)
            {
                AddInstance(t);
            }
        }

        public void RemoveInstance(IBatchInstance instance)
        {
            if (instance is T t)
            {
                RemoveInstance(t);
            }
        }

        public bool CanInstantiate(T instance)
        {
            return instances[0].CanInstantiate(this, instance);
        }

        public bool CanMerge(T instance)
        {
            return instances[0].CanMerge(this, instance);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return instances.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return instances.GetEnumerator();
        }
    }
}