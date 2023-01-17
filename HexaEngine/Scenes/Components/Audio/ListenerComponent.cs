namespace HexaEngine.Scenes.Components.Audio
{
    using HexaEngine.Audio;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.OpenAL;

    [EditorComponent<ListenerComponent>("Listener")]
    public class ListenerComponent : IComponent
    {
        private bool isActive;
        private Listener listener;
        private GameObject gameObject;

        [EditorProperty("Is Active")]
        public bool IsActive
        { get => isActive; set { listener.IsActive = value; isActive = value; } }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            listener = AudioManager.CreateListener();
            listener.IsActive = isActive;
            this.gameObject = gameObject;
        }

        public void Update()
        {
            listener.Position = gameObject.Transform.Position;
            listener.Orientation = new(gameObject.Transform.Forward, gameObject.Transform.Up);
        }

        public void Destory()
        {
            listener.Dispose();
        }
    }
}