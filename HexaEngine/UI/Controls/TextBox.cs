namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using Hexa.NET.Mathematics;
    using HexaEngine.UI.Controls.Primitives;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;
    using HexaEngine.UI.Markup;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [ContentProperty("Text")]
    public class TextBox : TextBoxBase
    {
        private TextLayout? textLayout;

        private int cursorPosition;

        public static readonly DependencyProperty<string> TextProperty = DependencyProperty.Register<TextBox, string>(nameof(Text), false, new PropertyMetadata(null));

        public string? Text
        {
            get => GetValue(TextProperty);
            set
            {
                if (value != null)
                {
                    textLayout ??= CreateTextLayout(value, float.MaxValue, float.MinValue);
                }
                SetValue(TextProperty, value);
            }
        }

        protected override void OnGotFocus(FocusGainedEventArgs args)
        {
            base.OnGotFocus(args);
            Application.StartTextInput();
            Application.SetTextInputRect((Rectangle)VisualClip);
        }

        protected override void OnLostFocus(FocusLostEventArgs args)
        {
            Application.StopTextInput();
            base.OnLostFocus(args);
        }

        protected override Vector2 MeasureOverwrite(Vector2 availableSize)
        {
            if (textLayout == null)
            {
                return default;
            }

            textLayout.MaxWidth = availableSize.X;
            textLayout.MaxHeight = availableSize.Y;
            textLayout.UpdateLayout();

            return textLayout.Metrics.Size;
        }

        protected override unsafe void OnTextInput(TextInputEventArgs args)
        {
            string? oldText = Text;
            string newText;

            if (oldText == null)
            {
                newText = ToStringFromUTF8(args.Text);
            }
            else
            {
                newText = Text.Insert(cursorPosition, ToStringFromUTF8(args.Text));
            }

            cursorPosition++;
            Text = newText;

            if (textLayout == null)
            {
                textLayout = CreateTextLayout(newText, float.MaxValue, float.MinValue);
                InvalidateMeasure();
                return;
            }

            textLayout.Text = newText;
            InvalidateMeasure();
        }

        protected override void OnKeyDown(KeyboardEventArgs args)
        {
            if (args.KeyCode == Key.Backspace)
            {
                string? text = Text;
                if (text != null && text.Length > 0)
                {
                    string newText = text[..^1];
                    Text = newText;
                    textLayout.Text = newText;
                    InvalidateMeasure();
                    cursorPosition--;
                }
            }
            if (args.KeyCode == Key.Return && AcceptsReturn)
            {
                string newText = Text + "\n";
                Text = newText;
                textLayout.Text = newText;
                InvalidateMeasure();
                cursorPosition++;
            }
            if (args.KeyCode == Key.Tab && AcceptsTab)
            {
                string newText = Text + "\t";
                Text = newText;
                textLayout.Text = newText;
                InvalidateMeasure();
                cursorPosition++;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            if (textLayout == null)
            {
                return;
            }

            Vector2 position = PointFromScreen(args.Position) + Padding.Offset + BorderThickness.Offset;
            cursorPosition = textLayout.HitTest(position);
        }

        protected override void OnRender(UICommandList commandList)
        {
            if (textLayout == null)
            {
                return;
            }

            textLayout.DrawText(commandList, Foreground);

            Vector2 cursorPos = textLayout.GetCursorPosition(cursorPosition);
            commandList.DrawLine(cursorPos, cursorPos + new Vector2(0, textLayout.Metrics.LineHeight), CaretBrush, 1);
        }
    }
}