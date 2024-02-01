namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scenes;
    using System.Reflection;

    public class GameObjectReferenceEditor : IPropertyEditor
    {
        private readonly ImGuiName guiName;

        public GameObjectReferenceEditor(string name, PropertyInfo property)
        {
            Name = name;
            Property = property;
            guiName = new(name);
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public unsafe bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            var scene = SceneManager.Current;

            if (scene == null)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);

            bool isOpen;

            if (value is GameObject gameObject)
            {
                isOpen = ImGui.BeginCombo(guiName.Id, gameObject.Name, ImGuiComboFlags.None);
            }
            else
            {
                isOpen = ImGui.BeginCombo(guiName.Id, "<null>", ImGuiComboFlags.None);
            }

            bool result = false;

            if (isOpen)
            {
                for (int i = 0; i < scene.GameObjects.Count; i++)
                {
                    var obj = scene.GameObjects[i];
                    bool isSelected = obj == value;
                    if (ImGui.Selectable(obj.Name, isSelected))
                    {
                        value = obj;
                        result = true;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            return result;
        }
    }
}