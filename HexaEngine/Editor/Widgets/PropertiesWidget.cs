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
            if (ImGui.CollapsingHeader(nameof(Transform)))
            {
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

                if (ImGui.RadioButton("Translate", Inspector.Operation == ImGuizmoOperation.TRANSLATE))
                {
                    Inspector.Operation = ImGuizmoOperation.TRANSLATE;
                }

                if (ImGui.RadioButton("Rotate", Inspector.Operation == ImGuizmoOperation.ROTATE))
                {
                    Inspector.Operation = ImGuizmoOperation.ROTATE;
                }

                if (ImGui.RadioButton("Scale", Inspector.Operation == ImGuizmoOperation.SCALE))
                {
                    Inspector.Operation = ImGuizmoOperation.SCALE;
                }

                if (ImGui.RadioButton("Local", Inspector.Mode == ImGuizmoNET.ImGuizmoMode.LOCAL))
                {
                    Inspector.Mode = ImGuizmoNET.ImGuizmoMode.LOCAL;
                }

                ImGui.SameLine();
                if (ImGui.RadioButton("World", Inspector.Mode == ImGuizmoNET.ImGuizmoMode.WORLD))
                {
                    Inspector.Mode = ImGuizmoNET.ImGuizmoMode.WORLD;
                }
            }
            ImGui.Separator();

            Type type = element.Editor.Type;

            if (ImGui.CollapsingHeader(type.Name))
            {
                element.Editor?.Draw();
            }

            ImGui.Separator();
            if (ImGui.CollapsingHeader("Components"))
            {
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

                    ImGui.Separator();
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
                    if (ImGui.CollapsingHeader(component.Editor.Name))
                    {
                        if (ImGui.Button("Delete"))
                        {
                            scene.Dispatcher.Invoke(() => element.RemoveComponent(component));
                        }
                        component.Editor?.Draw();
                    }
                }
            }

            EndWindow();
        }
    }
}