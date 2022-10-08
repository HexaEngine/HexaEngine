namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public class LayoutWidget : Widget
    {
        private readonly Dictionary<string, EditorNodeAttribute> cache = new();
        private readonly Dictionary<string, int> newInstances = new();

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public LayoutWidget()
        {
            IsShown = true;
            foreach (var type in Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .AsParallel()
                    .Where(x =>
                    x.IsAssignableTo(typeof(SceneNode)) &&
                    x.GetCustomAttribute<EditorNodeAttribute>() != null &&
                    !x.IsAbstract))
            {
                var attr = type.GetCustomAttribute<EditorNodeAttribute>();
                if (attr == null) continue;
                cache.Add(attr.Name, attr);
            }
            cache = cache.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public override void Dispose()
        {
        }

        public override void Draw(IGraphicsContext context)
        {
            if (!IsShown) return;
            ImGuiWindowFlags flags = ImGuiWindowFlags.MenuBar;
            if (IsDocked)
                flags |= ImGuiWindowFlags.NoBringToFrontOnFocus;

            if (!ImGui.Begin("Layout", ref IsShown, flags))
            {
                ImGui.End();
                return;
            }

            IsDocked = ImGui.IsWindowDocked();

            var scene = SceneManager.Current;
            if (scene is null)
            {
                ImGui.End();
                return;
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("+"))
            {
                foreach (var item in cache)
                {
                    if (ImGui.MenuItem(item.Key))
                    {
                        var node = item.Value.Constructor();
                        if (newInstances.TryGetValue(item.Key, out int instance))
                        {
                            node.Name = $"{item.Key} {newInstances[item.Key]++}";
                            scene.AddChild(node);
                        }
                        else
                        {
                            newInstances.Add(item.Key, 2);
                            node.Name = $"{item.Key} {1}";
                            scene.AddChild(node);
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            for (int i = 0; i < scene.Root.Children.Count; i++)
            {
                var element = scene.Root.Children[i];

                DisplayNode(element);

                void DisplayNode(SceneNode element)
                {
                    ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
                    if (element.IsSelected)
                        flags |= ImGuiTreeNodeFlags.Selected;
                    if (element.Children.Count == 0)
                        flags |= ImGuiTreeNodeFlags.Leaf;
                    bool isOpen = ImGui.TreeNodeEx(element.Name, flags);
                    if (element.IsSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && CameraManager.Center != element.Transform.GlobalPosition)
                    {
                        CameraManager.Center = element.Transform.GlobalPosition;
                    }
                    else if (element.IsSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && CameraManager.Center == element.Transform.GlobalPosition)
                    {
                        CameraManager.Center = Vector3.Zero;
                    }
                    if (element.IsSelected && ImGui.IsKeyReleased(ImGuiKey.Delete))
                    {
                        element.Parent?.RemoveChild(element);
                        element.IsSelected = false;
                    }
                    if (element.IsSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.U))
                    {
                        element.IsSelected = false;
                    }
                    ImGui.PushID(element.Name);
                    if (ImGui.BeginPopupContextItem(element.Name))
                    {
                        if (ImGui.MenuItem("Focus (F)"))
                        {
                            CameraManager.Center = element.Transform.GlobalPosition;
                        }
                        if (ImGui.MenuItem("Unfocus (F)"))
                        {
                            CameraManager.Center = Vector3.Zero;
                        }
                        if (ImGui.MenuItem("Unselect"))
                        {
                            element.IsSelected = false;
                        }
                        if (ImGui.MenuItem("Delete"))
                        {
                            element.Parent?.RemoveChild(element);
                            element.IsSelected = false;
                        }
                        ImGui.EndPopup();
                    }
                    ImGui.PopID();
                    if (ImGui.BeginDragDropTarget())
                    {
                        unsafe
                        {
                            var payload = ImGui.AcceptDragDropPayload(nameof(SceneNode));
                            if (payload.NativePtr != null)
                            {
                                Guid id = ((Guid*)payload.Data)[0];
                                element.AddChild(scene.Find(id));
                            }
                        }
                        ImGui.EndDragDropTarget();
                    }
                    if (ImGui.BeginDragDropSource())
                    {
                        unsafe
                        {
                            ImGui.SetDragDropPayload(nameof(SceneNode), (IntPtr)Utilities.AsPointer(element.ID), (uint)sizeof(Guid));
                        }
                        ImGui.Text(element.Name);
                        ImGui.EndDragDropSource();
                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        element.IsSelected = true;
                    }

                    if (isOpen)
                    {
                        for (int j = 0; j < element.Children.Count; j++)
                        {
                            DisplayNode(element.Children[j]);
                        }
                        ImGui.TreePop();
                    }
                }
            }

            ImGui.End();
        }

        public override void DrawMenu()
        {
            if (ImGui.MenuItem("Layout"))
            {
                IsShown = true;
            }
        }

        public override void Init(IGraphicsDevice device)
        {
        }
    }
}