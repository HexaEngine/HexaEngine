namespace HexaEngine.Volumes
{
    using HexaEngine.Editor;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using System.Collections.Generic;
    using System.Management;
    using System.Numerics;

    /// <summary>
    /// Represents a proxy for an IPostFx object, allowing dynamic access to its properties.
    /// </summary>
    public class PostFxProxy : ProxyBase
    {
        public PostFxProxy(IPostFx target) : base(target)
        {
        }

        [JsonConstructor]
        public PostFxProxy(Dictionary<string, object?> data, string typeName) : base(data, typeName)
        {
        }

        [JsonIgnore]
        public bool Enabled { get => (bool)Data["Enabled"]; set => Data["Enabled"] = value; }

        public void Apply(object target, PostFxProxy proxyBase, float blend, VolumeTransitionMode mode)
        {
            foreach (var property in properties)
            {
                if (propertyData.TryGetValue(property.Name, out var value) && proxyBase.propertyData.TryGetValue(property.Name, out var baseValue) && value != null)
                {
                    if (property.CanWrite)
                    {
                        try
                        {
                            value = mode switch
                            {
                                VolumeTransitionMode.Constant => value,
                                VolumeTransitionMode.Linear => BlendValueLerp(baseValue, value, blend),
                                VolumeTransitionMode.Smoothstep => BlendValueSmoothStep(baseValue, value, blend),
                                _ => value
                            };
                            property.SetValue(target, value);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private static object BlendValueLerp(object? baseValue, object value, float blend)
        {
            if (baseValue is Vector2 v20 && value is Vector2 v21)
            {
                return MathUtil.Lerp(v20, v21, blend);
            }
            else if (baseValue is Vector3 v30 && value is Vector3 v31)
            {
                return MathUtil.Lerp(v30, v31, blend);
            }
            else if (baseValue is Vector4 v40 && value is Vector4 v41)
            {
                return MathUtil.Lerp(v40, v41, blend);
            }
            else if (baseValue is float f0 && value is float f1)
            {
                return MathUtil.Lerp(f0, f1, blend);
            }
            else if (baseValue is int i0 && value is int i1)
            {
                return (int)MathUtil.Lerp(i0, i1, blend);
            }
            else if (baseValue is double d0 && value is double d1)
            {
                return MathUtil.Lerp(d0, d1, blend);
            }
            else if (baseValue is bool b0 && value is bool b1)
            {
                return blend >= 0.5f ? b0 : b1;
            }

            return value;
        }

        private static object BlendValueSmoothStep(object? baseValue, object value, float blend)
        {
            if (baseValue is Vector2 v20 && value is Vector2 v21)
            {
                return MathUtil.SmoothStep(v20, v21, blend);
            }
            else if (baseValue is Vector3 v30 && value is Vector3 v31)
            {
                return MathUtil.SmoothStep(v30, v31, blend);
            }
            else if (baseValue is Vector4 v40 && value is Vector4 v41)
            {
                return MathUtil.SmoothStep(v40, v41, blend);
            }
            else if (baseValue is float f0 && value is float f1)
            {
                return MathUtil.SmoothStep(f0, f1, blend);
            }
            else if (baseValue is int i0 && value is int i1)
            {
                return (int)MathUtil.SmoothStep(i0, i1, blend);
            }
            else if (baseValue is double d0 && value is double d1)
            {
                return MathUtil.SmoothStep(d0, d1, blend);
            }
            else if (baseValue is bool b0 && value is bool b1)
            {
                return blend <= 0.5f ? b0 : b1;
            }

            return value;
        }
    }
}