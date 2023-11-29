namespace HexaEngine.Graphics.Batching
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;

    public class Batch<T> : IBatch<T>, IBatch where T : IBatchRenderer
    {
        private readonly List<BatchRendererPair<T>> objects = new();
        private readonly IBatchRenderer renderer;

        public Batch(IBatchRenderer renderer)
        {
            this.renderer = renderer;
        }

        public IReadOnlyList<BatchRendererPair<T>> Objects => objects;

        public int Count => objects.Count;

        IEnumerable<BatchRendererPair> IBatch.Objects => objects.Select(x => (BatchRendererPair)x);

        BatchRendererPair IBatch.this[int index] { get => objects[index]; }

        public BatchRendererPair<T> this[int index]
        {
            get => objects[index];
        }

        public void AddObject(GameObject parent, T t)
        {
            objects.Add(new(parent, t));
        }

        public void RemoveObject(GameObject parent, T t)
        {
            objects.Remove(new(parent, t));
        }

        public void RemoveAt(int index)
        {
            objects.RemoveAt(index);
        }

        public void Clear()
        {
            objects.Clear();
        }

        public bool Contains(GameObject parent, T t)
        {
            return objects.Contains(new(parent, t));
        }

        public int IndexOf(GameObject parent, T t)
        {
            return objects.IndexOf(new(parent, t));
        }

        public void Sort()
        {
            objects.Sort();
        }

        public bool CanBatch(IBatchRenderer renderer)
        {
            return this.renderer.Equals(renderer);
        }

        public void DrawDeferred(IGraphicsContext context)
        {
            renderer.DrawDeferred(context, this);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            renderer.DrawDepth(context, this);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer camera)
        {
            renderer.DrawDepth(context, this, camera);
        }

        public void DrawForward(IGraphicsContext context)
        {
            renderer.DrawForward(context, this);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            renderer.DrawShadowMap(context, this, light, type);
        }

        public void Update(IGraphicsContext context)
        {
            renderer.Update(context, this);
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
            renderer.VisibilityTest(context, this, camera);
        }

        public void AddObject(GameObject parent, IBatchRenderer t)
        {
            if (t is T renderer)
            {
                AddObject(parent, renderer);
            }
        }

        public bool Contains(GameObject parent, IBatchRenderer t)
        {
            if (t is T renderer)
                return Contains(parent, renderer);
            return false;
        }

        public int IndexOf(GameObject parent, IBatchRenderer t)
        {
            if (t is T renderer)
                return IndexOf(parent, renderer);
            return -1;
        }

        public void RemoveObject(GameObject parent, IBatchRenderer t)
        {
            if (t is T renderer)
            {
                RemoveObject(parent, renderer);
            }
        }
    }
}