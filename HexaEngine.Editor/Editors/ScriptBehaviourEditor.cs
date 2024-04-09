namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Properties;

    public class ScriptBehaviourEditor : IObjectEditor
    {
        private ImGuiName guiName = new("Script Behaviour");
        private IObjectEditor? editor;

        public string Name => guiName.Name;

        public Type Type => typeof(ScriptBehaviour);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        public string Symbol { get; } = "\xf000";

        public void Dispose()
        {
        }

        public bool Draw(IGraphicsContext context)
        {
            if (Instance is not ScriptBehaviour component)
            {
                return false;
            }

            AssetRef val = component.ScriptRef;

            bool changed = ComboHelper.ComboForAssetRef(guiName.Id, ref val, AssetType.Script);
            if (changed)
            {
                component.ScriptRef = val;
            }

            Type? type = component.ScriptType;

            if (type != null)
            {
                editor = ObjectEditorFactory.CreateEditor(type);
                editor.Instance = component.Instance;
                ImGui.Separator();
                changed |= editor.Draw(context);
            }

            return changed;
        }
    }
}