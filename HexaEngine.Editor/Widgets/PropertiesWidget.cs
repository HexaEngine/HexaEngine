namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;

    /// <summary>
    /// A Property editor for editing.
    /// </summary>
    public class PropertiesWidget : EditorWindow
    {
        public PropertiesWidget()
        {
            IsShown = true;
        }

        protected override string Name => "Properties";

        public override void DrawContent(IGraphicsContext context)
        {
            lock (SelectionCollection.Global.SyncRoot)
            {
                if (SelectionCollection.Global.Count == 0)
                {
                    return;
                }

                var type = SelectionCollection.Global.Type;

                if (type == null)
                {
                    return;
                }

                if (!PropertyObjectEditorRegistry.TryGetEditor(type, out var editor))
                {
                    return;
                }

                ImGui.BeginDisabled(!Application.InDesignMode);

                if (SelectionCollection.Global.SelectedMultiple && editor.CanEditMultiple)
                {
                    editor.EditMultiple(context, SelectionCollection.Global);
                }
                else
                {
                    editor.Edit(context, SelectionCollection.Global[0]);
                }

                ImGui.EndDisabled();
            }
        }
    }
}