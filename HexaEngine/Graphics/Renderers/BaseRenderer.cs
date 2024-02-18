namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;

    public abstract class BaseRenderer<T> : IRenderer1<T> where T : IRendererInstance
    {
        protected readonly RendererInstanceQueueCollection<T> queueCollection = new();
        private bool disposedValue;

        public void Clear()
        {
            queueCollection.Clear();
        }

        public void AddInstance(T instance)
        {
            queueCollection.AddInstance(instance);
        }

        public bool ContainsInstance(T instance)
        {
            return queueCollection.ContainsInstance(instance);
        }

        public bool RemoveInstance(T instance)
        {
            return queueCollection.RemoveInstance(instance);
        }

        public bool AddInstance(IRendererInstance instance)
        {
            if (instance is T t)
            {
                AddInstance(t);
                return true;
            }
            return false;
        }

        public bool RemoveInstance(IRendererInstance instance)
        {
            if (instance is T t)
            {
                return RemoveInstance(t);
            }
            return false;
        }

        public bool ContainsInstance(IRendererInstance instance)
        {
            if (instance is T t)
            {
                return ContainsInstance(t);
            }
            return false;
        }

        public abstract void DrawDeferred(IGraphicsContext context);

        public abstract void DrawDepth(IGraphicsContext context);

        public abstract void DrawForward(IGraphicsContext context);

        public abstract void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type);

        public abstract void Update(IGraphicsContext context);

        public abstract void VisibilityTest(CullingContext context);

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Clear();
                DisposeCore();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}