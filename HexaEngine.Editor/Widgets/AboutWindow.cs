namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Editor.Dialogs;
    using System.Numerics;
    using System.Reflection;

    public class AboutWindow : Modal
    {
        private bool first = false;

        public override string Name { get; } = "About";
        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.AlwaysAutoResize;

        public override unsafe void Draw()
        {
            if (first)
            {
                ImGui.SetNextWindowSize(new(800, 500));
                Vector2 size = new(800, 500);
                Vector2 mainViewportPos = ImGui.GetMainViewport().Pos;
                var s = ImGui.GetPlatformIO().Monitors.Data[0].MainSize;

                ImGui.SetNextWindowPos(mainViewportPos + (s / 2 - size / 2));
                first = false;
            }
            base.Draw();
        }

        protected override void DrawContent()
        {
            ImGui.Text($"HexaEngine {Assembly.GetExecutingAssembly().GetName().Version}");

            ImGui.Dummy(new(0, 20));

            ImGui.Indent();
            if (ImGui.MenuItem("\xE734 HexaEngine on GitHub")) Designer.OpenLink("https://github.com/HexaEngine/HexaEngine");
            if (ImGui.MenuItem("\xE82D HexaEngine Documentation")) Designer.OpenLink("https://hexaengine.github.io/HexaEngine/");
            if (ImGui.MenuItem("\xEBE8 Report a bug")) Designer.OpenLink("https://github.com/HexaEngine/HexaEngine/issues");
            if (ImGui.MenuItem("\xE939 Join our Discord")) Designer.OpenLink("https://discord.gg/VawN5d8HMh");
            ImGui.Unindent();

            ImGui.Dummy(new(0, 20));

            ImGui.Text("Licenced under MIT");
            ImGui.Text("Copyright (c) 2022 Juna Meinhold");
        }

        public override void Reset()
        {
            first = true;
        }
    }
}