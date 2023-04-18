#nullable disable

namespace HexaEngine.Scenes.Components.Audio
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;

    [EditorComponent<EmitterComponent>("Emitter")]
    public class EmitterComponent : IAudioComponent
    {
        private GameObject gameObject;
        private IEmitter emitter;

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            emitter = AudioManager.CreateEmitter();
            this.gameObject = gameObject;
        }

        public void Update()
        {
            emitter.Position = gameObject.Transform.Position;
            emitter.Direction = gameObject.Transform.Forward;
        }

        public void Destory()
        {
            emitter = null;
        }
    }
}