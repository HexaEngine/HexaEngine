﻿namespace HexaEngine.Editor.Properties.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;

    /// <summary>
    /// Represents a category within an object editor that contains properties, buttons, and child categories.
    /// </summary>
    public class EditorCategory : IPropertyEditor
    {
        private readonly List<(PropertyInfo, IPropertyEditor)> editors = new();
        private readonly List<ObjectEditorButton> buttons = new();
        private PropertyInfo? enableProp;
        private ImGuiName enablePropName = new("Enable");
        private List<EditorCategory> childCategories = new();
        private readonly ImGuiName guiName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCategory"/> class with the specified category attribute.
        /// </summary>
        /// <param name="attribute">The attribute containing information about the category.</param>
        public EditorCategory(EditorCategoryAttribute attribute)
        {
            CategoryName = attribute.Name;
            CategoryParent = attribute.Parent;
            guiName = new(attribute.Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCategory"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        public EditorCategory(string name)
        {
            CategoryName = name;
            CategoryParent = null;
            guiName = new(name);
        }

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
        public List<EditorCategory> ChildCategories => childCategories;

        /// <summary>
        /// Gets the list of properties within this category.
        /// </summary>
        public List<(PropertyInfo, IPropertyEditor)> Properties => editors;

        /// <summary>
        /// Gets the list of buttons within this category.
        /// </summary>
        public List<ObjectEditorButton> Buttons => buttons;

        /// <summary>
        /// Gets the name associated with the property editor (always an empty string for categories).
        /// </summary>
        public string Name { get; } = string.Empty;

        /// <summary>
        /// Gets the property information associated with the property editor (not applicable for categories).
        /// </summary>
        public PropertyInfo Property => throw new NotSupportedException();

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

        /// <summary>
        /// Applies background color based on interaction status.
        /// </summary>
        private unsafe void Color()
        {
            var hovered = ImGui.IsItemHovered(ImGuiHoveredFlags.None);
            var active = ImGui.IsItemActive();

            Vector4 col = *ImGui.GetStyleColorVec4(ImGuiCol.Header);
            if (hovered)
            {
                col = *ImGui.GetStyleColorVec4(ImGuiCol.HeaderHovered);
            }
            if (active)
            {
                col = *ImGui.GetStyleColorVec4(ImGuiCol.HeaderActive);
            }

            if (CategoryParent == null || hovered || active)
            {
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.ColorConvertFloat4ToU32(col));
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.ColorConvertFloat4ToU32(col));
            }
        }

        /// <summary>
        /// Draws the category within the specified graphics context.
        /// </summary>
        /// <param name="context">The graphics context used for drawing.</param>
        /// <param name="instance">The instance of the object containing the category.</param>
        /// <param name="value">The current value of the category (not applicable for categories).</param>
        /// <returns><c>true</c> if the category was modified; otherwise, <c>false</c>.</returns>
        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            if (value == null)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (ImGui.TreeNodeEx(guiName.UniqueName))
            {
                Color();
                ImGui.TableSetColumnIndex(0);
                Draw(context, value);
                ImGui.TreePop();
                return false;
            }
            Color();

            return false;
        }

        /// <summary>
        /// Draws the properties, buttons, and child categories within this category.
        /// </summary>
        /// <param name="context">The graphics context used for drawing.</param>
        /// <param name="instance">The instance of the object containing the category.</param>
        private void Draw(IGraphicsContext context, object instance)
        {
            for (int i = 0; i < editors.Count; i++)
            {
                var editor = editors[i];
                var value = editor.Item1.GetValue(instance);
                var oldValue = value;

                if (editor.Item2.Draw(context, instance, ref value))
                {
                    History.Default.Do((instance, editor.Item1), oldValue, value, DoAction, UndoAction);
                }
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Draw(instance);
            }

            for (int i = 0; i < childCategories.Count; i++)
            {
                var category = childCategories[i];
#nullable disable
                category.Draw(context, instance, ref instance);
#nullable restore
            }
        }

        /// <summary>
        /// Action to perform when applying changes during a history action.
        /// </summary>
        /// <param name="context">The context containing information about the history action.</param>
        private static void DoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.NewValue);
        }

        /// <summary>
        /// Action to perform when undoing changes during a history action.
        /// </summary>
        /// <param name="context">The context containing information about the history action.</param>
        private static void UndoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.OldValue);
        }
    }
}