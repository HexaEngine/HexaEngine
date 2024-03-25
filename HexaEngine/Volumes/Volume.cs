namespace HexaEngine.Volumes
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Scenes;

    /// <summary>
    /// Represents a Volume in 3D space and controls post-processing and weather effects.
    /// </summary>
    [EditorGameObject<Volume>("Volume")]
    public class Volume : GameObject
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
        /// Gets or sets the bounding box of the Volume.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the bounding sphere of the Volume.
        /// </summary>
        public BoundingSphere BoundingSphere { get; set; }

        public PostFxSettingsContainer Container { get; } = new();

        public override void Initialize()
        {
            base.Initialize();
            PostProcessingManager postManager = PostProcessingManager.Current ?? throw new();
            Container.Build(postManager.Effects);
            if (Mode == VolumeMode.Global)
            {
                Container.Apply(postManager.Effects);
            }
        }
    }
}