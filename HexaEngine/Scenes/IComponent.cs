namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Scenes;

    public interface IComponent
    {
        public IPropertyEditor? Editor { get; }

        public void Initialize(IGraphicsDevice device, SceneNode node);

        public void Uninitialize();

        public void FixedUpdate()
        {
        }

        public void Update()
        {
        }
    }
}