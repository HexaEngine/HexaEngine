﻿namespace HexaEngine.Editor.ImagePainter
{
    using HexaEngine.Core.Graphics;
    using Hexa.NET.ImGui;
    using System.Numerics;
    using HexaEngine.Core.UI;

    public class BrushMask : IDisposable
    {
        private readonly Texture2D _resource;
        private readonly IShaderResourceView _shaderResourceView;
        private readonly ISamplerState _samplerState;
        private readonly ImGuiName name = new(string.Empty);
        private bool disposedValue;

        public BrushMask(IGraphicsDevice device, string path)
        {
            _resource = new(device, new TextureFileDescription(path), path);
            _shaderResourceView = device.CreateShaderResourceView(_resource);
            _samplerState = device.CreateSamplerState(SamplerStateDescription.PointWrap);
        }

        public unsafe BrushMask(IGraphicsDevice device, Vector4[] pixels, int width, int height)
        {
            Vector4* pData = AllocCopyT(pixels);
            *pData = Vector4.One;
            SubresourceData data = new(pData, width * sizeof(Vector4));

            _resource = new(device, new Texture2DDescription(Format.R8G8B8A8UNorm, width, height, 1, 1, BindFlags.ShaderResource, Usage.Immutable), new SubresourceData[] { data });
            _shaderResourceView = device.CreateShaderResourceView(_resource);
            _samplerState = device.CreateSamplerState(SamplerStateDescription.PointWrap);

            Free(pData);
        }

        public string Id => name.Id;

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