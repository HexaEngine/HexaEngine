namespace HexaEngine.Editor.Editors
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;
    using Hexa.NET.ImGui;
    using System.Numerics;
    using System.Reflection;
    using HexaEngine.Mathematics;

    public class QuaternionPropertyEditor : IPropertyEditor
    {
        private ImGuiName guiName;

        public QuaternionPropertyEditor(string name, PropertyInfo property)
        {
            Name = name;
            Property = property;
            guiName = new(name);
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            Quaternion q = (Quaternion)value;

            var val = q.ToYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(guiName.Name);
            ImGui.TableSetColumnIndex(1);
            ImGui.PushItemWidth(0);
            if (ImGui.InputFloat3(guiName.Id, ref val))
            {
                value = val.NormalizeEulerAngleDegrees().ToRad().ToQuaternion();
                ImGui.PopItemWidth();
                return true;
            }
            ImGui.PopItemWidth();

            return false;
        }
    }
}