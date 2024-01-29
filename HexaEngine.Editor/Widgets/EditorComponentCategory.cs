namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scenes;
    using System.Collections.Generic;

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
}