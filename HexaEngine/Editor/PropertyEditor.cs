namespace HexaEngine.Editor
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Rendering;
    using ImGuiNET;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;

    public class PropertyEditor
    {
        private readonly KeyValuePair<PropertyInfo, Func<object, (bool, object)>>[] callbacks;
        private static readonly Dictionary<Type, KeyValuePair<Array, string[]>> enumCache = new();
        private readonly string name;
        private readonly bool isHidden;

        public PropertyEditor(Type type)
        {
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

            var properties = type.GetProperties();
            List<KeyValuePair<PropertyInfo, Func<object, (bool, object)>>> values = new();
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

                if (propType.IsEnum)
                {
                    if (!(property.CanWrite && property.CanRead)) continue;
                    if (!enumCache.ContainsKey(propType))
                    {
                        enumCache.Add(propType, new(Enum.GetValues(propType), Enum.GetNames(propType)));
                    }

                    values.Add(new(property, value =>
                    {
                        var enums = enumCache[propType];
                        int index = Array.IndexOf(enums.Key, value);
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
                        bool val = (bool)value;
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
                            float val = (float)value;
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
                        float val = (float)value;
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
                        Vector2 val = (Vector2)value;
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
                            Vector3 val = (Vector3)value;
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
                        Vector3 val = (Vector3)value;
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
                            Vector4 val = (Vector4)value;
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
                        Vector4 val = (Vector4)value;
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
                        string val = (string)value;
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
                        Texture val = (Texture)value;
                        if (val != null)
                            ImGui.Image(ImGuiRenderer.TryRegisterTexture(val.ResourceView), new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowWidth()));
                        return (false, null);
                    }));
                    continue;
                }
            }

            callbacks = values.ToArray();
        }

        public void Draw(object instance)
        {
            if (isHidden) return;
            ImGui.Text(name);
            for (int i = 0; i < callbacks.Length; i++)
            {
                var callback = callbacks[i];
                var value = callback.Key.GetValue(instance);
                var result = callback.Value(value);
                if (result.Item1)
                {
                    callback.Key.SetValue(instance, result.Item2);
                }
            }
        }
    }
}