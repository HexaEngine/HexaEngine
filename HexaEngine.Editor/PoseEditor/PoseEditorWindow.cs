namespace HexaEngine.Editor.PoseEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;

    [EditorWindowCategory("Tools")]
    public class PoseEditorWindow : EditorWindow
    {
        private readonly Sequencer sequencer = new();

        public PoseEditorWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar;
            sequencer.Show();
        }

        protected override string Name { get; } = "Pose Editor";

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New"))
                    {
                        sequencer.Animation = new();
                        sequencer.Animation.NodeChannels.Add(new("Test"));
                    }

                    if (ImGui.MenuItem("Open"))
                    {
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Close"))
                    {
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Save"))
                    {
                    }

                    if (ImGui.MenuItem("Save As"))
                    {
                    }

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMenuBar();
        }

        public override void DrawContent(IGraphicsContext context)
        {
            sequencer.DrawWindow(context);

            DrawMenuBar();
        }
    }
}