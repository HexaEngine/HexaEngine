namespace HexaEngine.Editor.Dialogs
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Textures;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes.Importer;
    using ImGuiNET;
    using Silk.NET.Assimp;
    using System.Numerics;

    public class ImportDialog : Modal, IDialog
    {
        private static readonly TexFileFormat[] fileFormats = Enum.GetValues<TexFileFormat>();
        private static readonly string[] fileFormatNames = Enum.GetNames<TexFileFormat>();
        private static readonly Format[] formats = Enum.GetValues<Format>();
        private static readonly string[] formatNames = Enum.GetNames<Format>();
        private readonly AssimpSceneImporter importer = new();
        private readonly IGraphicsDevice device;
        private string path = string.Empty;
        private OpenFileDialog dialog = new();
        private bool loaded = false;

        public ImportDialog(IGraphicsDevice device)
        {
            this.device = device;
        }

        public override string Name => "Import Scene";

        public bool Shown { get; }

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.None;

        public override void Reset()
        {
            loaded = false;
            importer.Clear();
        }

        protected override void DrawContent()
        {
            if (dialog.Draw())
            {
                if (dialog.Result == OpenFileResult.Ok)
                {
                    path = dialog.FullPath;
                }
            }

            if (!loaded)
            {
                if (ImGui.InputText("Path", ref path, 2048))
                {
                    dialog.FullPath = path;
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
                ImGui.CheckboxFlags("Flip UVs", ref flags, (int)PostProcessSteps.FlipUVs);
                ImGui.CheckboxFlags("Generate UVs", ref flags, (int)PostProcessSteps.GenerateUVCoords);
                importer.PostProcessSteps = (PostProcessSteps)flags;

                if (ImGui.Button("Load"))
                {
                    Task.Factory.StartNew(() => importer.LoadAsync(path).ContinueWith(x => { loaded = true; }));
                }
                if (ImGui.Button("Cancel"))
                {
                    Close();
                }
            }
            else
            {
                if (ImGui.Button("Import"))
                {
                    if (!importer.CheckForProblems() && SceneManager.Current != null)
                    {
                        importer.Import(device, SceneManager.Current);

                        Close();
                        Reset();
                    }
                }
                if (ImGui.BeginTabBar("ImporterTabs"))
                {
                    if (ImGui.BeginTabItem("Models"))
                    {
                        for (int i = 0; i < importer.Models.Count; i++)
                        {
                            var model = importer.Models[i];
                            var value = model.Name;
                            var invalid = value.Length > 255;
                            if (invalid)
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                            if (ImGui.InputText($"Model {i}", ref value, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
                            {
                                importer.ChangeNameOfModel(model, value);
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
                        {
                            var flags = (int)importer.TexPostProcessSteps;
                            ImGui.CheckboxFlags("Scale", ref flags, (int)TexPostProcessSteps.Scale);
                            ImGui.CheckboxFlags("Convert", ref flags, (int)TexPostProcessSteps.Convert);

                            ImGui.BeginDisabled(importer.TexFileFormat != TexFileFormat.DDS && importer.TexFileFormat != TexFileFormat.TGA && importer.TexFileFormat != TexFileFormat.HDR);
                            ImGui.CheckboxFlags("GenerateMips", ref flags, (int)TexPostProcessSteps.GenerateMips);
                            ImGui.EndDisabled();

                            importer.TexPostProcessSteps = (TexPostProcessSteps)flags;
                        }

                        {
                            var index = Array.IndexOf(fileFormats, importer.TexFileFormat);
                            if (ImGui.Combo("File Format", ref index, fileFormatNames, fileFormatNames.Length))
                            {
                                importer.TexFileFormat = fileFormats[index];
                            }
                        }

                        {
                            var index = Array.IndexOf(formats, importer.TexFormat);
                            if (ImGui.Combo("Tex Format", ref index, formatNames, formatNames.Length))
                            {
                                importer.TexFormat = formats[index];
                            }
                        }

                        ImGui.InputFloat("Tex Scale", ref importer.TexScaleFactor);

                        {
                            var flags = (int)importer.TexCompressFlags;
                            ImGui.BeginDisabled(importer.TexFileFormat != TexFileFormat.DDS && importer.TexFileFormat != TexFileFormat.TGA && importer.TexFileFormat != TexFileFormat.HDR);
                            ImGui.CheckboxFlags("Uniform", ref flags, (int)TexCompressFlags.Uniform);

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                                ImGui.SetTooltip("Uniform color weighting for BC1-3 compression; by default uses perceptual weighting");

                            ImGui.CheckboxFlags("DitherA", ref flags, (int)TexCompressFlags.DitherA);

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                                ImGui.SetTooltip("Enables dithering alpha for BC1-3 compression");

                            ImGui.CheckboxFlags("DitherRGB", ref flags, (int)TexCompressFlags.DitherRGB);

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                                ImGui.SetTooltip("Enables dithering RGB colors for BC1-3 compression");

                            ImGui.CheckboxFlags("BC7Use3Sunsets", ref flags, (int)TexCompressFlags.BC7Use3Sunsets);

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                                ImGui.SetTooltip("Enables exhaustive search for BC7 compress for mode 0 and 2; by default skips trying these modes");

                            ImGui.CheckboxFlags("BC7Quick", ref flags, (int)TexCompressFlags.BC7Quick);

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                                ImGui.SetTooltip("Minimal modes (usually mode 6) for BC7 compression");

                            ImGui.CheckboxFlags("SRGB", ref flags, (int)TexCompressFlags.SRGB);

                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                                ImGui.SetTooltip("if the input format type is IsSRGB(), then SRGB_IN is on by default\nif the output format type is IsSRGB(), then SRGB_OUT is on by default");

                            ImGui.EndDisabled();

                            importer.TexCompressFlags = (TexCompressFlags)flags;
                        }

                        for (int i = 0; i < importer.Textures.Count; i++)
                        {
                            var texture = importer.Textures[i];
                            ImGui.Text(texture);
                        }
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
        }
    }
}