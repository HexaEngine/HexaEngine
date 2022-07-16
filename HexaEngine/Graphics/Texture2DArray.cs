namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class RenderTextureArray : IDisposable
    {
        private readonly List<ShaderBinding> bindings = new();
        private ITexture2D[] textures;
        private IShaderResourceView[] resourceViews;
        private bool disposedValue;

        public int Width { get; }

        public int Height { get; }

        public Viewport Viewport => new(Width, Height);

        public int Count { get; }

        public RenderTargetViewArray RenderTargets { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderTextureArray(IGraphicsDevice device, int width, int height, int count = 1, Format format = Format.RGBA32Float)
        {
            Count = count;
            Width = width;
            Height = height;
            textures = new ITexture2D[count];
            resourceViews = new IShaderResourceView[count];
            for (int i = 0; i < count; i++)
            {
                ITexture2D texture;
                IShaderResourceView resourceView;

                Texture2DDescription textureDesc = new()
                {
                    Width = Width,
                    Height = Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CPUAccessFlags = CpuAccessFlags.None,
                    MiscFlags = ResourceMiscFlag.None
                };

                texture = device.CreateTexture2D(textureDesc);

                ShaderResourceViewDescription srvDesc = new()
                {
                    Format = texture.Description.Format,
                    ViewDimension = ShaderResourceViewDimension.Texture2D,
                };

                srvDesc.Texture2D.MipLevels = 1;
                srvDesc.Texture2D.MostDetailedMip = 0;

                resourceView = device.CreateShaderResourceView(texture, srvDesc);

                textures[i] = texture;
                resourceViews[i] = resourceView;
            }

            RenderTargets = new(device, textures, new(width, height));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ShaderBinding binding)
        {
            bindings.Add(binding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(IGraphicsContext context)
        {
            foreach (ShaderBinding binding in bindings)
            {
                for (int i = 0; i < resourceViews.Length; i++)
                {
                    context.SetShaderResource(resourceViews[i], binding.Stage, binding.Slot + i);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Bind(IGraphicsContext context, int index)
        {
            context.SetShaderResource(resourceViews[index], bindings[index].Stage, bindings[index].Slot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ShaderBinding binding)
        {
            bindings.Remove(binding);
        }

        public IShaderResourceView GetResourceView(int index)
        {
            return resourceViews[index];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                foreach (ITexture2D texture in textures)
                    texture.Dispose();
                foreach (IShaderResourceView view in resourceViews)
                    view.Dispose();
                RenderTargets.Dispose();

                disposedValue = true;
            }
        }

        ~RenderTextureArray()
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