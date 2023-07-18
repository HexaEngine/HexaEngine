namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public class LayoutWidget : EditorWindow
    {
        private readonly Dictionary<string, EditorGameObjectAttribute> cache = new();

        public static bool ShowHidden;

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
                            var name = scene.GetAvailableName(item.Key);
                            node.Name = name;
                            scene.AddChild(node);
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }

        private static void DisplayNodeContextMenu(GameObject element)
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
                if (ImGui.MenuItem("Show Hidden"))
                {
                    ShowHidden = !ShowHidden;
                }
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        private void DisplayNode(GameObject element)
        {
            if (element.IsHidden && !ShowHidden)
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
            if (element.IsEditorSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && CameraManager.Center != element.Transform.GlobalPosition)
            {
                CameraManager.Center = element.Transform.GlobalPosition;
            }
            else if (element.IsEditorSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.F) && CameraManager.Center == element.Transform.GlobalPosition)
            {
                CameraManager.Center = Vector3.Zero;
            }
            if (element.IsEditorSelected && ImGui.IsKeyReleased(ImGuiKey.Delete))
            {
                GameObject.Selected.PurgeSelection();
            }
            if (element.IsEditorSelected && ImGui.IsKeyPressed(ImGuiKey.LeftCtrl) && ImGui.IsKeyReleased(ImGuiKey.U))
            {
                GameObject.Selected.ClearSelection();
            }

            DisplayNodeContextMenu(element);

            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload(nameof(GameObject));
                    if (!payload.IsNull)
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
                    ImGui.SetDragDropPayload(nameof(GameObject), (&str), (uint)sizeof(Guid));
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
                        GameObject.Selected.AddSelection(element);
                    }
                    else if (ImGui.GetIO().KeyShift)
                    {
                        var last = GameObject.Selected.Last();
                        GameObject.Selected.AddMultipleSelection(SceneManager.Current.GetRange(last, element));
                    }
                    else if (!element.IsEditorSelected)
                    {
                        GameObject.Selected.AddOverwriteSelection(element);
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
                        string id = *(UnsafeString*)payload.Data;
                        var child = scene.Find(id);
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