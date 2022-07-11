namespace HexaEngine.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class Animation
    {
        public const string Extension = ".ani";

        public string Name { get; private set; }

        public List<KeyframeChannel> Channels { get; set; } = new();

        public bool PlayAnimationForward { get; set; }

        private float LastTime { get; set; }

        public float FramesPerSecond { get; set; }

        public float Duration { get; private set; }

        private List<MutableTuple<int, int, int>> LastPositions { get; set; }

        public List<List<Matrix4x4>> Transforms { get; private set; }

        public Animation(float fps, float duration, string name, List<KeyframeChannel> channels)
        {
            LastTime = 0.0f;
            FramesPerSecond = fps;
            Duration = duration;
            Name = name;
            Channels = channels;
            LastPositions = Enumerable.Repeat(new MutableTuple<int, int, int>(0, 0, 0), channels.Count).ToList();
            Transforms = new List<List<Matrix4x4>>();
            PlayAnimationForward = true;
        }

        public void Evaluate(float dt, Dictionary<string, Bone> bones)
        {
            dt *= FramesPerSecond;
            var time = 0.0f;
            if (Duration > 0.0f)
            {
                time = dt % Duration;
            }
            for (int i = 0; i < Channels.Count; i++)
            {
                var channel = Channels[i];
                if (!bones.ContainsKey(channel.Name))
                {
                    Console.WriteLine("Did not find the bone node " + channel.Name);
                    continue;
                }
                // interpolate position keyframes
                var pPosition = new Vector3();
                if (channel.PositionKeys.Count > 0)
                {
                    var frame = time >= LastTime ? LastPositions[i].Item1 : 0;
                    while (frame < channel.PositionKeys.Count - 1)
                    {
                        if (time < channel.PositionKeys[frame + 1].Time)
                        {
                            break;
                        }
                        frame++;
                    }
                    if (frame >= channel.PositionKeys.Count)
                    {
                        frame = 0;
                    }

                    var nextFrame = (frame + 1) % channel.PositionKeys.Count;

                    var key = channel.PositionKeys[frame];
                    var nextKey = channel.PositionKeys[nextFrame];
                    var diffTime = nextKey.Time - key.Time;
                    if (diffTime < 0.0)
                    {
                        diffTime += Duration;
                    }
                    if (diffTime > 0.0)
                    {
                        var factor = (float)((time - key.Time) / diffTime);
                        pPosition = key.Value + (nextKey.Value - key.Value) * factor;
                    }
                    else
                    {
                        pPosition = key.Value;
                    }
                    LastPositions[i].Item1 = frame;
                }
                // interpolate rotation keyframes
                var pRot = new Quaternion(1, 0, 0, 0);
                if (channel.RotationKeys.Count > 0)
                {
                    var frame = time >= LastTime ? LastPositions[i].Item2 : 0;
                    while (frame < channel.RotationKeys.Count - 1)
                    {
                        if (time < channel.RotationKeys[frame + 1].Time)
                        {
                            break;
                        }
                        frame++;
                    }
                    if (frame >= channel.RotationKeys.Count)
                    {
                        frame = 0;
                    }
                    var nextFrame = (frame + 1) % channel.RotationKeys.Count;

                    var key = channel.RotationKeys[frame];
                    var nextKey = channel.RotationKeys[nextFrame];
                    key.Value = Quaternion.Normalize(key.Value);
                    nextKey.Value = Quaternion.Normalize(nextKey.Value);
                    var diffTime = nextKey.Time - key.Time;
                    if (diffTime < 0.0)
                    {
                        diffTime += Duration;
                    }
                    if (diffTime > 0)
                    {
                        var factor = (float)((time - key.Time) / diffTime);
                        pRot = Quaternion.Slerp(key.Value, nextKey.Value, factor);
                    }
                    else
                    {
                        pRot = key.Value;
                    }
                    LastPositions[i].Item1 = frame;
                }
                // interpolate scale keyframes
                var pscale = new Vector3(1);
                if (channel.ScalingKeys.Count > 0)
                {
                    var frame = time >= LastTime ? LastPositions[i].Item3 : 0;
                    while (frame < channel.ScalingKeys.Count - 1)
                    {
                        if (time < channel.ScalingKeys[frame + 1].Time)
                        {
                            break;
                        }
                        frame++;
                    }
                    if (frame >= channel.ScalingKeys.Count)
                    {
                        frame = 0;
                    }
                    LastPositions[i].Item3 = frame;
                }

                // create the combined transformation matrix
                var mat = Matrix4x4.CreateFromQuaternion(pRot);
                mat.M11 *= pscale.X; mat.M21 *= pscale.X; mat.M31 *= pscale.X;
                mat.M12 *= pscale.Y; mat.M22 *= pscale.Y; mat.M32 *= pscale.Y;
                mat.M13 *= pscale.Z; mat.M23 *= pscale.Z; mat.M33 *= pscale.Z;
                mat.M14 = pPosition.X; mat.M24 = pPosition.Y; mat.M34 = pPosition.Z;

                // transpose to get DirectX style matrix
                mat = Matrix4x4.Transpose(mat);
                bones[channel.Name].LocalTransform = mat;
            }
            LastTime = time;
        }

        // AnimEvaluator
        public List<Matrix4x4> GetTransforms(float dt)
        {
            return Transforms[GetFrameIndexAt(dt)];
        }

        private int GetFrameIndexAt(float dt)
        {
            dt *= FramesPerSecond;
            var time = 0.0f;
            if (Duration > 0.0f)
            {
                time = dt % Duration;
            }
            var percent = time / Duration;
            if (!PlayAnimationForward)
            {
                percent = (percent - 1.0f) * -1.0f;
            }
            var frameIndexAt = (int)(Transforms.Count * percent);
            return frameIndexAt;
        }
    }
}