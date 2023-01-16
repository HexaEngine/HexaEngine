namespace HexaEngine.Scenes.Components
{
    using HexaEngine.Audio;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Objects;
    using HexaEngine.OpenAL;
    using HexaEngine.Scenes;

    [EditorComponent<EmitterComponent>("Emitter")]
    public class EmitterComponent : IComponent
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