﻿namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    // TODO: Needs major overhaul.
    /// <summary>
    ///
    /// </summary>
    public class LayoutWidget : EditorWindow
    {
        private readonly Dictionary<string, EditorGameObjectAttribute> cache = new();
        private bool showHidden;
        private bool focused;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public LayoutWidget()
        {
            IsShown = true;

            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x
                    .GetTypes()
                    .AsParallel()
                    .Where(x =>
                    x.IsAssignableTo(typeof(GameObject)) &&
                    x.GetCustomAttribute<EditorGameObjectAttribute>() != null &&
                    !x.IsAbstract)))
            {
                var attr = type.GetCustomAttribute<EditorGameObjectAttribute>();
                if (attr == null)
                {
                    continue;
                }

                cache.Add(attr.Name, attr);
            }
            cache.Add("Object", new EditorGameObjectAttribute("Object", typeof(GameObject), () => new GameObject(), x => x is GameObject));
            cache = cache.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        protected override string Name => "Layout";

        private void DrawSettingsMenu()
        {
            if (ImGui.BeginMenu("\xE713 Settings"))
            {
                ImGui.Checkbox("Show Hidden", ref showHidden);

                ImGui.EndMenu();
            }
        }

        private void DisplayContextMenu()
        {
            var scene = SceneManager.Current;
            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.BeginMenu("\xE710 Add"))
                {
                    foreach (var item in cache)
                    {
                        if (ImGui.MenuItem(item.Key))
                        {
                            var node = item.Value.Constructor();
                            var name = scene.GetAvailableName(item.Key);
                            node.Name = name;
                            scene.AddChild(node);
                        }
                    }

                    ImGui.EndMenu();
                }

                DrawSettingsMenu();

                ImGui.EndPopup();
            }
        }

        private static void DisplayNodeContextMenu(GameObject element)
        {
            ImGui.PushID(element.Name);
            if (ImGui.BeginPopupContextItem(element.Name))
            {
                if (ImGui.MenuItem("\xE71E Focus"))
                {
                    EditorCameraController.Center = element.Transform.GlobalPosition;
                }
                if (ImGui.MenuItem("\xE71F Defocus"))
                {
                    EditorCameraController.Center = Vector3.Zero;
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8E6 Unselect"))
                {
                    SelectionCollection.Global.ClearSelection();
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE738 Delete"))
                {
                    SelectionCollection.Global.PurgeSelection();
                }

                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        private void DisplayNode(GameObject element)
        {
            if (element.IsHidden && !showHidden)
            {
                return;
            }
            else if (element.IsHidden)
            {
                ImGui.BeginDisabled(true);
            }

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (element.IsEditorSelected)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }

            if (element.Children.Count == 0)
            {
                flags |= ImGuiTreeNodeFlags.Leaf;
            }

            bool isOpen = ImGui.TreeNodeEx(element.Name, flags);
            element.IsEditorOpen = isOpen;
            element.IsEditorVisible = true;

            if (focused)
            {
                if (element.IsEditorSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && EditorCameraController.Center != element.Transform.GlobalPosition)
                {
                    EditorCameraController.Center = element.Transform.GlobalPosition;
                }
                else if (element.IsEditorSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && EditorCameraController.Center == element.Transform.GlobalPosition)
                {
                    EditorCameraController.Center = Vector3.Zero;
                }
                if (element.IsEditorSelected && ImGui.IsKeyReleased(ImGuiKey.Delete))
                {
                    SelectionCollection.Global.PurgeSelection();
                }
                if (element.IsEditorSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.U))
                {
                    SelectionCollection.Global.ClearSelection();
                }
            }

            DisplayNodeContextMenu(element);

            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                    if (!payload.IsNull)
                    {
                        Guid id = *(Guid*)payload.Data;
                        var gameObject = SceneManager.Current.FindByGuid(id);
                        SelectionCollection.Global.MoveSelection(element);
                    }
                }
                ImGui.EndDragDropTarget();
            }
            if (ImGui.BeginDragDropSource())
            {
                unsafe
                {
                    Guid id = element.Guid;
                    ImGui.SetDragDropPayload(nameof(GameObject), &id, (uint)sizeof(Guid));
                }
                ImGui.Text(element.Name);
                ImGui.EndDragDropSource();
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                unsafe
                {
                    if (ImGui.GetIO().KeyCtrl)
                    {
                        SelectionCollection.Global.AddSelection(element);
                    }
                    else if (ImGui.GetIO().KeyShift)
                    {
                        var last = SelectionCollection.Global.Last<GameObject>();
                        if (last != null)
                        {
                            SelectionCollection.Global.AddMultipleSelection(SceneManager.Current.GetRange(last, element));
                        }
                    }
                    else if (!element.IsEditorSelected)
                    {
                        SelectionCollection.Global.AddOverwriteSelection(element);
                    }
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
                    element.Children[j].IsEditorVisible = false;
                }
            }

            if (element.IsHidden)
            {
                ImGui.EndDisabled();
            }
        }

        public override void DrawContent(IGraphicsContext context)
        {
            focused = ImGui.IsWindowFocused();
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
                    if (!payload.IsNull)
                    {
                        Guid id = *(Guid*)payload.Data;
                        var child = scene.FindByGuid(id);
                        if (child != null)
                        {
                            scene.AddChild(child);
                        }
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