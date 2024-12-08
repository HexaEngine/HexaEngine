namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    // TODO: Needs major overhaul.
    // TODO: Bug fix, context menus.
    // TODO: Add ability to use custom property editors, needs a registry or something and global selection tracking.
    public class GameObjectEditor : IPropertyObjectEditor<GameObject>
    {
        private const int TextBufSize = 2048;
        private readonly List<EditorComponentAttribute> componentCache = new();
        private readonly Dictionary<Type, EditorComponentCacheEntry> typeFilterComponentCache = new();

        private bool showHidden = false;

        public bool CanEditMultiple => false;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public GameObjectEditor()
        {
            componentCache.AddRange(
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(static x =>
                x.GetTypes()
                .AsParallel()
                .Where(x => x.IsAssignableTo(typeof(IComponent)))
                .Select(x => x.GetCustomAttribute<EditorComponentAttribute>())
                .Where(x => x != null && !x.IsHidden && !x.IsInternal)
                .Select(x => x!)));
        }

        private static void SetPosition(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.SetPositionOverwrite(ctx.NewValue);
        }

        private static void RestorePosition(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.SetPositionOverwrite(ctx.OldValue);
        }

        private static void SetRotation(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.SetRotationOverwrite(ctx.NewValue);
        }

        private static void RestoreRotation(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.SetRotationOverwrite(ctx.OldValue);
        }

        private static void SetScale(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.SetScaleOverwrite(ctx.NewValue);
        }

        private static void RestoreScale(object context)
        {
            var ctx = (HistoryContext<Transform, Vector3>)context;
            ctx.Target.SetScaleOverwrite(ctx.OldValue);
        }

        private static void SetFlags(object context)
        {
            var ctx = (HistoryContext<Transform, TransformFlags>)context;
            ctx.Target.Flags = ctx.NewValue;
        }

        private static void RestoreFlags(object context)
        {
            var ctx = (HistoryContext<Transform, TransformFlags>)context;
            ctx.Target.Flags = ctx.OldValue;
        }

        private void DoRemoveComponent(object context)
        {
            if (context is not (Scene scene, GameObject element, IComponent component))
            {
                return;
            }
            scene.Dispatcher.Invoke((element, component), GameObject.RemoveComponent);
        }

        private void DoAddComponent(object context)
        {
            if (context is not (Scene scene, GameObject element, IComponent component))
            {
                return;
            }
            scene.Dispatcher.Invoke((element, component), GameObject.AddComponent);
        }

        private bool DrawContextMenu(Type type, GameObject element)
        {
            bool changed = false;
            if (ImGui.BeginPopupContextWindow())
            {
                ImGui.BeginDisabled(element is PrefabObject);

                if (typeFilterComponentCache.TryGetValue(type, out EditorComponentCacheEntry? cacheEntry))
                {
                    changed |= cacheEntry.Draw(element);
                    ImGui.Separator();
                }
                else
                {
                    PopulateTypeCache(type);
                }

                ImGui.EndDisabled();

                DrawSettingsMenu();

                ImGui.EndPopup();
            }
            return changed;
        }

        private void DrawContextMenuComponent(string name, Scene scene, GameObject element, IComponent component)
        {
            if (ImGui.BeginPopupContextItem(name))
            {
                ImGui.BeginDisabled(element is PrefabObject);

                if (ImGui.MenuItem("\xE738 Delete"))
                {
                    History.Default.Do("Remove Component", (scene, element, component), DoRemoveComponent, DoAddComponent);
                }

                ImGui.Separator();

                if (ImGui.MenuItem("\xE8C8 Clone"))
                {
                    History.Default.Do("Clone Component", (scene, element, Instantiator.Instantiate(component)), DoAddComponent, DoRemoveComponent);
                }

                ImGui.EndDisabled();

                ImGui.Separator();

                DrawSettingsMenu();

                ImGui.EndPopup();
            }
        }

        private void DrawSettingsMenu()
        {
            if (ImGui.BeginMenu("\xE713 Settings"))
            {
                ImGui.Checkbox("Show Hidden", ref showHidden);

                ImGui.EndMenu();
            }
        }

        public void Edit(IGraphicsContext context, GameObject gameObject)
        {
            bool changed = false;
            Scene scene = gameObject.GetScene();
            var type = gameObject.Type;

            changed |= DrawContextMenu(type, gameObject);

            bool isEnabled = gameObject.IsEnabled;
            if (ImGui.Checkbox("##Enabled", ref isEnabled))
            {
                gameObject.IsEnabled = isEnabled; changed = true;
            }

            ImGui.SameLine();
            string id = gameObject.FullName;
            string name = gameObject.Name;
            if (ImGui.InputText("##Name", ref name, TextBufSize, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                gameObject.Name = name; changed = true;
            }
            ImGui.Separator();

            changed |= DrawObjectEditor(context, gameObject, type);

            ImGui.Separator();

            ImGui.PushID(id);
            ImGui.BeginGroup();

            ImGui.BeginDisabled(gameObject is PrefabObject);

            changed |= DrawComponents(context, gameObject, scene);

            Vector2 avail = ImGui.GetContentRegionAvail();
            avail.Y = 0;
            avail.X -= 40;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 20);
            if (ImGui.Button("Add Component", avail))
            {
                ImGui.OpenPopup("AddComponentPopup");
            }

            if (ImGui.BeginPopupContextItem("AddComponentPopup"))
            {
                if (typeFilterComponentCache.TryGetValue(type, out EditorComponentCacheEntry? cacheEntry))
                {
                    changed |= cacheEntry.DrawContent(gameObject);
                }
                else
                {
                    PopulateTypeCache(type);
                }

                ImGui.EndPopup();
            }

            ImGui.EndDisabled();

            // fill up space so that the context menu is completely filling the empty space.
            var space = ImGui.GetContentRegionAvail();
            ImGui.Dummy(space);

            ImGui.EndGroup();

            ImGui.PopID();

            if (changed)
            {
                scene.UnsavedChanged = true;
            }
        }

        private bool DrawComponents(IGraphicsContext context, GameObject element, Scene scene)
        {
            bool changed = false;
            for (int i = 0; i < element.Components.Count; i++)
            {
                var component = element.Components[i];
                var editor = ObjectEditorFactory.CreateEditor(component.GetType());

                if (editor.IsHidden && !showHidden)
                {
                    continue;
                }

                string name = $"{editor.Name}##{i}";
                string id = $"##{i}";

                ImGui.PushID(name);

                ImGui.BeginDisabled(editor.IsHidden);

                editor.Instance = component;

                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.NoTreePushOnOpen;
                if (!editor.IsHidden)
                {
                    flags |= ImGuiTreeNodeFlags.DefaultOpen;
                }

                if (TreeNode(editor, id, flags))
                {
                    changed |= editor.Draw(context);
                }

                DrawContextMenuComponent(name, scene, element, component);

                ImGui.EndDisabled();

                ImGui.PopID();

                ImGui.Separator();
            }
            return changed;
        }

        private static bool TreeNode(IObjectEditor editor, string id, string idCheckbox, ref bool enabled, ImGuiTreeNodeFlags flags)
        {
            bool open = ImGui.TreeNodeEx(id, flags);
            ImGui.SameLine();
            ImGui.Text(editor.Symbol);
            ImGui.SameLine();
            ImGui.Checkbox(idCheckbox, ref enabled);
            ImGui.SameLine();
            ImGui.Text(editor.Name);
            return open;
        }

        private static bool TreeNode(IObjectEditor editor, string id, ImGuiTreeNodeFlags flags)
        {
            bool open = ImGui.TreeNodeEx(id, flags);
            ImGui.SameLine(0, 30);
            ImGui.Text(editor.Symbol);
            ImGui.SameLine(0, 5);
            ImGui.Text(editor.Name);
            return open;
        }

        private static bool DrawObjectEditor(IGraphicsContext context, GameObject element, Type type)
        {
            bool changed = false;
            var transform = element.Transform;
            if (ImGui.CollapsingHeader(nameof(Transform), ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.BeginTable("Transform", 2, ImGuiTableFlags.SizingFixedFit);
                ImGui.TableSetupColumn("");
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Position");
                ImGui.TableSetColumnIndex(1);

                var flags = (int)transform.Flags;
                var oldFlags = flags;

                if (ImGui.SmallButton(transform.LockPosition ? "\xE72E##LockPosition" : "\xE785##LockPosition"))
                {
                    if (!transform.LockPosition)
                    {
                        flags |= (int)TransformFlags.LockPosition;
                    }
                    else
                    {
                        flags &= ~(int)TransformFlags.LockPosition;
                    }

                    History.Default.Do("Lock/Unlock Position", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                    changed = true;
                }

                if (ImGui.BeginPopupContextItem("##LockPosition"))
                {
                    if (ImGuiP.CheckboxFlags("\xE72E Axis-X Position", ref flags, (int)TransformFlags.LockPositionX))
                    {
                        History.Default.Do("Lock/Unlock Axis-X Position", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    if (ImGuiP.CheckboxFlags("\xE72E Axis-Y Position", ref flags, (int)TransformFlags.LockPositionY))
                    {
                        History.Default.Do("Lock/Unlock Axis-Y Position", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    if (ImGuiP.CheckboxFlags("\xE72E Axis-Z Position", ref flags, (int)TransformFlags.LockPositionZ))
                    {
                        History.Default.Do("Lock/Unlock Axis-Z Position", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    ImGui.EndPopup();
                }

                ImGui.SameLine();

                {
                    var val = transform.Position;

                    var oldVal = val;
                    if (ImGui.DragFloat3("##Position", ref val))
                    {
                        History.Default.Do("Set Position", transform, oldVal, val, SetPosition, RestorePosition);
                        changed = true;
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Rotation");
                ImGui.TableSetColumnIndex(1);

                if (ImGui.SmallButton(transform.LockRotation ? "\xE72E##LockRotation" : "\xE785##LockRotation"))
                {
                    if (!transform.LockRotation)
                    {
                        flags |= (int)TransformFlags.LockRotation;
                    }
                    else
                    {
                        flags &= ~(int)TransformFlags.LockRotation;
                    }

                    History.Default.Do("Lock/Unlock Rotation", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                    changed = true;
                }

                if (ImGui.BeginPopupContextItem("##LockRotation"))
                {
                    if (ImGuiP.CheckboxFlags("\xE72E Axis-X Rotation", ref flags, (int)TransformFlags.LockRotationX))
                    {
                        History.Default.Do("Lock/Unlock Axis-X Rotation", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    if (ImGuiP.CheckboxFlags("\xE72E Axis-Y Rotation", ref flags, (int)TransformFlags.LockRotationY))
                    {
                        History.Default.Do("Lock/Unlock Axis-Y Rotation", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    if (ImGuiP.CheckboxFlags("\xE72E Axis-Z Rotation", ref flags, (int)TransformFlags.LockRotationZ))
                    {
                        History.Default.Do("Lock/Unlock Axis-Z Rotation", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    ImGui.EndPopup();
                }

                ImGui.SameLine();

                {
                    var val = transform.Rotation;
                    var oldVal = val;
                    if (ImGui.DragFloat3("##Rotation", ref val))
                    {
                        History.Default.Do("Set Rotation", transform, oldVal, val, SetRotation, RestoreRotation);
                        changed = true;
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Scale");
                ImGui.TableSetColumnIndex(1);

                if (ImGui.SmallButton(transform.LockScale ? "\xE72E##LockScale" : "\xE785##LockScale"))
                {
                    if (!transform.LockScale)
                    {
                        flags |= (int)TransformFlags.LockScale;
                    }
                    else
                    {
                        flags &= ~(int)TransformFlags.LockScale;
                    }

                    History.Default.Do("Lock/Unlock Scale", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                    changed = true;
                }

                if (ImGui.BeginPopupContextItem("##LockScale"))
                {
                    if (ImGuiP.CheckboxFlags("\xE72E Axis-X Scale", ref flags, (int)TransformFlags.LockScaleX))
                    {
                        History.Default.Do("Lock/Unlock Axis-X Scale", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    if (ImGuiP.CheckboxFlags("\xE72E Axis-Y Scale", ref flags, (int)TransformFlags.LockScaleY))
                    {
                        History.Default.Do("Lock/Unlock Axis-Y Scale", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    if (ImGuiP.CheckboxFlags("\xE72E Axis-Z Scale", ref flags, (int)TransformFlags.LockScaleZ))
                    {
                        History.Default.Do("Lock/Unlock Axis-Z Scale", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                        changed = true;
                    }

                    ImGui.EndPopup();
                }

                ImGui.SameLine();

                {
                    var val = transform.Scale;
                    var oldVal = val;
                    if (ImGui.DragFloat3("##Scale", ref val))
                    {
                        if (transform.UniformScale)
                        {
                            val = new(oldVal.X != val.X ? val.X : oldVal.Y != val.Y ? val.Y : val.Z);
                        }
                        History.Default.Do("Set Scale", transform, oldVal, val, SetScale, RestoreScale);
                        changed = true;
                    }
                }

                ImGui.SameLine();

                if (ImGui.SmallButton(transform.UniformScale ? "\xF19F##UniformScale" : "\xF19E##UniformScale"))
                {
                    if (!transform.UniformScale)
                    {
                        flags |= (int)TransformFlags.UniformScale;
                    }
                    else
                    {
                        flags &= ~(int)TransformFlags.UniformScale;
                    }

                    History.Default.Do("Uniform Scale On/Off", transform, (TransformFlags)oldFlags, (TransformFlags)flags, SetFlags, RestoreFlags);
                    changed = true;
                }
                TooltipHelper.Tooltip("Uniform Scale On/Off");

                ImGui.EndTable();
            }

            var editor = ObjectEditorFactory.CreateEditor(type);
            editor.Instance = element;

            if (!editor.IsEmpty)
            {
                changed |= editor.Draw(context);
            }

            return changed;
        }

        private void PopulateTypeCache(Type type)
        {
            EditorComponentCacheEntry cacheEntry = new(type);
            cacheEntry.PopulateCache(componentCache);
            typeFilterComponentCache.Add(type, cacheEntry);
        }

        public void EditMultiple(IGraphicsContext context, ICollection<object> objects)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}