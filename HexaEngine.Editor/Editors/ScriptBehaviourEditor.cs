namespace HexaEngine.Editor.Editors
{
    using Hexa.NET.ImGui;
    using HexaEngine.Components;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Projects;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scripts;

    public class ScriptBehaviourEditor : IObjectEditor
    {
        private ImGuiName guiName = new("Script Behaviour");
        private IObjectEditor? editor;
        private bool changing;

        public ScriptBehaviourEditor()
        {
            ScriptAssemblyManager.AssembliesUnloaded += AssembliesUnloaded;
        }

        public string Name => guiName.Name;

        public Type Type => typeof(ScriptBehaviour);

        public object? Instance { get; set; }

        public bool IsEmpty => false;

        public bool IsHidden => false;

        public bool NoTable { get; set; }

        public string Symbol { get; } = "\xf000";

        public void Dispose()
        {
            ScriptAssemblyManager.AssembliesUnloaded -= AssembliesUnloaded;
        }

        private void AssembliesUnloaded(object? sender, EventArgs? e)
        {
            if (editor != null)
            {
                ObjectEditorFactory.DestroyEditor(editor.Type);
                editor = null;
            }
        }

        public bool Draw(IGraphicsContext context)
        {
            if (Instance is not ScriptBehaviour component)
            {
                return false;
            }

            if (changing)
            {
                return false;
            }

            AssetRef val = component.ScriptRef;

            bool changed = ComboHelper.ComboForAssetRef(guiName.Id, ref val, AssetType.Script);
            if (changed)
            {
                if (editor != null)
                {
                    ObjectEditorFactory.DestroyEditor(editor.Type);
                    editor = null;
                }

                Volatile.Write(ref changing, true);

                Task.Run(async () =>
                {
                    if (ProjectManager.ScriptProjectChanged)
                    {
                        await ProjectManager.BuildScriptsAsync();
                    }

                    component.ScriptRef = val;
                    Volatile.Write(ref changing, false);
                });

                return true;
            }

            Type? type = component.ScriptType;

            if ((editor == null || type != editor.Type) && type != null)
            {
                editor = ObjectEditorFactory.CreateEditor(type);
            }

            if (editor != null && type != editor.Type && type != null)
            {
                ObjectEditorFactory.DestroyEditor(type);
                editor = ObjectEditorFactory.CreateEditor(type);
            }

            if (editor != null)
            {
                editor.Instance = component.Instance;
                ImGui.Separator();
                changed |= editor.Draw(context);
            }

            return changed;
        }
    }
}