namespace HexaEngine.UI
{
    public partial class UIElement : InputElement
    {
        public event EventHandler<EventArgs>? Initialized;

        public event EventHandler<EventArgs>? Uninitialized;

        public bool IsInitialized { get; set; }

        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUninitialized()
        {
            Uninitialized?.Invoke(this, EventArgs.Empty);
        }

        internal virtual void Initialize()
        {
            IsInitialized = true;
            OnInitialized();
            InitializeComponent();
        }

        internal virtual void Uninitialize()
        {
            IsInitialized = false;
            OnUninitialized();
            UninitializeComponent();
        }

        public virtual void InitializeComponent()
        {
        }

        public virtual void UninitializeComponent()
        {
        }
    }
}