namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Batching;
    using HexaEngine.Lights;
    using HexaEngine.Scenes;

    public abstract class BaseBatchRenderer<T> : IBatchRenderer<T> where T : IBatchInstance
    {
        private bool disposedValue;

        public void DrawDeferred(IGraphicsContext context, IBatch batch)
        {
            if (batch is IBatch<MeshBatchInstance> t)
            {
                DrawDeferred(context, t);
            }
        }

        public abstract void DrawDeferred(IGraphicsContext context, IBatch<T> batch);

        public void DrawDepth(IGraphicsContext context, IBatch batch)
        {
            if (batch is IBatch<MeshBatchInstance> t)
            {
                DrawDepth(context, t);
            }
        }

        public void DrawDepth(IGraphicsContext context, IBatch batch, IBuffer camera)
        {
            if (batch is IBatch<MeshBatchInstance> t)
            {
                DrawDepth(context, t, camera);
            }
        }

        public abstract void DrawDepth(IGraphicsContext context, IBatch<T> batch);

        public abstract void DrawDepth(IGraphicsContext context, IBatch<T> batch, IBuffer camera);

        public void DrawForward(IGraphicsContext context, IBatch batch)
        {
            if (batch is IBatch<MeshBatchInstance> t)
            {
                DrawForward(context, t);
            }
        }

        public abstract void DrawForward(IGraphicsContext context, IBatch<T> batch);

        public void DrawShadowMap(IGraphicsContext context, IBatch batch, IBuffer light, ShadowType type)
        {
            if (batch is IBatch<MeshBatchInstance> t)
            {
                DrawShadowMap(context, t, light, type);
            }
        }

        public abstract void DrawShadowMap(IGraphicsContext context, IBatch<T> batch, IBuffer light, ShadowType type);

        public abstract void BeginUpdate(IGraphicsContext context);

        public void Update(IGraphicsContext context, IBatch batch)
        {
            if (batch is IBatch<MeshBatchInstance> t)
            {
                Update(context, t);
            }
        }

        public abstract void EndUpdate(IGraphicsContext context);

        public abstract void Update(IGraphicsContext context, IBatch<T> batch);

        public void VisibilityTest(IGraphicsContext context, IBatch batch, Camera camera)
        {
            if (batch is IBatch<MeshBatchInstance> t)
            {
                VisibilityTest(context, t, camera);
            }
        }

        public abstract void VisibilityTest(IGraphicsContext context, IBatch<T> batch, Camera camera);

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeCore();
                disposedValue = true;
            }
        }

        ~BaseBatchRenderer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}