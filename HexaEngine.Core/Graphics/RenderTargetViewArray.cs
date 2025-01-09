﻿namespace HexaEngine.Core.Graphics
{
    using Hexa.NET.Mathematics;
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents an array of render target views.
    /// </summary>
    [Obsolete("Legacy component, do not use")]
    public class RenderTargetViewArray : IDisposable
    {
        /// <summary>
        /// Gets or sets the clear color for the render target views.
        /// </summary>
        public Vector4 ClearColor;

        private bool disposedValue;
        private IRenderTargetView[] views;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewArray"/> class with existing render target views.
        /// </summary>
        /// <param name="views">An array of existing render target views.</param>
        public RenderTargetViewArray(IRenderTargetView[] views)
        {
            this.views = views;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewArray"/> class with resources and a viewport.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="resources">An array of resources to create render target views for.</param>
        /// <param name="viewport">The viewport associated with the render target views.</param>
        public RenderTargetViewArray(IGraphicsDevice device, IResource[] resources, Viewport viewport)
        {
            views = new IRenderTargetView[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                views[i] = device.CreateRenderTargetView(resources[i]);
                views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetViewArray"/> class with a single resource, array size, and viewport.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="resource">The resource to create render target views for.</param>
        /// <param name="arraySize">The array size.</param>
        /// <param name="viewport">The viewport associated with the render target views.</param>
        public RenderTargetViewArray(IGraphicsDevice device, IResource resource, int arraySize, Viewport viewport)
        {
            Viewport = viewport;
            views = new IRenderTargetView[arraySize];
            RenderTargetViewDescription description = new(RenderTargetViewDimension.Texture2DArray, arraySize: 1);
            for (int i = 0; i < arraySize; i++)
            {
                description.Texture2DArray.FirstArraySlice = i;
                views[i] = device.CreateRenderTargetView(resource, description);
                views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        /// <summary>
        /// Gets the viewport associated with the render target views.
        /// </summary>
        public Viewport Viewport { get; }

        /// <summary>
        /// Gets or sets the depth stencil view associated with the render target views.
        /// </summary>
        public IDepthStencilView? DepthStencil { get; set; }

        /// <summary>
        /// Clears and sets the specified render target view as the target in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="i">The index of the render target view.</param>
        public void ClearAndSetTarget(IGraphicsContext context, int i)
        {
            context.ClearRenderTargetView(views[i], new(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W));
            context.SetRenderTarget(views[i], DepthStencil);
        }

        /// <summary>
        /// Clears the specified render target view in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="i">The index of the render target view.</param>
        public void ClearTarget(IGraphicsContext context, int i)
        {
            context.ClearRenderTargetView(views[i], new(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W));
        }

        /// <summary>
        /// Sets the specified render target view as the target in the graphics context.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="i">The index of the render target view.</param>
        public void SetTarget(IGraphicsContext context, int i)
        {
            context.SetRenderTarget(views[i], DepthStencil);
        }

        /// <summary>
        /// Releases the resources used by the <see cref="RenderTargetViewArray"/>.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (IRenderTargetView view in views)
                {
                    view.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="RenderTargetViewArray"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}