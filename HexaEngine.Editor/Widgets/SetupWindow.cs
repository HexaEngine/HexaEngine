namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using HexaEngine.Editor.Icons;
    using System;
    using System.Numerics;

    public class SetupWindow : Dialogs.Modal
    {
        private int page = 0;
        private bool first = true;
        private const int pageCount = 3;

        public override string Name { get; } = "Getting started";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar;

        public override unsafe void Draw()
        {
            Vector2 main_viewport_pos = ImGui.GetMainViewport().Pos;
            Vector2 main_viewport_size = ImGui.GetMainViewport().Size;
            ImGui.SetNextWindowPos(main_viewport_pos);
            ImGui.SetNextWindowSize(main_viewport_size);
            ImGui.SetNextWindowBgAlpha(0.9f);
            ImGui.Begin("Overlay", null, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs);
            ImGui.End();

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
            ImGui.SeparatorText(Name);
            Vector2 avail = ImGui.GetContentRegionAvail();
            const float footerHeight = 50;
            avail.Y -= footerHeight;
            ImGui.BeginChild("Content", avail);

            Icon icon = IconManager.GetIconByName("Logo") ?? throw new();
            Vector2 imageSize = new(48);
            icon.Image(imageSize);
            ImGui.SameLine();

            switch (page)
            {
                case 0:
                    Page1();
                    break;

                case 1:
                    Page2();
                    break;

                case 2:
                    Page3();
                    break;
            }

            ImGui.EndChild();

            ImGui.BeginTable("#Table", 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("");

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);

            if (page > 0)
            {
                if (ImGui.Button("Back"))
                {
                    page--;
                }
                ImGui.SameLine();
            }

            if (page == pageCount - 1)
            {
                if (ImGui.Button("Finish"))
                {
                    FinishSetup();
                }
            }
            else
            {
                if (ImGui.Button("Next"))
                {
                    page++;
                }
            }

            ImGui.EndTable();
        }

        private void FinishSetup()
        {
            EditorConfig config = EditorConfig.Default;
            Directory.CreateDirectory(projectsFolder);
            config.ProjectsFolder = projectsFolder;
            config.SetupDone = true;
            config.Save();
            Close();
        }

        private static void Page1()
        {
            ImGui.Text("Welcome to HexaEngine");

            ImGui.Dummy(new(0, 20));

            ImGui.Indent(48);

            ImGui.Text("This game engine is still in a experimental stage, expect some bugs or breaking changes.");

            ImGui.Unindent();
        }

        private string projectsFolder = DetermineDefaultProjectsPath();

        private static string DetermineDefaultProjectsPath()
        {
            string projectsPath;
            if (OperatingSystem.IsWindows())
            {
                projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexaEngine", "Projects");
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                projectsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HexaEngine", "Projects");
            }
            else
            {
                throw new PlatformNotSupportedException("HexaEngine currently supports only Windows, Linux, and macOS platforms.");
            }
            return projectsPath;
        }

        private void Page2()
        {
            ImGui.Text($"Step {page}: Setting a projects folder");

            ImGui.Dummy(new(0, 20));

            ImGui.Indent(48);

            ImGui.Text("Projects folder");
            ImGui.InputText("##TextInputProjectsFolder", ref projectsFolder, 1024);
            ImGui.SameLine();
            if (ImGui.Button("..."))
            {
                OpenFileDialog dialog = new();
                dialog.OnlyAllowFolders = true;
                dialog.Show((s, e) =>
                {
                    if (e != DialogResult.Ok) return;
                    projectsFolder = ((OpenFileDialog)s!).SelectedFile!;
                    Show();
                });
            }

            ImGui.Unindent();
        }

        private static void Page3()
        {
            ImGui.Text("Done!");

            ImGui.Dummy(new(0, 20));

            ImGui.Indent(48);

            ImGui.Text("Links:");
            ImGui.Indent();
            if (ImGui.MenuItem($"{UwU.Github} HexaEngine on GitHub"))
            {
                Designer.OpenLink("https://github.com/HexaEngine/HexaEngine");
            }

            if (ImGui.MenuItem($"{UwU.Book} HexaEngine Documentation"))
            {
                Designer.OpenLink("https://hexaengine.github.io/HexaEngine/");
            }

            if (ImGui.MenuItem($"{UwU.Bug} Report a bug"))
            {
                Designer.OpenLink("https://github.com/HexaEngine/HexaEngine/issues");
            }

            if (ImGui.MenuItem($"{UwU.Discord} Join our Discord"))
            {
                Designer.OpenLink("https://discord.gg/VawN5d8HMh");
            }

            ImGui.Unindent();

            ImGui.Unindent();
        }

        public override void Reset()
        {
        }
    }
}