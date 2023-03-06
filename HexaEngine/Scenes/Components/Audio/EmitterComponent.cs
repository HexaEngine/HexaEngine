#nullable disable

namespace HexaEngine.Scenes.Components.Audio
{
    using HexaEngine.Core;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using HexaEngine.OpenAL;

    [EditorComponent<EmitterComponent>("Emitter")]
    public class EmitterComponent : IAudioComponent
    {
        private GameObject gameObject;
        private Emitter emitter;

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            emitter = AudioManager.CreateEmitter();
            this.gameObject = gameObject;
            if (!Application.InDesignMode)
            {
                WaveAudioStream stream = AudioManager.CreateStream("piano2.wav");
                SourceVoice voice = AudioManager.CreateSourceVoice(stream);
                voice.Looping = true;
                voice.Emitter = emitter;
                voice.Play();
            }
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