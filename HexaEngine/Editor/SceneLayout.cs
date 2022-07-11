namespace HexaEngine.Editor
{
    using HexaEngine.Core.Input;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class SceneLayout
    {
        private static bool isShown;
        private static Dictionary<string, Type> cache = new();
        private static Dictionary<string, int> newInstances = new();

        public static bool IsShown { get => isShown; set => isShown = value; }

        internal static void Draw()
        {
            if (cache.Count == 0)
            {
                foreach (var type in Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .AsParallel()
                    .Where(x =>
                    x.IsAssignableTo(typeof(SceneNode)) &&
                    x.GetCustomAttribute<EditorNodeAttribute>() != null &&
                    !x.IsAbstract))
                {
                    cache.Add(type.GetCustomAttribute<EditorNodeAttribute>().Name, type);
                }
            }

            if (!ImGui.Begin("Layout", ref isShown, ImGuiWindowFlags.MenuBar))
            {
                ImGui.End();
                return;
            }

            var scene = SceneManager.Current;
            if (scene is null)
            {
                ImGui.End();
                return;
            }

            int cameraIndex = scene.ActiveCamera;
            if (ImGui.Combo("Current Camera", ref cameraIndex, scene.Cameras.Select(x => x.Name).ToArray(), scene.Cameras.Count))
            {
                scene.ActiveCamera = cameraIndex;
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("+"))
            {
                foreach (var item in cache)
                {
                    if (ImGui.MenuItem(item.Key))
                    {
                        var node = (SceneNode)Activator.CreateInstance(item.Value);
                        if (newInstances.TryGetValue(item.Key, out int instance))
                        {
                            newInstances[item.Key]++;
                            node.Name = $"{item.Key} {instance}";
                            scene.AddChild(node);
                        }
                        else
                        {
                            newInstances.Add(item.Key, 1);
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

                static void DisplayNode(SceneNode element)
                {
                    ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
                    if (element.IsSelected)
                        flags |= ImGuiTreeNodeFlags.Selected;
                    if (element.Children.Count == 0)
                        flags |= ImGuiTreeNodeFlags.Leaf;
                    if (ImGui.TreeNodeEx(element.Name, flags))
                    {
                        for (int j = 0; j < element.Children.Count; j++)
                        {
                            DisplayNode(element.Children[j]);
                        }
                        ImGui.TreePop();
                    }

                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        element.IsSelected = true;
                    }
                }
            }

            ImGui.End();
        }
    }
}