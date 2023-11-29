namespace HexaEngine.Editor.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System;
    using System.Reflection;
    using YamlDotNet.Core.Tokens;

    public class SubTypePropertyEditor : IPropertyEditor
    {
        private ImGuiName guiName;
        private ImGuiName enabledName = new("Enable");
        private readonly IObjectEditor objectEditor;
        private readonly PropertyInfo? enabledInfo;

        public SubTypePropertyEditor(EditorPropertyAttribute attribute, PropertyInfo property)
        {
            Name = attribute.Name;
            Property = property;
            guiName = new(attribute.Name);
            objectEditor = ObjectEditorFactory.CreateEditor(property.PropertyType);
            objectEditor.NoTable = true;
            enabledInfo = property.PropertyType.GetProperty("Enable");
        }

        public string Name { get; }
        public PropertyInfo Property { get; }

        private void DisplayEnabled(object instance)
        {
            ImGui.TableSetColumnIndex(1);
            if (enabledInfo != null)
            {
                var enabled = (bool?)enabledInfo.GetValue(instance, null) ?? throw new();
                if (ImGui.Checkbox(enabledName.Id, ref enabled) && enabledInfo.CanWrite)
                {
                    enabledInfo.SetValue(instance, enabled);
                }
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
                DisplayEnabled(value);
                ImGui.TableSetColumnIndex(0);
                objectEditor.Instance = value;
                objectEditor.Draw(context);
                ImGui.TreePop();
                return false;
            }
            DisplayEnabled(value);
            return false;
        }
    }
}