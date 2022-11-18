﻿namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public class LayoutWidget : ImGuiWindow
    {
        private readonly Dictionary<string, EditorNodeAttribute> cache = new();
        private readonly Dictionary<string, int> newInstances = new();

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public LayoutWidget()
        {
            Flags = ImGuiWindowFlags.MenuBar;
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

        protected override string Name => "Layout";

        public override void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;

            if (scene == null)
            {
                EndWindow();
                return;
            }

            IsDocked = ImGui.IsWindowDocked();

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
                                string id = *(UnsafeString*)payload.Data;
                                element.AddChild(scene.Find(id));
                            }
                        }
                        ImGui.EndDragDropTarget();
                    }
                    if (ImGui.BeginDragDropSource())
                    {
                        unsafe
                        {
                            var str = new UnsafeString(element.Name);
                            ImGui.SetDragDropPayload(nameof(SceneNode), (nint)(&str), (uint)sizeof(Guid));
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
        }
    }
}