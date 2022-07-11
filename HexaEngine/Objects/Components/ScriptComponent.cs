namespace HexaEngine.Objects.Components
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;

    [EditorComponent(typeof(ScriptComponent), "Script")]
    public class ScriptComponent : IComponent
    {
        public void Initialize(IGraphicsDevice device, SceneNode node)
        {
            throw new System.NotImplementedException();
        }

        public void Uninitialize()
        {
            throw new System.NotImplementedException();
        }
    }
}