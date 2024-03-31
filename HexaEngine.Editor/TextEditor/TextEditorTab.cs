namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using HexaEngine.Core.Unsafes;
    using System.IO;
    using System.Numerics;

    public unsafe class TextEditorTab
    {
        private readonly ImGuiName name;
        private readonly ImGuiInputTextCallback callback;
        private bool open = true;
        private bool isFocused = false;
        private TextSource source;
        private TextHistory history;
        private bool jump;
        private float scroll;

        public TextEditorTab(string tabName, TextSource source)
        {
            name = new(tabName);
            this.source = source;
            history = new(source, 1024);
            callback = TextCallback;
        }

        public TextEditorTab(TextSource source, string currentFile)
        {
            name = new(Path.GetFileName(currentFile));
            CurrentFile = currentFile;
            this.source = source;
            history = new(source, 1024);
            callback = TextCallback;
        }

        public string TabName => name.Name;

        public string? CurrentFile { get; set; }

        public TextSource Source { get => source; set => source = value; }

        public TextHistory History { get => history; set => history = value; }

        public bool IsOpen { get => open; set => open = value; }

        public bool IsFocused => isFocused;

        private List<TextSpan> spans = new();

        public void Draw()
        {
            isFocused = false;
            ImGuiTabItemFlags flags = source.Changed ? ImGuiTabItemFlags.UnsavedDocument : ImGuiTabItemFlags.None;
            if (ImGui.BeginTabItem(name.UniqueName, ref open, flags))
            {
                isFocused = true;
                StdString* text = source.Text;

                SyntaxHighlightDefaults.CSharp.Analyse(text, spans);

                ImGui.BeginChild("LineNumbers", new Vector2(40, 0), ImGuiChildFlags.None, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                ImGui.SetScrollY(scroll);

                for (int line = 1; line <= source.LineCount; line++)
                {
                    ImGui.TextV("%d", (nuint)(&line));
                }

                ImGui.EndChild();

                ImGui.SameLine();

                if (jump)
                {
                    int targetLine = 10;
                    float lineHeight = ImGui.GetTextLineHeight();
                    ImGui.SetNextWindowScroll(new(0, targetLine * lineHeight));
                }

                var id = ImGui.GetID("##TextEdit");

                bool changed = ImGui.InputTextMultiline("##TextEdit", text->Data, (nuint)text->Capacity + 1, new Vector2(-1), ImGuiInputTextFlags.AllowTabInput | ImGuiInputTextFlags.CallbackResize | ImGuiInputTextFlags.CallbackHistory, callback, text);
                var inw = ImGui.GetScrollY();

                var state = ImGui.GetCurrentWindow();

                if (changed)
                {
                    source.Changed = true;
                    history.UndoPush();
                }

                ImGui.EndTabItem();
            }
        }

        private unsafe int TextCallback(ImGuiInputTextCallbackData* data)
        {
            switch (data->EventFlag)
            {
                case ImGuiInputTextFlags.CallbackHistory:
                    break;

                case ImGuiInputTextFlags.CallbackResize:
                    // Resize string callback
                    StdString* str = (StdString*)data->UserData;
                    Logger.Assert(str->Data == data->Buf);
                    str->Resize(data->BufTextLen);
                    data->Buf = str->CStr();
                    break;
            }
            return 0;
        }

        public void Dispose()
        {
            source.Dispose();
        }
    }
}