namespace HexaEngine.Scenes.Components.Audio
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.OpenAL;

    [EditorComponent<ListenerComponent>("Listener")]
    public class ListenerComponent : IAudioComponent
    {
        private bool isActive;
#pragma warning disable CS8618 // Non-nullable field 'listener' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private Listener listener;
#pragma warning restore CS8618 // Non-nullable field 'listener' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
#pragma warning disable CS8618 // Non-nullable field 'gameObject' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private GameObject gameObject;
#pragma warning restore CS8618 // Non-nullable field 'gameObject' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

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