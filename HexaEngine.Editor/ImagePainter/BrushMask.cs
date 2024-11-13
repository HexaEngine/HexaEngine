namespace HexaEngine.Editor.ImagePainter
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using System.Numerics;

    public class BrushMask : IDisposable
    {
        private readonly Texture2D _resource;
        private readonly IShaderResourceView _shaderResourceView;
        private readonly ISamplerState _samplerState;
        private readonly ImGuiName name = new(string.Empty);
        private bool disposedValue;

        public BrushMask(IGraphicsDevice device, string path)
        {
            _resource = new(new TextureFileDescription(path), path);
            _shaderResourceView = device.CreateShaderResourceView(_resource);
            _samplerState = device.CreateSamplerState(SamplerStateDescription.PointWrap);
        }

        public unsafe BrushMask(IGraphicsDevice device, Vector4[] pixels, int width, int height)
        {
            Vector4* pData = AllocCopyT(pixels);
            *pData = Vector4.One;
            SubresourceData data = new(pData, width * sizeof(Vector4));

            _resource = new(new Texture2DDescription(Format.R8G8B8A8UNorm, width, height, 1, 1, GpuAccessFlags.Read), new SubresourceData[] { data });
            _shaderResourceView = device.CreateShaderResourceView(_resource);
            _samplerState = device.CreateSamplerState(SamplerStateDescription.PointWrap);

            Free(pData);
        }

        public string Id => name.Id;

        public void Apply(IGraphicsContext context, IResourceBindingList bindingList)
        {
            bindingList.SetSRV("brushMaskTex", _shaderResourceView);
            bindingList.SetSampler("brushMaskSamplerState", _samplerState);
        }

        public void DrawPreview(Vector2 size)
        {
            ImGui.Image((ulong)_shaderResourceView.NativePointer, size);
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}