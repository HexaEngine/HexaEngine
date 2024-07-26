namespace HexaEngine.Volumes
{
    using HexaEngine.Editor.Attributes;
    using Hexa.NET.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Scenes;

    /// <summary>
    /// Represents a Volume in 3D space and controls post-processing and weather effects.
    /// </summary>
    [EditorGameObject<Volume>("Volume")]
    public class Volume : GameObject, IVolume
    {
        /// <summary>
        /// Gets or sets the Volume mode.
        /// </summary>
        public VolumeMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the Volume shape.
        /// </summary>
        public VolumeShape Shape { get; set; }

        /// <summary>
        /// Gets or sets the Volume transition mode.
        /// </summary>
        public VolumeTransitionMode TransitionMode { get; set; }

        /// <summary>
        /// The volume transition duration in milliseconds.
        /// </summary>
        public int TransitionDuration { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the bounding box of the Volume.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the bounding sphere of the Volume.
        /// </summary>
        public BoundingSphere BoundingSphere { get; set; }

        public PostFxSettingsContainer Container { get; } = new();

        void IVolume.Init(PostProcessingManager manager)
        {
            Container.Build(manager.Effects);
        }

        void IVolume.Apply(PostProcessingManager manager)
        {
            using (manager.SupressReload())
            {
                Container.Apply(manager.Effects);
            }
        }

        void IVolume.Apply(PostProcessingManager manager, IVolume baseVolume, float blend, VolumeTransitionMode mode)
        {
            using (manager.SupressReload())
            {
                Container.Apply(manager.Effects, baseVolume.Container, blend, mode);
            }
        }
    }
}