namespace HexaEngine.Core.UI
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Utilities;
    using System;
    using System.Reflection;

    /// <summary>
    /// A helper class for working with ImGui combo boxes to select enum values of a specified enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public static class ComboEnumHelper<T> where T : struct, Enum
    {
        private static readonly T[] values = Enum.GetValues<T>();

        private static readonly string[] names;

        static ComboEnumHelper()
        {
            Type enumType = typeof(T);
            names = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                var name = value.ToString();
                var field = enumType.GetField(name);
                var attribute = field?.GetCustomAttribute<EnumNameAttribute>();

                if (attribute != null)
                {
                    name = attribute.Name;
                }

                names[i] = name;
            }
        }

        /// <summary>
        /// Displays a combo box to select an enum value.
        /// </summary>
        /// <param name="label">The label for the combo box.</param>
        /// <param name="value">The currently selected enum value (modified by user interaction).</param>
        /// <returns><c>true</c> if the user selects a new value, <c>false</c> otherwise.</returns>
        public static bool Combo(string label, ref T value)
        {
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, names, names.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Displays the text representation of an enum value.
        /// </summary>
        /// <param name="value">The enum value to display.</param>
        public static void Text(T value)
        {
            int index = Array.IndexOf(values, value);
            ImGui.Text(names[index]);
        }
    }

    /// <summary>
    /// A helper class for working with ImGui combo boxes to select enum values of various enum types.
    /// </summary>
    public static class ComboEnumHelper
    {
        private static readonly Dictionary<Type, object[]> values = new();
        private static readonly Dictionary<Type, string[]> names = new();

        private static void Get(Type type, out object[] values, out string[] names)
        {
            if (ComboEnumHelper.values.TryGetValue(type, out var objects))
            {
                values = objects;
                names = ComboEnumHelper.names[type];
                return;
            }

            values = type.GetEnumValuesAsUnderlyingType().Cast<object>().ToArray();
            names = Enum.GetNames(type);
            ComboEnumHelper.values.Add(type, values);
            ComboEnumHelper.names.Add(type, names);
        }

        /// <summary>
        /// Displays a combo box to select an enum value of a specified enum type.
        /// </summary>
        /// <param name="label">The label for the combo box.</param>
        /// <param name="type">The enum type to select values from.</param>
        /// <param name="value">The currently selected enum value (modified by user interaction).</param>
        /// <returns><c>true</c> if the user selects a new value, <c>false</c> otherwise.</returns>
        public static bool Combo(string label, Type type, ref object value)
        {
            Get(type, out var values, out var names);
            int index = Array.IndexOf(values, value);
            if (ImGui.Combo(label, ref index, names, names.Length))
            {
                value = values[index];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Displays the text representation of an enum value of a specified enum type.
        /// </summary>
        /// <param name="type">The enum type to select values from.</param>
        /// <param name="value">The enum value to display.</param>
        public static void Text(Type type, object value)
        {
            Get(type, out var values, out var names);
            int index = Array.IndexOf(values, value);
            ImGui.Text(names[index]);
        }
    }

    /// <summary>
    /// A helper class for working with ImGui combo boxes.
    /// </summary>
    public static class ComboHelper
    {
        /// <summary>
        /// Displays a combo box to select an asset value of a specified asset type.
        /// </summary>
        /// <param name="label">The label for the combo box.</param>
        /// <param name="assetRef">The currently selected asset value (modified by user interaction).</param>
        /// <param name="type">The asset type to select values from.</param>
        /// <returns><c>true</c> if the user selects a new value, <c>false</c> otherwise.</returns>
        public static unsafe bool ComboForAssetRef(string label, ref AssetRef assetRef, AssetType type)
        {
            var meta = assetRef.GetMetadata();

            bool isOpen;
            if (meta != null)
            {
                isOpen = ImGui.BeginCombo(label, meta.Name);
            }
            else
            {
                if (assetRef.Guid == Guid.Empty)
                {
                    isOpen = ImGui.BeginCombo(label, (byte*)null);
                }
                else
                {
                    isOpen = ImGui.BeginCombo(label, $"{assetRef.Guid}");
                }
            }

            bool changed = false;
            if (isOpen)
            {
                changed |= DrawAssetRefSelectable(ref assetRef, Artifact.Empty);
                foreach (var asset in ArtifactDatabase.GetArtifactsFromType(type))
                {
                    changed |= DrawAssetRefSelectable(ref assetRef, asset);
                }

                ImGui.EndCombo();
            }
            else if (assetRef.Guid != Guid.Empty && ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
            {
                SourceAssetMetadata? sourceAssetMetadata = meta?.GetSourceMetadata();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted($"{meta?.Name} -> {sourceAssetMetadata?.FilePath}");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }

            return changed;
        }

        public static unsafe bool DrawAssetRefSelectable(ref AssetRef assetRef, Artifact asset)
        {
            bool changed = false;
            bool isSelected = assetRef.Guid == asset.Guid;
            if (ImGui.Selectable(asset.DisplayName, isSelected))
            {
                assetRef.Guid = asset.Guid;
                changed = true;
            }

            if (isSelected)
            {
                ImGui.SetItemDefaultFocus();
            }

            if (asset.Guid != Guid.Empty && ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
            {
                SourceAssetMetadata? sourceAssetMetadata = asset?.GetSourceMetadata();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted($"{asset?.Name} -> {sourceAssetMetadata?.FilePath}");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
            return changed;
        }
    }
}