namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.PostFx;

    [EditorWindowCategory("Debug")]
    public class PostProcessWindow : EditorWindow
    {
        private Texture2D? buffer;
        private ITexture2D? selected;

        protected override string Name => "Post Processing";

        protected override void DisposeCore()
        {
            Unselect();
        }

        private void Select(IGraphicsContext context, IResource? resource)
        {
            if (resource == null)
            {
                Unselect();
            }

            if (selected == resource)
            {
                return;
            }

            if (resource is not ITexture2D tex)
            {
                return;
            }

            selected = tex;
            var desc = tex.Description;
            desc.Usage = Usage.Default;
            desc.BindFlags = BindFlags.ShaderResource;

            buffer?.Dispose();
            buffer = new(desc);
        }

        private void Unselect()
        {
            selected = null;
            buffer?.Dispose();
            buffer = null;
        }

        private void UpdateSelection(IGraphicsContext context)
        {
            if (selected == null || buffer == null)
            {
                return;
            }

            if (selected.IsDisposed)
            {
                Unselect();
            }

            context.CopyResource(buffer, selected);
        }

        public override void DrawContent(IGraphicsContext context)
        {
            PostProcessingManager? manager = PostProcessingManager.Current;
            if (manager == null)
            {
                return;
            }

            UpdateSelection(context);

            object lockObject = manager.SyncObject;

            var flags = (int)manager.Flags;

            if (ImGui.Button("Invalidate"))
            {
                manager.Invalidate();
            }

            ImGui.SameLine();

            if (ImGui.Button("Reload"))
            {
                manager.Reload();
            }

            bool textBefore = false;
            if (manager.IsInitialized)
            {
                ImGui.Text("Initialized");
                textBefore = true;
            }
            if (manager.IsDirty)
            {
                if (textBefore)
                {
                    ImGui.SameLine();
                }
                ImGui.Text("Dirty");
            }
            if (manager.IsReloading)
            {
                if (textBefore)
                {
                    ImGui.SameLine();
                }
                ImGui.Text("Reloading");
            }

            ImGui.CheckboxFlags("HDR", ref flags, (int)PostProcessingFlags.HDR);
            ImGui.SameLine();
            ImGui.CheckboxFlags("Debug", ref flags, (int)PostProcessingFlags.Debug);
            ImGui.SameLine();
            ImGui.CheckboxFlags("Force Dynamic", ref flags, (int)PostProcessingFlags.ForceDynamic);

            ImGui.BeginTable("PostProcess", 3, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.BeginGroup();

            lock (lockObject)
            {
                for (int i = 0; i < manager.ActiveEffects.Count; i++)
                {
                    var effect = manager.ActiveEffects[i];
                    if (ImGui.TreeNode(effect.Name))
                    {
                        ImGui.TreeNodeEx($"Flags: {effect.Flags}", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                        ImGui.TreeNodeEx($"Color space: {effect.ColorSpace}", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                        if (manager.Graph.TryGetNode(effect, out var node))
                        {
                            if (ImGui.TreeNode($"Dependencies: {node.Dependencies.Count}"))
                            {
                                for (var j = 0; j < node.Dependencies.Count; j++)
                                {
                                    var dependency = node.Dependencies[j];
                                    ImGui.TreeNodeEx($"{dependency.Name}", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                }
                                ImGui.TreePop();
                            }
                            if (ImGui.TreeNode($"Dependents: {node.Dependents.Count}"))
                            {
                                for (var j = 0; j < node.Dependents.Count; j++)
                                {
                                    var dependant = node.Dependents[j];
                                    ImGui.TreeNodeEx($"{dependant.Name}", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                }
                                ImGui.TreePop();
                            }
                            var container = node.Container;
                            lock (container.SyncObject)
                            {
                                if (ImGui.TreeNode($"Memory Usage: {container.Size.FormatDataSize()}"))
                                {
                                    for (int j = 0; j < container.Entries.Count; j++)
                                    {
                                        var entry = container.Entries[j];
                                        ImGui.TreeNodeEx($"{entry.Name} ({entry.Resource.Dimension}): {entry.Size.FormatDataSize()}", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                                        {
                                            Select(context, entry.Resource);
                                        }
                                    }
                                    ImGui.TreePop();
                                }

                                if (ImGui.TreeNode($"Resources: {container.Resources.Count}"))
                                {
                                    for (int j = 0; j < container.Resources.Count; j++)
                                    {
                                        var resourceRef = container.Resources[j];
                                        var resource = resourceRef.Value as IResource;
                                        ImGui.TreeNodeEx($"{resourceRef.Name} ({resource?.Dimension ?? ResourceDimension.Unknown}){(resourceRef.ShareSource != null ? $" -> {resourceRef.ShareSource.Name}" : string.Empty)}", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                                        {
                                            Select(context, resource);
                                        }
                                    }
                                    ImGui.TreePop();
                                }

                                if (ImGui.TreeNode($"Shared Resources: {container.SharedResources.Count}"))
                                {
                                    for (int j = 0; j < container.SharedResources.Count; j++)
                                    {
                                        var entry = container.SharedResources[j];
                                        ImGui.TreeNodeEx($"{entry.Name} ({(entry.Value as IResource)?.Dimension})", ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                    }
                                    ImGui.TreePop();
                                }
                            }
                        }

                        ImGui.TreePop();
                    }
                }
            }

            ImGui.EndGroup();

            ImGui.TableSetColumnIndex(1);

            ImGui.BeginGroup();

            lock (lockObject)
            {
                for (int i = 0; i < manager.Groups.Count; i++)
                {
                    var group = manager.Groups[i];
                    if (ImGui.TreeNode($"Execution Group {i} ({(group.IsDynamic ? "Dynamic" : "Static")})"))
                    {
                        if (group.IsDynamic)
                        {
                            TooltipHelper.Tooltip("A dynamic execution group with conditional render code");
                        }
                        else
                        {
                            TooltipHelper.Tooltip("A static execution group recorded once at the start or at updates");
                        }

                        for (int j = 0; j < group.Passes.Count; j++)
                        {
                            var pass = group.Passes[j];
                            ImGui.TreeNodeEx(pass.Name, ImGuiTreeNodeFlags.Leaf);
                            ImGui.TreePop();
                        }

                        ImGui.TreePop();
                    }
                }
            }

            ImGui.EndGroup();

            ImGui.TableSetColumnIndex(2);

            if (buffer != null)
            {
                var size = ImGui.GetContentRegionAvail();

                float aspect = buffer.Viewport.Height / buffer.Viewport.Width;
                size.X = MathF.Min(buffer.Viewport.Width, size.X);
                size.Y = buffer.Viewport.Height;
                var dx = buffer.Viewport.Width - size.X;
                if (dx > 0)
                {
                    size.Y = size.X * aspect;
                }
                ImGui.Image(buffer.SRV!.ToTexRef(), size);
            }

            ImGui.EndTable();
        }
    }
}