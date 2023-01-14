namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Scenes;

    public interface IComponent
    {
        public void Awake(IGraphicsDevice device, GameObject gameObject);

        public void Destory();

        public void FixedUpdate()
        {
        }

        public void Update()
        {
        }
    }

    public interface IScriptComponent : IComponent
    {
        public void Awake()
        {
        }

        public void Destroy()
        {
        }
    }
}