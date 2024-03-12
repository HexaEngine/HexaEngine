namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public class Grid : UIElement, IChildContainer
    {
        public Grid()
        {
            Children = new(this);
        }

        public UIElementCollection Children { get; }

        public List<ColumnDefinition> ColumnDefinitions { get; } = [];

        public List<RowDefinition> RowDefinitions { get; } = [];

        public override void InitializeComponent()
        {
            Children.ForEach(child => child.InitializeComponent());
            Children.ElementAdded += OnChildAdded;
            Children.ElementRemoved += OnChildRemoved;
        }

        private void OnChildAdded(object? sender, UIElement e)
        {
            InvalidateLayout();
        }

        private void OnChildRemoved(object? sender, UIElement e)
        {
            InvalidateLayout();
        }

        internal override void Initialize()
        {
            base.Initialize();
            Children.ForEach(child => child.Initialize());
        }

        internal override void Uninitialize()
        {
            Children.ElementAdded -= OnChildAdded;
            Children.ElementRemoved -= OnChildRemoved;
            Children.ForEach(child => child.Uninitialize());
            base.Uninitialize();
        }

        public override void DrawContent(UICommandList commandList)
        {
            Children.ForEach(child => child.Draw(commandList));
        }

        public override void CalculateBounds()
        {
            UIElement? ancestor = ResolveObject<UIElement>();
            Vector2 avail = NormalizeAvailSize(ancestor.GetAvailableContentSize(this));

            int columnStarCount = 0;
            for (int i = 0; i < ColumnDefinitions.Count; i++)
            {
                float actualWidth = 0;
                var column = ColumnDefinitions[i];

                if (column.Width.GridUnitType == GridUnitType.Star)
                {
                    columnStarCount++;
                    continue;
                }

                switch (column.Width.GridUnitType)
                {
                    case GridUnitType.Auto:
                        foreach (var child in GetChildrenInColumn(i))
                        {
                            var bounds = child.GetBounds(this);
                            actualWidth = MathF.Max(bounds.Left + bounds.Right, actualWidth);
                        }
                        break;

                    case GridUnitType.Pixel:
                        actualWidth = column.Width.Value;
                        break;
                }

                actualWidth = column.NormalizeWidth(actualWidth);

                avail.X -= actualWidth;
                column.ActualWidth = actualWidth;
            }

            for (int i = 0; i < ColumnDefinitions.Count; i++)
            {
                var column = ColumnDefinitions[i];

                if (column.Width.GridUnitType != GridUnitType.Star)
                    continue;

                column.ActualWidth = column.NormalizeWidth(avail.X / (column.Width.Value * columnStarCount));
            }

            int rowStarCount = 0;
            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                float actualHeight = 0;
                var row = RowDefinitions[i];

                if (row.Height.GridUnitType == GridUnitType.Star)
                {
                    rowStarCount++;
                    continue;
                }

                switch (row.Height.GridUnitType)
                {
                    case GridUnitType.Auto:
                        foreach (var child in GetChildrenInRow(i))
                        {
                            var bounds = child.GetBounds(this);
                            actualHeight = MathF.Max(bounds.Top + bounds.Bottom, actualHeight);
                        }
                        break;

                    case GridUnitType.Pixel:
                        actualHeight = row.Height.Value;
                        break;
                }

                actualHeight = row.NormalizeHeight(actualHeight);
                avail.Y -= actualHeight;
                row.ActualHeight = actualHeight;
            }

            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                var row = RowDefinitions[i];

                if (row.Height.GridUnitType != GridUnitType.Star)
                    continue;

                row.ActualHeight = row.NormalizeHeight(avail.Y / (row.Height.Value * rowStarCount));
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].CalculateBounds();
            }

            base.CalculateBounds();
        }

        protected override Vector2 GetPositionInElement(UIElement? child)
        {
            Vector2 origin = base.GetPositionInElement(child);

            if (child == null)
            {
                return origin;
            }

            if (child.GridColumn >= 0 && child.GridColumn < ColumnDefinitions.Count)
            {
                for (int i = 0; i < child.GridColumn; i++)
                {
                    var column = ColumnDefinitions[i];
                    origin.X += column.ActualWidth;
                }
            }

            if (child.GridRow >= 0 && child.GridRow < RowDefinitions.Count)
            {
                for (int i = 0; i < child.GridRow; i++)
                {
                    var row = RowDefinitions[i];
                    origin.Y += row.ActualHeight;
                }
            }

            return origin;
        }

        public override Vector2 GetAvailableContentSize(UIElement? child)
        {
            Vector2 avail = base.GetAvailableContentSize(child);

            if (child == null)
            {
                return avail;
            }

            if (child.GridColumn >= 0 && child.GridColumn < ColumnDefinitions.Count)
            {
                avail.X = 0;
                int end = Math.Min(child.GridColumn + child.GridColumnSpan, ColumnDefinitions.Count);
                for (int i = child.GridColumn; i < end; i++)
                {
                    avail.X += ColumnDefinitions[i].ActualWidth;
                }
            }

            if (child.GridRow >= 0 && child.GridRow < RowDefinitions.Count)
            {
                avail.Y = 0;
                int end = Math.Min(child.GridRow + child.GridRowSpan, RowDefinitions.Count);
                for (int i = child.GridRow; i < end; i++)
                {
                    avail.Y += RowDefinitions[i].ActualHeight;
                }
            }

            return avail;
        }

        public override Vector2 GetContentSize(UIElement? ancestor)
        {
            if (Children.Count == 0)
                return default;

            Vector2 size = default;

            for (int i = 0; i < ColumnDefinitions.Count; i++)
            {
                size.X += ColumnDefinitions[i].ActualWidth;
            }

            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                size.Y += RowDefinitions[i].ActualHeight;
            }

            if (ColumnDefinitions.Count == 0 && RowDefinitions.Count == 0)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var child = Children[i];
                    size += child.GetBounds(this).ToSize();
                }
            }

            return size;
        }

        public IEnumerable<UIElement> GetChildrenInColumn(int columnIndex)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (InSpan(child.GridColumn, child.GridColumnSpan, columnIndex))
                {
                    yield return child;
                }
            }
        }

        public IEnumerable<UIElement> GetChildrenInRow(int rowIndex)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (InSpan(child.GridRow, child.GridRowSpan, rowIndex))
                {
                    yield return child;
                }
            }
        }

        public static bool InSpan(int startIndex, int length, int searchIndex)
        {
            return startIndex >= searchIndex && (searchIndex - startIndex) < length;
        }
    }
}