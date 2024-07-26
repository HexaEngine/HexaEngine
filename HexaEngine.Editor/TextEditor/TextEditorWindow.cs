namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Logging;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.TextEditor.Panels;
    using System.IO;
    using System.Numerics;

    [EditorWindowCategory("Tools")]
    public class TextEditorWindow : EditorWindow
    {
        internal static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(TextEditor));
        private readonly OpenFileDialog openDialog = new();
        private readonly SaveFileDialog saveDialog = new();

        private readonly List<TextEditorTab> tabs = new();
        private readonly List<SidePanel> sidePanels = new();
        private TextEditorTab? currentTab;

        public TextEditorWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar;
            sidePanels.Add(new ExplorerSidePanel());
        }

        protected override string Name => "Text Editor";

        public IReadOnlyList<TextEditorTab> Tabs => tabs;

        public TextEditorTab? CurrentTab => currentTab;

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem($"{UwU.CreateFile} New"))
                    {
                        New();
                    }

                    if (ImGui.MenuItem($"{UwU.OpenFile} Open"))
                    {
                        openDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem($"{UwU.FloppyDisk} Save", "Ctrl+S"))
                    {
                        Save();
                    }

                    if (ImGui.MenuItem($"{UwU.ShareFromSquare} Save As"))
                    {
                        saveDialog.Show();
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem($"{UwU.Xmark} Close"))
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
                    if (ImGui.MenuItem($"{UwU.RotateLeft} Undo", "Ctrl+Z"))
                    {
                        currentTab?.Undo();
                    }

                    if (ImGui.MenuItem($"{UwU.RotateRight} Redo", "Ctrl+Y"))
                    {
                        currentTab?.Redo();
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

        [Profiling.Profile]
        public override unsafe void DrawContent(IGraphicsContext context)
        {
            DrawWindows();
            DrawMenuBar();
            HandleShortcuts();

            Vector2 cursor = ImGui.GetCursorPos();
            Vector2 avail = ImGui.GetContentRegionAvail();

            float actualSidePanelWidth = sidePanelCollapsed ? 0 : sidePanelWidth;

            if (ImGui.BeginTabBar("##TextEditor"))
            {
                Vector2 cursorTab = ImGui.GetCursorPos();
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

                    Vector2 size = new(avail.X - sideBarWidth - actualSidePanelWidth - resizeBarWidth, avail.Y - (cursorTab.Y - cursor.Y));
                    tab.Draw(context, size);
                }
                ImGui.EndTabBar();
            }

            ImGui.SetCursorPos(cursor + new Vector2(avail.X - sideBarWidth - actualSidePanelWidth - resizeBarWidth, 0));

            ImGui.InvisibleButton("ResizeBar", new Vector2(resizeBarWidth, avail.Y));

            if (ImGui.IsItemHovered())
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);

            if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                sidePanelWidth -= ImGui.GetIO().MouseDelta.X;
                sidePanelWidth = Math.Max(sidePanelWidth, 0);
                sidePanelCollapsed = sidePanelWidth <= 100;
            }

            ImGui.SetCursorPos(cursor + new Vector2(avail.X - sideBarWidth - actualSidePanelWidth + resizeBarWidth, 0));
            DrawSidePanel();

            ImGui.SetCursorPos(cursor + new Vector2(avail.X - sideBarWidth + resizeBarWidth, 0));
            DrawSideBar();
        }

        private unsafe void DrawSidePanel()
        {
            if (sidePanelCollapsed)
            {
                return;
            }

            if (activeSidePanel < 0 && activeSidePanel >= sidePanels.Count)
            {
                return;
            }

            ImGui.BeginChild("##SidePanel", new Vector2(sidePanelWidth, 0));
            var cursor = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail();
            var max = cursor + new Vector2(sidePanelWidth, avail.Y);
            ImDrawList* drawList = ImGui.GetWindowDrawList();
            drawList->AddRectFilled(cursor, max, 0xff1c1c1c);

            sidePanels[activeSidePanel].Draw();

            ImGui.EndChild();
        }

        private const float resizeBarWidth = 2.0f;
        private const float sideBarWidth = 40f;
        private float sidePanelWidth = 200f;
        private bool sidePanelCollapsed = false;
        private int activeSidePanel;

        private unsafe void DrawSideBar()
        {
            ImGui.BeginChild("##SideBar", new Vector2(sideBarWidth, 0));
            var cursor = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail();
            var max = cursor + new Vector2(sideBarWidth, avail.Y);
            ImDrawList* drawList = ImGui.GetWindowDrawList();
            drawList->AddRectFilled(cursor, max, 0xff2c2c2c);

            for (int i = 0; i < sidePanels.Count; i++)
            {
                var sidePanel = sidePanels[i];
                if (ImGui.Button(sidePanel.Icon))
                {
                    activeSidePanel = i;
                    sidePanelWidth = sidePanelWidth <= 100 ? 200f : sidePanelWidth;
                    sidePanelCollapsed = false;
                }
            }
            ImGui.EndChild();
        }

        private void HandleShortcuts()
        {
            if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.S)))
            {
                Save();
            }

            if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.Z)))
            {
                CurrentTab?.Undo();
            }

            if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.Y)))
            {
                CurrentTab?.Redo();
            }

            if (ImGui.Shortcut((int)(ImGuiKey.ModCtrl | ImGuiKey.F)))
            {
                CurrentTab?.ShowFind();
            }
        }

        protected override void DisposeCore()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].Dispose();
            }
        }
    }
}