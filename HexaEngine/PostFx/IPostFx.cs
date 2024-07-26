namespace HexaEngine.PostFx
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Graphics.Graph;
    using Hexa.NET.Mathematics;
    using System.ComponentModel;

    /// <summary>
    /// Represents a post-processing effect in a graphics pipeline.
    /// </summary>
    public interface IPostFx : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the name of the post-processing effect.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the display name of the post-processing effect.
        /// </summary>
        public ImGuiName DisplayName { get; }

        /// <summary>
        /// Gets the flags associated with the post-processing effect.
        /// </summary>
        public PostFxFlags Flags { get; }

        /// <summary>
        /// Gets the color space of the post-processing effect. (eg. SDR or HDR)
        /// </summary>
        public PostFxColorSpace ColorSpace { get; }

        /// <summary>
        /// Gets a value indicating whether the post-processing effect has been initialized.
        /// </summary>
        public bool Initialized { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the post-processing effect is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Occurs when the <see cref="Enabled"/> property changes.
        /// </summary>
        public event Action<IPostFx, bool>? OnEnabledChanged;

        /// <summary>
        /// Occurs when the post-processing effect needs to be reloaded.
        /// </summary>
        public event Action<IPostFx>? OnReload;

        /// <summary>
        /// Initializes the post-processing effect.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="creator">The resource builder for creating resources.</param>
        /// <param name="width">The width of the rendering area.</param>
        /// <param name="height">The height of the rendering area.</param>
        /// <param name="macros">The shader macros to use during initialization.</param>
        void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros);

        /// <summary>
        /// Sets up dependencies for the post-processing effect.
        /// </summary>
        /// <param name="builder">The dependency builder.</param>
        void SetupDependencies(PostFxDependencyBuilder builder);

        /// <summary>
        /// Resizes the post-processing effect to match the new dimensions.
        /// </summary>
        /// <param name="width">The new width of the rendering area.</param>
        /// <param name="height">The new height of the rendering area.</param>
        void Resize(int width, int height);

        /// <summary>
        /// Draws the post-processing effect.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        ///
        void Draw(IGraphicsContext context);

        /// <summary>
        /// Composes the post-processing effect in a later stage if configured.
        /// </summary>
        /// <param name="context"></param>
        void Compose(IGraphicsContext context);

        /// <summary>
        /// Draws the post-processing effect during the pre-pass stage.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        /// <param name="creator">The resource builder for creating resources.</param>
        void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
        {
        }

        /// <summary>
        /// Sets the output target for the post-processing effect.
        /// </summary>
        /// <param name="view">The render target view.</param>
        /// <param name="resource">The texture resource.</param>
        /// <param name="viewport">The viewport dimensions.</param>
        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport);

        /// <summary>
        /// Sets the input for the post-processing effect.
        /// </summary>
        /// <param name="view">The shader resource view.</param>
        /// <param name="resource">The texture resource.</param>
        public void SetInput(IShaderResourceView view, ITexture2D resource);

        /// <summary>
        /// Updates the post-processing effect.
        /// </summary>
        /// <param name="context">The graphics context.</param>
        public void Update(IGraphicsContext context);

        public void ResetSettings();
    }
}