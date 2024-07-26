namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics.Text;
    using HexaEngine.UI.Graphics;
    using Hexa.NET.Mathematics;
    using System.Numerics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Serialization;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Core.Input.Events;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class Control : FrameworkElement
    {
        private readonly List<TextLayout> layouts = [];
        private TextFactory? textFactory;

        static Control()
        {
            MouseDoubleClickEvent.AddClassHandler<Control>((x, args) => x?.OnDoubleClick(args));
        }

        [XmlIgnore]
        public TextFormat? Format
        {
            get;
            private set;
        }

        public static readonly DependencyProperty<Brush> ForegroundProperty = DependencyProperty.Register<Control, Brush>(nameof(Foreground), false, new(Brushes.Black));

        public Brush? Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

        public static readonly DependencyProperty<Brush> BackgroundProperty = DependencyProperty.Register<Control, Brush>(nameof(Background), false, new(Brushes.Transparent));

        public Brush? Background { get => GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }

        public static readonly DependencyProperty<Brush> BorderProperty = DependencyProperty.Register<Control, Brush>(nameof(Border), false, new(Brushes.Transparent));

        public Brush? Border { get => GetValue(BorderProperty); set => SetValue(BorderProperty, value); }

        public static readonly DependencyProperty<Thickness> BorderThicknessProperty = DependencyProperty.Register<Control, Thickness>(nameof(BorderThickness), false, new(new Thickness(0)));

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseDoubleClickEvent = EventManager.Register<Control, MouseButtonEventArgs>(nameof(MouseDoubleClick), RoutingStrategy.Direct);

        public event RoutedEventHandler<MouseButtonEventArgs> MouseDoubleClick
        {
            add => AddHandler(MouseDoubleClickEvent, value);
            remove => RemoveHandler(MouseDoubleClickEvent, value);
        }

        public Thickness BorderThickness
        {
            get => GetValue(BorderThicknessProperty);
            set
            {
                SetValue(BorderThicknessProperty, value);
                InvalidateArrange();
            }
        }

        public static readonly DependencyProperty<string> FontFamilyNameProperty = DependencyProperty.Register<Control, string>(nameof(FontFamilyName), false, new("Arial"));

        public string FontFamilyName
        {
            get => GetValue(FontFamilyNameProperty) ?? "Arial";
            set
            {
                SetValue(FontFamilyNameProperty, value);
                CreateTextFormat();
            }
        }

        public static readonly DependencyProperty<FontStyle> FontStyleProperty = DependencyProperty.Register<Control, FontStyle>(nameof(FontStyle), false, new(FontStyle.Regular));

        public FontStyle FontStyle
        {
            get => GetValue(FontStyleProperty);
            set
            {
                SetValue(FontStyleProperty, value);
                CreateTextFormat();
            }
        }

        public static readonly DependencyProperty<FontWeight> FontWeightProperty = DependencyProperty.Register<Control, FontWeight>(nameof(FontWeight), false, new(FontWeight.Normal));

        public FontWeight FontWeight
        {
            get => GetValue(FontWeightProperty);
            set
            {
                SetValue(FontWeightProperty, value);
                CreateTextFormat();
            }
        }

        public static readonly DependencyProperty<float> IncrementalTabStopProperty = DependencyProperty.Register<Control, float>(nameof(IncrementalTabStop), false, new(48f));

        public float IncrementalTabStop
        {
            get => GetValue(IncrementalTabStopProperty);
            set
            {
                SetValue(IncrementalTabStopProperty, value);
                UpdateTextFormat();
            }
        }

        public static readonly DependencyProperty<FlowDirection> FlowDirectionProperty = DependencyProperty.Register<Control, FlowDirection>(nameof(FlowDirection), false, new(FlowDirection.TopToBottom));

        public FlowDirection FlowDirection
        {
            get => GetValue(FlowDirectionProperty);
            set
            {
                SetValue(FlowDirectionProperty, value);
                UpdateTextFormat();
            }
        }

        public static readonly DependencyProperty<ReadingDirection> ReadingDirectionProperty = DependencyProperty.Register<Control, ReadingDirection>(nameof(ReadingDirection), false, new(ReadingDirection.LeftToRight));

        public ReadingDirection ReadingDirection
        {
            get => GetValue(ReadingDirectionProperty);
            set
            {
                SetValue(ReadingDirectionProperty, value);
                UpdateTextFormat();
            }
        }

        public static readonly DependencyProperty<WordWrapping> WordWrappingProperty = DependencyProperty.Register<Control, WordWrapping>(nameof(WordWrapping), false, new(WordWrapping.NoWrap));

        public WordWrapping WordWrapping
        {
            get => GetValue(WordWrappingProperty);
            set
            {
                SetValue(WordWrappingProperty, value);
                UpdateTextFormat();
            }
        }

        public static readonly DependencyProperty<ParagraphAlignment> ParagraphAlignmentProperty = DependencyProperty.Register<Control, ParagraphAlignment>(nameof(ParagraphAlignment), false, new(ParagraphAlignment.Left));

        public ParagraphAlignment ParagraphAlignment
        {
            get => GetValue(ParagraphAlignmentProperty);
            set
            {
                SetValue(ParagraphAlignmentProperty, value);
                UpdateTextFormat();
            }
        }

        public static readonly DependencyProperty<TextAlignment> TextAlignmentProperty = DependencyProperty.Register<Control, TextAlignment>(nameof(TextAlignment), false, new(TextAlignment.Leading));

        public TextAlignment TextAlignment
        {
            get => GetValue(TextAlignmentProperty);
            set
            {
                SetValue(TextAlignmentProperty, value);
                UpdateTextFormat();
            }
        }

        public static readonly DependencyProperty<float> FontSizeProperty = DependencyProperty.Register<Control, float>(nameof(FontSize), false, new(17f));

        public float FontSize
        {
            get => GetValue(FontSizeProperty);
            set
            {
                SetValue(FontSizeProperty, value);
                UpdateTextFormat();
            }
        }

        internal void CreateTextFormat()
        {
            textFactory ??= UISystem.Current?.TextFactory;
            if (textFactory == null)
            {
                return;
            }
            Format?.Dispose();
            Format = textFactory.CreateTextFormat(FontFamilyName, FontStyle, FontWeight, FontSize);
            Format.FontSize = FontSize;
            Format.IncrementalTabStop = IncrementalTabStop;
            Format.FlowDirection = FlowDirection;
            Format.ReadingDirection = ReadingDirection;
            Format.WordWrapping = WordWrapping;
            Format.ParagraphAlignment = ParagraphAlignment;
            Format.TextAlignment = TextAlignment;

            for (int i = 0; i < layouts.Count; i++)
            {
                var layout = layouts[i];
                if (layout.IsDisposed)
                {
                    layouts.RemoveAt(i);
                    i--;
                }
                layout.Format = Format;
            }
        }

        public override sealed void Render(UICommandList commandList)
        {
            if (Visibility != Visibility.Visible)
            {
                return;
            }

            commandList.PushClipRect(VisualClip);
            var before = commandList.Transform;

            var padding = Padding;
            var border = BorderThickness;
            var bounds = new RectangleF(0, 0, ActualWidth, ActualHeight);
            var contentBounds = InnerContentBounds;
            var contentRectSize = contentBounds.Size - border.Size;
            var contentPaddingSize = contentRectSize + padding.Size;

            if (border.Size != Vector2.Zero)
            {
                Brush borderBrush = Border ?? Brushes.Alpha;
                commandList.Transform = BaseOffset;
                commandList.FillRect(new(-border.Left, -border.Top, bounds.Right + border.Right, bounds.Bottom + border.Bottom), borderBrush);
            }

            commandList.Transform = Matrix3x2.CreateTranslation(BaseOffset.Translation + new Vector2(BorderThickness.Left, BorderThickness.Top));
            Brush backgroundBrush = Background ?? Brushes.Alpha;
            commandList.FillRect(new(0, 0, contentPaddingSize.X, contentPaddingSize.Y), backgroundBrush);

            commandList.Transform = Matrix3x2.CreateTranslation(ContentOffset.Translation + new Vector2(BorderThickness.Left, BorderThickness.Top));
            OnRender(commandList);
            commandList.Transform = before;
            commandList.PopClipRect();
        }

        protected TextLayout? CreateTextLayout(string text, float maxWidth, float maxHeight)
        {
            if (Format is null)
            {
                CreateTextFormat();
            }
            if (Format is null || textFactory == null)
            {
                return null;
            }

            var layout = textFactory.CreateTextLayout(text, Format, maxWidth, maxHeight);
            layouts.Add(layout);
            return layout;
        }

        private void UpdateTextFormat()
        {
            if (Format == null)
            {
                return;
            }
            Format.FontSize = FontSize;
            Format.IncrementalTabStop = IncrementalTabStop;
            Format.FlowDirection = FlowDirection;
            Format.ReadingDirection = ReadingDirection;
            Format.WordWrapping = WordWrapping;
            Format.ParagraphAlignment = ParagraphAlignment;
            Format.TextAlignment = TextAlignment;
        }

        protected override void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.Clicks % 2 == 0)
            {
                args.RoutedEvent = MouseDoubleClickEvent;
                args.Handled = false;
                RaiseEvent(args);
            }
        }

        protected virtual void OnDoubleClick(MouseButtonEventArgs args)
        {
        }
    }
}