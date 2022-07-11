namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;

    public interface IComponent
    {
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