#nullable disable

namespace HexaEngine.Audio
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Serialization;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Audio.EmitterComponent")]
    [EditorCategory("Audio")]
    [EditorComponent<EmitterComponent>("Emitter")]
    public class EmitterComponent : IAudioComponent
    {
        private IEmitter emitter;

        /// <summary>
        /// The GUID of the <see cref="EmitterComponent"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public bool IsSerializable { get; } = true;

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