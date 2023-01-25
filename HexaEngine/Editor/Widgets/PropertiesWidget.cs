﻿#nullable disable

namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using ImGuizmoNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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

            bool isEnabled = element.IsEnabled;
            if (ImGui.Checkbox("Enabled", ref isEnabled))
            {
                element.IsEnabled = isEnabled;
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader(nameof(Transform), ImGuiTreeNodeFlags.DefaultOpen))
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
            }
            ImGui.Separator();

            var type = element.Type;

            {
                var editor = ObjectEditorFactory.CreateEditor(type);
                editor.Instance = element;

                if (!editor.IsEmpty)
                {
                    editor.Draw();
                }
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
                var editor = ObjectEditorFactory.CreateEditor(component.GetType());
                editor.Instance = component;
                if (ImGui.CollapsingHeader(editor.Name))
                {
                    if (ImGui.BeginPopupContextWindow(editor.Name))
                    {
                        if (ImGui.MenuItem("Delete"))
                        {
                            scene.Dispatcher.Invoke(() => element.RemoveComponent(component));
                        }
                        ImGui.EndPopup();
                    }

                    editor?.Draw();
                }
            }

            EndWindow();
        }
    }
}