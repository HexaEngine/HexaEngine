namespace HexaEngine.Audio
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Serialization;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Audio.Listener")]
    [EditorCategory("Audio")]
    [EditorComponent<ListenerComponent>("Listener")]
    public sealed class ListenerComponent : AudioComponent
    {
        private IListener? listener;
        private bool isActive;
        private float coneInnerAngle = float.Pi * 2;
        private float coneOuterAngle = float.Pi * 2;
        private float coneOuterGain = 0;

        [EditorProperty("Is Active")]
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (listener != null) listener.IsActive = value;
                SetAndNotifyWithEqualsTest(ref isActive, value);
            }
        }

        /// <summary>
        /// Gets or sets the inner angle of the cone in radians for sound directionality.
        /// </summary>
        [EditorProperty("Cone Inner Angle", min: 0, max: MathF.PI * 2, DefaultValue = MathF.PI * 2)]
        [EditorTooltip("The inner angle of the cone in radians for sound directionality. Within this angle, the sound is at full volume.")]
        public float ConeInnerAngle
        {
            get => coneInnerAngle;
            set
            {
                SetAndNotifyWithEqualsTest(ref coneInnerAngle, value);
                if (listener != null)
                {
                    listener.ConeInnerAngle = coneInnerAngle;
                }
            }
        }

        /// <summary>
        /// Gets or sets the outer angle of the cone in radians for sound directionality.
        /// </summary>
        [EditorProperty("Cone Outer Angle", min: 0, max: MathF.PI * 2)]
        [EditorTooltip("The outer angle of the cone in radians for sound directionality. Outside this angle, the sound volume is reduced based on ConeOuterGain.")]
        public float ConeOuterAngle
        {
            get => coneOuterAngle;
            set
            {
                SetAndNotifyWithEqualsTest(ref coneOuterAngle, value);
                if (listener != null)
                {
                    listener.ConeOuterAngle = coneOuterAngle;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gain outside of the outer cone for sound directionality.
        /// </summary>
        [EditorProperty("Cone Outer Gain", min: 0.0f, max: 1.0f)]
        [EditorTooltip("The gain multiplier applied to sound outside the outer cone angle. 0 means silent, 1 means full volume.")]
        public float ConeOuterGain
        {
            get => coneOuterGain;
            set
            {
                SetAndNotifyWithEqualsTest(ref coneOuterGain, value);
                if (listener != null)
                {
                    listener.ConeOuterGain = coneOuterGain;
                }
            }
        }

        public override void Awake()
        {
            listener = AudioManager.CreateListener();
            listener.IsActive = isActive;
        }

        public override void Update()
        {
            if (listener == null) return;
            listener.Position = GameObject.Transform.Position;
            listener.Orientation = new(GameObject.Transform.Forward, GameObject.Transform.Up);
        }

        public override void Destroy()
        {
            listener?.Dispose();
        }
    }
}