#nullable disable

namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Mathematics;
    using HexaEngine.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Numerics;
    using System.Reflection;

    public class EditorComponentCategory
    {
        private List<EditorComponentAttribute> components = new();
        private readonly List<EditorComponentCategory> childCategories = new();
        public ImGuiName guiName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCategory"/> class with the specified category attribute.
        /// </summary>
        /// <param name="attribute">The attribute containing information about the category.</param>
        public EditorComponentCategory(EditorCategoryAttribute attribute)
        {
            CategoryName = attribute.Name;
            CategoryParent = attribute.Parent;
            guiName = new(attribute.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCategory"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        public EditorComponentCategory(string name)
        {
            CategoryName = name;
            CategoryParent = null;
            guiName = new(name);
        }

        public int Count => components.Count;

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// Gets the name of the parent category, if any.
        /// </summary>
        public string? CategoryParent { get; }

        /// <summary>
        /// Gets the list of child categories within this category.
        /// </summary>
        public List<EditorComponentCategory> ChildCategories => childCategories;

        /// <summary>
        /// Gets the list of components within this category.
        /// </summary>
        public List<EditorComponentAttribute> Components => components;

        /// <summary>
        /// Sorts the child categories within this category.
        /// </summary>
        public void Sort()
        {
            for (int i = 0; i < childCategories.Count; i++)
            {
                childCategories[i].Sort();
            }
        }

        public void Add(EditorComponentAttribute component)
        {
            components.Add(component);
        }

        public void Remove(EditorComponentAttribute component)
        {
            components.Remove(component);
        }

        public bool Contains(EditorComponentAttribute component)
        {
            return components.Contains(component);
        }

        public void Clear()
        {
            components.Clear();
        }

        public void Draw(GameObject gameObject)
        {
            if (CategoryName == string.Empty)
            {
                DrawComponents(gameObject);
            }
            else if (ImGui.BeginMenu(guiName.UniqueName))
            {
                DrawComponents(gameObject);
                ImGui.EndMenu();
            }

            for (int i = 0; i < childCategories.Count; i++)
            {
                var category = childCategories[i];
                category.Draw(gameObject);
            }
        }

        private void DrawComponents(GameObject gameObject)
        {
            for (int i = 0; i < components.Count; i++)
            {
                EditorComponentAttribute editorComponent = components[i];
                if (ImGui.MenuItem(editorComponent.Name))
                {
                    IComponent component = editorComponent.Constructor();
                    gameObject.AddComponent(component);
                }
            }
        }
    }

    public class EditorComponentCacheEntry
    {
        public Type Type;
        private readonly List<EditorComponentCategory> categories = new();
        public readonly EditorComponentCategory Default = new(string.Empty);
        private Dictionary<string, EditorComponentCategory> nameToCategory = new();
        private bool initialized;

        public EditorComponentCacheEntry(Type type)
        {
            Type = type;
            categories.Add(Default);
        }

        public bool Initialized => initialized;

        private EditorComponentCategory CreateOrGetCategory(EditorCategoryAttribute categoryAttr)
        {
            if (nameToCategory.TryGetValue(categoryAttr.Name, out var editor))
            {
                return editor;
            }

            editor = new(categoryAttr);
            nameToCategory.Add(categoryAttr.Name, editor);
            categories.Add(editor);
            return editor;
        }

        private EditorComponentCategory CreateOrGetCategory(string category)
        {
            if (nameToCategory.TryGetValue(category, out var editor))
            {
                return editor;
            }

            editor = new(category);
            nameToCategory.Add(category, editor);
            categories.Add(editor);
            return editor;
        }

        public void PopulateCache(IList<EditorComponentAttribute> componentCache)
        {
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
                    CacheType(editorComponent);
                    continue;
                }
                else if (editorComponent.AllowedTypes.Length != 0 && !editorComponent.AllowedTypes.Contains(Type))
                {
                    continue;
                }

                if (editorComponent.DisallowedTypes == null)
                {
                    CacheType(editorComponent);
                    continue;
                }
                else if (!editorComponent.DisallowedTypes.Contains(Type))
                {
                    CacheType(editorComponent);
                    continue;
                }
            }

            Queue<EditorComponentCategory> removeQueue = new();
            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                if (category.CategoryParent != null)
                {
                    var parent = CreateOrGetCategory(category.CategoryParent);
                    parent.ChildCategories.Add(category);
                    removeQueue.Enqueue(category);
                }
            }

            while (removeQueue.TryDequeue(out var category))
            {
                categories.Remove(category);
            }

            for (int i = 0; i < categories.Count; i++)
            {
                categories[i].Sort();
            }

            initialized = true;
        }

        private void CacheType(EditorComponentAttribute componentAttribute)
        {
            var categoryAttr = componentAttribute.Type.GetCustomAttribute<EditorCategoryAttribute>();
            if (categoryAttr == null)
            {
                Default.Add(componentAttribute);
                return;
            }

            var category = CreateOrGetCategory(categoryAttr);
            category.Components.Add(componentAttribute);
        }

        public void Draw(GameObject gameObject)
        {
            if (ImGui.BeginMenu("\xE710 Add Component"))
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];
                    category.Draw(gameObject);
                }
                ImGui.EndMenu();
            }
        }
    }

    // TODO: Needs major overhaul.
    // TODO: Bug fix.
    // TODO: Add ability to use custom property editors, needs a registry or something and global selection tracking.
    /// <summary>
    /// A Property editor for editing.
    /// </summary>
    public class PropertiesWidget : EditorWindow
    {
        private const int TextBufSize = 2048;
        private readonly List<EditorComponentAttribute> componentCache = new();
        private readonly Dictionary<Type, EditorComponentCacheEntry> typeFilterComponentCache = new();

        private bool showHidden = false;

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
                .Where(x => x != null && !x.IsHidden && !x.IsInternal)));
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

        private void DrawContextMenu(Type type, GameObject element)
        {
            if (ImGui.BeginPopupContextWindow())
            {
                if (typeFilterComponentCache.TryGetValue(type, out EditorComponentCacheEntry cacheEntry))
                {
                    cacheEntry.Draw(element);
                    ImGui.Separator();
                }
                else
                {
                    PopulateTypeCache(type);
                }

                DrawSettingsMenu();

                ImGui.EndPopup();
            }
        }

        private void DoRemoveComponent(object context)
        {
            if (context is not (Scene scene, GameObject element, IComponent component))
            {
                return;
            }
            scene.Dispatcher.Invoke((element, component), GameObject.RemoveComponent);
        }

        private void UndoRemoveComponent(object context)
        {
            if (context is not (Scene scene, GameObject element, IComponent component))
            {
                return;
            }
            scene.Dispatcher.Invoke((element, component), GameObject.AddComponent);
        }

        private void DrawContextMenuComponent(Scene scene, GameObject element, IComponent component)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("\xE738 Delete"))
                {
                    History.Default.Do("Remove Component", (scene, element, component), DoRemoveComponent, UndoRemoveComponent);
                }

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

        public override void DrawContent(IGraphicsContext context)
        {
            if (GameObject.Selected.Count == 0)
            {
                EndWindow();
                return;
            }

            GameObject element = GameObject.Selected.First();
            Scene scene = element.GetScene();
            var type = element.Type;

            DrawContextMenu(type, element);

            bool isEnabled = element.IsEnabled;
            if (ImGui.Checkbox("##Enabled", ref isEnabled))
            {
                element.IsEnabled = isEnabled;
            }

            ImGui.SameLine();

            string name = element.Name;
            if (ImGui.InputText("##Name", ref name, TextBufSize, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                element.Name = name;
            }

            ImGui.Separator();

            DrawObjectEditor(context, element, type);

            ImGui.Separator();

            ImGui.PushID(name);

            ImGui.BeginGroup();

            DrawComponents(context, element, scene);

            // fill up space so that the context menu is completely filling the empty space.
            var space = ImGui.GetContentRegionAvail();
            ImGui.Dummy(space);

            ImGui.EndGroup();

            ImGui.PopID();

            EndWindow();
        }

        private void DrawComponents(IGraphicsContext context, GameObject element, Scene scene)
        {
            for (int i = 0; i < element.Components.Count; i++)
            {
                var component = element.Components[i];
                var editor = ObjectEditorFactory.CreateEditor(component.GetType());

                if (editor.IsHidden && !showHidden)
                {
                    continue;
                }

                ImGui.BeginDisabled(editor.IsHidden);

                editor.Instance = component;

                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.Framed;
                if (!editor.IsHidden)
                {
                    flags |= ImGuiTreeNodeFlags.DefaultOpen;
                }

                string name = $"{editor.Name}##{i}";

                if (ImGui.CollapsingHeader(name, flags))
                {
                    editor?.Draw(context);
                }

                DrawContextMenuComponent(scene, element, component);

                ImGui.EndDisabled();
            }
        }

        private static void DrawObjectEditor(IGraphicsContext context, GameObject element, Type type)
        {
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
                        History.Default.Do("Set Position", element.Transform, oldVal, val, SetPosition, RestorePosition);
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
                        History.Default.Do("Set Rotation", element.Transform, oldVal, val, SetRotation, RestoreRotation);
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
                        History.Default.Do("Set Scale", element.Transform, oldVal, val, SetScale, RestoreScale);
                    }
                }
                ImGui.EndTable();
            }

            var editor = ObjectEditorFactory.CreateEditor(type);
            editor.Instance = element;

            if (!editor.IsEmpty)
            {
                editor.Draw(context);
            }
        }

        private void PopulateTypeCache(Type type)
        {
            EditorComponentCacheEntry cacheEntry = new(type);
            cacheEntry.PopulateCache(componentCache);
            typeFilterComponentCache.Add(type, cacheEntry);

            /*
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

            typeFilterComponentCache.Add(type, [.. allowedComponents]);
            */
        }
    }
}