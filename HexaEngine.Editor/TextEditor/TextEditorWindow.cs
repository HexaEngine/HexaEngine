namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Dialogs;
    using System.IO;

    [EditorWindowCategory("Tools")]
    public class TextEditorWindow : EditorWindow
    {
        private readonly OpenFileDialog openDialog = new();
        private readonly SaveFileDialog saveDialog = new();

        private readonly List<TextEditorTab> tabs = new();
        private TextEditorTab? currentTab;

        public TextEditorWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.HorizontalScrollbar;
        }

        protected override string Name => "Text Editor";

        public IReadOnlyList<TextEditorTab> Tabs => tabs;

        public TextEditorTab? CurrentTab => currentTab;

        public void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("\xE710 New"))
                    {
                        New();
                    }

                    if (ImGui.MenuItem("\xE845 Open"))
                    {
                        openDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("\xE74E Save"))
                    {
                        Save();
                    }

                    if (ImGui.MenuItem("\xE74E Save As"))
                    {
                        saveDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("\xE8BB Close"))
                    {
                        if (currentTab != null)
                        {
                            tabs.Remove(currentTab);
                            currentTab = null;
                        }
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("\xE7A7 Undo"))
                    {
                        currentTab?.History.Undo();
                    }

                    if (ImGui.MenuItem("\xE7A6 Redo"))
                    {
                        currentTab?.History.Redo();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        public void DrawWindows()
        {
            if (openDialog.Draw())
            {
                if (openDialog.Result == OpenFileResult.Ok)
                {
                    Open(openDialog.FullPath);
                }
            }

            if (saveDialog.Draw())
            {
                if (saveDialog.Result == SaveFileResult.Ok)
                {
                    SaveAs(saveDialog.FullPath);
                }
            }
        }

        public void Open(string path)
        {
            try
            {
                TextEditorTab tab = new(new(File.ReadAllText(path)), path);
                tabs.Add(tab);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load text file!");
                Logger.Log(ex);
                MessageBox.Show("Failed to load text file!", ex.Message);
            }
        }

        public void New()
        {
            TextEditorTab tab = new("New File", new(string.Empty));
            tabs.Add(tab);
        }

        public void Save()
        {
            if (currentTab?.CurrentFile != null)
            {
                SaveAs(currentTab.CurrentFile);
            }
        }

        public unsafe void SaveAs(string path)
        {
            if (currentTab == null)
            {
                return;
            }
            try
            {
                File.WriteAllText(path, currentTab.Source.Text->ToString());
                currentTab.Source.Changed = false;
                currentTab.CurrentFile = path;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save text file!");
                Logger.Log(ex);
                MessageBox.Show("Failed to save text file!", ex.Message);
            }
        }

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            DrawWindows();
            DrawMenuBar();

            if (ImGui.BeginTabBar("##TextEditor"))
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    var tab = tabs[i];
                    if (tab.IsFocused)
                    {
                        currentTab = tab;
                    }
                    if (!tab.IsOpen)
                    {
                        tab.Dispose();
                        tabs.RemoveAt(i);
                        i--;
                    }
                    tab.Draw();
                }
                ImGui.EndTabBar();
            }
        }
    }
}