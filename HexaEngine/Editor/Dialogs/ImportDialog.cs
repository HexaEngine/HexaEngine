namespace HexaEngine.Editor.Dialogs
{
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Scenes.Importer;
    using ImGuiNET;
    using System.Numerics;

    public class ImportDialog : DialogBase, IDialog
    {
        private AssimpSceneImporter importer = new();
        private OpenFileDialog dialog = new();
        private bool loaded = false;
        private Task? loadTask;

        public override string Name => "Import Scene";

        protected override ImGuiWindowFlags Flags { get; }

        public override void Reset()
        {
            importer.Clear();
            dialog.Show();
        }

        protected override void DrawContent()
        {
            if (!loaded)
            {
                if (dialog.Draw())
                {
                    if (dialog.Result != OpenFileResult.Ok)
                    {
                        Hide();
                        return;
                    }

                    loadTask = importer.LoadAsync(dialog.SelectedFile).ContinueWith(LoadTaskDone);
                }
            }
            else
            {
                if (ImGui.Button("Import"))
                {
                    if (!importer.CheckForProblems())
                    {
                        importer.Import(SceneManager.Current);

                        Hide();
                        Reset();
                    }
                }
                if (ImGui.BeginTabBar("ImporterTabs"))
                {
                    if (ImGui.BeginTabItem("Meshes"))
                    {
                        for (int i = 0; i < importer.Meshes.Length; i++)
                        {
                            var mesh = importer.Meshes[i];
                            var value = mesh.Path;
                            var invalid = value.Length > 255;
                            if (invalid)
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                            if (ImGui.InputText($"Mesh {i}", ref value, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                            {
                                importer.ChangeNameOfMesh(mesh, value);
                            }
                            if (invalid)
                                ImGui.PopStyleColor();
                        }
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
        }

        private void LoadTaskDone(Task task)
        {
            loaded = task.IsCompletedSuccessfully;
        }
    }
}