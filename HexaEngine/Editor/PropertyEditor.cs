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

    public class PropertyEditor
    {
        private readonly KeyValuePair<PropertyInfo, Func<(object, object), (bool, object)>>[] callbacks;
        private static readonly Dictionary<Type, KeyValuePair<Array, string[]>> enumCache = new();
        private static readonly Dictionary<Type, KeyValuePair<Type[], string[]>> typeCache = new();
        private readonly string name;
        private readonly bool isHidden;

        public PropertyEditor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
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
                    if (!typeCache.ContainsKey(nameAttr.Type))
                    {
                        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).AsParallel().Where(x => x.IsAssignableTo(nameAttr.Type) && !x.IsInterface && !x.IsAbstract).ToArray();
                        var names = types.Select(x => x.Name).ToArray();
                        typeCache.Add(nameAttr.Type, new(types, names));
                    }

                    values.Add(new(property, value =>
                    {
                        var types = typeCache[nameAttr.Type];
                        int index = Array.IndexOf(types.Key, value);
                        if (ImGui.Combo(name, ref index, types.Value, types.Value.Length))
                        {
                            return (true, types.Key.GetValue(index));
                        }
                        return (false, null);
                    }));
                }

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

                if (propType == typeof(RenderTexture))
                {
                    if (!(property.CanRead)) continue;
                    values.Add(new(property, value =>
                    {
                        RenderTexture val = (RenderTexture)value.Item2;
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