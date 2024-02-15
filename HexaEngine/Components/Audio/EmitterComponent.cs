#nullable disable

namespace HexaEngine.Components.Audio
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;

    [EditorCategory("Audio")]
    [EditorComponent<EmitterComponent>("Emitter")]
    public class EmitterComponent : IAudioComponent
    {
        private IEmitter emitter;

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public void Awake()
        {
            emitter = AudioManager.CreateEmitter();
        }

        public void Update()
        {
            emitter.Position = GameObject.Transform.Position;
            emitter.Direction = GameObject.Transform.Forward;
        }

        public void Destroy()
        {
            emitter = null;
        }
    }
}