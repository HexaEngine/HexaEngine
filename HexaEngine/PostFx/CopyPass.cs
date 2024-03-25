namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Mathematics;
    using System;
    using System.ComponentModel;

    public sealed class CopyPass : IPostFx
    {
        private IShaderResourceView Input;
        private IResource InputResource;
        private IRenderTargetView Output;
        private IResource OutputResource;
        private Viewport Viewport;

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
            Output = view;
            OutputResource = resource;
            Viewport = viewport;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            InputResource = resource;
        }

        public void Update(IGraphicsContext context)
        {
        }

        public void Dispose()
        {
            // nothing to dispose.
        }
    }
}