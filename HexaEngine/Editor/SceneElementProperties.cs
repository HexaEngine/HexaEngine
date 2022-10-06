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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public static class SceneElementProperties
    {
        private static bool isShown;
        private static readonly List<EditorComponentAttribute> componentCache = new();
        private static readonly Dictionary<Type, EditorComponentAttribute[]> typeFilterComponentCache = new();
        private static OPERATION operation = OPERATION.TRANSLATE;
        private static MODE mode = MODE.LOCAL;
        private static bool gimbalGrabbed;
        private static Matrix4x4 gimbalBefore;

        public static bool IsShown { get => isShown; set => isShown = value; }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
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

            ImGui.Text($"Global Position: {element.Transform.GlobalPosition}");
            ImGui.Text($"Global Rotation: {element.Transform.GlobalOrientation.GetRotation().ToDeg()}");
            ImGui.Text($"Global Scale: {element.Transform.GlobalScale}");

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

            Type type = element.Editor.Type;
            element.Editor?.Draw();

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
                            IComponent component = editorComponent.Constructor();
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
                element.Components[i].Editor?.Draw();
            }

            ImGui.End();

            ImGuizmo.Enable(true);
            ImGuizmo.SetOrthographic(false);
            Matrix4x4 view = camera.Transform.View;
            Matrix4x4 proj = camera.Transform.Projection;
            Matrix4x4 transform = element.Transform.Global;

            if (ImGuizmo.Manipulate(ref view, ref proj, operation, mode, ref transform))
            {
                gimbalGrabbed = true;
                if (element.Transform.Parent == null)
                    element.Transform.Local = transform;
                else
                    element.Transform.Local = transform * element.Transform.Parent.GlobalInverse;
            }
            else if (!ImGuizmo.IsUsing())
            {
                if (gimbalGrabbed)
                {
                    var oldValue = gimbalBefore;
                    Designer.History.Push(() => element.Transform.Local = transform, () => element.Transform.Local = oldValue);
                }
                gimbalGrabbed = false;
                gimbalBefore = element.Transform.Local;
            }

            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                var mesh = scene.Meshes[i];
                if (mesh.Bones.Length == 0)
                    continue;
                var root = mesh.Skeleton.FindRoot();
                var ele = scene.Find(root);
                var trans = ele.Transform.Global;
                for (int j = 0; j < mesh.Bones.Length; j++)
                {
                    var skele = mesh.Skeleton;
                    var bone = mesh.Bones[j];
                    var originMtx = skele.GetGlobalTransform(skele.Relationships[bone.Name].ParentName);
                    var destMtx = skele.GetGlobalTransform(bone.Name);
                    var origin = Vector4.UnitW.ApplyMatrix(trans * originMtx);
                    var dest = Vector4.UnitW.ApplyMatrix(trans * destMtx);
                    DebugDraw.DrawLine(new(origin.X, origin.Y, origin.Z), new(dest.X, dest.Y, dest.Z), false, Vector4.One);
                }
            }
        }
    }
}