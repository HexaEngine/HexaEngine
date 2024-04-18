namespace HexaEngine.Animations
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System;

    [EditorComponent<SpriteAnimator>("Sprite Animator")]
    public class SpriteAnimator : IComponent
    {
        /// <summary>
        /// The GUID of the <see cref="SpriteAnimator"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        public GameObject GameObject { get; set; }

        public void Awake()
        {
            throw new NotImplementedException();
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}