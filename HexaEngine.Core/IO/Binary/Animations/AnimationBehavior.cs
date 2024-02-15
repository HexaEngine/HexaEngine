namespace HexaEngine.Core.IO.Binary.Animations
{
    /// <summary>
    /// Defines the behavior of an animation.
    /// </summary>
    public enum AnimationBehavior
    {
        /// <summary>
        /// The default behavior of the animation.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The animation has a constant behavior.
        /// </summary>
        Constant,

        /// <summary>
        /// The animation has a linear behavior.
        /// </summary>
        Linear,

        /// <summary>
        /// The animation repeats its behavior.
        /// </summary>
        Repeat
    }
}