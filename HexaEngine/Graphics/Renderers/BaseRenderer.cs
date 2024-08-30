namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using System.Numerics;

    public abstract class BaseRenderer<T> : IRenderer1<T>, IRenderer1
    {
        private bool disposedValue;

        void IRenderer1.Initialize(IGraphicsDevice device, CullingContext cullingContext)
        {
            Initialize(device, cullingContext);
        }

        protected abstract void Initialize(IGraphicsDevice device, CullingContext cullingContext);

        public bool CanRender(object instance)
        {
            return instance is T;
        }

        public virtual void Prepare(T instance)
        {
        }

        public abstract void Bake(IGraphicsContext context, T instance);

        public void Bake(IGraphicsContext context, object instance)
        {
            if (instance is T t)
            {
                Bake(context, t);
            }
        }

        public abstract void DrawDeferred(IGraphicsContext context, T instance);

        public void DrawDeferred(IGraphicsContext context, object instance)
        {
            if (instance is T t)
            {
                DrawDeferred(context, t);
            }
        }

        public abstract void DrawDepth(IGraphicsContext context, T instance);

        public abstract void DrawDepth(IGraphicsContext context, T instance, IBuffer camera);

        public void DrawDepth(IGraphicsContext context, object instance)
        {
            if (instance is T t)
            {
                DrawDepth(context, t);
            }
        }

        public void DrawDepth(IGraphicsContext context, object instance, IBuffer camera)
        {
            if (instance is T t)
            {
                DrawDepth(context, t, camera);
            }
        }

        public abstract void DrawForward(IGraphicsContext context, T instance);

        public void DrawForward(IGraphicsContext context, object instance)
        {
            if (instance is T t)
            {
                DrawForward(context, t);
            }
        }

        public abstract void DrawShadowMap(IGraphicsContext context, T instance, IBuffer light, ShadowType type);

        public void DrawShadowMap(IGraphicsContext context, object instance, IBuffer light, ShadowType type)
        {
            if (instance is T t)
            {
                DrawShadowMap(context, t, light, type);
            }
        }

        public abstract void Update(IGraphicsContext context, Matrix4x4 transform, T instance);

        public void Update(IGraphicsContext context, Matrix4x4 transform, object instance)
        {
            if (instance is T t)
            {
                Update(context, transform, t);
            }
        }

        public abstract void VisibilityTest(CullingContext context, T instance);

        public void VisibilityTest(CullingContext context, object instance)
        {
            if (instance is T t)
            {
                VisibilityTest(context, t);
            }
        }

        protected abstract void DisposeCore();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
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