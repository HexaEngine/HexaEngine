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
    public sealed class EmitterComponent : AudioComponent
    {
        private IEmitter emitter;
        private float coneInnerAngle = float.Pi * 2;
        private float coneOuterAngle = float.Pi * 2;
        private float coneOuterGain = 0;
        private float maxDistance = float.MaxValue;
        private float maxGain = 1.0f;
        private float minGain = 0.0f;
        private float referenceDistance = 1;
        private float rolloffFactor = 1;
        private AttenuationModel attenuationModel = AttenuationModel.Inverse;
        private float directionalAttenuationFactor = 1.0f;

        [JsonIgnore]
        public IEmitter Emitter => emitter;

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
                if (emitter != null)
                {
                    emitter.ConeInnerAngle = coneInnerAngle;
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
                if (emitter != null)
                {
                    emitter.ConeOuterAngle = coneOuterAngle;
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
                if (emitter != null)
                {
                    emitter.ConeOuterGain = coneOuterGain;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance at which the audio is audible.
        /// </summary>
        [EditorProperty("Max Distance", min: 0.0f, max: float.MaxValue, mode: EditorPropertyMode.Default)]
        [EditorTooltip("The maximum distance at which the audio is audible. Beyond this distance, the sound will not be heard.")]
        public float MaxDistance
        {
            get => maxDistance;
            set
            {
                SetAndNotifyWithEqualsTest(ref maxDistance, value);
                if (emitter != null)
                {
                    emitter.MaxDistance = maxDistance;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum gain for the audio emitter.
        /// </summary>
        [EditorProperty("Max Gain", min: 0.0f, max: 1.0f)]
        [EditorTooltip("The maximum gain (volume) for the audio emitter. Value ranges from 0 (silent) to 1 (full volume).")]
        public float MaxGain
        {
            get => maxGain;
            set
            {
                SetAndNotifyWithEqualsTest(ref maxGain, value);
                if (emitter != null)
                {
                    emitter.MaxGain = maxGain;
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum gain for the audio emitter.
        /// </summary>
        [EditorProperty("Min Gain", min: 0.0f, max: 1.0f)]
        [EditorTooltip("The minimum gain (volume) for the audio emitter. Value ranges from 0 (silent) to 1 (full volume).")]
        public float MinGain
        {
            get => minGain;
            set
            {
                SetAndNotifyWithEqualsTest(ref minGain, value);
                if (emitter != null)
                {
                    emitter.MinGain = minGain;
                }
            }
        }

        /// <summary>
        /// Gets or sets the reference distance for the audio emitter.
        /// </summary>
        [EditorProperty("Reference Distance", min: 0.0f, max: float.MaxValue, mode: EditorPropertyMode.Default)]
        [EditorTooltip("The distance at which the audio volume starts to attenuate. Sound remains at full volume up to this distance.")]
        public float ReferenceDistance
        {
            get => referenceDistance;
            set
            {
                SetAndNotifyWithEqualsTest(ref referenceDistance, value);
                if (emitter != null)
                {
                    emitter.ReferenceDistance = referenceDistance;
                }
            }
        }

        /// <summary>
        /// Gets or sets the rolloff factor for attenuation of the audio emitter.
        /// </summary>
        [EditorProperty("Rolloff Factor", min: 0.0f, max: float.MaxValue, mode: EditorPropertyMode.Default)]
        [EditorTooltip("Controls how quickly the sound attenuates with distance. Higher values mean faster attenuation.")]
        public float RolloffFactor
        {
            get => rolloffFactor;
            set
            {
                SetAndNotifyWithEqualsTest(ref rolloffFactor, value);
                if (emitter != null)
                {
                    emitter.RolloffFactor = rolloffFactor;
                }
            }
        }

        /// <summary>
        /// Gets or sets the attenuation model used to determine how sound decreases in volume over distance.
        /// </summary>
        /// <remarks>Selecting an appropriate attenuation model can affect both the perceived audio
        /// quality and the performance of audio simulations. Different models may be suitable for different
        /// environments or use cases.</remarks>
        [EditorProperty<AttenuationModel>("Attenuation Model")]
        [EditorTooltip("The attenuation model that determines how sound volume decreases over distance. Different models provide different falloff curves.")]
        public AttenuationModel AttenuationModel
        {
            get => attenuationModel;
            set
            {
                SetAndNotifyWithEqualsTest(ref attenuationModel, value, (a, b) => a == b);
                if (emitter != null)
                {
                    emitter.AttenuationModel = attenuationModel;
                }
            }
        }

        /// <summary>
        /// Gets or sets the directional attenuation factor for the audio emitter.
        /// </summary>
        [EditorProperty("Directional Attenuation Factor", min: 0.0f, max: float.MaxValue, mode: EditorPropertyMode.Default)]
        [EditorTooltip("Controls the strength of directional attenuation applied to the audio emitter.")]
        public float DirectionalAttenuationFactor
        {
            get => directionalAttenuationFactor;
            set
            {
                SetAndNotifyWithEqualsTest(ref directionalAttenuationFactor, value);
                if (emitter != null)
                {
                    emitter.DirectionalAttenuationFactor = directionalAttenuationFactor;
                }
            }
        }

        public override void Awake()
        {
            emitter = AudioManager.CreateEmitter();
            emitter.ConeInnerAngle = coneInnerAngle;
            emitter.ConeOuterAngle = coneOuterAngle;
            emitter.ConeOuterGain = coneOuterGain;
            emitter.MaxDistance = maxDistance;
            emitter.MaxGain = maxGain;
            emitter.MinGain = minGain;
            emitter.ReferenceDistance = referenceDistance;
            emitter.RolloffFactor = rolloffFactor;
            emitter.AttenuationModel = attenuationModel;
            emitter.DirectionalAttenuationFactor = directionalAttenuationFactor;
        }

        public override void Update()
        {
            emitter.Position = GameObject.Transform.Position;
            emitter.Direction = GameObject.Transform.Forward;
        }

        public override void Destroy()
        {
            emitter = null;
        }
    }
}