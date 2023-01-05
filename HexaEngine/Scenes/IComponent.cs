namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Scenes;

    public interface IComponent
    {
        public IPropertyEditor? Editor { get; }

        public void Awake(IGraphicsDevice device, GameObject gameObject);

        public void Destory();

        public void FixedUpdate()
        {
        }

        public void Update()
        {
        }
    }
}