namespace HexaEngine.Objects.Components
{
    using HexaEngine.Audio;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.IO;
    using HexaEngine.OpenAL;
    using HexaEngine.Scenes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [EditorComponent<EmitterComponent>("Emitter")]
    public class EmitterComponent : IComponent
    {
        private GameObject gameObject;
        private Emitter emitter;

        public EmitterComponent()
        {
            Editor = new PropertyEditor<EmitterComponent>(this);
        }

        public IPropertyEditor? Editor { get; }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            emitter = AudioManager.CreateEmitter();
            this.gameObject = gameObject;
            if (!Designer.InDesignMode)
            {
                AudioStream stream = AudioManager.CreateStream("piano2.wav");
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