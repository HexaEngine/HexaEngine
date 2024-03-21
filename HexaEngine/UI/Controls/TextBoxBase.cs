namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.UI.Graphics;

    public abstract class TextBoxBase : Control
    {
        public static readonly DependencyProperty<bool> AcceptsReturnProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(AcceptsReturn), false, new PropertyMetadata(false));

        public bool AcceptsReturn { get => GetValue(AcceptsReturnProperty); set => SetValue(AcceptsReturnProperty, value); }

        public static readonly DependencyProperty<bool> AcceptsTabProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(AcceptsTab), false, new PropertyMetadata(true));

        public bool AcceptsTab { get => GetValue(AcceptsTabProperty); set => SetValue(AcceptsTabProperty, value); }

        public static readonly DependencyProperty<bool> AutoWordSelectionProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(AutoWordSelection), false, new PropertyMetadata(true));

        public bool AutoWordSelection { get => GetValue(AutoWordSelectionProperty); set => SetValue(AutoWordSelectionProperty, value); }

        public static readonly DependencyProperty<Brush> CaretBrushProperty = DependencyProperty.Register<TextBoxBase, Brush>(nameof(CaretBrush), false, new PropertyMetadata(Brushes.Black));

        public Brush? CaretBrush { get => GetValue(CaretBrushProperty); set => SetValue(CaretBrushProperty, value); }

        public static readonly DependencyProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty = DependencyProperty.Register<TextBoxBase, ScrollBarVisibility>(nameof(HorizontalScrollBarVisibility), false, new PropertyMetadata(ScrollBarVisibility.Disabled));

        public ScrollBarVisibility HorizontalScrollBarVisibility { get => GetValue(HorizontalScrollBarVisibilityProperty); set => SetValue(HorizontalScrollBarVisibilityProperty, value); }

        public static readonly DependencyProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty = DependencyProperty.Register<TextBoxBase, ScrollBarVisibility>(nameof(VerticalScrollBarVisibility), false, new PropertyMetadata(ScrollBarVisibility.Auto));

        public ScrollBarVisibility VerticalScrollBarVisibility { get => GetValue(VerticalScrollBarVisibilityProperty); set => SetValue(VerticalScrollBarVisibilityProperty, value); }

        public static readonly DependencyProperty<bool> IsInactiveSelectionHighlightEnabledProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(IsInactiveSelectionHighlightEnabled), false, new PropertyMetadata(false));

        public bool IsInactiveSelectionHighlightEnabled { get => GetValue(IsInactiveSelectionHighlightEnabledProperty); set => SetValue(IsInactiveSelectionHighlightEnabledProperty, value); }

        public static readonly DependencyProperty<bool> IsReadOnlyCaretVisibleProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(IsReadOnlyCaretVisible), false, new PropertyMetadata(false));

        public bool IsReadOnlyCaretVisible { get => GetValue(IsReadOnlyCaretVisibleProperty); set => SetValue(IsReadOnlyCaretVisibleProperty, value); }

        public static readonly DependencyProperty<bool> IsReadOnlyProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(IsReadOnly), false, new PropertyMetadata(false));

        public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }

        public static readonly DependencyProperty<bool> IsSelectionActiveProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(IsSelectionActive), false, new PropertyMetadata(false));

        public bool IsSelectionActive { get => GetValue(IsSelectionActiveProperty); }

        public static readonly DependencyProperty<bool> IsUndoEnabledProperty = DependencyProperty.Register<TextBoxBase, bool>(nameof(IsUndoEnabled), false, new PropertyMetadata(true));

        public bool IsUndoEnabled { get => GetValue(IsUndoEnabledProperty); set => SetValue(IsUndoEnabledProperty, value); }

        public static readonly DependencyProperty<Brush> SelectionBrushProperty = DependencyProperty.Register<TextBoxBase, Brush>(nameof(SelectionBrush), false, new PropertyMetadata(Brushes.LightBlue));

        public Brush? SelectionBrush { get => GetValue(SelectionBrushProperty); set => SetValue(SelectionBrushProperty, value); }

        public static readonly DependencyProperty<float> SelectionOpacityProperty = DependencyProperty.Register<TextBoxBase, float>(nameof(SelectionOpacity), false, new PropertyMetadata(0.5f));

        public float SelectionOpacity { get => GetValue(SelectionOpacityProperty); set => SetValue(SelectionOpacityProperty, value); }

        public static readonly DependencyProperty<Brush> SelectionTextBrushProperty = DependencyProperty.Register<TextBoxBase, Brush>(nameof(SelectionTextBrush), false, new PropertyMetadata(Brushes.Black));

        public Brush? SelectionTextBrush { get => GetValue(SelectionTextBrushProperty); set => SetValue(SelectionTextBrushProperty, value); }

        public static readonly DependencyProperty<int> UndoLimitProperty = DependencyProperty.Register<TextBoxBase, int>(nameof(UndoLimit), false, new PropertyMetadata(-1));

        public int UndoLimit { get => GetValue(UndoLimitProperty); set => SetValue(UndoLimitProperty, value); }

        public static readonly RoutedEvent<RoutedEventArgs> SelectionChangedEvent = EventManager.Register<TextBoxBase, RoutedEventArgs>(nameof(SelectionChanged), RoutingStrategy.Bubble);

        public event RoutedEventHandler<RoutedEventArgs> SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent = EventManager.Register<TextBoxBase, TextChangedEventArgs>(nameof(TextChanged), RoutingStrategy.Bubble);

        public event RoutedEventHandler<TextChangedEventArgs> TextChanged
        {
            add => AddHandler(TextChangedEvent, value);
            remove => RemoveHandler(TextChangedEvent, value);
        }
    }

    public class TextBox : TextBoxBase
    {
    }
}