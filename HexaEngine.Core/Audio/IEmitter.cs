namespace HexaEngine.Core.Audio
{
    using System.Numerics;

    public interface IEmitter
    {
        float ConeInnerAngle { get; set; }

        float ConeOuterAngle { get; set; }

        float ConeOuterGain { get; set; }

        Vector3 Direction { get; set; }

        float MaxDistance { get; set; }

        float MaxGain { get; set; }

        float MinGain { get; set; }

        Vector3 Position { get; set; }

        float ReferenceDistance { get; set; }

        float RolloffFactor { get; set; }

        Vector3 Velocity { get; set; }
    }
}