namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A base class for implementing post-processing effects.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public abstract class PostFxBase : IPostFx
    {
        private bool initialized = false;
        private bool enabled = false;

        /// <summary>
        /// Indicates whether the post-processing effect is dirty and needs an update.
        /// </summary>
        protected bool dirty = true;

        /// <summary>
        /// The input shader resource view.
        /// </summary>
        protected IShaderResourceView Input;

        /// <summary>
        /// The input resource.
        /// </summary>
        protected IResource InputResource;

        /// <summary>
        /// The output render target view.
        /// </summary>
        protected IRenderTargetView Output;

        /// <summary>
        /// The output resource.
        /// </summary>
        protected IResource OutputResource;

        /// <summary>
        /// The viewport associated with the output.
        /// </summary>
        protected Viewport Viewport;

        private ImGuiName? displayName;

        /// <inheritdoc/>
        public abstract string Name { get; }

        public virtual ImGuiName DisplayName
        {
            get => displayName ??= CreateDisplayName();
        }

        /// <inheritdoc/>
        public abstract PostFxFlags Flags { get; }

        /// <inheritdoc/>
        public abstract PostFxColorSpace ColorSpace { get; }

        /// <inheritdoc/>
        public bool Initialized => initialized;

        /// <inheritdoc/>
        public unsafe bool Enabled
        {
            get
            {
                if ((Flags & PostFxFlags.AlwaysEnabled) != 0)
                {
                    return true;
                }

                return enabled;
            }
            set
            {
                // prevent raising a reload event.
                if ((Flags & PostFxFlags.AlwaysEnabled) != 0)
                {
                    return;
                }

                if (value == enabled)
                {
                    return;
                }

                enabled = value;
                dirty = true;
                OnEnabledChanged?.Invoke(this, value);
            }
        }

        /// <inheritdoc/>
        public event Action<IPostFx, bool>? OnEnabledChanged;

        /// <inheritdoc/>
        public event Action<IPostFx>? OnReload;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        void IPostFx.Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            if (initialized)
            {
                return;
            }

            Initialize(device, creator, width, height, macros);
            initialized = true;
            dirty = true;
        }

        protected virtual ImGuiName CreateDisplayName()
        {
            EditorDisplayNameAttribute? displayNameAttribute = GetType().GetCustomAttribute<EditorDisplayNameAttribute>();
            return displayNameAttribute == null ? new(Name) : new(displayNameAttribute.Name);
        }

        /// <summary>
        /// Initializes the post-processing effect.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="creator">The resource builder for creating resources.</param>
        /// <param name="width">The width of the rendering area.</param>
        /// <param name="height">The height of the rendering area.</param>
        /// <param name="macros">The shader macros to use during initialization.</param>
        public abstract void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros);

        /// <inheritdoc/>
        public abstract void SetupDependencies(PostFxDependencyBuilder builder);

        /// <inheritdoc/>
        public virtual void Resize(int width, int height)
        {
        }

        /// <inheritdoc/>
        public virtual void Update(IGraphicsContext context)
        {
        }

        /// <inheritdoc/>
        public virtual void Draw(IGraphicsContext context)
        {
        }

        /// <inheritdoc/>
        public virtual void Compose(IGraphicsContext context)
        {
        }

        /// <inheritdoc/>
        public virtual void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
        {
        }

        /// <inheritdoc/>
        public virtual void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            OutputResource = resource;
            Viewport = viewport;
        }

        /// <inheritdoc/>
        public virtual void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            InputResource = resource;
        }

        /// <summary>
        /// Performs the core disposal logic for the post-processing effect.
        /// </summary>
        protected abstract void DisposeCore();

        /// <summary>
        /// Notifies subscribers when a property has changed.
        /// </summary>
        /// <param name="name">The name of the changed property.</param>
        protected void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new(name));
            dirty = true;
        }

        /// <summary>
        /// Notifies subscribers when a property has changed and sets the property value.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="target">The target property to set.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="name">The name of the changed property.</param>
        protected void NotifyPropertyChangedAndSet<T>(ref T target, T value, [CallerMemberName] string name = "")
        {
            if (target.Equals(value))
            {
                return;
            }

            target = value;
            NotifyPropertyChanged(name);
        }

        /// <summary>
        /// Notifies subscribers when a property has changed, sets the property value, and triggers a reload.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="target">The target property to set.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="name">The name of the changed property.</param>
        protected void NotifyPropertyChangedAndSetAndReload<T>(ref T target, T value, [CallerMemberName] string name = "")
        {
            if (target.Equals(value))
            {
                return;
            }

            target = value;
            NotifyPropertyChanged(name);
            NotifyReload();
        }

        /// <summary>
        /// Notifies subscribers that a reload is required.
        /// </summary>
        protected void NotifyReload()
        {
            OnReload?.Invoke(this);
        }

        private void DisposeInternal()
        {
            if (initialized)
            {
                DisposeCore();
                initialized = false;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private string GetDebuggerDisplay()
        {
            return $"{Name}, {(Initialized ? "I" : "")}{(Enabled ? "E" : "")}{(dirty ? "D" : "")}, {Flags}";
        }
    }
}