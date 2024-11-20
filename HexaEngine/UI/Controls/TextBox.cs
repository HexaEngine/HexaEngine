namespace HexaEngine.UI.Controls
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.UI.Controls.Primitives;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;
    using HexaEngine.UI.Markup;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Text;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [ContentProperty("Text")]
    public class TextBox : TextBoxBase
    {
        private TextLayout? textLayout;
        private int cursorPosition;
        private TextSelection textSelection;

        private readonly Stack<(string text, int cursorPos, TextSelection selection)> undoHistory = [];
        private readonly Stack<(string text, int cursorPos, TextSelection selection)> redoHistory = [];

        private bool hold = false;

        private struct TextSelection
        {
            public int Start;
            public int End;

            public readonly int Length => Math.Abs(End - Start);

            public static readonly TextSelection Invalid = new TextSelection(-1, -1);

            public TextSelection(int start, int end)
            {
                Start = start;
                End = end;
            }

            public readonly bool IsValid() => Start >= 0 && End >= 0 && Length > 0;

            public readonly void GetIndices(out int start, out int end)
            {
                if (Start > End)
                {
                    (start, end) = (End, Start);
                }
                else
                {
                    (start, end) = (Start, End);
                }
            }
        }

        public static readonly DependencyProperty<string> TextProperty = DependencyProperty.Register<TextBox, string>(nameof(Text), false, new PropertyMetadata(null));

        public string? Text
        {
            get => GetValue(TextProperty);
            set
            {
                if (value != null)
                {
                    textLayout ??= CreateTextLayout(value, float.MaxValue, float.MaxValue);
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

        protected override Vector2 MeasureOverride(Vector2 availableSize)
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

            int len = StrLen(args.Text);

            var text = Encoding.UTF8.GetString(args.Text, len);

            OnPreEdit(ref oldText);
            if (oldText == null)
            {
                newText = text;
            }
            else
            {
                newText = Text!.Insert(cursorPosition, text);
            }
            cursorPosition += text.Length;
            OnPostEdit(newText);
        }

        protected override void OnKeyDown(KeyboardEventArgs args)
        {
            string? text = Text;

            if ((args.Modifier & KeyMod.Ctrl) != 0)
            {
                switch (args.KeyCode)
                {
                    case Key.A:
                        if (!string.IsNullOrEmpty(text))
                        {
                            textSelection.Start = 0;
                            textSelection.End = text.Length;
                            cursorPosition = text.Length;
                            InvalidateVisual();
                        }
                        return;

                    case Key.C:
                        {
                            if (textSelection.IsValid() && text != null)
                            {
                                textSelection.GetIndices(out var start, out var end);
                                string selectedText = text[start..end];
                                Clipboard.SetText(selectedText);
                            }
                            return;
                        }

                    case Key.X:
                        {
                            if (textSelection.IsValid() && text != null)
                            {
                                textSelection.GetIndices(out var start, out var end);
                                string selectedText = text[start..end];
                                Clipboard.SetText(selectedText);
                                OnPreEdit(ref text);
                                OnPostEdit(text);
                            }
                            return;
                        }

                    case Key.V:
                        {
                            string clipboardText = Clipboard.GetText();
                            if (!string.IsNullOrEmpty(clipboardText))
                            {
                                OnPreEdit(ref text);
                                string newText = text!.Insert(cursorPosition, clipboardText);
                                cursorPosition += clipboardText.Length;
                                OnPostEdit(newText);
                            }
                            return;
                        }

                    case Key.Z:
                        TryUndo();
                        return;

                    case Key.Y:
                        TryRedo();
                        return;
                }
            }

            if (args.KeyCode == Key.Backspace && text != null && text.Length > 0 && cursorPosition > 0)
            {
                OnPreEdit(ref text!);

                if (!textSelection.IsValid())
                {
                    int lengthToRemove = Math.Abs(GetLengthToMove(text, cursorPosition, -1));
                    text = text.Remove(cursorPosition - lengthToRemove, lengthToRemove);
                    cursorPosition -= lengthToRemove;
                }

                OnPostEdit(text);
            }

            if ((args.KeyCode == Key.Return || args.KeyCode == Key.NumEnter) && AcceptsReturn)
            {
                OnPreEdit(ref text);
                string newText = text?.Insert(cursorPosition, Environment.NewLine) ?? Environment.NewLine;
                cursorPosition += Environment.NewLine.Length;
                OnPostEdit(newText);
            }

            if (args.KeyCode == Key.Tab && AcceptsTab)
            {
                OnPreEdit(ref text);
                string newText = Text + "\t";
                cursorPosition += Environment.NewLine.Length;
                OnPostEdit(newText);
            }

            if (args.KeyCode == Key.Left && text != null)
            {
                int move = GetLengthToMove(text, cursorPosition, -1);
                cursorPosition = Math.Max(cursorPosition + move, 0);
                InvalidateVisual();
            }

            if (args.KeyCode == Key.Right && text != null)
            {
                int move = GetLengthToMove(text, cursorPosition, 1);
                cursorPosition = Math.Min(cursorPosition + move, text.Length);
                InvalidateVisual();
            }

            if (args.KeyCode == Key.Up && text != null)
            {
                (int line, int column) = GetLineAndColumn(text, cursorPosition);
                if (line <= 0) return;
                var nextLine = textLayout!.Metrics.LineMetrics[line - 1];
                cursorPosition = nextLine.Text.Start + Math.Min(column, nextLine.Length);
                InvalidateVisual();
            }

            if (args.KeyCode == Key.Down && text != null)
            {
                (int line, int column) = GetLineAndColumn(text, cursorPosition);
                if (line == -1 || line + 1 >= textLayout!.Metrics.LineMetrics.Count) return;
                var nextLine = textLayout!.Metrics.LineMetrics[line + 1];
                cursorPosition = nextLine.Text.Start + Math.Min(column, nextLine.Length);
                InvalidateVisual();
            }
        }

        public (int line, int column) GetLineAndColumn(string text, int index)
        {
            if (index < 0 || textLayout == null || textLayout.Metrics.LineMetrics.Count == 0)
            {
                return (-1, -1);
            }

            for (int i = 0; i < textLayout!.Metrics.LineMetrics.Count; i++) // lines use absolute indices including CLRF.
            {
                LineMetrics line = textLayout!.Metrics.LineMetrics[i];
                if (line.Text.Start <= index && line.Text.End >= index)
                {
                    return (i, index - line.Text.Start);
                }
            }

            if (index == text.Length)
            {
                return (textLayout!.Metrics.LineMetrics.Count - 1, textLayout!.Metrics.LineMetrics[^1].Text.Length);
            }

            return (-1, -1);
        }

        private static int GetLengthToMove(string text, int index, int direction)
        {
            // cursor aka index is ahead of current char means we have to substract one anyway.
            int lengthToMove = 1;

            if (direction == 1) // Moving forward
            {
                if (index < text.Length - 1 && text[index] == '\r' && text[index + 1] == '\n')
                {
                    lengthToMove = 2;
                }
                else if (index < text.Length - 1 && char.IsSurrogatePair(text[index], text[index + 1]))
                {
                    lengthToMove = 2;
                }
            }
            else if (direction == -1) // Moving backward
            {
                if (index > 1 && text[index - 2] == '\r' && text[index - 1] == '\n')
                {
                    lengthToMove = 2;
                }
                else if (index > 1 && char.IsSurrogatePair(text[index - 2], text[index - 1]))
                {
                    lengthToMove = 2;
                }
            }

            return lengthToMove * direction;
        }

        public virtual void OnPreEdit(ref string? text)
        {
            if (text == null) return;

            undoHistory.Push((text, cursorPosition, textSelection));
            redoHistory.Clear();

            if (textSelection.IsValid())
            {
                textSelection.GetIndices(out var start, out var end);
                text = text.Remove(start, end - start);
                cursorPosition = start;
            }
        }

        public virtual void OnPostEdit(string? text)
        {
            if (text == null) return;
            Text = text;

            if (textLayout == null)
            {
                textLayout = CreateTextLayout(text, float.MaxValue, float.MaxValue);
                InvalidateMeasure();
                return;
            }

            textLayout!.Text = text;
            textSelection = TextSelection.Invalid;
            InvalidateMeasure();
        }

        public bool TryUndo()
        {
            if (undoHistory.Count == 0) return false;
            Undo();
            return true;
        }

        public void Undo()
        {
            redoHistory.Push((Text!, cursorPosition, textSelection));

            (string lastText, int lastCursor, TextSelection selection) = undoHistory.Pop();
            Text = lastText;
            textLayout!.Text = lastText;
            cursorPosition = lastCursor;
            textSelection = selection;

            InvalidateMeasure();
        }

        public bool TryRedo()
        {
            if (redoHistory.Count == 0) return false;
            Redo();
            return true;
        }

        public void Redo()
        {
            undoHistory.Push((Text!, cursorPosition, textSelection));

            (string lastText, int lastCursor, TextSelection selection) = redoHistory.Pop();
            Text = lastText;
            textLayout!.Text = lastText;
            cursorPosition = lastCursor;
            textSelection = selection;

            InvalidateMeasure();
        }

        private static bool IsWordCharacter(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '$';

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            if (textLayout == null)
            {
                return;
            }

            Vector2 position = PointFromScreen(args.Position) + Padding.Offset + BorderThickness.Offset;
            cursorPosition = textLayout.HitTest(position);

            textSelection.Start = textSelection.End = cursorPosition;

            if (args.Clicks == 2)
            {
                var text = Text;
                var textLen = text?.Length ?? 0;

                var hitPos = Math.Clamp(cursorPosition, 0, textLen - 1);

                int start = hitPos;
                while (start > 0 && IsWordCharacter(text![start - 1]))
                {
                    start--;
                }

                int end = hitPos;
                while (end < textLen && IsWordCharacter(text![end]))
                {
                    end++;
                }

                textSelection.Start = start;
                textSelection.End = end;
                cursorPosition = end;
            }

            hold = true;
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            if (hold)
            {
                if (textLayout == null)
                {
                    return;
                }

                Vector2 position = PointFromScreen(args.Position) + Padding.Offset + BorderThickness.Offset;
                cursorPosition = textSelection.End = textLayout.HitTest(position);
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            hold = false;
        }

        protected override void OnRender(UICommandList commandList)
        {
            if (textLayout == null)
            {
                return;
            }

            textLayout.DrawText(commandList, Foreground);

            Vector2 cursorPos = textLayout.GetCursorPosition(cursorPosition);
            commandList.DrawLine(cursorPos, cursorPos + new Vector2(0, textLayout.Metrics.LineHeight), CaretBrush!, 1);

            if (textSelection.IsValid())
            {
                textSelection.GetIndices(out var start, out var end);

                var selectionBrush = SelectionBrush;
                var opacity = SelectionOpacity;
                uint tint = (uint.MaxValue & 0x00FFFFFF) | ((uint)(opacity * 255) << 24);
                float lineHeight = textLayout.Metrics.LineHeight;

                int i = 0;
                for (; i < textLayout.Metrics.LineMetrics.Count; i++)
                {
                    var lineMetric = textLayout.Metrics.LineMetrics[i];
                    int lineStartIndex = lineMetric.Text.Start;
                    int lineEndIndex = lineMetric.Text.End;
                    if (start >= lineStartIndex && start < lineEndIndex)
                    {
                        break;
                    }
                }

                for (; i < textLayout.Metrics.LineMetrics.Count; i++)
                {
                    var lineMetric = textLayout.Metrics.LineMetrics[i];
                    int lineStartIndex = lineMetric.Text.Start;
                    int lineEndIndex = lineMetric.Text.End;
                    bool isEnd = end < lineEndIndex;

                    Vector2 lineStartPos, lineEndPos;

                    lineStartPos = (start > lineStartIndex)
                        ? textLayout.GetCursorPosition(start)  // Partial line start
                        : lineMetric.Position;                 // Full line start

                    lineEndPos = isEnd
                        ? new Vector2(textLayout.GetCursorPosition(end).X, lineMetric.Position.Y + lineHeight) // Partial line end
                        : lineMetric.Position + lineMetric.Size; // Full line end

                    commandList.FillRect(lineStartPos, lineEndPos, selectionBrush!, tint);

                    if (isEnd)
                    {
                        break;
                    }
                }
            }
        }
    }
}