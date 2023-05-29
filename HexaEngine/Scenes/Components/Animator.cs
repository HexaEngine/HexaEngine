namespace HexaEngine.Scenes.Components
{
    using HexaEngine.Core.Animations;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Scenes;
    using System.Collections.Generic;
    using System.Numerics;

    [EditorComponent(typeof(Animator), "Animator")]
    public class Animator : IComponent, IAnimator
    {
        private readonly Dictionary<string, GameObject> _cache = new();
        private Animation? currentAnimation;
        private float currentTime;
        private bool playing;
        private Vector3 translation = default;
        private Quaternion rotation = Quaternion.Identity;
        private Vector3 scale = Vector3.One;

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
        }

        public void Destory()
        {
            Stop();
            _cache.Clear();
        }

        public void Play(Animation animation)
        {
            currentAnimation = animation;
            currentTime = 0;
            playing = true;
        }

        public void Pause()
        {
            playing = false;
        }

        public void Resume()
        {
            playing = true;
        }

        public void Stop()
        {
            currentAnimation = null;
            currentTime = 0;
            playing = false;
        }

        public void Update(Scene scene, float deltaTime)
        {
            if (!playing)
            {
                return;
            }

            if (currentAnimation == null)
            {
                return;
            }

            currentTime += deltaTime * (float)currentAnimation.TicksPerSecond;
            for (int i = 0; i < currentAnimation.NodeChannels.Count; i++)
            {
                var ct = currentTime % (float)currentAnimation.Duration;
                var channel = currentAnimation.NodeChannels[i];

                if (!_cache.TryGetValue(channel.NodeName, out var node))
                {
                    _cache.Add(channel.NodeName, node = scene.Find(channel.NodeName) ?? throw new Exception());
                }

                channel.Update(ct);

                Matrix4x4.Decompose(channel.Local, out scale, out rotation, out translation);
                node.Transform.Scale *= scale;
                node.Transform.Orientation *= rotation;
                node.Transform.Position += translation;
            }
        }
    }
}