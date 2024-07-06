namespace HexaEngine.Editor.ColorGradingEditor
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Editors;
    using HexaEngine.Editor.Properties;
    using HexaEngine.PostFx;
    using HexaEngine.PostFx.BuildIn;
    using HexaEngine.Volumes;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Numerics;
    using System.Reflection;

    public delegate bool CustomPropertyEditorCallback(IGraphicsContext context, object instance, ref object? value);

    public class CustomPropertyEditor : IPropertyEditor
    {
        private readonly CustomPropertyEditorCallback callback;

        public CustomPropertyEditor(string name, PropertyInfo property, CustomPropertyEditorCallback callback)
        {
            Name = name;
            Property = property;
            this.callback = callback;
        }

        public string Name { get; }

        public PropertyInfo Property { get; }

        public bool Draw(IGraphicsContext context, object instance, ref object? value)
        {
            return callback(context, instance, ref value);
        }
    }

    public class EditorBuilder
    {
        private readonly Type type;
        private readonly PropertyInfo[] properties;
        private readonly MethodInfo[] methods;
        private readonly List<IPropertyEditor> editors = new();
        private readonly List<IObjectEditorElement> elements = new();
        private readonly List<EditorCategory> categories = new();
        private readonly Dictionary<string, EditorCategory> nameToCategory = new();

        public EditorBuilder([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
        {
            this.type = type;
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        }

        public Type Type => type;

        public EditorBuilder AddDefaults()
        {
            foreach (var prop in properties)
            {
                var editor = ObjectEditorFactory.CreatePropertyEditor(prop);
                if (editor == null)
                {
                    continue;
                }
                editors.Add(editor);
            }
            return this;
        }

        public EditorBuilder AddDefault(string name)
        {
            var prop = properties.FirstOrDefault(x => x.Name == name);
            if (prop == null)
            {
                return this;
            }

            var editor = ObjectEditorFactory.CreatePropertyEditor(prop);
            if (editor == null)
            {
                return this;
            }
            editors.Add(editor);
            return this;
        }

        public EditorBuilder Overwrite(string name, IPropertyEditor editor)
        {
            var prop = properties.FirstOrDefault(x => x.Name == name);
            if (prop == null)
            {
                return this;
            }

            int key = 0;
            while (key < editors.Count && editors[key].Property != prop)
            {
                key++;
            }

            if (key == editors.Count)
            {
                return this;
            }

            editors.RemoveAt(key);
            editors.Insert(key, editor);
            return this;
        }

        public EditorBuilder Overwrite(string name, CustomPropertyEditorCallback callback)
        {
            int keyProp = 0;
            while (keyProp < properties.Length && properties[keyProp].Name != name)
            {
                keyProp++;
            }

            if (keyProp == properties.Length)
            {
                return this;
            }

            var prop = properties[keyProp];

            int key = 0;
            while (key < editors.Count && editors[key].Property != prop)
            {
                key++;
            }

            if (key == editors.Count)
            {
                editors.Add(new CustomPropertyEditor(name, prop, callback));
                return this;
            }

            editors.RemoveAt(key);
            editors.Insert(key, new CustomPropertyEditor(name, prop, callback));
            return this;
        }

        public EditorBuilder Ignore(string name)
        {
            var prop = properties.FirstOrDefault(x => x.Name == name);
            if (prop == null)
            {
                return this;
            }

            int key = 0;
            while (key < editors.Count && editors[key].Property != prop)
            {
                key++;
            }

            if (key == editors.Count)
            {
                return this;
            }

            editors.RemoveAt(key);
            return this;
        }

        public int GetOrderIndex(string name)
        {
            for (int i = 0; i < editors.Count; i++)
            {
                if (editors[i].Property.Name == name)
                {
                    return i; // Return the index of the editor with matching property
                }
            }

            return -1; // Editor not found
        }

        public void SetOrderIndex(string name, int order)
        {
            var prop = properties.FirstOrDefault(x => x.Name == name);
            if (prop == null)
            {
                return;
            }

            int key = 0;
            while (key < editors.Count && editors[key].Property != prop)
            {
                key++;
            }

            if (key == editors.Count)
            {
                return;
            }

            var editor = editors[key];
            editors.RemoveAt(key);
            editors.Insert(order, editor);
        }

        public void Build()
        {
            foreach (var editor in editors)
            {
                var prop = editor.Property;
                var categoryAttr = prop.GetCustomAttribute<EditorCategoryAttribute>();
                var conditionAttr = prop.GetCustomAttribute<EditorPropertyConditionAttribute>();

                PropertyEditorObjectEditorElement element = new(editor)
                {
                    Condition = conditionAttr?.Condition,
                    ConditionMode = conditionAttr?.Mode ?? EditorPropertyConditionMode.None
                };

                if (categoryAttr != null)
                {
                    var category = CreateOrGetCategory(categoryAttr);
                    category.Elements.Add(element);
                }
                else
                {
                    elements.Add(element);
                }
            }

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo? method = methods[i];
                var buttonAttr = method.GetCustomAttribute<EditorButtonAttribute>();
                if (buttonAttr == null)
                {
                    continue;
                }

                var categoryAttr = method.GetCustomAttribute<EditorCategoryAttribute>();
                var conditionAttr = method.GetCustomAttribute<EditorPropertyConditionAttribute>();

                EditorButtonObjectEditorElement element = new(new(buttonAttr, method))
                {
                    Condition = conditionAttr?.Condition,
                    ConditionMode = conditionAttr?.Mode ?? EditorPropertyConditionMode.None
                };

                if (categoryAttr != null)
                {
                    var category = CreateOrGetCategory(categoryAttr);
                    category.Elements.Add(element);
                }
                else
                {
                    elements.Add(element);
                    break;
                }
            }

            Queue<EditorCategory> removeQueue = new();
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
        }

        private EditorCategory CreateOrGetCategory(EditorCategoryAttribute categoryAttr)
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

        private EditorCategory CreateOrGetCategory(string category)
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

        public bool Draw(IGraphicsContext context, object instance, ProxyBase proxy, bool setOnInstance)
        {
            if (instance == null)
            {
                return false;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].UpdateVisibility(instance);
            }

            bool changed = false;

            for (int i = 0; i < elements.Count; i++)
            {
                changed |= elements[i].Draw(context, instance, proxy, setOnInstance);
            }

            object? nullObj = null;

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];

                changed |= category.Draw(context, instance, ref nullObj, proxy, setOnInstance);
            }

            return changed;
        }

        public bool Draw(IGraphicsContext context, object instance)
        {
            if (instance == null)
            {
                return false;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].UpdateVisibility(instance);
            }

            bool changed = false;

            for (int i = 0; i < elements.Count; i++)
            {
                changed |= elements[i].Draw(context, instance);
            }

            object? nullObj = null;

            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];

                changed |= category.Draw(context, instance, ref nullObj);
            }

            return changed;
        }

        public void Reset()
        {
            editors.Clear();
            elements.Clear();
            categories.Clear();
            nameToCategory.Clear();
        }
    }

    public sealed class ColorGradingObjectEditor : IPostProcessingEditor
    {
        private readonly EditorBuilder builder;
        private TrackballMode trackballMode;
        private GradingCurveType gradingCurveType;
        private int selection;

        private enum TrackballMode
        {
            Linear,
            Log
        }

        public enum GradingCurveType
        {
            Value,
            R,
            G,
            B,
            HueVsHue,
            HueVsSat,
            SatVsSat,
            LumaVsSat
        }

        public Type Type { get; } = typeof(ColorGrading);

        public bool IsEmpty { get; }

        public bool IsHidden { get; }

        public ColorGradingObjectEditor()
        {
            builder = new EditorBuilder(typeof(ColorGrading));
            builder.AddDefaults()
                .Ignore(nameof(ColorGrading.Enabled))
                .Overwrite(nameof(ColorGrading.Lift), EditLift)
                .Overwrite(nameof(ColorGrading.Gamma), EditGamma)
                .Overwrite(nameof(ColorGrading.Gain), EditGain)
                .Overwrite(nameof(ColorGrading.Offset), EditOffset)
                .Overwrite(nameof(ColorGrading.Power), EditPower)
                .Overwrite(nameof(ColorGrading.Slope), EditSlope)
                .Overwrite(nameof(ColorGrading.Curves), EditCurves);

            int index = builder.GetOrderIndex(nameof(ColorGrading.Lift));
            builder.SetOrderIndex(nameof(ColorGrading.Gamma), index + 1);
            builder.SetOrderIndex(nameof(ColorGrading.Gain), index + 2);
            builder.Build();
        }

        private bool EditCurves(IGraphicsContext context, object instance, ref object? value)
        {
            if (value is not ColorGradingCurves curves)
            {
                return false;
            }

            ImGui.PushItemWidth(200);
            if (ComboEnumHelper<GradingCurveType>.Combo("Curve Type", ref gradingCurveType))
            {
                selection = -1;
            }

            string label = "Curve";
            Vector2 size = new(400, 400);

            ImGui.PopItemWidth();
            bool changed = false;
            switch (gradingCurveType)
            {
                case GradingCurveType.Value:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFFFFFFFF);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.ValueCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;

                case GradingCurveType.R:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFF0000FF);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.RedCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;

                case GradingCurveType.G:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFF00FF00);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.GreenCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;

                case GradingCurveType.B:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFFFF0000);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.BlueCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;

                case GradingCurveType.HueVsHue:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFFFFFFFF);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.HueCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;

                case GradingCurveType.HueVsSat:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFFFFFFFF);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.HueSatCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;

                case GradingCurveType.SatVsSat:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFFFFFFFF);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.SatCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;

                case GradingCurveType.LumaVsSat:
                    ImGui.PushStyleColor(ImGuiCol.PlotLines, 0xFFFFFFFF);
                    changed = ImGuiCurveEditor3.Curve(label, size, ref curves.LumaSatCurve, Vector2.Zero, Vector2.One, ref selection);
                    break;
            }

            ImGui.PopStyleColor();

            if (changed)
            {
                value = curves;
            }

            return changed;
        }

        private bool EditLift(IGraphicsContext context, object instance, ref object? value)
        {
            if (ImGui.Button("Linear"))
            {
                trackballMode = TrackballMode.Linear;
            }

            ImGui.SameLine();

            if (ImGui.Button("Log"))
            {
                trackballMode = TrackballMode.Log;
            }

            if (trackballMode == TrackballMode.Log)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Trackballs");
            ImGui.TableSetColumnIndex(1);

            if (value is not Vector3 lift)
            {
                return false;
            }

            bool changed = ImGuiColorTrackballWidget.ColorTrackballs("Lift", ref lift, new(150, 200));
            if (changed)
            {
                value = lift;
            }

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Reset"))
                {
                    value = Vector3.Zero;
                    changed = true;
                }
                ImGui.EndPopup();
            }
            ImGui.SameLine();
            return changed;
        }

        private bool EditGamma(IGraphicsContext context, object instance, ref object? value)
        {
            if (trackballMode == TrackballMode.Log)
            {
                return false;
            }

            if (value is not Vector3 gamma)
            {
                return false;
            }

            bool changed = ImGuiColorTrackballWidget.ColorTrackballs("Gamma", ref gamma, new(150, 200));
            if (changed)
            {
                value = gamma;
            }
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Reset"))
                {
                    value = Vector3.One;
                    changed = true;
                }
                ImGui.EndPopup();
            }
            ImGui.SameLine();
            return changed;
        }

        private bool EditGain(IGraphicsContext context, object instance, ref object? value)
        {
            if (trackballMode == TrackballMode.Log)
            {
                return false;
            }

            if (value is not Vector3 gain)
            {
                return false;
            }

            bool changed = ImGuiColorTrackballWidget.ColorTrackballs("Gain", ref gain, new(150, 200));
            if (changed)
            {
                value = gain;
            }
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Reset"))
                {
                    value = Vector3.One;
                    changed = true;
                }
                ImGui.EndPopup();
            }
            return changed;
        }

        private bool EditOffset(IGraphicsContext context, object instance, ref object? value)
        {
            if (trackballMode == TrackballMode.Linear)
            {
                return false;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Trackballs");
            ImGui.TableSetColumnIndex(1);

            if (value is not Vector3 offset)
            {
                return false;
            }

            bool changed = ImGuiColorTrackballWidget.ColorTrackballs("Offset", ref offset, new(150, 200));
            if (changed)
            {
                value = offset;
            }

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Reset"))
                {
                    value = Vector3.Zero;
                    changed = true;
                }
                ImGui.EndPopup();
            }
            ImGui.SameLine();
            return changed;
        }

        private bool EditPower(IGraphicsContext context, object instance, ref object? value)
        {
            if (trackballMode == TrackballMode.Linear)
            {
                return false;
            }

            if (value is not Vector3 power)
            {
                return false;
            }

            bool changed = ImGuiColorTrackballWidget.ColorTrackballs("Power", ref power, new(150, 200));
            if (changed)
            {
                value = power;
            }
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Reset"))
                {
                    value = Vector3.One;
                    changed = true;
                }
                ImGui.EndPopup();
            }
            ImGui.SameLine();
            return changed;
        }

        private bool EditSlope(IGraphicsContext context, object instance, ref object? value)
        {
            if (trackballMode == TrackballMode.Linear)
            {
                return false;
            }

            if (value is not Vector3 slope)
            {
                return false;
            }

            bool changed = ImGuiColorTrackballWidget.ColorTrackballs("Slope", ref slope, new(150, 200));
            if (changed)
            {
                value = slope;
            }
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Reset"))
                {
                    value = Vector3.One;
                    changed = true;
                }
                ImGui.EndPopup();
            }
            return changed;
        }

        public bool Draw(IGraphicsContext context, IPostFx effect, PostFxProxy proxy, Volume volume)
        {
            return builder.Draw(context, effect, proxy, volume.Mode == VolumeMode.Global);
        }

        public void Dispose()
        {
            // nothing to dispose.
        }
    }
}