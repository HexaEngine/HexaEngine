#nullable disable

using HexaEngine;

namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using ImGuizmoNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public class PropertiesWidget : ImGuiWindow
    {
        private readonly List<EditorComponentAttribute> componentCache = new();
        private readonly Dictionary<Type, EditorComponentAttribute[]> typeFilterComponentCache = new();
        private OPERATION operation = OPERATION.TRANSLATE;
        private MODE mode = MODE.LOCAL;
        private bool gimbalGrabbed;
        private Matrix4x4 gimbalBefore;
        private int currentMesh;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public PropertiesWidget()
        {
            IsShown = true;
            componentCache.AddRange(Assembly.GetExecutingAssembly()
                .GetTypes()
                .AsParallel()
                .Where(x => x.IsAssignableTo(typeof(IComponent)))
                .Select(x => x.GetCustomAttribute<EditorComponentAttribute>())
                .Where(x => x != null));
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name => "Properties";

        public override void DrawContent(IGraphicsContext context)
        {
            IsDocked = ImGui.IsWindowDocked();

            if (GameObject.Selected.Count == 0)
            {
                EndWindow();
                return;
            }

            GameObject element = GameObject.Selected.First();
            Scene scene = element.GetScene();
            Camera camera = CameraManager.Current;

            string name = element.Name;
            if (ImGui.InputText("Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
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

            if (ImGui.Button("Add mesh"))
            {
                if (currentMesh > -1 && currentMesh < scene.Meshes.Count)
                {
                    element.AddMesh(currentMesh);
                }
            }

            ImGui.SameLine();

            ImGui.Combo("Mesh", ref currentMesh, scene.Meshes.Select(x => x.Name).ToArray(), scene.Meshes.Count);

            for (int i = 0; i < element.Meshes.Count; i++)
            {
                var mesh = element.Meshes[i];
                ImGui.Text(scene.Meshes[mesh].Name);
                ImGui.SameLine();
                if (ImGui.Button("Remove Mesh"))
                {
                    element.RemoveMesh(mesh);
                }
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
                    else if (editorComponent.AllowedTypes.Length != 0 && !editorComponent.AllowedTypes.Any(x => x == type))
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
                var component = element.Components[i];
                if (ImGui.BeginChild(component.Editor.Name))
                {
                    if (ImGui.Button("Delete"))
                    {
                        scene.Dispatcher.Invoke(() => element.RemoveComponent(component));
                    }
                    component.Editor?.Draw();
                }
                ImGui.EndChild();
            }

            EndWindow();

            ImGuizmo.Enable(true);
            ImGuizmo.SetOrthographic(false);
            if (camera == null) return;
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
        }
    }
}