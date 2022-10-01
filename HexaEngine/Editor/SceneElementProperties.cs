#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Cameras;
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
        private static bool gimbalGrabbed;
        private static Matrix4x4 gimbalBefore;

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
                var oldVal = val;
                if (ImGui.InputFloat3("Position", ref val))
                {
                    Designer.History.Do(() => element.Transform.Position = val, () => element.Transform.Position = oldVal);
                }
            }
            {
                var val = element.Transform.Rotation;
                var oldVal = val;
                if (ImGui.InputFloat3("Rotation", ref val))
                {
                    Designer.History.Do(() => element.Transform.Rotation = val, () => element.Transform.Rotation = oldVal);
                }
            }
            {
                var val = element.Transform.Scale;
                var oldVal = val;
                if (ImGui.InputFloat3("Scale", ref val))
                {
                    Designer.History.Do(() => element.Transform.Scale = val, () => element.Transform.Scale = oldVal);
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
            Matrix4x4 transform = element.Transform.Local;

            if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
            {
                gimbalGrabbed = true;
                element.Transform.Local = transform;
            }
            else
            {
                if (gimbalGrabbed)
                {
                    var oldValue = gimbalBefore;
                    Designer.History.Push(() => element.Transform.Local = transform, () => element.Transform.Local = oldValue);
                }
                gimbalGrabbed = false;
                gimbalBefore = transform;
            }

            for (int i = 0; i < element.Meshes.Count; i++)
            {
                var mesh = element.Meshes[i];
                for (int j = 0; j < mesh.Bones.Length; j++)
                {
                    var skele = mesh.Skeleton;
                    var bone = mesh.Bones[j];
                    var origin = Vector3.Transform(Vector3.Zero, skele.GetGlobalTransform(skele.Relationships[bone.Name].ParentName));
                    var dest = Vector3.Transform(origin, skele.GetGlobalTransform(bone.Name));
                    DebugDraw.DrawLine(origin, dest, false, Vector4.One);
                }
            }
        }
    }
}