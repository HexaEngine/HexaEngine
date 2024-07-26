namespace HexaEngine.Core.IO.Binary.Animations
{
    using HexaEngine.Core.IO;
    using Hexa.NET.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Represents a channel for animating a node's transformation.
    /// </summary>
    public struct NodeChannel
    {
        /// <summary>
        /// Gets or sets the name of the node associated with this channel.
        /// </summary>
        public string NodeName;

        /// <summary>
        /// Gets a list of keyframes for node position.
        /// </summary>
        public List<VectorKeyframe> PositionKeyframes = new();

        /// <summary>
        /// Gets a list of keyframes for node rotation.
        /// </summary>
        public List<QuatKeyframe> RotationKeyframes = new();

        /// <summary>
        /// Gets a list of keyframes for node scale.
        /// </summary>
        public List<VectorKeyframe> ScaleKeyframes = new();

        /// <summary>
        /// Gets or sets the animation behavior before the first keyframe.
        /// </summary>
        public AnimationBehavior PreState;

        /// <summary>
        /// Gets or sets the animation behavior after the last keyframe.
        /// </summary>
        public AnimationBehavior PostState;

        /// <summary>
        /// Gets or sets the local transformation matrix of the node.
        /// </summary>
        public Matrix4x4 Local;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeChannel"/> struct with the specified node name.
        /// </summary>
        /// <param name="nodeName">The name of the node associated with this channel.</param>
        public NodeChannel(string nodeName) : this()
        {
            NodeName = nodeName;
        }

        /// <summary>
        /// Updates the local transformation matrix based on the current time.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(float deltaTime)
        {
            Local = InterpolateScaling(deltaTime) * InterpolateRotation(deltaTime) * InterpolatePosition(deltaTime);
        }

        /// <summary>
        /// Gets the index of the position keyframe to use for interpolation at the given animation time.
        /// </summary>
        /// <param name="animationTime">The current animation time.</param>
        /// <returns>The index of the position keyframe to use for interpolation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int GetPositionIndex(float animationTime)
        {
            for (int index = 0; index < PositionKeyframes.Count - 1; ++index)
            {
                if (animationTime < PositionKeyframes[index + 1].Time)
                {
                    return index;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the index of the rotation keyframe to use for interpolation at the given animation time.
        /// </summary>
        /// <param name="animationTime">The current animation time.</param>
        /// <returns>The index of the rotation keyframe to use for interpolation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int GetRotationIndex(float animationTime)
        {
            for (int index = 0; index < RotationKeyframes.Count - 1; ++index)
            {
                if (animationTime < RotationKeyframes[index + 1].Time)
                {
                    return index;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the index of the scale keyframe to use for interpolation at the given animation time.
        /// </summary>
        /// <param name="animationTime">The current animation time.</param>
        /// <returns>The index of the scale keyframe to use for interpolation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int GetScaleIndex(float animationTime)
        {
            for (int index = 0; index < ScaleKeyframes.Count - 1; ++index)
            {
                if (animationTime < ScaleKeyframes[index + 1].Time)
                {
                    return index;
                }
            }
            return 0;
        }

        /// <summary>
        /// Calculates the interpolation factor for the given animation time within the specified keyframe time range.
        /// </summary>
        /// <param name="lastTimeStamp">The timestamp of the previous keyframe.</param>
        /// <param name="nextTimeStamp">The timestamp of the next keyframe.</param>
        /// <param name="animationTime">The current animation time.</param>
        /// <returns>The interpolation factor.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetScaleFactor(float lastTimeStamp, float nextTimeStamp, float animationTime)
        {
            float midWayLength = animationTime - lastTimeStamp;
            float framesDiff = nextTimeStamp - lastTimeStamp;
            float scaleFactor = midWayLength / framesDiff;
            return scaleFactor;
        }

        /// <summary>
        /// Interpolates the position for the given animation time.
        /// </summary>
        /// <param name="animationTime">The current animation time.</param>
        /// <returns>The interpolated position matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix4x4 InterpolatePosition(float animationTime)
        {
            if (1 == PositionKeyframes.Count)
            {
                return Matrix4x4.CreateTranslation(PositionKeyframes[0].Value);
            }

            int p0Index = GetPositionIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor((float)PositionKeyframes[p0Index].Time, (float)PositionKeyframes[p1Index].Time, animationTime);
            Vector3 finalPosition = Vector3.Lerp(PositionKeyframes[p0Index].Value, PositionKeyframes[p1Index].Value, scaleFactor);
            return Matrix4x4.CreateTranslation(finalPosition);
        }

        /// <summary>
        /// Interpolates the rotation for the given animation time.
        /// </summary>
        /// <param name="animationTime">The current animation time.</param>
        /// <returns>The interpolated rotation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix4x4 InterpolateRotation(float animationTime)
        {
            if (1 == RotationKeyframes.Count)
            {
                return Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(RotationKeyframes[0].Value));
            }

            int p0Index = GetRotationIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor((float)RotationKeyframes[p0Index].Time, (float)RotationKeyframes[p1Index].Time, animationTime);
            Quaternion finalRotation = Quaternion.Slerp(RotationKeyframes[p0Index].Value, RotationKeyframes[p1Index].Value, scaleFactor);
            finalRotation = Quaternion.Normalize(finalRotation);
            return Matrix4x4.CreateFromQuaternion(finalRotation);
        }

        /// <summary>
        /// Interpolates the scaling for the given animation time.
        /// </summary>
        /// <param name="animationTime">The current animation time.</param>
        /// <returns>The interpolated scaling matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Matrix4x4 InterpolateScaling(float animationTime)
        {
            if (1 == ScaleKeyframes.Count)
            {
                return Matrix4x4.CreateScale(ScaleKeyframes[0].Value);
            }

            int p0Index = GetScaleIndex(animationTime);
            int p1Index = p0Index + 1;
            float scaleFactor = GetScaleFactor((float)ScaleKeyframes[p0Index].Time, (float)ScaleKeyframes[p1Index].Time, animationTime);
            Vector3 finalScale = Vector3.Lerp(ScaleKeyframes[p0Index].Value, ScaleKeyframes[p1Index].Value, scaleFactor);
            return Matrix4x4.CreateScale(finalScale);
        }

        /// <summary>
        /// Writes the NodeChannel data to a binary stream.
        /// </summary>
        /// <param name="stream">The binary stream to write to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public readonly void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(NodeName, encoding, endianness);
            stream.WriteInt32((int)PreState, endianness);
            stream.WriteInt32((int)PostState, endianness);
            stream.WriteInt32(PositionKeyframes.Count, endianness);
            for (int i = 0; i < PositionKeyframes.Count; i++)
            {
                PositionKeyframes[i].Write(stream, endianness);
            }
            stream.WriteInt32(RotationKeyframes.Count, endianness);
            for (int i = 0; i < RotationKeyframes.Count; i++)
            {
                RotationKeyframes[i].Write(stream, endianness);
            }
            stream.WriteInt32(ScaleKeyframes.Count, endianness);
            for (int i = 0; i < ScaleKeyframes.Count; i++)
            {
                ScaleKeyframes[i].Write(stream, endianness);
            }
        }

        /// <summary>
        /// Reads a NodeChannel from a binary stream with the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the channel from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            NodeName = stream.ReadString(encoding, endianness) ?? string.Empty;
            PreState = (AnimationBehavior)stream.ReadInt32(endianness);
            PostState = (AnimationBehavior)stream.ReadInt32(endianness);
            var positionKeyframesCount = stream.ReadInt32(endianness);
            PositionKeyframes = new(positionKeyframesCount);
            for (int i = 0; i < positionKeyframesCount; i++)
            {
                PositionKeyframes.Add(VectorKeyframe.ReadFrom(stream, endianness));
            }
            var rotationKeyframesCount = stream.ReadInt32(endianness);
            RotationKeyframes = new(rotationKeyframesCount);
            for (int i = 0; i < rotationKeyframesCount; i++)
            {
                RotationKeyframes.Add(QuatKeyframe.ReadFrom(stream, endianness));
            }
            var scaleKeyframesCount = stream.ReadInt32(endianness);
            ScaleKeyframes = new(scaleKeyframesCount);
            for (int i = 0; i < scaleKeyframesCount; i++)
            {
                ScaleKeyframes.Add(VectorKeyframe.ReadFrom(stream, endianness));
            }
        }

        /// <summary>
        /// Reads a NodeChannel from a binary stream with the specified encoding and endianness.
        /// </summary>
        /// <param name="stream">The binary stream to read the channel from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of binary data.</param>
        /// <returns>The NodeChannel read from the stream.</returns>
        public static NodeChannel ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            NodeChannel channel = default;
            channel.Read(stream, encoding, endianness);
            return channel;
        }

        /// <summary>
        /// Deep clones a <see cref="NodeChannel"/> instance.
        /// </summary>
        /// <returns>The deep cloned <see cref="NodeChannel"/> instance.</returns>
        public NodeChannel Clone()
        {
            return new NodeChannel((string)NodeName.Clone()) { Local = Local, PositionKeyframes = new(PositionKeyframes), RotationKeyframes = new(RotationKeyframes), ScaleKeyframes = new(ScaleKeyframes), PostState = PostState, PreState = PreState };
        }
    }
}