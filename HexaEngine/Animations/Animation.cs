namespace HexaEngine.Animations
{
    using HexaEngine.Core.IO.Binary.Animations;
    using HexaEngine.Core.IO.Binary.Meshes;

    public class Animation
    {
        private readonly AnimationClip clip;
        private readonly List<NodeChannelState> nodeChannels = [];
        private float currentTime;

        public Animation(AnimationClip clip, PlainNode[] nodes, PlainNode[] bones)
        {
            static int GetNodeIdByName(PlainNode[] nodes, string name)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    if (node.Name == name)
                        return node.Id;
                }
                return -1;
            }
            static int GetBoneIdByName(PlainNode[] nodes, string name)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    var bone = nodes[i];
                    if (bone.Name == name)
                        return bone.Id;
                }
                return -1;
            }

            this.clip = clip;
            for (int i = 0; i < clip.NodeChannels.Count; i++)
            {
                NodeChannel channel = clip.NodeChannels[i];
                NodeId node;
                int nodeId = GetNodeIdByName(nodes, channel.NodeName);
                int boneId = GetBoneIdByName(bones, channel.NodeName);
                if (boneId != -1)
                {
                    node = new((uint)boneId, channel.NodeName, true);
                }
                else if (nodeId != -1)
                {
                    node = new((uint)nodeId, channel.NodeName, false);
                }
                else
                {
                    throw new Exception($"Could not find bone/node in model hierarchy with the name {channel.NodeName}");
                }
                NodeChannelState state = new(channel.NodeName, node, channel);
                nodeChannels.Add(state);
            }
        }

        public AnimationClip Clip => clip;

        public IReadOnlyList<NodeChannelState> NodeChannels => nodeChannels;

        public float CurrentTime => currentTime;

        public bool Repeat { get; set; }

        public bool Tick(float deltaTime)
        {
            currentTime += deltaTime * (float)clip.TicksPerSecond;
            float ct;
            if (Repeat)
            {
                ct = currentTime % (float)clip.Duration;
            }
            else
            {
                ct = currentTime;
                if (ct >= clip.Duration)
                {
                    return false;
                }
            }

            for (int i = 0; i < nodeChannels.Count; i++)
            {
                nodeChannels[i].Update(ct);
            }

            return true;
        }

        public void Reset()
        {
            currentTime = 0;
            for (int i = 0; i < nodeChannels.Count; i++)
            {
                nodeChannels[i].Reset();
            }
        }
    }
}