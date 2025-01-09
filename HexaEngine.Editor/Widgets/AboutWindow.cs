namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using HexaEngine.Editor.Dialogs;
    using HexaEngine.Editor.Extensions;
    using HexaEngine.Editor.Icons;
    using System.Numerics;
    using System.Reflection;

    public class AboutWindow : Modal
    {
        private static readonly string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
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

        protected override unsafe void DrawContent()
        {
            byte* buffer = stackalloc byte[2048];
            StrBuilder builder = new(buffer, 2048);

            Icon icon = IconManager.GetIconByName("Logo") ?? throw new();

            icon.ImageCenteredH(new(64));

            builder.Reset();
            builder.Append("HexaEngine "u8);
            builder.Append(version);
            builder.End();
            ImGui.Text(builder);

            ImGui.Dummy(new(0, 20));

            ImGui.Indent();
            if (ImGui.MenuItem(builder.BuildLabel(UwU.Github, " HexaEngine on GitHub"u8)))
            {
                Designer.OpenLink("https://github.com/HexaEngine/HexaEngine");
            }

            if (ImGui.MenuItem(builder.BuildLabel(UwU.Book, " HexaEngine Documentation"u8)))
            {
                Designer.OpenLink("https://hexaengine.github.io/HexaEngine/");
            }

            if (ImGui.MenuItem(builder.BuildLabel(UwU.Bug, " Report a bug"u8)))
            {
                Designer.OpenLink("https://github.com/HexaEngine/HexaEngine/issues");
            }

            if (ImGui.MenuItem(builder.BuildLabel(UwU.Discord, " Join our Discord"u8)))
            {
                Designer.OpenLink("https://discord.gg/VawN5d8HMh");
            }

            ImGui.Unindent();

            ImGui.Dummy(new(0, 20));

            ImGui.Text("Licenced under MIT"u8);
            ImGui.Text("Copyright (c) 2022 Juna Meinhold"u8);
        }

        public override void Reset()
        {
            first = true;
        }
    }
}