namespace HexaEngine.OpenAL
{
    using HexaEngine.Core.Audio;
    using System.Numerics;

    public class OpenALEmitter : IEmitter
    {
        private float referenceDistance = 1;
        private float maxDistance = float.PositiveInfinity;
        private float rolloffFactor = 1;
        private float minGain = 0;
        private float maxGain = 1;
        private float coneInnerAngle = 360;
        private float coneOuterAngle = 360;
        private float coneOuterGain = 0;
        private Vector3 position;
        private Vector3 velocity;
        private Vector3 direction;

        /// <summary>
        /// Source specific reference distance. Type: float Range: [0.0f - float.PositiveInfinity]
        /// At 0.0f, no distance attenuation occurs. Type: float Default: 1.0f.
        /// </summary>
        public float ReferenceDistance
        {
            get => referenceDistance;
            set
            {
                referenceDistance = value;
            }
        }

        /// <summary>
        /// Indicate distance above which Sources are not attenuated using the inverse clamped
        /// distance model. Default: float.PositiveInfinity Type: float Range: [0.0f - float.PositiveInfinity].
        /// </summary>
        public float MaxDistance
        {
            get => maxDistance;
            set
            {
                maxDistance = value;
            }
        }

        /// <summary>
        /// Source specific rolloff factor. Type: float Range: [0.0f - float.PositiveInfinity].
        /// </summary>
        public float RolloffFactor
        {
            get => rolloffFactor;
            set
            {
                rolloffFactor = value;
            }
        }

        /// <summary>
        /// Indicate minimum Source attenuation. Type: float Range: [0.0f - 1.0f] (Logarthmic).
        /// </summary>
        public float MinGain
        {
            get => minGain;
            set
            {
                minGain = value;
            }
        }

        /// <summary>
        /// Indicate maximum Source attenuation. Type: float Range: [0.0f - 1.0f] (Logarthmic).
        /// </summary>
        public float MaxGain
        {
            get => maxGain;
            set
            {
                maxGain = value;
            }
        }

        /// <summary>
        /// Directional Source, inner cone angle, in degrees. Range: [0-360] Default: 360.
        /// </summary>
        public float ConeInnerAngle
        {
            get => coneInnerAngle;
            set
            {
                coneInnerAngle = value;
            }
        }

        /// <summary>
        /// Directional Source, outer cone angle, in degrees. Range: [0-360] Default: 360.
        /// </summary>
        public float ConeOuterAngle
        {
            get => coneOuterAngle;
            set
            {
                coneOuterAngle = value;
            }
        }

        /// <summary>
        /// Directional Source, outer cone gain. Default: 0.0f Range: [0.0f - 1.0] (Logarithmic).
        /// </summary>
        public float ConeOuterGain
        {
            get => coneOuterGain;
            set
            {
                coneOuterGain = value;
            }
        }

        /// <summary>
        /// Specify the current location in three dimensional space. OpenAL, like OpenGL,
        /// uses a right handed coordinate system, where in a frontal default view X (thumb)
        /// points right, Y points up (index finger), and Z points towards the viewer/camera
        /// (middle finger). To switch from a left handed coordinate system, flip the sign
        /// on the Z coordinate. Listener position is always in the world coordinate system.
        /// </summary>
        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// Specify the current velocity in three dimensional space.
        /// </summary>
        public Vector3 Velocity
        {
            get => velocity;
            set
            {
                velocity = value;
            }
        }

        /// <summary>
        /// Specify the current direction vector.
        /// </summary>
        public Vector3 Direction
        {
            get => direction;
            set
            {
                direction = value;
            }
        }
    }
}