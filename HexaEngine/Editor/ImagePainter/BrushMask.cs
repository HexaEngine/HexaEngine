namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using ImGuiNET;
    using System.Numerics;

    public class BrushMask : IDisposable
    {
        private readonly IResource _resource;
        private readonly IShaderResourceView _shaderResourceView;
        private readonly ISamplerState _samplerState;
        private bool disposedValue;

        public BrushMask(IGraphicsDevice device, string path)
        {
            var image = device.TextureLoader.LoadFormAssets(path);
            _resource = image.CreateTexture2D(device, Usage.Immutable, BindFlags.ShaderResource, CpuAccessFlags.None, ResourceMiscFlag.None);
            image.Dispose();
            _shaderResourceView = device.CreateShaderResourceView(_resource);
            _samplerState = device.CreateSamplerState(SamplerDescription.PointWrap);
        }

        public unsafe BrushMask(IGraphicsDevice device, Vector4[] pixels, int width, int height)
        {
            Vector4* pData = AllocCopy(pixels);
            *pData = Vector4.One;
            SubresourceData data = new(pData, width * sizeof(Vector4));

            _resource = device.CreateTexture2D(Format.R32G32B32A32Float, width, height, 1, 1, new SubresourceData[] { data }, BindFlags.ShaderResource);
            _shaderResourceView = device.CreateShaderResourceView(_resource);
            _samplerState = device.CreateSamplerState(SamplerDescription.PointWrap);

            Free(pData);
        }

        public void Apply(IGraphicsContext context)
        {
            context.PSSetShaderResource(0, _shaderResourceView);
            context.PSSetSampler(0, _samplerState);
        }

        public void DrawPreview(Vector2 size)
        {
            ImGui.Image(_shaderResourceView.NativePointer, size);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _resource.Dispose();
                _shaderResourceView.Dispose();
                _samplerState.Dispose();
                disposedValue = true;
            }
        }

        ~BrushMask()
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