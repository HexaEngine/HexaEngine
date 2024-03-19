namespace HexaEngine.UI.Controls
{
    using HexaEngine.UI.Graphics.Text;
    using HexaEngine.UI.Graphics;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class Control : FrameworkElement
    {
        private readonly List<TextLayout> layouts = [];
        private TextFactory? textFactory;
        private Brush? foreground;

        public TextFormat? Format
        {
            get;
            private set;
        }

        public Brush? Foreground
        {
            get => foreground;
            set => foreground = value;
        }

        public Brush? Background { get; set; }

        public Brush? Border { get; set; }

        public static readonly DependencyProperty<Thickness> BorderThicknessProperty = DependencyProperty.Register<Control, Thickness>(nameof(BorderThickness), false, new(new Thickness(0)));

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

        public override void Initialize()
        {
            base.Initialize();
            foreground ??= UIFactory.CreateSolidColorBrush(Colors.Black);
            Border ??= UIFactory.CreateSolidColorBrush(Colors.Gray);
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

            var bounds = new RectangleF(0, 0, ActualWidth, ActualHeight);

            if (Background != null)
            {
                commandList.Transform = BaseOffset;
                commandList.FillRect(bounds, Background);
            }

            if (Border != null)
            {
                var border = BorderThickness;
                commandList.Transform = BaseOffset;
                commandList.DrawLine(Vector2.Zero, new(bounds.Right, 0), Border, border.Top * 2);
                commandList.DrawLine(Vector2.Zero, new(0, bounds.Bottom), Border, border.Left * 2);
                commandList.DrawLine(new(0, bounds.Bottom), new(bounds.Right, bounds.Bottom), Border, border.Bottom * 2);
                commandList.DrawLine(new(bounds.Right, 0), new(bounds.Right, bounds.Bottom), Border, border.Right * 2);
            }

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
    }
}