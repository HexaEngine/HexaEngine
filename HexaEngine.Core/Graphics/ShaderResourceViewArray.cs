namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents an array of shader resource views.
    /// </summary>
    [Obsolete("Legacy component, do not use")]
    public class ShaderResourceViewArray : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the array of shader resource views.
        /// </summary>
        public readonly IShaderResourceView[] Views;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewArray"/> class with existing shader resource views.
        /// </summary>
        /// <param name="views">An array of existing shader resource views.</param>
        public ShaderResourceViewArray(IShaderResourceView[] views)
        {
            Views = views;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewArray"/> class with resources.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="resources">An array of resources to create shader resource views for.</param>
        public ShaderResourceViewArray(IGraphicsDevice device, IResource[] resources)
        {
            Views = new IShaderResourceView[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                Views[i] = device.CreateShaderResourceView(resources[i]);
                Views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderResourceViewArray"/> class with a single resource and array size.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="resource">The resource to create shader resource views for.</param>
        /// <param name="arraySize">The array size.</param>
        public ShaderResourceViewArray(IGraphicsDevice device, IResource resource, int arraySize)
        {
            Views = new IShaderResourceView[arraySize];
            ShaderResourceViewDescription description = new(ShaderResourceViewDimension.Texture2DArray, arraySize: 1);
            for (int i = 0; i < arraySize; i++)
            {
                description.Texture2DArray.FirstArraySlice = i;
                Views[i] = device.CreateShaderResourceView(resource, description);
                Views[i].DebugName = nameof(RenderTargetViewArray) + "." + i;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="ShaderResourceViewArray"/>.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (IShaderResourceView view in Views)
                {
                    view.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases the resources used by the <see cref="ShaderResourceViewArray"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}