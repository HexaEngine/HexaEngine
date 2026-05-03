namespace HexaEngine.MiniAudio
{
    using HexaEngine.Core.Audio;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public sealed class MiniAudioEmitter : IEmitter
    {
        internal ulong Version;
        private float coneInnerAngle = 6.283185f;
        private float coneOuterAngle = 6.283185f;
        private float coneOuterGain;
        private Vector3 direction;
        private float maxDistance = float.MaxValue;
        private float maxGain = 1.0f;
        private float minGain;
        private Vector3 position;
        private float referenceDistance = 1.0f;
        private float rolloffFactor = 1.0f;
        private Vector3 velocity;
        private float dopplerEffectFactor = 1;
        private AttenuationModel attenuationModel = AttenuationModel.Inverse;
        private float directionalAttenuationFactor = 1;

        public float ConeInnerAngle { get => coneInnerAngle; set => SetAndVersion(ref coneInnerAngle, value); }

        public float ConeOuterAngle { get => coneOuterAngle; set => SetAndVersion(ref coneOuterAngle, value); }

        public float ConeOuterGain { get => coneOuterGain; set => SetAndVersion(ref coneOuterGain, value); }

        public Vector3 Direction { get => direction; set => SetAndVersion(ref direction, value); }

        public float MaxDistance { get => maxDistance; set => SetAndVersion(ref maxDistance, value); }

        public float MaxGain { get => maxGain; set => SetAndVersion(ref maxGain, value); }

        public float MinGain { get => minGain; set => SetAndVersion(ref minGain, value); }

        public Vector3 Position { get => position; set => SetAndVersion(ref position, value); }

        public float ReferenceDistance { get => referenceDistance; set => SetAndVersion(ref referenceDistance, value); }

        public float RolloffFactor { get => rolloffFactor; set => SetAndVersion(ref rolloffFactor, value); }

        public float DopplerEffectFactor { get => dopplerEffectFactor; set => SetAndVersion(ref dopplerEffectFactor, value); }

        public Vector3 Velocity { get => velocity; set => SetAndVersion(ref velocity, value); }

        public float DirectionalAttenuationFactor { get => directionalAttenuationFactor; set => SetAndVersion(ref directionalAttenuationFactor, value); }

        public AttenuationModel AttenuationModel { get => attenuationModel; set => SetAndVersionEnum(ref attenuationModel, value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetAndVersion<T>(ref T field, in T value) where T : IEquatable<T>
        {
            if (field.Equals(value)) return;
            field = value;
            Interlocked.Increment(ref Version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetAndVersionEnum<T>(ref T field, in T value) where T : struct, Enum
        {
            if (field.Equals(value)) return;
            field = value;
            Interlocked.Increment(ref Version);
        }

        public void Dispose()
        {
        }
    }
}