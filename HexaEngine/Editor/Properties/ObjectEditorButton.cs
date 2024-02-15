namespace HexaEngine.Editor.Properties
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor.Attributes;
    using System.Reflection;

    /// <summary>
    /// Represents a button in an object editor, associated with a specific method.
    /// </summary>
    public class ObjectEditorButton
    {
        private ImGuiName guiName;
        private readonly MethodInfo info;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectEditorButton"/> class.
        /// </summary>
        /// <param name="attribute">The attribute containing information about the button.</param>
        /// <param name="info">The method information associated with the button.</param>
        public ObjectEditorButton(EditorButtonAttribute attribute, MethodInfo info)
        {
            guiName = new(attribute.Name);
            this.info = info;
        }

        /// <summary>
        /// The Name of the editor button.
        /// </summary>
        public string Name => guiName;

        /// <summary>
        /// The target method of the editor button.
        /// </summary>
        public MethodInfo Method => info;

        /// <summary>
        /// Draws the button in the object editor.
        /// </summary>
        /// <param name="instance">The instance of the object associated with the button.</param>
        public void Draw(object instance)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);
            if (ImGui.Button(guiName.UniqueName))
            {
                info.Invoke(instance, null);
            }
        }
    }
}