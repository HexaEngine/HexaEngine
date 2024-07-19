using Hexa.NET.ImGui;

namespace HexaEngine.Editor.TextEditor.Panels
{
    public class ExplorerSidePanel : SidePanel
    {
        public override string Icon { get; } = $"{UwU.File}";

        public override string Title { get; } = "Explorer";

        public override void DrawContent()
        {
            ImGui.Text("Hier könnte ihre werbung stehen!");
        }
    }
}