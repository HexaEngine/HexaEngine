﻿namespace HexaEngine.Editor.Dialogs
{
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Scenes.Importer;
    using ImGuiNET;
    using Silk.NET.Assimp;
    using System.Numerics;

    public class ImportDialog : DialogBase, IDialog
    {
        private AssimpSceneImporter importer = new();
        private string path = string.Empty;
        private OpenFileDialog dialog = new();
        private bool loaded = false;
        private Task? loadTask;

        public override string Name => "Import Scene";

        protected override ImGuiWindowFlags Flags { get; }

        public override void Reset()
        {
            importer.Clear();
        }

        protected override void DrawContent()
        {
            if (dialog.Draw())
            {
                if (dialog.Result == OpenFileResult.Ok)
                {
                    path = dialog.SelectedFile;
                }
            }

            if (!loaded)
            {
                if (ImGui.InputText("Path", ref path, 2048))
                {
                    dialog.SelectedFile = path;
                }
                ImGui.SameLine();
                if (ImGui.Button("..."))
                {
                    dialog.Show();
                }
                var flags = (int)importer.PostProcessSteps;
                ImGui.CheckboxFlags("Join Identical Vertices", ref flags, (int)PostProcessSteps.JoinIdenticalVertices);
                ImGui.CheckboxFlags("Optimize Meshes", ref flags, (int)PostProcessSteps.OptimizeMeshes);
                ImGui.CheckboxFlags("Optimize Graph", ref flags, (int)PostProcessSteps.OptimizeGraph);
                ImGui.CheckboxFlags("Improve Cache Locality", ref flags, (int)PostProcessSteps.ImproveCacheLocality);
                ImGui.CheckboxFlags("Find Degenerates", ref flags, (int)PostProcessSteps.FindDegenerates);
                ImGui.CheckboxFlags("Find Invalid Data", ref flags, (int)PostProcessSteps.FindInvalidData);
                ImGui.CheckboxFlags("Find Instances", ref flags, (int)PostProcessSteps.FindInstances);
                importer.PostProcessSteps = (PostProcessSteps)flags;

                if (ImGui.Button("Load"))
                {
                    Task.Factory.StartNew(() => importer.LoadAsync(path).ContinueWith(x => { loaded = true; }));
                }
            }
            else
            {
                if (ImGui.Button("Import"))
                {
                    if (!importer.CheckForProblems() && SceneManager.Current != null)
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

                    if (ImGui.BeginTabItem("Materials"))
                    {
                        for (int i = 0; i < importer.Materials.Length; i++)
                        {
                            var material = importer.Materials[i];
                            var value = material.Name;
                            var invalid = value.Length > 255;
                            if (invalid)
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                            if (ImGui.InputText($"Material {i}", ref value, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                            {
                                importer.ChangeNameOfMaterial(material, value);
                            }
                            if (invalid)
                                ImGui.PopStyleColor();
                        }
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Textures"))
                    {
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