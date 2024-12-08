namespace HexaEngine.PostFx
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using System;
    using System.ComponentModel;

    public class PostFxComposeTarget : IPostFx
    {
        private readonly IPostFx parent;

        public PostFxComposeTarget(IPostFx parent)
        {
            this.parent = parent;
            Flags = parent.Flags;
            Flags &= ~PostFxFlags.Compose;
            Flags |= PostFxFlags.ComposeTarget;
            Name = $"{parent.Name}.Compose";
            DisplayName = new(Name);
        }

        public string Name { get; }

        public PostFxFlags Flags { get; }

        public bool Initialized => parent.Initialized;

        public bool Enabled { get => parent.Enabled; set => parent.Enabled = value; }

        public PostFxColorSpace ColorSpace => parent.ColorSpace;

        public ImGuiName DisplayName { get; }

        public event Action<IPostFx, bool>? OnEnabledChanged
        {
            add => parent.OnEnabledChanged += value;
            remove => parent.OnEnabledChanged -= value;
        }

        public event Action<IPostFx>? OnReload;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Compose(IGraphicsContext context)
        {
            throw new NotSupportedException();
        }

        public void Draw(IGraphicsContext context)
        {
            parent.Compose(context);
        }

        public void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            throw new NotSupportedException();
        }

        public void Resize(int width, int height)
        {
            throw new NotSupportedException();
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            parent.SetInput(view, resource);
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            parent.SetOutput(view, resource, viewport);
        }

        public void SetupDependencies(PostFxDependencyBuilder builder)
        {
            throw new NotSupportedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }

        public void ResetSettings()
        {
            // nothing to do.
        }

        public void UpdateBindings()
        {
        }
    }
}