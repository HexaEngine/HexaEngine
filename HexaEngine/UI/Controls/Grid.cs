namespace HexaEngine.UI.Controls
{
    using HexaEngine.Mathematics;
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
            InvalidateArrange();
        }

        private void OnChildRemoved(object? sender, UIElement e)
        {
            InvalidateArrange();
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

        public override void OnRender(UICommandList commandList)
        {
#if DEBUG
            float pen = 0;
            for (int i = 0; i < ColumnDefinitions.Count; i++)
            {
                var column = ColumnDefinitions[i];
                pen += column.ActualWidth;
                commandList.DrawLine(new(pen, 0), new(pen, ActualHeight), Border, 1);
            }
            pen = 0;
            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                var row = RowDefinitions[i];
                pen += row.ActualHeight;
                commandList.DrawLine(new(0, pen), new(ActualWidth, pen), Border, 1);
            }
#endif
            Children.ForEach(child => child.Draw(commandList));
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
            return searchIndex >= startIndex && searchIndex - startIndex < length;
        }

        public Vector2 GetAvailableSizeInGrid(UIElement element)
        {
            Vector2 size = default;

            int columnStart = Math.Min(element.GridColumn, ColumnDefinitions.Count - 1);
            int columnEnd = Math.Min(columnStart + element.GridColumnSpan, ColumnDefinitions.Count);
            for (int i = columnStart; i < columnEnd; i++)
            {
                ColumnDefinition column = ColumnDefinitions[i];
                size.X += column.ActualWidth;
            }

            int rowStart = Math.Min(element.GridRow, RowDefinitions.Count - 1);
            int rowEnd = Math.Min(rowStart + element.GridRowSpan, RowDefinitions.Count);
            for (int i = rowStart; i < rowEnd; i++)
            {
                RowDefinition row = RowDefinitions[i];
                size.Y += row.ActualHeight;
            }

            return size;
        }

        public Vector2 GetPositionInGrid(UIElement element)
        {
            Vector2 origin = default;

            if (element.GridColumn >= 0 && element.GridColumn < ColumnDefinitions.Count)
            {
                for (int i = 0; i < element.GridColumn; i++)
                {
                    var column = ColumnDefinitions[i];
                    origin.X += column.ActualWidth;
                }
            }

            if (element.GridRow >= 0 && element.GridRow < RowDefinitions.Count)
            {
                for (int i = 0; i < element.GridRow; i++)
                {
                    var row = RowDefinitions[i];
                    origin.Y += row.ActualHeight;
                }
            }

            return origin;
        }

        protected override void ArrangeCore(RectangleF finalRect)
        {
            base.ArrangeCore(finalRect);

            RectangleF nextRect = ContentBounds;
            Vector2 origin = nextRect.Offset;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var childOrigin = GetPositionInGrid(child);
                var desiredSize = GetAvailableSizeInGrid(child);
                RectangleF nextChildRect = new(origin + childOrigin, desiredSize);
                Children[i].Arrange(nextChildRect);
            }
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            Vector2 avail = availableSize;

            // first pass, fixed size grid columns and rows, subtract from available space.
            foreach (var column in ColumnDefinitions)
            {
                if (column.Width.GridUnitType != GridUnitType.Pixel) continue;
                float actualWidth = column.ActualWidth = column.NormalizeWidth(column.Width.Value);
                availableSize.X -= actualWidth;
            }

            foreach (var row in RowDefinitions)
            {
                if (row.Height.GridUnitType != GridUnitType.Pixel) continue;
                float actualHeight = row.ActualHeight = row.NormalizeHeight(row.Height.Value);
                availableSize.Y -= actualHeight;
            }

            // second pass, auto size grid columns and rows, subtract from available space.
            int columnStarCount = 0;
            for (int i = 0; i < ColumnDefinitions.Count; i++)
            {
                float actualWidth = 0;
                var column = ColumnDefinitions[i];
                var unit = column.Width.GridUnitType;
                if (unit == GridUnitType.Star)
                {
                    columnStarCount++;
                    continue;
                }
                if (unit == GridUnitType.Pixel) continue;

                foreach (var child in GetChildrenInColumn(i))
                {
                    child.Measure(availableSize);
                    // skip stretch items, would cause wrong layout.
                    if (child.HorizontalAlignment == HorizontalAlignment.Stretch) continue;
                    actualWidth = MathF.Max(child.DesiredSize.X, actualWidth);
                }

                actualWidth = column.NormalizeWidth(actualWidth);

                availableSize.X -= actualWidth;
                column.ActualWidth = actualWidth;
            }

            int rowStarCount = 0;
            for (int i = 0; i < RowDefinitions.Count; i++)
            {
                float actualHeight = 0;
                var row = RowDefinitions[i];
                var unit = row.Height.GridUnitType;
                if (unit == GridUnitType.Star)
                {
                    rowStarCount++;
                    continue;
                }
                if (unit == GridUnitType.Pixel) continue;

                foreach (var child in GetChildrenInRow(i))
                {
                    child.Measure(availableSize);
                    // skip stretch items, would cause wrong layout.
                    if (child.VerticalAlignment == VerticalAlignment.Stretch) continue;
                    actualHeight = MathF.Max(child.DesiredSize.Y, actualHeight);
                }

                actualHeight = row.NormalizeHeight(actualHeight);
                availableSize.Y -= actualHeight;
                row.ActualHeight = actualHeight;
            }

            // third pass, star grid columns and rows, use remaining space.
            foreach (var column in ColumnDefinitions)
            {
                if (column.Width.GridUnitType != GridUnitType.Star)
                    continue;

                column.ActualWidth = column.NormalizeWidth(availableSize.X / (column.Width.Value * columnStarCount));
            }

            foreach (var row in RowDefinitions)
            {
                if (row.Height.GridUnitType != GridUnitType.Star)
                    continue;

                row.ActualHeight = row.NormalizeHeight(availableSize.Y / (row.Height.Value * rowStarCount));
            }

            // update sizes of children.
            foreach (var child in Children)
            {
                child.Measure(GetAvailableSizeInGrid(child));
            }

            return avail;
        }
    }
}