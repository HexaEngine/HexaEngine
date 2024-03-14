namespace HexaEngine.UI
{
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public partial class UIElement : Visual
    {
        private Visibility visibility;
        private RectangleF outerRectangle;
        private RectangleF contentRectangle;

        private Matrix3x2 positionMatrix;
        private Matrix3x2 positionContentMatrix;

        public Brush? Background { get; set; }

        public Brush? Border { get; set; }

        public event EventHandler<EventArgs>? Initialized;

        public event EventHandler<EventArgs>? Uninitialized;

        public bool IsInitialized { get; set; }

        public int GridColumn { get; set; }

        public int GridRow { get; set; }

        public int GridColumnSpan { get; set; } = 1;

        public int GridRowSpan { get; set; } = 1;

        public Visibility Visibility
        {
            get => visibility;
            set
            {
                if (visibility == value)
                {
                    return;
                }

                Visibility old = visibility;
                visibility = value;

                if (old == Visibility.Collapsed || value == Visibility.Collapsed)
                {
                    InvalidateArrange();
                }
                else
                {
                    InvalidateVisual();
                }
            }
        }

        public RectangleF BoundingBox => RectangleF.Transform(outerRectangle, positionMatrix);

        public virtual void Draw(UICommandList commandList)
        {
            if (visibility != Visibility.Visible)
            {
                return;
            }

            RectangleF rect = BoundingBox;
            commandList.PushClipRect(new(rect));
            var before = commandList.Transform;

            if (Background != null)
            {
                commandList.Transform = positionMatrix;
                commandList.FillRect(outerRectangle, Background);
            }

            if (Border != null)
            {
                commandList.Transform = positionMatrix;
                commandList.DrawLine(Vector2.Zero, new(outerRectangle.Right, 0), Border, border.Top * 2);
                commandList.DrawLine(Vector2.Zero, new(0, outerRectangle.Bottom), Border, border.Left * 2);
                commandList.DrawLine(new(0, outerRectangle.Bottom), new(outerRectangle.Right, outerRectangle.Bottom), Border, border.Bottom * 2);
                commandList.DrawLine(new(outerRectangle.Right, 0), new(outerRectangle.Right, outerRectangle.Bottom), Border, border.Right * 2);
            }

            commandList.Transform = positionContentMatrix;
            OnRender(commandList);
            commandList.Transform = before;
            commandList.PopClipRect();
        }

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