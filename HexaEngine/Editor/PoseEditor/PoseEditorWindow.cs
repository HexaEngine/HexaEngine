﻿namespace HexaEngine.Editor.PoseEditor
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.ImGuiNET;
    using System.Data.SqlTypes;

    public class PoseEditorWindow : EditorWindow
    {
        private readonly Sequencer sequencer = new();

        public PoseEditorWindow()
        {
            Flags = ImGuiWindowFlags.MenuBar;
            IsShown = true;
            sequencer.Show();
        }

        protected override string Name { get; } = "Pose Editor";

        public override void DrawContent(IGraphicsContext context)
        {
            sequencer.DrawWindow(context);
        }
    }
}