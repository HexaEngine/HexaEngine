namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Projects;
    using System;
    using System.Threading.Tasks;

    public class PublishProjectWindow : EditorWindow
    {
        private readonly PublishSettings options = new();
        private static readonly string[] profiles = { "Release", "Debug" };
        private static readonly string[] rids = { "win-x64", "linux-x64" };
        private Task? task;

        protected override string Name => $"{UwU.Upload} Publish";

        public override void DrawContent(IGraphicsContext context)
        {
            bool disable = task != null && !task.IsCompleted;
            if (disable)
            {
                ImGui.BeginDisabled(true);
            }

            {
                int index = Array.IndexOf(profiles, options.Profile);
                if (ImGui.Combo("Profile", ref index, profiles, profiles.Length))
                {
                    options.Profile = profiles[index];
                }
            }

            {
                int index = Array.IndexOf(rids, options.RuntimeIdentifier);
                if (ImGui.Combo("Runtime", ref index, rids, rids.Length))
                {
                    options.RuntimeIdentifier = rids[index];
                }
            }

            {
                bool value = options.SingleFile;
                if (ImGui.Checkbox("Single file", ref value))
                {
                    options.SingleFile = value;
                }
            }

            {
                bool value = options.ReadyToRun;
                if (ImGui.Checkbox("Ready to Run", ref value))
                {
                    options.ReadyToRun = value;
                }
            }

            {
                bool value = options.StripDebugInfo;
                if (ImGui.Checkbox("Strip Debug Info", ref value))
                {
                    options.StripDebugInfo = value;
                }
            }

            {
                string[] scenes = options.Scenes.ToArray();
                int index = Array.IndexOf(scenes, options.StartupScene);
                if (ImGui.Combo("Startup Scene", ref index, scenes, scenes.Length))
                {
                    options.StartupScene = scenes[index];
                }
            }
            {
                if (ImGui.BeginListBox("Scenes"))
                {
                    for (int i = 0; i < options.Scenes.Count; i++)
                    {
                        ImGui.Text(options.Scenes[i]);
                    }
                    ImGui.EndListBox();
                }

                if (ImGui.Button("Add Scene"))
                {
                    OpenFileDialog dialog = new("", ".hexlvl");
                    dialog.CurrentFolder = (ProjectManager.CurrentProjectAssetsFolder ?? string.Empty);
                    dialog.Show(AddSceneCallback);
                }
            }

            if (ImGui.Button("Publish"))
            {
                task = Task.Run(() => ProjectManager.Publish(options));
            }

            if (disable)
            {
                ImGui.EndDisabled();
            }
        }

        private void AddSceneCallback(object? sender, DialogResult result)
        {
            if (result != DialogResult.Ok || sender is not OpenFileDialog dialog) return;
            options.Scenes.Add(Path.GetFileName(dialog.SelectedFile!));
        }
    }
}