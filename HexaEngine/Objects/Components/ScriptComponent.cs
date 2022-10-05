namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using HexaEngine.Scripting;

    [EditorComponent<ScriptComponent>("Script")]
    public class ScriptComponent : IComponent
    {
        public ScriptComponent()
        {
            Editor = new PropertyEditor<ScriptComponent>(this);
        }

        [EditorProperty("Script", typeof(IScript), EditorPropertyMode.TypeSelector)]
        public Type? Type { get; set; }

        public IPropertyEditor? Editor { get; }

        public void Initialize(IGraphicsDevice device, SceneNode node)
        {
        }

        public void Uninitialize()
        {
        }
    }
}