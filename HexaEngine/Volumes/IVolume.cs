using HexaEngine.Mathematics;
using HexaEngine.PostFx;

namespace HexaEngine.Volumes
{
    public interface IVolume
    {
        BoundingBox BoundingBox { get; set; }

        BoundingSphere BoundingSphere { get; set; }

        PostFxSettingsContainer Container { get; }

        VolumeMode Mode { get; set; }

        VolumeShape Shape { get; set; }

        void Apply(PostProcessingManager manager);

        void Apply(PostProcessingManager manager, IVolume baseVolume, float blend, VolumeTransitionMode mode);

        void Init(PostProcessingManager manager);
    }
}