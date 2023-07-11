namespace HexaEngine.Animations
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Animations;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System.Collections.Generic;

    [EditorComponent(typeof(Animator), "Animator")]
    public class Animator : IComponent, IAnimator
    {
        private readonly Dictionary<string, NodeId> _cache = new();
        private Animation? currentAnimation;
        private float currentTime;
        private bool playing;
        private bool invalid = false;
        private SkinnedMeshRendererComponent? renderer;

        private struct NodeId
        {
            public uint Id;
            public string Name;
            public bool IsBone;
        }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            invalid = false;
            renderer = gameObject.GetComponent<SkinnedMeshRendererComponent>();
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
            if (invalid)
            {
                return;
            }

            if (!playing)
            {
                return;
            }

            if (currentAnimation == null || renderer == null)
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
                    var nodeId = renderer.GetNodeIdByName(channel.NodeName);
                    var boneId = renderer.GetBoneIdByName(channel.NodeName);
                    if (boneId != -1)
                    {
                        node = new() { Id = (uint)boneId, Name = channel.NodeName, IsBone = true };
                    }
                    else if (nodeId != -1)
                    {
                        node = new() { Id = (uint)nodeId, Name = channel.NodeName, IsBone = false };
                    }
                    else
                    {
                        invalid = true;
                        break;
                    }
                    _cache.Add(channel.NodeName, node);
                }

                channel.Update(ct);

                if (node.IsBone)
                {
                    renderer.SetBoneLocal(channel.Local, node.Id);
                }
                else
                {
                    renderer.SetLocal(channel.Local, node.Id);
                }
            }
        }
    }
}