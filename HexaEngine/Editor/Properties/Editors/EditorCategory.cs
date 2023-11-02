namespace HexaEngine.Editor.Properties.Editors
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

    public class EditorCategory : IPropertyEditor
    {
        private readonly List<(PropertyInfo, IPropertyEditor)> editors = new();
        private readonly List<ObjectEditorButton> buttons = new();
        private PropertyInfo? enableProp;
        private ImGuiName enablePropName = new("Enable");
        private List<EditorCategory> childCategories = new();
        private readonly ImGuiName guiName;

        public EditorCategory(EditorCategoryAttribute attribute)
        {
            CategoryName = attribute.Name;
            CategoryParent = attribute.Parent;
            guiName = new(attribute.Name);
        }

        public EditorCategory(string name)
        {
            CategoryName = name;
            CategoryParent = null;
            guiName = new(name);
        }

        public string CategoryName { get; }

        public string? CategoryParent { get; }

        public List<EditorCategory> ChildCategories => childCategories;

        public List<(PropertyInfo, IPropertyEditor)> Properties => editors;

        public List<ObjectEditorButton> Buttons => buttons;

        public string Name { get; } = string.Empty;

        public PropertyInfo Property => throw new NotSupportedException();

        public void Sort()
        {
            for (int i = 0; i < childCategories.Count; i++)
            {
                childCategories[i].Sort();
            }
        }

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
                category.Draw(context, instance, ref instance);
            }
        }

        private static void DoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.NewValue);
        }

        private static void UndoAction(object context)
        {
            var ctx = (HistoryContext<(object, PropertyInfo), object>)context;
            ctx.Target.Item2.SetValue(ctx.Target.Item1, ctx.OldValue);
        }
    }
}