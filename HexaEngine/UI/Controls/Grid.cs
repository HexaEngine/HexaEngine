namespace HexaEngine.UI.Controls
{
    using Hexa.NET.Mathematics;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class Grid : Panel, IChildContainer
    {
        public List<ColumnDefinition> ColumnDefinitions { get; } = [];

        public List<RowDefinition> RowDefinitions { get; } = [];

        static Grid()
        {
            ColumnProperty.AddOwner<UIElement>();
            RowProperty.AddOwner<UIElement>();
            ColumnSpanProperty.AddOwner<UIElement>();
            RowSpanProperty.AddOwner<UIElement>();
        }

        public static readonly DependencyProperty<int> ColumnProperty = DependencyProperty.RegisterAttached<Grid, int>("Column", false, new FrameworkMetadata(0) { AffectsMeasure = true, AffectsArrange = true });

        public static readonly DependencyProperty<int> RowProperty = DependencyProperty.RegisterAttached<Grid, int>("Row", false, new FrameworkMetadata(0) { AffectsMeasure = true, AffectsArrange = true });

        public static readonly DependencyProperty<int> ColumnSpanProperty = DependencyProperty.RegisterAttached<Grid, int>("ColumnSpan", false, new FrameworkMetadata(1) { AffectsMeasure = true, AffectsArrange = true });

        public static readonly DependencyProperty<int> RowSpanProperty = DependencyProperty.RegisterAttached<Grid, int>("RowSpan", false, new FrameworkMetadata(1) { AffectsMeasure = true, AffectsArrange = true });

        public static int GetColumn(UIElement element)
        {
            return element.GetValue(ColumnProperty);
        }

        public static int GetRow(UIElement element)
        {
            return element.GetValue(RowProperty);
        }

        public static int GetColumnSpan(UIElement element)
        {
            return element.GetValue(ColumnSpanProperty);
        }

        public static int GetRowSpan(UIElement element)
        {
            return element.GetValue(RowSpanProperty);
        }

        public static void SetColumn(UIElement element, int column)
        {
            element.SetValue(ColumnProperty, column);
        }

        public static void SetRow(UIElement element, int row)
        {
            element.SetValue(RowProperty, row);
        }

        public static void SetColumnSpan(UIElement element, int columnSpan)
        {
            element.SetValue(ColumnSpanProperty, columnSpan);
        }

        public static void SetRowSpan(UIElement element, int rowSpan)
        {
            element.SetValue(RowSpanProperty, rowSpan);
        }

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

        public override void Initialize()
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

        public IEnumerable<UIElement> GetChildrenInColumn(int columnIndex)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var column = GetColumn(child);
                var columnSpan = GetColumnSpan(child);
                if (InSpan(column, columnSpan, columnIndex))
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
                var row = GetRow(child);
                var rowSpan = GetRowSpan(child);
                if (InSpan(row, rowSpan, rowIndex))
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

            if (ColumnDefinitions.Count == 0 || RowDefinitions.Count == 0)
            {
                return DesiredSize;
            }

            var columnIndex = GetColumn(element);
            var columnSpan = GetColumnSpan(element);

            var rowIndex = GetRow(element);
            var rowSpan = GetRowSpan(element);

            int columnStart = Math.Min(columnIndex, ColumnDefinitions.Count - 1);
            int columnEnd = Math.Min(columnStart + columnSpan, ColumnDefinitions.Count);
            for (int i = columnStart; i < columnEnd; i++)
            {
                ColumnDefinition column = ColumnDefinitions[i];
                size.X += column.ActualWidth;
            }

            int rowStart = Math.Min(rowIndex, RowDefinitions.Count - 1);
            int rowEnd = Math.Min(rowStart + rowSpan, RowDefinitions.Count);
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

            var columnIndex = GetColumn(element);

            var rowIndex = GetRow(element);

            if (columnIndex >= 0 && columnIndex < ColumnDefinitions.Count)
            {
                for (int i = 0; i < columnIndex; i++)
                {
                    var column = ColumnDefinitions[i];
                    origin.X += column.ActualWidth;
                }
            }

            if (rowIndex >= 0 && rowIndex < RowDefinitions.Count)
            {
                for (int i = 0; i < rowIndex; i++)
                {
                    var row = RowDefinitions[i];
                    origin.Y += row.ActualHeight;
                }
            }

            return origin;
        }

        protected override Vector2 ArrangeOverwrite(Vector2 size)
        {
            Vector2 origin = ContentOffset.Translation;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var childOrigin = GetPositionInGrid(child);
                var desiredSize = GetAvailableSizeInGrid(child);
                RectangleF nextChildRect = new(origin + childOrigin, desiredSize);
                Children[i].Arrange(nextChildRect);
            }

            return size;
        }

        protected override Vector2 MeasureOverwrite(Vector2 availableSize)
        {
            Vector2 avail = availableSize;

            // first pass, fixed size grid columns and rows, subtract from available space.
            foreach (var column in ColumnDefinitions)
            {
                if (column.Width.GridUnitType != GridUnitType.Pixel)
                {
                    continue;
                }

                float actualWidth = column.ActualWidth = column.NormalizeWidth(column.Width.Value);
                availableSize.X -= actualWidth;
            }

            foreach (var row in RowDefinitions)
            {
                if (row.Height.GridUnitType != GridUnitType.Pixel)
                {
                    continue;
                }

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
                if (unit == GridUnitType.Pixel)
                {
                    continue;
                }

                foreach (var child in GetChildrenInColumn(i))
                {
                    child.Measure(availableSize);
                    // skip stretch items, would cause wrong layout.
                    if (child.HorizontalAlignment == HorizontalAlignment.Stretch)
                    {
                        continue;
                    }

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
                if (unit == GridUnitType.Pixel)
                {
                    continue;
                }

                foreach (var child in GetChildrenInRow(i))
                {
                    child.Measure(availableSize);
                    // skip stretch items, would cause wrong layout.
                    if (child.VerticalAlignment == VerticalAlignment.Stretch)
                    {
                        continue;
                    }

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
                {
                    continue;
                }

                column.ActualWidth = column.NormalizeWidth(availableSize.X / (column.Width.Value * columnStarCount));
            }

            foreach (var row in RowDefinitions)
            {
                if (row.Height.GridUnitType != GridUnitType.Star)
                {
                    continue;
                }

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