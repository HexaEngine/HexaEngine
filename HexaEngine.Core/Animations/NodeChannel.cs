namespace HexaEngine.Core.Animations
{
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public struct NodeChannel
    {
        public string NodeName;
        public List<VectorKeyframe> PositionKeyframes = new();
        public List<QuatKeyframe> RotationKeyframes = new();
        public List<VectorKeyframe> ScaleKeyframes = new();
        public AnimationBehavior PreState;
        public AnimationBehavior PostState;

        public Matrix4x4 Local;

        public NodeChannel(string nodeName) : this()
        {
            NodeName = nodeName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(float deltaTime)
        {
            Local = InterpolatePosition(deltaTime) * InterpolateRotation(deltaTime) * InterpolateScaling(deltaTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetPositionIndex(float animationTime)
        {
            for (int index = 0; index < PositionKeyframes.Count - 1; ++index)
            {
                if (animationTime < PositionKeyframes[index + 1].Time)
                    return index;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetRotationIndex(float animationTime)
        {
            for (int index = 0; index < RotationKeyframes.Count - 1; ++index)
            {
                if (animationTime < RotationKeyframes[index + 1].Time)
                    return index;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetScaleIndex(float animationTime)
        {
            for (int index = 0; index < ScaleKeyframes.Count - 1; ++index)
            {
                if (animationTime < ScaleKeyframes[index + 1].Time)
                    return index;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetScaleFactor(float lastTimeStamp, float nextTimeStamp, float animationTime)
        {
            float midWayLength = animationTime - lastTimeStamp;
            float framesDiff = nextTimeStamp - lastTimeStamp;
            float scaleFactor = midWayLength / framesDiff;
            return scaleFactor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 InterpolatePosition(float animationTime)
        {
            if (1 == PositionKeyframes.Count)
                return Matrix4x4.CreateTranslation(PositionKeyframes[0].Value);

            int p0Index = GetPositionIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor((float)PositionKeyframes[p0Index].Time, (float)PositionKeyframes[p1Index].Time, animationTime);
            Vector3 finalPosition = Vector3.Lerp(PositionKeyframes[p0Index].Value, PositionKeyframes[p1Index].Value, scaleFactor);
            return Matrix4x4.CreateTranslation(finalPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 InterpolateRotation(float animationTime)
        {
            if (1 == RotationKeyframes.Count)
                return Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(RotationKeyframes[0].Value));

            int p0Index = GetRotationIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor((float)RotationKeyframes[p0Index].Time, (float)RotationKeyframes[p1Index].Time, animationTime);
            Quaternion finalRotation = Quaternion.Slerp(RotationKeyframes[p0Index].Value, RotationKeyframes[p1Index].Value, scaleFactor);
            finalRotation = Quaternion.Normalize(finalRotation);
            return Matrix4x4.CreateFromQuaternion(finalRotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 InterpolateScaling(float animationTime)
        {
            if (1 == ScaleKeyframes.Count)
                return Matrix4x4.CreateScale(ScaleKeyframes[0].Value);

            int p0Index = GetScaleIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor((float)ScaleKeyframes[p0Index].Time, (float)ScaleKeyframes[p1Index].Time, animationTime);
            Vector3 finalScale = Vector3.Lerp(ScaleKeyframes[p0Index].Value, ScaleKeyframes[p1Index].Value, scaleFactor);
            return Matrix4x4.CreateScale(finalScale);
        }
    }
}