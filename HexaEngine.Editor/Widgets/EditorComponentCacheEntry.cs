namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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
}