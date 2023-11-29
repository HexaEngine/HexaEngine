namespace HexaEngine.Graphics.Batching
{
    using HexaEngine.Core.Scenes;

    public class Batcher<T> : IBatcher where T : IBatchRenderer
    {
        private readonly List<Batch<T>> batches = new();

        private Batch<T> GetOrCreateBatch(T renderer)
        {
            Batch<T>? batch = null;
            for (int i = 0; i < batches.Count; i++)
            {
                batch = batches[i];
                if (batch.CanBatch(renderer))
                {
                    return batch;
                }
            }

            if (batch == null)
            {
                batch = new(renderer);
                batches.Add(batch);
            }

            return batch;
        }

        private (Batch<T>, int)? FindBatch(GameObject parent, T renderer)
        {
            for (int i = 0; i < batches.Count; i++)
            {
                var batch = batches[i];
                var idx = batch.IndexOf(parent, renderer);
                if (idx != -1)
                {
                    return (batch, idx);
                }
            }

            return null;
        }

        public bool TryBatch(GameObject gameObject, IBatchRenderer renderer)
        {
            if (renderer is not T t)
            {
                return false;
            }

            var batch = GetOrCreateBatch(t);
            batch.AddObject(gameObject, t);
            batch.Sort();

            return true;
        }

        public bool TryRemove(GameObject gameObject, IBatchRenderer renderer)
        {
            if (renderer is not T t)
            {
                return false;
            }

            var result = FindBatch(gameObject, t);
            if (result.HasValue)
            {
                (Batch<T> batch, int idx) = result.Value;
                batch.RemoveAt(idx);
                return true;
            }
            return false;
        }
    }
}