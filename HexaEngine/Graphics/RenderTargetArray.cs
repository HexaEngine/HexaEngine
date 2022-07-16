namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class RenderTargetViewArray : IDisposable
    {
        public Vector4 ClearColor;
        private bool disposedValue;
        private IRenderTargetView[] views;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RenderTargetViewArray(IRenderTargetView[] views)
        {
            this.views = views;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RenderTargetViewArray(IGraphicsDevice device, IResource[] resources, Viewport viewport)
        {
            views = new IRenderTargetView[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                views[i] = device.CreateRenderTargetView(resources[i], viewport);
                views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RenderTargetViewArray(IGraphicsDevice device, IResource resource, int arraySize, Viewport viewport)
        {
            Viewport = viewport;
            views = new IRenderTargetView[arraySize];
            RenderTargetViewDescription description = new(RenderTargetViewDimension.Texture2DArray, arraySize: 1);
            for (int i = 0; i < arraySize; i++)
            {
                description.Texture2DArray.FirstArraySlice = i;
                views[i] = device.CreateRenderTargetView(resource, description, viewport);
                views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        public Viewport Viewport { get; }

        public IDepthStencilView? DepthStencil { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTargets(IGraphicsContext context)
        {
            context.SetRenderTargets(views, DepthStencil);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearTargets(IGraphicsContext context)
        {
            foreach (IRenderTargetView view in views)
                context.ClearRenderTargetView(view, new(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAndSetTargets(IGraphicsContext context)
        {
            ClearTargets(context);
            SetTargets(context);
        }

        public void ClearAndSetTarget(IGraphicsContext context, int i)
        {
            context.ClearRenderTargetView(views[i], new(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W));
            context.SetRenderTarget(views[i], DepthStencil);
        }

        public void ClearTarget(IGraphicsContext context, int i)
        {
            context.ClearRenderTargetView(views[i], new(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W));
        }

        public void SetTarget(IGraphicsContext context, int i)
        {
            context.SetRenderTarget(views[i], DepthStencil);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (IRenderTargetView view in views)
                    view.Dispose();

                disposedValue = true;
            }
        }

        ~RenderTargetViewArray()
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