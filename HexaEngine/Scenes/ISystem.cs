using HexaEngine.Core.Scenes;

namespace HexaEngine.Scenes
{
    using HexaEngine.Collections;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics.Graph;

    public interface ISystem : IHasFlags<SystemFlags>
    {
        public string Name { get; }

        public void Register(GameObject gameObject)
        {
        }

        public void Unregister(GameObject gameObject)
        {
        }

        public void Awake(Scene scene)
        {
        }

        public void GraphicsAwake(IGraphicsDevice device, GraphResourceBuilder creator)
        {
        }

        public void GraphicsUpdate(IGraphicsContext context)
        {
        }

        public void Update(float delta)
        {
        }

        public void FixedUpdate()
        {
        }

        public void Destroy()
        {
        }

        public void Load(IGraphicsDevice device)
        {
        }

        public void Unload()
        {
        }
    }
}