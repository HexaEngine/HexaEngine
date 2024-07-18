namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.UI.Graphics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public readonly struct CaseInsensitiveComparer : IEqualityComparer<char>
    {
        public static readonly CaseInsensitiveComparer Default = new();

        public readonly bool Equals(char x, char y)
        {
            return char.ToLowerInvariant(x) == char.ToLowerInvariant(y);
        }

        public readonly int GetHashCode(char obj)
        {
            return char.ToLowerInvariant(obj).GetHashCode();
        }
    }

    /*
     * [X] Multiline + Editor with Colors.
     * [X] Selection
     * [X] Copy, Cut & Paste
     * [X] Undo Redo
     *
     *
     *
     *
     */

    /// <summary>
    ///
    /// </summary>
    public unsafe class TextEditorTab
    {
        private readonly ImGuiName name;

        private bool open = true;
        private bool isFocused = false;
        private TextSource source;
        private TextHistory history;

        private bool needsUpdate = true;

        public TextEditorTab(string tabName, TextSource source)
        {
            name = new(tabName);
            this.source = source;
            history = new(source, 1024);
        }

        public TextEditorTab(TextSource source, string currentFile)
        {
            name = new(Path.GetFileName(currentFile));
            CurrentFile = currentFile;
            this.source = source;
            history = new(source, 1024);
        }

        public string TabName => name.Name;

        public string? CurrentFile { get; set; }

        public TextSource Source { get => source; set => source = value; }

        public TextHistory History { get => history; set => history = value; }

        public bool IsOpen { get => open; set => open = value; }

        public bool IsFocused => isFocused;

        private readonly List<TextHighlightSpan> highlightSpans = [];

        public struct CursorState
        {
            public int Index;
            public int Line;
            public int Column;

            public CursorState(int index, int line, int column)
            {
                Index = index;
                Line = line;
                Column = column;
            }

            public static readonly CursorState NewLineLF = new(1, 1, 0);
            public static readonly CursorState NewLineCR = new(1, 1, 0);
            public static readonly CursorState NewLineCRLF = new(2, 1, 0);
            public static readonly CursorState Invalid = new(-1, -1, -1);

            public static CursorState FromOffset(int offset)
            {
                return new CursorState(offset, 0, offset);
            }

            public static CursorState FromIndex(int index, TextSource source)
            {
                int line = 0;
                int column = 0;
                for (; line < source.LineCount; line++)
                {
                    var lineSpan = source.Lines[line];
                    if (lineSpan.Start <= index && lineSpan.End >= index)
                    {
                        column = index - lineSpan.Start;
                        break;
                    }
                }

                return new(index, line, column);
            }

            public static CursorState FromLineColumn(int line, int column, TextSource source)
            {
                var lineSpan = source.Lines[line];
                var index = lineSpan.Start + column;
                return new(index, line, column);
            }

            public static CursorState operator ++(CursorState state)
            {
                int newIndex = state.Index + 1;
                int newColumn = state.Column + 1;
                return new(newIndex, state.Line, newColumn);
            }

            public static CursorState operator --(CursorState state)
            {
                int newLine = state.Line;
                int newColumn = state.Column - 1;
                if (newColumn < 0)
                {
                    newLine--;
                    newColumn = 0;
                }
                return new(state.Index - 1, newLine, newColumn);
            }

            public static CursorState operator +(CursorState a, CursorState b)
            {
                return new CursorState(a.Index + b.Index, a.Line + b.Line, a.Column + b.Column);
            }

            public static CursorState operator -(CursorState a, CursorState b)
            {
                return new CursorState(a.Index - b.Index, a.Line - b.Line, a.Column - b.Column);
            }

            public static implicit operator int(CursorState state) => state.Index;
        }

        private CursorState cursorState;

        public struct TextSelection
        {
            public StdWString* Text;
            public CursorState Start;
            public CursorState End;

            public TextSelection()
            {
                Start = CursorState.Invalid;
                End = CursorState.Invalid;
            }

            public TextSelection(StdWString* text, CursorState start, CursorState end)
            {
                Text = text;
                Start = start;
                End = end;
            }

            public static readonly TextSelection Invalid = new(null, CursorState.Invalid, CursorState.Invalid);

            public char* Data => Text->Data + Math.Min(Start.Index, End.Index);

            public readonly int Length => Math.Abs(End - Start);

            public bool IsValid()
            {
                var start = Start.Index;
                var end = End.Index;
                if (start > end)
                {
                    (start, end) = (end, start);
                }
                return start >= 0 && Text != null && end <= Text->Size;
            }

            public readonly CursorState EffectiveStart => Start.Index <= End.Index ? Start : End;

            public readonly CursorState EffectiveEnd => Start.Index <= End.Index ? End : Start;
        }

        private readonly List<TextSelection> selections = [];

        public void Draw(IGraphicsContext context)
        {
            isFocused = false;
            ImGuiTabItemFlags flags = source.Changed ? ImGuiTabItemFlags.UnsavedDocument : ImGuiTabItemFlags.None;
            if (ImGui.BeginTabItem(name.UniqueName, ref open, flags))
            {
                isFocused = true;
                StdWString* text = source.Text;
                ImGuiManager.PushFont("TextEditorFont");

                ImGui.BeginChild("##TextEditorChild", new Vector2(-(sideBarWidth + sidePanelWidth), 0), ImGuiWindowFlags.HorizontalScrollbar);

                Vector2 avail = ImGui.GetContentRegionAvail();
                bool changed = DrawEditor("##TextEditor", text, avail);
                ImGui.EndChild();

                DrawFindWindow();

                ImGuiManager.PopFont();

                DrawSideBar();

                ImGui.EndTabItem();
            }
        }

        private float sideBarWidth = 40f;
        private float sidePanelWidth = 0f;

        private void DrawSideBar()
        {
            ImGui.BeginChild("##SideBar", new Vector2(sideBarWidth, 0));
            var cursor = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail();
            var max = cursor + new Vector2(sideBarWidth, avail.Y);
            ImDrawList* drawList = ImGui.GetWindowDrawList();
            drawList->AddRectFilled(cursor, max, 0xff2c2c2c);
            ImGui.EndChild();
        }

        private enum TextFindFlags
        {
            None = 0,
            CaseSensitive = 1,
            WholeWorld = 2,
            Regex = 4,
            Selection = 8,
        }

        private string searchText = string.Empty;
        private TextFindFlags findFlags;

        private void DrawFindWindow()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            // Get the font size
            float fontSize = ImGui.GetFontSize();

            // Calculate the frame height
            Vector2 framePadding = style.FramePadding;
            Vector2 itemSpacing = style.ItemSpacing;
            float frameHeight = fontSize + framePadding.Y * 4.0f + itemSpacing.Y;

            const float padding = 40;
            const float width = 440;

            ImGui.SameLine();
            var cursor = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new(cursor.X - width - padding, cursor.Y));
            ImGui.PushStyleColor(ImGuiCol.ChildBg, 0xff181818);
            if (ImGui.BeginChild($"Find##{name.Id}", new Vector2(width, frameHeight), ImGuiChildFlags.Border))
            {
                ImGui.PopStyleColor();

                ImGui.SameLine();

                bool updateSearch = ImGui.InputTextEx("##SearchText", "Find...", ref searchText, 1024, new(200, 0), 0, null, null);
                ImGui.SameLine();

                if (IconButton($"{GUwU.MatchCase}", (findFlags & TextFindFlags.CaseSensitive) != 0))
                {
                    InvertFlag(ref findFlags, TextFindFlags.CaseSensitive);
                    updateSearch = true;
                }
                ImGui.SameLine();
                if (IconButton($"{GUwU.MatchWord}", (findFlags & TextFindFlags.WholeWorld) != 0))
                {
                    InvertFlag(ref findFlags, TextFindFlags.WholeWorld);
                    updateSearch = true;
                }
                ImGui.SameLine();
                if (IconButton($"{GUwU.RegularExpression}", (findFlags & TextFindFlags.Regex) != 0))
                {
                    InvertFlag(ref findFlags, TextFindFlags.Regex);
                    updateSearch = true;
                }

                ImGui.SameLine();
                if (IconButton($"{GUwU.ArrowUpward}")) // goes upwards in the search
                {
                }
                ImGui.SameLine();
                if (IconButton($"{GUwU.ArrowDownward}")) // goes downwards in the search
                {
                }
                ImGui.SameLine();
                if (IconButton($"{GUwU.Subject}", (findFlags & TextFindFlags.Selection) != 0))
                {
                    InvertFlag(ref findFlags, TextFindFlags.Selection);
                    updateSearch = true;
                }

                if (updateSearch)
                {
                    PerformFind();
                }
            }
            else
            {
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();

            ImGui.SetCursorPos(cursor);
        }

        private readonly List<TextSpan> findResults = [];
        private int findIndex = -1;

        private void PerformFind()
        {
            findResults.Clear();
            if ((findFlags & TextFindFlags.Regex) != 0)
            {
                RegexOptions options = RegexOptions.Multiline | RegexOptions.Compiled;
                if ((findFlags & TextFindFlags.CaseSensitive) == 0)
                {
                    options |= RegexOptions.IgnoreCase;
                }
                if ((findFlags | TextFindFlags.WholeWorld) == 0)
                {
                    options |= RegexOptions.ExplicitCapture;
                }

                int startPos = 0;
                int textLength = source.Text->Size;
                if ((findFlags & TextFindFlags.Selection) != 0)
                {
                    startPos = Math.Max(0, selection.EffectiveStart);
                    textLength = Math.Min(textLength, selection.Length);
                }

                foreach (var match in Regex.EnumerateMatches(source.Text->AsReadOnlySpan().Slice(startPos, textLength), searchText, options))
                {
                    var idx = match.Index;
                    var len = match.Length;
                    TextSpan span = new(source.Text, idx, len);
                    findResults.Add(span);
                }
            }
            else
            {
                bool caseSensitive = (findFlags & TextFindFlags.CaseSensitive) != 0;
                bool wholeWord = (findFlags & TextFindFlags.WholeWorld) != 0;
                bool selectionOnly = (findFlags & TextFindFlags.Selection) != 0;

                int textLength = source.Text->Size;
                char* pText = source.Text->CStr();

                int startPos = 0;
                if (selectionOnly)
                {
                    startPos = Math.Max(0, selection.EffectiveStart);
                    textLength = Math.Min(textLength, selection.Length);
                }

                IEqualityComparer<char> comparer = caseSensitive ? EqualityComparer<char>.Default : CaseInsensitiveComparer.Default;

                int pos = startPos;
                fixed (char* pSearchText = searchText)
                {
                    while ((pos = Find(pText, textLength, pSearchText, searchText.Length, pos, comparer)) != -1)
                    {
                        if (wholeWord && !IsWholeWordMatch(pText, pos, searchText.Length))
                        {
                            pos++;
                            continue;
                        }

                        findResults.Add(new TextSpan(source.Text, pos, searchText.Length));
                        pos += searchText.Length;
                    }
                }
            }

            findIndex = Math.Clamp(findIndex, 0, findResults.Count - 1);
        }

        private bool IsWholeWordMatch(char* pText, int index, int length)
        {
            bool startIsWordBoundary = index == 0 || !char.IsLetterOrDigit(pText[index - 1]);
            bool endIsWordBoundary = index + length >= source.Text->Size || !char.IsLetterOrDigit(pText[index + length]);

            return startIsWordBoundary && endIsWordBoundary;
        }

        public void ClearFind()
        {
            findResults.Clear();
            searchText = string.Empty;
            findIndex = -1;
        }

        private static bool VerticalCollapseButton(string label)
        {
            return false;
        }

        private static bool IconButton(string label, bool selected = false)
        {
            uint textColor = ImGui.GetColorU32(ImGuiCol.Text);
            uint hoverColor = ImGui.GetColorU32(ImGuiCol.ButtonHovered);
            uint activeColor = ImGui.GetColorU32(ImGuiCol.ButtonActive);
            uint selectedColor = ImGui.GetColorU32(ImGuiCol.TabSelectedOverline);
            uint selectedBgColor = ImGui.GetColorU32(ImGuiCol.TabSelected);
            int id = ImGui.GetID(label);

            ImGuiWindow* window = ImGui.GetCurrentWindow();
            ImDrawList* draw = ImGui.GetWindowDrawList();
            ImGuiStylePtr style = ImGui.GetStyle();

            Vector2 pos = ImGui.GetCursorScreenPos();
            Vector2 size = ImGui.CalcTextSize(label);
            Vector2 padding = style.FramePadding - new Vector2(style.FrameBorderSize * 2);
            ImRect bb = new() { Min = pos + new Vector2(padding.X, 0), Max = new(pos.X + size.X, pos.Y + size.Y) };
            ImRect bbFull = new() { Min = pos, Max = new Vector2(pos.X + size.X, pos.Y + size.Y) + padding * 2 };

            ImGui.ItemSizeRect(bbFull, 0.0f);
            if (!ImGui.ItemAdd(bbFull, id, &bbFull, ImGuiItemFlags.None))
                return false;

            bool isHovered = false;
            bool isClicked = ImGui.ButtonBehavior(bbFull, id, &isHovered, null, 0);
            bool isActive = isHovered && ImGui.IsMouseDown(0);

            uint color = isActive ? activeColor : isHovered ? hoverColor : selected ? selectedBgColor : default;

            if (isActive || isHovered || selected)
            {
                draw->AddRectFilled(bbFull.Min, bbFull.Max, color, style.FrameRounding);
            }

            if (selected)
            {
                draw->AddRect(bbFull.Min, bbFull.Max, selectedColor, style.FrameRounding, 2);
            }

            bb.Min.Y += size.Y * 0.5f;

            draw->AddText(bb.Min, textColor, label);

            return isClicked;
        }

        private static void InvertFlag(ref TextFindFlags flags, TextFindFlags flag)
        {
            flags ^= flag;
        }

        private void Update(float lineHeight, StdWString* text)
        {
            ClearSelection();
            needsUpdate = false;
            source.Update(lineHeight);
            SyntaxHighlightDefaults.CSharp.Analyze(text, source.Lines, highlightSpans, lineHeight);
        }

        private bool wasDragging = false;
        private bool wasFocused = false;
        private TextSelection selection = TextSelection.Invalid;

        private bool DrawEditor(string label, StdWString* text, Vector2 size)
        {
            int id = ImGui.GetID(label);

            ImGuiWindow* window = ImGui.GetCurrentWindow();
            ImGuiStyle* style = ImGui.GetStyle();
            ImGuiIO* io = ImGui.GetIO();
            ImDrawList* draw = ImGui.GetWindowDrawList();
            float lineHeight = ImGui.GetTextLineHeight();

            if (needsUpdate)
            {
                Update(lineHeight, text);
            }

            var fieldSize = Vector2.Max(size, source.LayoutSize + size - new Vector2(0, lineHeight));

            Vector2 cursor = ImGui.GetCursorScreenPos();
            ImRect bb = new() { Min = cursor, Max = cursor + fieldSize };

            ImGui.ItemSizeRect(bb, 0.0f);
            if (!ImGui.ItemAdd(bb, id, &bb, ImGuiItemFlags.None))
                return false;

            bool isHovered = ImGui.IsItemHovered();
            bool isFocused = ImGui.IsItemFocused();

            bool changed = false;

            Vector2 mousePos = ImGui.GetMousePos();

            char* pText = text->CStr();

            {
                const float widthBreakpointsWidth = 20;
                var max = cursor + new Vector2(widthBreakpointsWidth, fieldSize.Y);
                draw->AddRectFilled(cursor, max, 0xff2c2c2c);
                cursor.X += widthBreakpointsWidth + style->FramePadding.X;
                fieldSize.X -= widthBreakpointsWidth + style->FramePadding.X;
            }

            DrawLineNumbers(ref fieldSize, style, draw, ref cursor, lineHeight);
            DrawCursorLine(draw, source, lineHeight, cursor, fieldSize, cursorState);
            DrawText(draw, cursor, pText);
            DrawSelection(draw, cursor, source, lineHeight);

            HandleMouseInput(id, window, lineHeight, text, isHovered, isFocused, mousePos, cursor, pText);
            HandleKeyboardInput(text, ref changed, isFocused);

            if (isFocused != wasFocused)
            {
                wasFocused = isFocused;
            }

            if (changed)
            {
                ImGui.SetActiveID(id, window);
                source.Changed = true;
                Update(lineHeight, text);
            }
            else
            {
                ImGui.ClearActiveID();
            }

            return changed;
        }

        private static void FormatIntUTF8(int value, byte* buffer)
        {
            int i = 0;
            if (value == 0)
            {
                buffer[i++] = (byte)'0';
            }
            else
            {
                while (value > 0)
                {
                    buffer[i++] = (byte)('0' + value % 10);
                    value /= 10;
                }
            }
            buffer[i] = 0; // Null-terminate

            // Reverse the digits for correct order
            for (int j = 0, k = i - 1; j < k; j++, k--)
            {
                (buffer[k], buffer[j]) = (buffer[j], buffer[k]);
            }
        }

        private void DrawLineNumbers(ref Vector2 size, ImGuiStyle* style, ImDrawList* draw, ref Vector2 cursor, float lineHeight)
        {
            int nCount = (int)Math.Log10(source.LineCount) + 1;
            byte* buf = stackalloc byte[nCount + 1]; // no need to set last byte to \0 since stacks are naturally zeroed.
            Memset(buf, (byte)'0', nCount);
            float width = ImGui.CalcTextSize(buf).X;
            Vector2 current = cursor;
            for (int line = 1; line <= source.LineCount; line++)
            {
                FormatIntUTF8(line, buf);
                draw->AddText(current, 0xff4c4c4c, buf);
                current.Y += lineHeight;
            }
            cursor.X += width + style->FramePadding.X;
            size.X -= width + style->FramePadding.X;
        }

        private void HandleMouseInput(int id, ImGuiWindow* window, float lineHeight, StdWString* text, bool isHovered, bool isFocused, Vector2 mousePos, Vector2 origin, char* pText)
        {
            bool isMouseDown = isHovered && ImGui.IsMouseDown(ImGuiMouseButton.Left);

            if (isHovered)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.TextInput);
            }

            if (isMouseDown)
            {
                ImGui.SetFocusID(id, window);
            }

            bool isDragging = ImGui.IsMouseDragging(ImGuiMouseButton.Left);
            if (!isDragging)
            {
                wasDragging = false;
            }

            if (isHovered)
            {
                if (isDragging || isMouseDown)
                {
                    ImGui.SetActiveID(id, window);
                }
                else
                {
                    ImGui.ClearActiveID();
                }
            }

            if (isMouseDown && isFocused)
            {
                if (!wasFocused)
                {
                    SetCursor(HitTest(source, origin, mousePos, lineHeight));
                }

                if (isDragging && !wasDragging)
                {
                    selection.Text = text;
                    selection.Start = selection.End = cursorState;
                    wasDragging = true;
                }
                else if (isDragging && wasDragging)
                {
                    SetCursor(HitTest(source, origin, mousePos, lineHeight));
                    selection.End = cursorState;
                }

                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    var index = HitTest(source, origin, mousePos, lineHeight);
                    int start = index;
                    while (char.IsLetter((char)pText[start]) && index > 0)
                    {
                        start--;
                    }
                    int end = index;
                    while (char.IsLetter((char)pText[end]) && end < text->Size)
                    {
                        end++;
                    }
                    SetSelection(start + 1, end);
                    SetCursor(end);
                    return;
                }

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    SetCursor(HitTest(source, origin, mousePos, lineHeight));
                    ClearSelection();
                    return;
                }
            }
        }

        public void SetCursor(int index)
        {
            cursorState = CursorState.FromIndex(index, source);
        }

        public void OffsetCursor(int offset)
        {
            cursorState = CursorState.FromIndex(cursorState.Index + offset, source);
        }

        public void SetSelection(int start, int end)
        {
            selection.Text = source.Text;
            selection.Start = CursorState.FromIndex(start, source);
            selection.End = CursorState.FromIndex(end, source);
        }

        public void ClearSelection()
        {
            selection.Start = selection.End = CursorState.Invalid;
        }

        public void EraseSelection()
        {
            if (!selection.IsValid())
            {
                return;
            }

            source.Text->Erase(selection.EffectiveStart.Index, selection.Length);
            source.Update(ImGui.GetTextLineHeight());
            SetCursor(selection.EffectiveStart.Index);
        }

        private void PreEdit()
        {
            // push history.
            history.UndoPush();

            // erase selection
            EraseSelection();
        }

        private void PostEdit()
        {
            // clear selection
            ClearSelection();
        }

        private void HandleKeyboardInput(StdWString* text, ref bool changed, bool isFocused)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if (isFocused)
            {
                if (io.InputQueueCharacters.Size > 0)
                {
                    PreEdit();
                    for (int i = 0; i < io.InputQueueCharacters.Size; i++)
                    {
                        char c = io.InputQueueCharacters.Data[i];
                        if (c >= 32) // Handle regular characters
                        {
                            text->Insert(cursorState, c);
                            cursorState++;
                            changed = true;
                        }
                    }
                    io.InputQueueCharacters.Size = 0; // Clear
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.Backspace) && cursorState > 0) // Handle backspace
                {
                    PreEdit();
                    if (!selection.IsValid() && text->Size > 0)
                    {
                        int index = cursorState - 1;
                        int length = 1;

                        char c = text->At(index);
                        if (c == '\n' || c == '\r')
                        {
                            if (c == '\n' && index - 1 >= 0 && text->At(index - 1) == '\r')
                            {
                                length++;
                                index--;
                                cursorState -= CursorState.NewLineCRLF;
                            }
                            else
                            {
                                cursorState -= CursorState.NewLineCR;
                            }
                        }
                        else
                        {
                            cursorState--;
                        }
                        text->Erase(index, length);
                    }

                    changed = true;
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)) // Handle enter
                {
                    PreEdit();
                    switch (source.NewLineType)
                    {
                        case NewLineType.CRLF:
                            text->Insert(cursorState, '\r');
                            text->Insert(cursorState, '\n');
                            cursorState += CursorState.NewLineCRLF;
                            break;

                        case NewLineType.LF:
                            text->Insert(cursorState, '\n');
                            cursorState += CursorState.NewLineLF;
                            break;

                        case NewLineType.CR:
                            text->Insert(cursorState, '\r');
                            cursorState += CursorState.NewLineCR;
                            break;

                        case NewLineType.Mixed:
                        default:
                            text->Insert(cursorState, '\n');
                            cursorState += CursorState.NewLineLF;
                            break;
                    }
                    cursorState.Column = 0;
                    changed = true;
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.Tab))
                {
                    PreEdit();
                    text->Insert(cursorState, '\t');
                    changed = true;
                    PostEdit();
                }

                if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow) && cursorState > 0)
                {
                    MoveCursorHorizontally(-1);
                }
                if (ImGui.IsKeyPressed(ImGuiKey.RightArrow) && cursorState < text->Size)
                {
                    MoveCursorHorizontally(1);
                }

                if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && cursorState.Line > 0)
                {
                    MoveCursorVertically(-1);
                }

                if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) && cursorState.Line < source.Lines.Count - 1)
                {
                    MoveCursorVertically(1);
                }

                if (io.KeyCtrl && ImGui.IsKeyDown(ImGuiKey.V))
                {
                    if (!pasted)
                    {
                        pasted = true;
                        pasteRepeatTimer = pasteInitialRepeatDelay;
                        InsertTextFromClipboard();
                        changed = true;
                    }
                    else
                    {
                        pasteRepeatTimer -= ImGui.GetIO().DeltaTime;
                        if (pasteRepeatTimer <= 0.0f)
                        {
                            pasteRepeatTimer = pasteRepeatDelay;
                            InsertTextFromClipboard();
                            changed = true;
                        }
                    }
                }
                else
                {
                    pasted = false;
                    pasteRepeatTimer = 0.0f;
                }

                if (io.KeyCtrl && ImGui.IsKeyDown(ImGuiKey.C) && selection.IsValid())
                {
                    CopySelectionToClipboard();
                }

                if (io.KeyCtrl && ImGui.IsKeyDown(ImGuiKey.X) && selection.IsValid())
                {
                    CopySelectionToClipboard();
                    PreEdit();
                    PostEdit();
                    changed = true;
                }
            }
        }

        public void CopySelectionToClipboard()
        {
            var text = source.Text;
            var subString = text->SubString(selection.EffectiveStart.Index, selection.Length);
            ImGui.SetClipboardText(subString.ToString());
            subString.Release();
        }

        public void ShowFind()
        {
            showFindWindow = true;
        }

        public void Undo()
        {
            if (history.CanUndo)
            {
                history.Undo();
                needsUpdate = true;
            }
        }

        public void Redo()
        {
            if (history.CanRedo)
            {
                history.Redo();
                needsUpdate = true;
            }
        }

        private void MoveCursorHorizontally(int index)
        {
            OffsetCursor(index);
        }

        private void MoveCursorVertically(int offset)
        {
            var lineIndex = cursorState.Line + offset;
            var line = source.Lines[lineIndex];
            var column = Math.Min(line.Length, cursorState.Column);
            var index = line.Start + column;
            cursorState = new(index, lineIndex, column);
        }

        private bool pasted = false;
        private readonly float pasteRepeatDelay = 0.1f; // adjust this value to control the repeat delay
        private readonly float pasteInitialRepeatDelay = 0.5f; // adjust this value to control the repeat delay
        private float pasteRepeatTimer = 0.0f;

        public void InsertTextFromClipboard()
        {
            var text = source.Text;
            PreEdit();
            var clipboardText = ImGui.GetClipboardText();
            int textSize = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(clipboardText).Length;
            text->InsertRange(cursorState, clipboardText, textSize);
            source.Update(ImGui.GetTextLineHeight());
            OffsetCursor(textSize);
            PostEdit();
        }

        private void DrawText(ImDrawList* drawList, Vector2 origin, char* pText)
        {
            for (int i = 0; i < highlightSpans.Count; i++)
            {
                var lineSpan = highlightSpans[i];

                if (lineSpan.HasColor)
                {
                    ImGuiWChar.AddText(drawList, origin + lineSpan.Origin, lineSpan.Color, pText + lineSpan.Start, pText + lineSpan.End);
                }
                else
                {
                    ImGuiWChar.AddText(drawList, origin + lineSpan.Origin, 0xffffffff, pText + lineSpan.Start, pText + lineSpan.End);
                }
            }
        }

        private void DrawSelection(ImDrawList* drawList, Vector2 origin, TextSource source, float lineHeight)
        {
            if (!selection.IsValid())
            {
                return;
            }

            const uint selectionColor = 0x99FF9933;  // Light blue with transparency in ABGR format

            var start = selection.Start;
            var end = selection.End;

            if (start.Index > end.Index)
            {
                // swap.
                (start, end) = (end, start);
            }

            int startLine = start.Line;
            int endLine = end.Line;

            for (int lineIndex = startLine; lineIndex <= endLine; lineIndex++)
            {
                var line = source.Lines[lineIndex];
                Vector2 quadOrigin = origin + new Vector2(0, lineHeight * lineIndex);

                if (lineIndex == startLine)
                {
                    // Selection starts on this line
                    float startX = ImGuiWChar.CalcTextSize(line.Data, line.Data + start.Column).X;
                    float endX = (lineIndex == endLine)
                        ? ImGuiWChar.CalcTextSize(line.Data, line.Data + end.Column).X
                        : ImGuiWChar.CalcTextSize(line.Data, line.DataEnd).X;

                    Vector2 topLeft = quadOrigin + new Vector2(startX, 0);
                    Vector2 bottomLeft = quadOrigin + new Vector2(startX, lineHeight);
                    Vector2 topRight = quadOrigin + new Vector2(endX, 0);
                    Vector2 bottomRight = quadOrigin + new Vector2(endX, lineHeight);

                    drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, selectionColor);
                }
                else if (lineIndex == endLine)
                {
                    // Selection ends on this line
                    float endX = ImGuiWChar.CalcTextSize(line.Data, line.Data + end.Column).X;

                    Vector2 topLeft = quadOrigin;
                    Vector2 bottomLeft = quadOrigin + new Vector2(0, lineHeight);
                    Vector2 topRight = quadOrigin + new Vector2(endX, 0);
                    Vector2 bottomRight = quadOrigin + new Vector2(endX, lineHeight);

                    drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, selectionColor);
                }
                else
                {
                    // Full line selection
                    float lineWidth = ImGuiWChar.CalcTextSize(line.Data, line.DataEnd).X;

                    Vector2 topLeft = quadOrigin;
                    Vector2 bottomLeft = quadOrigin + new Vector2(0, lineHeight);
                    Vector2 topRight = quadOrigin + new Vector2(lineWidth, 0);
                    Vector2 bottomRight = quadOrigin + new Vector2(lineWidth, lineHeight);

                    drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, selectionColor);
                }
            }
        }

        private bool cursorVisible = true;
        private readonly float cursorBlinkInterval = 0.5f;
        private float cursorBlinkTimer = 0.0f;
        private bool showFindWindow;

        private void DrawCursorLine(ImDrawList* drawList, TextSource source, float lineHeight, Vector2 origin, Vector2 avail, CursorState cursorState)
        {
            if (isFocused)
            {
                float cursorX = origin.X;
                float cursorY = origin.Y;

                if (cursorState.Line < source.Lines.Count)
                {
                    var lineSpan = source.Lines[cursorState.Line];

                    cursorX = origin.X + ImGuiWChar.CalcTextSize(lineSpan.Data, lineSpan.Data + cursorState.Column).X;
                    cursorY = origin.Y + cursorState.Line * lineHeight;
                }
                else if (source.Lines.Count > 0)
                {
                    TextSpan lastLineSpan = source.Lines[^1];
                    cursorX = origin.X + ImGuiWChar.CalcTextSize(lastLineSpan.Data, lastLineSpan.Data + lastLineSpan.Length).X;
                    cursorY = origin.Y + (source.LineCount - 1) * lineHeight;
                }

                Vector2 quadOrigin = origin + new Vector2(0, lineHeight * cursorState.Line);
                Vector2 topLeft = quadOrigin;
                Vector2 bottomLeft = quadOrigin + new Vector2(0, lineHeight);
                Vector2 topRight = quadOrigin + new Vector2(avail.X, 0);
                Vector2 bottomRight = quadOrigin + new Vector2(avail.X, lineHeight);

                drawList->AddQuadFilled(topLeft, topRight, bottomRight, bottomLeft, 0xff2c2c2c);

                cursorBlinkTimer -= ImGui.GetIO().DeltaTime;

                if (cursorBlinkTimer < 0)
                {
                    cursorBlinkTimer = cursorBlinkInterval;
                    cursorVisible = !cursorVisible;
                }

                if (cursorVisible)
                {
                    drawList->AddLine(new(cursorX, cursorY), new(cursorX, cursorY + lineHeight), 0xffffffff);
                }
            }
        }

        private static int HitTest(TextSource source, Vector2 origin, Vector2 mousePos, float lineHeight)
        {
            char* pText = source.Text->Data;
            Vector2 relativeMousePos = mousePos - origin;

            int lineIndex = (int)MathF.Floor(relativeMousePos.Y / lineHeight);

            if (lineIndex < 0 || lineIndex >= source.Lines.Count)
            {
                return source.Text->Size;
            }

            TextSpan lineSpan = source.Lines[lineIndex];

            for (int j = lineSpan.Start; j < lineSpan.End; j++)
            {
                float last = ImGuiWChar.CalcTextSize(pText + lineSpan.Start, pText + j).X;
                float penX = ImGuiWChar.CalcTextSize(pText + lineSpan.Start, pText + j + 1).X;

                if (penX > relativeMousePos.X)
                {
                    if (Math.Abs(penX - relativeMousePos.X) < Math.Abs(last - relativeMousePos.X))
                    {
                        return j + 1;
                    }

                    return j;
                }
            }

            return lineSpan.End;
        }

        public void Dispose()
        {
            source.Dispose();
            history.Dispose();
        }
    }
}