namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering.Graph;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public abstract class PostFxBase : IPostFx
    {
        protected bool initialized = false;
        private bool enabled = true;
        protected bool dirty = true;

        protected IShaderResourceView Input;
        protected IResource InputResource;
        protected IRenderTargetView Output;
        protected IResource OutputResource;
        protected Viewport Viewport;

        public abstract string Name { get; }

        public abstract PostFxFlags Flags { get; }

        public bool Initialized => initialized;

        public unsafe bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                dirty = true;
                OnEnabledChanged?.Invoke(value);
            }
        }

        public event Action<bool>? OnEnabledChanged;

        public event Action<IPostFx>? OnReload;

        public event PropertyChangedEventHandler? PropertyChanged;

        void IPostFx.Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            Initialize(device, builder, creator, width, height, macros);
            initialized = true;
        }

        public abstract void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros);

        public virtual void Resize(int width, int height)
        {
        }

        public virtual void Update(IGraphicsContext context)
        {
        }

        public abstract void Draw(IGraphicsContext context, GraphResourceBuilder creator);

        public virtual void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
        {
        }

        public virtual void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            OutputResource = resource;
            Viewport = viewport;
        }

        public virtual void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
            InputResource = resource;
        }

        protected abstract void DisposeCore();

        protected void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new(name));
            dirty = true;
        }

        protected void NotifyPropertyChangedAndSet<T>(ref T target, T value, [CallerMemberName] string name = "")
        {
            target = value;
            NotifyPropertyChanged(name);
        }

        protected void NotifyPropertyChangedAndSetAndReload<T>(ref T target, T value, [CallerMemberName] string name = "")
        {
            target = value;
            NotifyPropertyChanged(name);
            NotifyReload();
        }

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

        ~PostFxBase()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            DisposeInternal();
        }

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

    public interface IPostFx : INotifyPropertyChanged, IDisposable
    {
        public string Name { get; }

        public PostFxFlags Flags { get; }

        public bool Initialized { get; }

        public bool Enabled { get; set; }

        public event Action<bool>? OnEnabledChanged;

        public event Action<IPostFx>? OnReload;

        void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros);

        void Resize(int width, int height);

        void Draw(IGraphicsContext context, GraphResourceBuilder creator);

        void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
        {
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport);

        public void SetInput(IShaderResourceView view, ITexture2D resource);

        public void Update(IGraphicsContext context);
    }
}