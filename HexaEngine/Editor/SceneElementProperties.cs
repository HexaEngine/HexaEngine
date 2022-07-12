namespace HexaEngine.Editor
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using ImGuiNET;
    using ImGuizmoNET;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public static class SceneElementProperties
    {
        private static bool isShown;
        private static readonly Dictionary<Type, PropertyEditor> propertyCache = new();
        private static readonly List<EditorComponentAttribute> componentCache = new();
        private static readonly Dictionary<Type, EditorComponentAttribute[]> typeFilterComponentCache = new();
        private static OPERATION operation = OPERATION.TRANSLATE;
        private static MODE mode = MODE.LOCAL;

        public static bool IsShown { get => isShown; set => isShown = value; }

        public static void ClearCache()
        {
            propertyCache.Clear();
        }

        static SceneElementProperties()
        {
            componentCache.AddRange(Assembly.GetExecutingAssembly()
                .GetTypes()
                .AsParallel()
                .Where(x => x.IsAssignableTo(typeof(IComponent)))
                .Select(x => x.GetCustomAttribute<EditorComponentAttribute>())
                .Where(x => x != null));
        }

        internal static void Draw()
        {
            if (!ImGui.Begin("Properties", ref isShown, ImGuiWindowFlags.MenuBar))
            {
                ImGui.End();
                return;
            }

            if (SceneNode.SelectedNode is null)
            {
                ImGui.End();
                return;
            }

            SceneNode element = SceneNode.SelectedNode;
            Scene scene = element.GetScene();
            Camera camera = CameraManager.Current;

            string name = element.Name;
            if (ImGui.InputText("Name", ref name, 256))
            {
                element.Name = name;
            }

            ImGui.Separator();

            ImGui.Text(nameof(Transform));
            {
                var val = element.Transform.Position;
                if (ImGui.InputFloat3("Position", ref val))
                {
                    element.Transform.Position = val;
                }
            }
            {
                var val = element.Transform.Rotation;
                if (ImGui.InputFloat3("Rotation", ref val))
                {
                    element.Transform.Rotation = val;
                }
            }
            {
                var val = element.Transform.Scale;
                if (ImGui.InputFloat3("Scale", ref val))
                {
                    element.Transform.Scale = val;
                }
            }

            ImGui.Separator();

            if (ImGui.RadioButton("Translate", operation == OPERATION.TRANSLATE))
            {
                operation = OPERATION.TRANSLATE;
            }

            if (ImGui.RadioButton("Rotate", operation == OPERATION.ROTATE))
            {
                operation = OPERATION.ROTATE;
            }

            if (ImGui.RadioButton("Scale", operation == OPERATION.SCALE))
            {
                operation = OPERATION.SCALE;
            }

            if (ImGui.RadioButton("Local", mode == MODE.LOCAL))
            {
                mode = MODE.LOCAL;
            }

            ImGui.SameLine();
            if (ImGui.RadioButton("World", mode == MODE.WORLD))
            {
                mode = MODE.WORLD;
            }

            ImGui.Separator();

            Type type = element.GetType();
            if (propertyCache.TryGetValue(type, out var typeEditor))
            {
                typeEditor.Draw(element);
            }
            else
            {
                propertyCache.Add(type, new(type));
            }

            ImGui.Separator();

            if (typeFilterComponentCache.TryGetValue(type, out EditorComponentAttribute[] editorComponents))
            {
                if (ImGui.BeginMenu("+"))
                {
                    for (int i = 0; i < editorComponents.Length; i++)
                    {
                        EditorComponentAttribute editorComponent = editorComponents[i];
                        if (ImGui.MenuItem(editorComponent.Name))
                        {
                            IComponent component = (IComponent)Activator.CreateInstance(editorComponent.Type);
                            element.AddComponent(component);
                        }
                    }
                    ImGui.EndMenu();
                }
            }
            else
            {
                List<EditorComponentAttribute> allowedComponents = new();
                foreach (var editorComponent in componentCache)
                {
                    if (editorComponent.IsHidden)
                    {
                        continue;
                    }

                    if (editorComponent.IsInternal)
                    {
                        continue;
                    }

                    if (editorComponent.AllowedTypes == null)
                    {
                        allowedComponents.Add(editorComponent);
                        continue;
                    }
                    else if (!editorComponent.AllowedTypes.Any(x => x == type))
                    {
                        continue;
                    }

                    if (editorComponent.DisallowedTypes == null)
                    {
                        allowedComponents.Add(editorComponent);
                        continue;
                    }
                    else if (!editorComponent.DisallowedTypes.Any(x => x == type))
                    {
                        allowedComponents.Add(editorComponent);
                        continue;
                    }
                }

                typeFilterComponentCache.Add(type, allowedComponents.ToArray());
            }

            for (int i = 0; i < element.Components.Count; i++)
            {
                IComponent component = element.Components[i];
                Type componentType = component.GetType();
                if (propertyCache.TryGetValue(componentType, out var componentEditor))
                {
                    componentEditor.Draw(component);
                }
                else
                {
                    propertyCache.Add(componentType, new(componentType));
                }
            }

            ImGui.End();

            ImGuizmo.Enable(true);
            ImGuizmo.SetOrthographic(false);
            Matrix4x4 view = camera.Transform.View;
            Matrix4x4 proj = camera.Transform.Projection;
            Matrix4x4 transform = element.Transform;

            if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
            {
                element.Transform.Matrix = transform;
            }
        }
    }
}