namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using HexaEngine.Scripting;

    [EditorComponent(typeof(ScriptComponent), "Script")]
    public class ScriptComponent : IComponent
    {
        [EditorProperty("Script", typeof(IScript), EditorPropertyMode.TypeSelector)]
        public Type? Type { get; set; }

        public void Initialize(IGraphicsDevice device, SceneNode node)
        {
        }

        public void Uninitialize()
        {
        }
    }
}