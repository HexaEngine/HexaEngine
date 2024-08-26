namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.ColorGradingEditor;
    using HexaEngine.Editor.Properties;
    using HexaEngine.PostFx;
    using HexaEngine.Volumes;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    public static class PostProcessingEditorFactory
    {
        private static readonly Dictionary<Type, IPostProcessingEditor> editors = [];
        private static readonly object _lock = new();

        public static void RegisterEditor(Type type, IPostProcessingEditor editor)
        {
            lock (_lock)
            {
                editors.Add(type, editor);
            }
        }

        public static void RegisterEditor<T>(IPostProcessingEditor editor) where T : IPostFx
        {
            RegisterEditor(typeof(T), editor);
        }

        public static void RegisterEditor<T, TEditor>() where T : IPostFx where TEditor : IPostProcessingEditor, new()
        {
            RegisterEditor(typeof(T), new TEditor());
        }

        public static IPostProcessingEditor CreateEditor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
        {
            if (!editors.TryGetValue(type, out IPostProcessingEditor? editor))
            {
                editor = new DefaultPostProcessingEditor(type);
                editors.Add(type, editor);
            }
            return editor;
        }

        /// <summary>
        /// Destroys an editor for a specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void DestroyEditor(Type type)
        {
            lock (editors)
            {
                if (editors.TryGetValue(type, out var editor))
                {
                    editors.Remove(type);
                    editor.Dispose();
                }
            }
        }

        /// <summary>
        /// Destroys an editor for a specified type.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        public static void DestroyEditor<T>()
        {
            DestroyEditor(typeof(T));
        }

        /// <summary>
        /// Destroys an editor.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public static void DestroyEditor(IPostProcessingEditor editor)
        {
            DestroyEditor(editor.Type);
        }
    }

    public interface IPostProcessingEditor : IDisposable
    {
        bool IsEmpty { get; }

        bool IsHidden { get; }

        Type Type { get; }

        bool Draw(IGraphicsContext context, IPostFx effect, PostFxProxy proxy, Volume volume);
    }

    public class DefaultPostProcessingEditor : IPostProcessingEditor
    {
        private readonly List<IPropertyEditor> editors = [];
        private readonly EditorBuilder builder;

        public bool IsEmpty => editors.Count == 0;

        public bool IsHidden { get; }

        public Type Type { get; }

        public DefaultPostProcessingEditor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
        {
            Type = type;
            builder = new EditorBuilder(type);
            builder.AddDefaults().Ignore("Initialized").Ignore("Enabled").Ignore("Name").Build();

            var props = type.GetProperties();
            foreach (var property in props)
            {
                if (property.Name == "Initialized")
                {
                    continue;
                }

                if (property.Name == "Enabled")
                {
                    continue;
                }

                if (property.Name == "Name")
                {
                    continue;
                }

                var editor = ObjectEditorFactory.CreatePropertyEditor(property);

                if (editor != null)
                {
                    editors.Add(editor);
                }
            }
        }

        public bool Draw(IGraphicsContext context, IPostFx effect, PostFxProxy proxy, Volume volume)
        {
            return builder.Draw(context, effect, proxy, volume.Mode == VolumeMode.Global);
        }

        public void Dispose()
        {
            builder.Reset();
            GC.SuppressFinalize(this);
        }
    }

    public class VolumeObjectEditor : IObjectEditor
    {
        private ImGuiName guiName = new("Volume");

        public string Name => "Volume";

        public Type Type => typeof(Volume);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        public string Symbol { get; } = "?";

        public unsafe bool Draw(IGraphicsContext context)
        {
            PostProcessingManager? postManager = PostProcessingManager.Current;
            if (postManager == null || Instance is not Volume volume)
            {
                return false;
            }

            bool changed = false;

            if (!NoTable)
            {
                ImGui.BeginTable("Volume", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PreciseWidths);
                ImGui.TableSetupColumn("", 0.0f);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Volume Mode");
            ImGui.TableSetColumnIndex(1);

            var mode = volume.Mode;
            if (ComboEnumHelper<VolumeMode>.Combo("##Mode", ref mode))
            {
                volume.Mode = mode;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Volume Shape");
            ImGui.TableSetColumnIndex(1);

            var shape = volume.Shape;
            if (ComboEnumHelper<VolumeShape>.Combo("##Shape", ref shape))
            {
                volume.Shape = shape;
            }

            switch (shape)
            {
                case VolumeShape.Box:
                    Vector3 min = volume.BoundingBox.Min;
                    Vector3 max = volume.BoundingBox.Max;
                    if (ImGui.InputFloat3("Min", ref min))
                    {
                        volume.BoundingBox = new(min, max);
                    }
                    if (ImGui.InputFloat3("Max", ref max))
                    {
                        volume.BoundingBox = new(min, max);
                    }
                    break;

                case VolumeShape.Sphere:
                    Vector3 center = volume.BoundingSphere.Center;
                    float radius = volume.BoundingSphere.Radius;
                    if (ImGui.InputFloat3("Center", ref center))
                    {
                        volume.BoundingSphere = new(center, radius);
                    }
                    if (ImGui.InputFloat("Radius", ref radius))
                    {
                        volume.BoundingSphere = new(center, radius);
                    }
                    break;
            }

            VolumeTransitionMode transitionMode = volume.TransitionMode;
            if (ComboEnumHelper<VolumeTransitionMode>.Combo("Transition Mode", ref transitionMode))
            {
                volume.TransitionMode = transitionMode;
            }

            int transitionDuration = volume.TransitionDuration;
            if (ImGui.InputInt("Transition Duration", ref transitionDuration))
            {
                volume.TransitionDuration = transitionDuration;
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.EndTable();

            ImGui.Separator();

            if (!NoTable)
            {
                ImGui.BeginTable(guiName.RawId, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PreciseWidths);
                ImGui.TableSetupColumn("", 0.0f);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
            }

            for (int i = 0; i < postManager.Effects.Count; i++)
            {
                var effect = postManager.Effects[i];
                var proxy = volume.Container[effect];

                if (proxy.TargetType == null)
                {
                    continue;
                }

                var editor = PostProcessingEditorFactory.CreateEditor(proxy.TargetType);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                uint id = ImGui.GetID(effect.DisplayName.Id);

                if (ImGui.TreeNodeEx(effect.DisplayName.Id))
                {
                    DrawEnable(id, effect, proxy, true);

                    ImGui.TableSetColumnIndex(0);
#pragma warning disable CS8602 // False positive.
                    changed |= editor.Draw(context, effect, proxy, volume);
#pragma warning restore CS8602 //

                    ImGui.TreePop();

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Separator();
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Separator();
                }
                else
                {
                    DrawEnable(id, effect, proxy, false);

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Separator();
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Separator();
                }
            }

            if (!NoTable)
            {
                ImGui.EndTable();
            }

            return changed;
        }

        private static void DrawEnable(uint id, IPostFx effect, PostFxProxy proxy, bool isOpen)
        {
            ImGui.SameLine();

            if ((effect.Flags & (PostFxFlags.AlwaysEnabled | PostFxFlags.Optional)) == 0)
            {
                bool enabled = proxy.Enabled;
                if (ImGui.Checkbox($"{effect.DisplayName.Id}Enable", ref enabled))
                {
                    effect.Enabled = enabled;
                    proxy.Enabled = enabled;
                }
                ImGui.SameLine();
                ImGui.Text(effect.DisplayName.Name);
            }
            else
            {
                ImGui.Text(effect.DisplayName.Name);
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                ImGui.TreeNodeSetOpen(id, !isOpen);
            }
        }

        public void Dispose()
        {
        }
    }
}