namespace HexaEngine.Graphics.Batching
{
    public class Batcher<T> where T : IBatchInstance
    {
        private readonly List<T> instances = new();
        private readonly List<Batch<T>> batches = new();
        private readonly Dictionary<T, Batch<T>> map = new();

        public IReadOnlyList<T> Instances => instances;

        public IReadOnlyList<Batch<T>> Batches => batches;

        public void AddInstance(T instance)
        {
            if (instances.Contains(instance))
            {
                return;
            }

            instances.Add(instance);

            bool createNew = true;
            for (int i = 0; i < batches.Count; i++)
            {
                var batch = batches[i];

                if (batch.Mode != BatchMode.Merged && batch.CanInstantiate(instance))
                {
                    batch.AddInstance(instance);
                    map.Add(instance, batch);
                    createNew = false;
                    break;
                }
                else if (batch.Mode != BatchMode.Instanced && batch.CanMerge(instance))
                {
                    batch.AddInstance(instance);
                    map.Add(instance, batch);
                    createNew = false;
                    break;
                }
            }

            if (createNew)
            {
                Batch<T> batch = new(instance, BatchMode.None);
                batches.Add(batch);
                map.Add(instance, batch);
            }
        }

        public void RemoveInstance(T instance)
        {
            int index = instances.IndexOf(instance);
            if (index == -1)
            {
                return;
            }

            instances.RemoveAt(index);

            var batch = map[instance];
            batch.RemoveInstance(instance);

            if (batch.Count == 0)
            {
                batches.Remove(batch);
            }

            map.Remove(instance);
        }
    }
}