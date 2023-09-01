namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using ImGuiNET;

    public class TerminalWindow : EditorWindow
    {
        private readonly List<ITerminal> terminals = new();
        private readonly List<string> terminalNames = new();

        public TerminalWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar;
            Show();
            terminals.Add(new OutputTerminal());
            terminalNames.Add("ao");
            terminals.Add(new ShellTerminal("cmd.exe"));
            terminalNames.Add("Cmd");
            terminals.Add(new ShellTerminal("powershell.exe"));
            terminalNames.Add("Powershell");
        }

        protected override string Name => "Terminal";

        public override void DrawContent(IGraphicsContext context)
        {
            ImGui.BeginTabBar("Terminals", ImGuiTabBarFlags.Reorderable);

            for (int i = 0; i < terminals.Count; i++)
            {
                var terminal = terminals[i];
                var name = terminalNames[i];
                var open = true;

                if (ImGui.BeginTabItem(name, ref open))
                {
                    terminal.Draw();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
}