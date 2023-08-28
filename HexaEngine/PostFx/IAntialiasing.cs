#nullable disable

using HexaEngine;

namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public interface IAntialiasing
    {
        public string Name { get; }

        public bool Enabled { get; set; }

        Task InitializeAsync(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros);

        void Resize(int width, int height);

        void Draw(IGraphicsContext context);

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport);

        public void SetInput(IShaderResourceView view, ITexture2D resource);

        public void Update(IGraphicsContext context);
    }
}