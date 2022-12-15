#nullable disable

namespace HexaEngine.Editor
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Rendering;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;
    using System.Reflection;

    public class PropertyEditor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : IPropertyEditor
    {
        private readonly KeyValuePair<PropertyInfo, Func<(object, object), (bool, object)>>[] callbacks;
        private static readonly Dictionary<Type, KeyValuePair<Array, string[]>> enumCache = new();
        private static readonly Dictionary<Type, KeyValuePair<Type[], string[]>> typeCache = new();
        private readonly string name;
        private readonly bool isHidden;
        private readonly T instance;

        public string Name => name;

        public Type Type { get; }

        public PropertyInfo[] Properties { get; }

        public PropertyEditor(T instance)
        {
            Type type = Type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            var componentNameAttr = type.GetCustomAttribute<EditorComponentAttribute>();
            if (componentNameAttr == null)
            {
                name = type.Name;
            }
            else
            {
                name = componentNameAttr.Name;
                isHidden = componentNameAttr.IsHidden;
            }

            var nodeNameAttr = type.GetCustomAttribute<EditorNodeAttribute>();
            if (nodeNameAttr != null)
            {
                name = nodeNameAttr.Name;
            }

            List<KeyValuePair<PropertyInfo, Func<(object, object), (bool, object)>>> values = new();
            List<KeyValuePair<PropertyInfo, Array>> cache = new();
            foreach (var property in properties)
            {
                var propType = property.PropertyType;

                var nameAttr = property.GetCustomAttribute<EditorPropertyAttribute>();
                if (nameAttr == null)
                {
                    continue;
                }

                var name = nameAttr.Name;

                if (propType == typeof(Type) && nameAttr.Mode == EditorPropertyMode.TypeSelector)
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    if (!typeCache.ContainsKey(nameAttr.TargetType))
                    {
                        typeCache.Add(nameAttr.TargetType, new(nameAttr.Types, nameAttr.TypeNames));
                    }

                    values.Add(new(property, value =>
                    {
                        var types = typeCache[nameAttr.TargetType];
                        int index = Array.IndexOf(types.Key, value);
                        if (ImGui.Combo(name, ref index, types.Value, types.Value.Length))
                        {
                            return (true, types.Key.GetValue(index));
                        }
                        return (false, null);
                    }));
                }

                if (propType.IsEnum && nameAttr.Mode == EditorPropertyMode.Enum)
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    if (!enumCache.ContainsKey(propType))
                    {
                        enumCache.Add(propType, new(nameAttr.EnumValues, nameAttr.EnumNames));
                    }

                    values.Add(new(property, value =>
                    {
                        var enums = enumCache[propType];
                        int index = Array.IndexOf(enums.Key, value.Item2);
                        if (ImGui.Combo(name, ref index, enums.Value, enums.Value.Length))
                        {
                            return (true, enums.Key.GetValue(index));
                        }
                        return (false, null);
                    }));
                    continue;
                }

                if (propType == typeof(bool))
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        bool val = (bool)value.Item2;
                        if (ImGui.Checkbox(name, ref val))
                        {
                            return (true, val);
                        }
                        return (false, null);
                    }));
                    continue;
                }

                if (nameAttr.Mode == EditorPropertyMode.Slider)
                    if (propType == typeof(float))
                    {
                        float min = (float)nameAttr.Min;
                        float max = (float)nameAttr.Max;
                        if (!(property.CanWrite && property.CanRead)) continue;
                        values.Add(new(property, value =>
                        {
                            float val = (float)value.Item2;
                            if (ImGui.SliderFloat(name, ref val, min, max))
                            {
                                return (true, val);
                            }
                            return (false, null);
                        }));
                        continue;
                    }

                if (propType == typeof(float))
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        float val = (float)value.Item2;
                        if (ImGui.InputFloat(name, ref val))
                        {
                            return (true, val);
                        }
                        return (false, null);
                    }));
                    continue;
                }

                if (propType == typeof(Vector2))
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        Vector2 val = (Vector2)value.Item2;
                        if (ImGui.InputFloat2(name, ref val))
                        {
                            return (true, val);
                        }
                        return (false, null);
                    }));
                    continue;
                }
                if (nameAttr.Mode == EditorPropertyMode.Colorpicker)
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    if (propType == typeof(Vector3))
                    {
                        values.Add(new(property, value =>
                        {
                            Vector3 val = (Vector3)value.Item2;
                            if (ImGui.ColorEdit3(name, ref val, ImGuiColorEditFlags.Float | ImGuiColorEditFlags.DisplayRGB | ImGuiColorEditFlags.InputRGB))
                            {
                                return (true, val);
                            }
                            return (false, null);
                        }));
                        continue;
                    }
                }

                if (propType == typeof(Vector3))
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        Vector3 val = (Vector3)value.Item2;
                        if (ImGui.InputFloat3(name, ref val))
                        {
                            return (true, val);
                        }
                        return (false, null);
                    }));
                    continue;
                }

                if (nameAttr.Mode == EditorPropertyMode.Colorpicker)
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    if (propType == typeof(Vector4))
                    {
                        values.Add(new(property, value =>
                        {
                            Vector4 val = (Vector4)value.Item2;
                            if (ImGui.ColorEdit4(name, ref val, ImGuiColorEditFlags.Float | ImGuiColorEditFlags.DisplayRGB | ImGuiColorEditFlags.InputRGB))
                            {
                                return (true, val);
                            }
                            return (false, null);
                        }));
                        continue;
                    }
                }

                if (propType == typeof(Vector4))
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        Vector4 val = (Vector4)value.Item2;
                        if (ImGui.InputFloat4(name, ref val))
                        {
                            return (true, val);
                        }
                        return (false, null);
                    }));
                    continue;
                }

                if (propType == typeof(string))
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        string val = (string)value.Item2;
                        if (ImGui.InputText(name, ref val, 256))
                        {
                            return (true, val);
                        }
                        return (false, null);
                    }));
                    continue;
                }

                if (propType == typeof(Texture))
                {
                    if (!(property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        Texture val = (Texture)value.Item2;
                        if (val != null)
                            ImGui.Image(val.ResourceView.NativePointer, new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowWidth()));
                        return (false, null);
                    }));
                    continue;
                }
            }

            callbacks = values.ToArray();
            Properties = properties;
            this.instance = instance;
        }

        public void Draw()
        {
            if (isHidden) return;
            for (int i = 0; i < callbacks.Length; i++)
            {
                var callback = callbacks[i];
                var value = callback.Key.GetValue(instance);
                var oldValue = value;
                var result = callback.Value((instance, value));

                if (result.Item1)
                {
                    Designer.History.Do(
                        () => callback.Key.SetValue(instance, result.Item2),
                        () => callback.Key.SetValue(instance, oldValue));
                }
            }
        }
    }
}