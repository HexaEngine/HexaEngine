namespace HexaEngine.Editor.TextEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using HexaEngine.UI.Graphics.Text;
    using System.IO;
    using System.Numerics;

    [EditorWindowCategory("Tools")]
    public class TextEditorWindow : EditorWindow
    {
        internal static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(TextEditor));
        private readonly OpenFileDialog openDialog = new();
        private readonly SaveFileDialog saveDialog = new();

        private readonly List<TextEditorTab> tabs = new();
        private TextEditorTab? currentTab;

        public TextEditorWindow()
        {
            IsShown = true;
            Flags = ImGuiWindowFlags.MenuBar;
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

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            DrawWindows();
            DrawMenuBar();
            HandleShortcuts();

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
                    tab.Draw(context);
                }
                ImGui.EndTabBar();
            }
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