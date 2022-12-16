﻿namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using Silk.NET.Assimp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;
    using System.Xml.Linq;

    public class LayoutWidget : ImGuiWindow
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
                    x.IsAssignableTo(typeof(GameObject)) &&
                    x.GetCustomAttribute<EditorNodeAttribute>() != null &&
                    !x.IsAbstract))
            {
                var attr = type.GetCustomAttribute<EditorNodeAttribute>();
                if (attr == null) continue;
                cache.Add(attr.Name, attr);
            }
            cache.Add("Object", new EditorNodeAttribute("Object", typeof(GameObject), () => new GameObject(), x => x is GameObject));
            cache = cache.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        protected override string Name => "Layout";

        private void DisplayContextMenu()
        {
            var scene = SceneManager.Current;
            if (ImGui.BeginPopupContextWindow("LayoutContextMenu"))
            {
                if (ImGui.BeginMenu("Add"))
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

                ImGui.EndPopup();
            }
        }

        private void DisplayNodeContextMenu(GameObject element)
        {
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
                    GameObject.Selected.ClearSelection();
                }
                if (ImGui.MenuItem("Delete"))
                {
                    GameObject.Selected.PurgeSelection();
                }
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        private void DisplayNode(GameObject element)
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (element.IsSelected)
                flags |= ImGuiTreeNodeFlags.Selected;
            if (element.Children.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf;

            bool isOpen = ImGui.TreeNodeEx(element.Name, flags);
            element.IsOpen = isOpen;
            element.IsVisible = true;
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
                GameObject.Selected.PurgeSelection();
            }
            if (element.IsSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.U))
            {
                GameObject.Selected.ClearSelection();
            }

            DisplayNodeContextMenu(element);

            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                    if (payload.NativePtr != null)
                    {
                        string id = *(UnsafeString*)payload.Data;
                        var gameObject = SceneManager.Current.Find(id);
                        GameObject.Selected.MoveSelection(element);
                    }
                }
                ImGui.EndDragDropTarget();
            }
            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    var str = new UnsafeString(element.Name);
                    ImGui.SetDragDropPayload(nameof(GameObject), (nint)(&str), (uint)sizeof(Guid));
                }
                ImGui.Text(element.Name);
                ImGui.EndDragDropSource();
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
                {
                    GameObject.Selected.AddSelection(element);
                }
                else if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
                {
                    var last = GameObject.Selected.Last();
                    GameObject.Selected.AddMultipleSelection(SceneManager.Current.GetRange(last, element));
                }
                else if (!element.IsSelected)
                {
                    GameObject.Selected.AddOverwriteSelection(element);
                }
            }

            if (isOpen)
            {
                for (int j = 0; j < element.Children.Count; j++)
                {
                    DisplayNode(element.Children[j]);
                }
                ImGui.TreePop();
            }
            else
            {
                for (int j = 0; j < element.Children.Count; j++)
                {
                    element.Children[j].IsVisible = false;
                }
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            var scene = SceneManager.Current;

            if (scene == null)
            {
                EndWindow();
                return;
            }

            ImGui.BeginChild("LayoutContent");

            DisplayContextMenu();

            bool rootIsOpen = ImGui.TreeNodeEx("Scene", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.Framed);

            if (!rootIsOpen)
            {
                ImGui.EndChild();
                return;
            }

            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                    if (payload.NativePtr != null)
                    {
                        string id = *(UnsafeString*)payload.Data;
                        var child = scene.Find(id);
                        if (child != null)
                            scene.AddChild(child);
                    }
                }
                ImGui.EndDragDropTarget();
            }

            for (int i = 0; i < scene.Root.Children.Count; i++)
            {
                var element = scene.Root.Children[i];

                DisplayNode(element);
            }

            ImGui.TreePop();
            ImGui.EndChild();
        }
    }
}