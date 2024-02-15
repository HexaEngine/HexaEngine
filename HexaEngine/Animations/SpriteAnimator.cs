namespace HexaEngine.Animations
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System;

    [EditorComponent<SpriteAnimator>("Sprite Animator")]
    public class SpriteAnimator : IComponent
    {
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