#nullable disable

namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public class PropertiesWidget : EditorWindow
    {
        private readonly List<EditorComponentAttribute> componentCache = new();
        private readonly Dictionary<Type, EditorComponentAttribute[]> typeFilterComponentCache = new();

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public PropertiesWidget()
        {
            IsShown = true;
            componentCache.AddRange(
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
                x.GetTypes()
                .AsParallel()
                .Where(x => x.IsAssignableTo(typeof(IComponent)))
                .Select(x => x.GetCustomAttribute<EditorComponentAttribute>())
                .Where(x => x != null)));
            Flags = ImGuiWindowFlags.MenuBar;
        }

        protected override string Name => "Properties";

        private static void SetPosition(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.Position = ctx.NewValue;
        }

        private static void RestorePosition(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.Position = ctx.OldValue;
        }

        private static void SetRotation(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.Rotation = ctx.NewValue;
        }

        private static void RestoreRotation(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.Rotation = ctx.OldValue;
        }

        private static void SetScale(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.Scale = ctx.NewValue;
        }

        private static void RestoreScale(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.Scale = ctx.OldValue;
        }

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

            bool isEnabled = element.IsEnabled;
            if (ImGui.Checkbox("##Enabled", ref isEnabled))
            {
                element.IsEnabled = isEnabled;
            }

            ImGui.SameLine();

            string name = element.Name;
            if (ImGui.InputText("##Name", ref name, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                element.Name = name;
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader(nameof(Transform), ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginTable("Transform", 2, ImGuiTableFlags.SizingFixedFit);
                ImGui.TableSetupColumn("");
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Position");
                ImGui.TableSetColumnIndex(1);
                {
                    var val = element.Transform.Position;
                    var oldVal = val;
                    if (ImGui.InputFloat3("##Position", ref val))
                    {
                        History.Default.Do(element.Transform, oldVal, val, SetPosition, RestorePosition);
                    }
                }
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Rotation");
                ImGui.TableSetColumnIndex(1);
                {
                    var val = element.Transform.Rotation;
                    var oldVal = val;
                    if (ImGui.InputFloat3("##Rotation", ref val))
                    {
                        History.Default.Do(element.Transform, oldVal, val, SetRotation, RestoreRotation);
                    }
                }
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Scale");
                ImGui.TableSetColumnIndex(1);
                {
                    var val = element.Transform.Scale;
                    var oldVal = val;
                    if (ImGui.InputFloat3("##Scale", ref val))
                    {
                        History.Default.Do(element.Transform, oldVal, val, SetScale, RestoreScale);
                    }
                }
                ImGui.EndTable();
            }
            ImGui.Separator();

            var type = element.Type;

            {
                var editor = ObjectEditorFactory.CreateEditor(type);
                editor.Instance = element;

                if (!editor.IsEmpty)
                {
                    editor.Draw(context);
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
                    else if (editorComponent.AllowedTypes.Length != 0 && !editorComponent.AllowedTypes.Contains(type))
                    {
                        continue;
                    }

                    if (editorComponent.DisallowedTypes == null)
                    {
                        allowedComponents.Add(editorComponent);
                        continue;
                    }
                    else if (!editorComponent.DisallowedTypes.Contains(type))
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
                ImGui.BeginGroup();

                if (ImGui.CollapsingHeader($"{editor.Name}##{i}"))
                {
                    if (ImGui.BeginPopupContextItem(editor.Name))
                    {
                        if (ImGui.MenuItem("Delete"))
                        {
                            scene.Dispatcher.Invoke((element, component), GameObject.RemoveComponent);
                        }
                        ImGui.EndPopup();
                    }

                    editor?.Draw(context);
                }

                ImGui.EndGroup();
            }

            EndWindow();
        }
    }
}