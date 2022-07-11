namespace HexaEngine.Animations
{
    using System.Collections.Generic;
    using System.Numerics;

    public class Behavior
    {
        public const string Extension = ".beh";

        private readonly Bone _skeleton;
        private readonly Dictionary<string, Bone> _bonesByName;
        private readonly Dictionary<string, int> _bonesToIndex;
        private readonly Dictionary<string, int> _animationNameToId;
        private readonly List<Bone> _bones;

        public List<Animation> Animations { get; private set; }

        private int CurrentAnimationIndex { get; set; }

        public bool HasSkeleton
        { get { return _bones.Count > 0; } }

        public string AnimationName
        { get { return Animations[CurrentAnimationIndex].Name; } }

        public float AnimationSpeed
        { get { return Animations[CurrentAnimationIndex].FramesPerSecond; } }

        public float Duration
        {
            get { return Animations[CurrentAnimationIndex].Duration / Animations[CurrentAnimationIndex].FramesPerSecond; }
        }

        public Behavior()
        {
            _skeleton = null;
            CurrentAnimationIndex = -1;
            _bonesByName = new Dictionary<string, Bone>();
            _bonesToIndex = new Dictionary<string, int>();
            _animationNameToId = new Dictionary<string, int>();
            _bones = new List<Bone>();
            Animations = new List<Animation>();

            const float timestep = 1.0f / 30.0f;
            for (var i = 0; i < Animations.Count; i++)
            {
                CurrentAnimationIndex = i;
                var dt = 0.0f;
                for (var ticks = 0.0f; ticks < Animations[i].Duration; ticks += Animations[i].FramesPerSecond / 30.0f)
                {
                    dt += timestep;
                    Calculate(dt);
                    var trans = new List<Matrix4x4>();
                    for (var a = 0; a < _bones.Count; a++)
                    {
                        var rotMat = _bones[a].Offset * _bones[a].GlobalTransform;
                        trans.Add(rotMat);
                    }
                    Animations[i].Transforms.Add(trans);
                }
            }
        }

        private void Calculate(float dt)
        {
            if (CurrentAnimationIndex < 0 | CurrentAnimationIndex >= Animations.Count)
            {
                return;
            }
            Animations[CurrentAnimationIndex].Evaluate(dt, _bonesByName);
            UpdateTransforms(_skeleton);
        }

        private static void UpdateTransforms(Bone node)
        {
            CalculateBoneToWorldTransform(node);
            foreach (var child in node.Children)
            {
                UpdateTransforms(child);
            }
        }

        private static void CalculateBoneToWorldTransform(Bone child)
        {
            child.GlobalTransform = child.LocalTransform;
            var parent = child.Parent;
            while (parent != null)
            {
                child.GlobalTransform *= parent.LocalTransform;
                parent = parent.Parent;
            }
        }

        // SceneAnimator
        public List<Matrix4x4> GetTransforms(float dt)
        {
            return Animations[CurrentAnimationIndex].GetTransforms(dt);
        }
    }
}