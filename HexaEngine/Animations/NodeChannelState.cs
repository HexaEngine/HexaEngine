namespace HexaEngine.Animations
{
    using HexaEngine.Core.IO.Binary.Animations;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class NodeChannelState
    {
        public string NodeName;
        public NodeId Node;
        public NodeChannel Channel;
        public Matrix4x4 Local;

        private int positionIndex;
        private int rotationIndex;
        private int scaleIndex;

        public NodeChannelState(string nodeName, NodeId node, NodeChannel channel)
        {
            NodeName = nodeName;
            Node = node;
            Channel = channel;

            if (channel.PositionKeyframes.Count == 0)
            {
                throw new InvalidOperationException("Cannot play an empty position animation channel");
            }
            if (channel.RotationKeyframes.Count == 0)
            {
                throw new InvalidOperationException("Cannot play an empty rotation animation channel");
            }
            if (channel.ScaleKeyframes.Count == 0)
            {
                throw new InvalidOperationException("Cannot play an empty scale animation channel");
            }
        }

        public void Reset()
        {
            positionIndex = rotationIndex = scaleIndex = 0;
        }

        public void Update(float animationTime)
        {
            UpdateIndices(animationTime);
            Local = InterpolateScaling(scaleIndex, animationTime) * InterpolateRotation(rotationIndex, animationTime) * InterpolatePosition(positionIndex, animationTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateIndices(float animationTime)
        {
            positionIndex = GetPositionIndex(positionIndex, animationTime);
            rotationIndex = GetRotationIndex(rotationIndex, animationTime);
            scaleIndex = GetScaleIndex(scaleIndex, animationTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetPositionIndex(int start, float animationTime)
        {
            for (int index = start; index < Channel.PositionKeyframes.Count - 1; ++index)
            {
                if (animationTime < Channel.PositionKeyframes[index + 1].Time)
                {
                    return index;
                }
            }

            // return 0 to wrap around.
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetRotationIndex(int start, float animationTime)
        {
            for (int index = start; index < Channel.RotationKeyframes.Count - 1; ++index)
            {
                if (animationTime < Channel.RotationKeyframes[index + 1].Time)
                {
                    return index;
                }
            }

            // return 0 to wrap around.
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetScaleIndex(int start, float animationTime)
        {
            for (int index = start; index < Channel.ScaleKeyframes.Count - 1; ++index)
            {
                if (animationTime < Channel.ScaleKeyframes[index + 1].Time)
                {
                    return index;
                }
            }

            // return 0 to wrap around.
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
        private Matrix4x4 InterpolateScaling(int start, float animationTime)
        {
            if (1 == Channel.ScaleKeyframes.Count)
            {
                return Matrix4x4.CreateScale(Channel.ScaleKeyframes[0].Value);
            }

            int p0Index = start;
            int p1Index = start + 1;
            float scaleFactor = GetScaleFactor((float)Channel.ScaleKeyframes[p0Index].Time, (float)Channel.ScaleKeyframes[p1Index].Time, animationTime);
            Vector3 finalScale = Vector3.Lerp(Channel.ScaleKeyframes[p0Index].Value, Channel.ScaleKeyframes[p1Index].Value, scaleFactor);
            return Matrix4x4.CreateScale(finalScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Matrix4x4 InterpolateRotation(int start, float animationTime)
        {
            if (1 == Channel.RotationKeyframes.Count)
            {
                return Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(Channel.RotationKeyframes[0].Value));
            }

            int p0Index = start;
            int p1Index = start + 1;
            float scaleFactor = GetScaleFactor((float)Channel.RotationKeyframes[p0Index].Time, (float)Channel.RotationKeyframes[p1Index].Time, animationTime);
            Quaternion finalRotation = Quaternion.Slerp(Channel.RotationKeyframes[p0Index].Value, Channel.RotationKeyframes[p1Index].Value, scaleFactor);
            finalRotation = Quaternion.Normalize(finalRotation);
            return Matrix4x4.CreateFromQuaternion(finalRotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Matrix4x4 InterpolatePosition(int start, float animationTime)
        {
            if (1 == Channel.PositionKeyframes.Count)
            {
                return Matrix4x4.CreateTranslation(Channel.PositionKeyframes[0].Value);
            }

            int p0Index = start;
            int p1Index = start + 1;
            float scaleFactor = GetScaleFactor((float)Channel.PositionKeyframes[p0Index].Time, (float)Channel.PositionKeyframes[p1Index].Time, animationTime);
            Vector3 finalPosition = Vector3.Lerp(Channel.PositionKeyframes[p0Index].Value, Channel.PositionKeyframes[p1Index].Value, scaleFactor);
            return Matrix4x4.CreateTranslation(finalPosition);
        }
    }
}