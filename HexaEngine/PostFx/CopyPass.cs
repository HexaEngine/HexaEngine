namespace HexaEngine.PostFx
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using System;
    using System.ComponentModel;

    public sealed class CopyPass : IPostFx
    {
        private IResource InputResource = null!;
        private IResource OutputResource = null!;

        public string Name { get; } = "Copy";

        public PostFxFlags Flags { get; } = PostFxFlags.None;

        public PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.None;

        public ImGuiName DisplayName { get; }

        public bool Initialized { get; } = true;

        public bool Enabled { get; set; } = true;

        public event Action<IPostFx, bool>? OnEnabledChanged;

        public event Action<IPostFx>? OnReload;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
        }

        public void SetupDependencies(PostFxDependencyBuilder builder)
        {
        }

        public void UpdateBindings()
        {
        }

        public void Draw(IGraphicsContext context)
        {
            context.CopyResource(OutputResource, InputResource);
        }

        public void Resize(int width, int height)
        {
        }

        public void Compose(IGraphicsContext context)
        {
            throw new NotSupportedException();
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            OutputResource = resource;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            InputResource = resource;
        }

        public void Update(IGraphicsContext context)
        {
        }

        public void Dispose()
        {
            // nothing to dispose.
        }

        public void ResetSettings()
        {
            // nothing to do.
        }
    }
}