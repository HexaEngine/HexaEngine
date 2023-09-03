namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;

    public static class MathUtil
    {
        public const double DegToRadFactor = Math.PI / 180;
        public const double RadToDefFactor = 180 / Math.PI;

        public const float PI = MathF.PI;

        public const float PI2 = 2 * MathF.PI;

        public const float PIDIV2 = MathF.PI / 2;

        public const float SQRT2 = 1.41421356237309504880f;
        public const float SQRT3 = 1.73205080756887729352f;
        public const float SQRT6 = 2.44948974278317809820f;

        public static readonly Vector4 SplatEpsilon = new(BitConverter.UInt32BitsToSingle(0x34000000));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Round(this float x)
        {
            return (int)MathF.Floor(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            return RotationQuaternion(quaternion);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, Vector3 rotation, Vector3 scale)
        {
            return Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) * Matrix4x4.CreateScale(scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, Vector3 rotation, float scale)
        {
            return Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) * Matrix4x4.CreateScale(scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, float scale)
        {
            return Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, Vector3 scale)
        {
            return Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetYawPitchRoll(this Quaternion r, out float yaw, out float pitch, out float roll)
        {
            yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            pitch = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
            roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetYawPitchRoll(this Quaternion r)
        {
            float yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            float pitch = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
            float roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
            return new Vector3(yaw, pitch, roll);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ApplyMatrix(this Vector4 self, Matrix4x4 matrix)
        {
            return new Vector4(
                matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14 * self.W,
                matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24 * self.W,
                matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34 * self.W,
                matrix.M41 * self.X + matrix.M42 * self.Y + matrix.M43 * self.Z + matrix.M44 * self.W
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ApplyMatrix(this Vector3 self, Matrix4x4 matrix)
        {
            return new Vector3(
                matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14,
                matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24,
                matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToDeg(this Vector3 v)
        {
            return new Vector3((float)(v.X * RadToDefFactor), (float)(v.Y * RadToDefFactor), (float)(v.Z * RadToDefFactor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToRad(this Vector3 v)
        {
            return new Vector3((float)(v.X * DegToRadFactor), (float)(v.Y * DegToRadFactor), (float)(v.Z * DegToRadFactor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToDeg(this float v)
        {
            return (float)(v * RadToDefFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToRad(this float v)
        {
            return (float)(v * DegToRadFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion GetQuaternion(this Vector3 vector)
        {
            return Quaternion.CreateFromYawPitchRoll(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Creates a left-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <param name="result">When the method completes, contains the created billboard matrix.</param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 BillboardLH(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
        {
            Vector3 crossed;
            Vector3 final;
            Vector3 difference = cameraPosition - objectPosition;

            float lengthSq = difference.LengthSquared();
            if (lengthSq == 0)
            {
                difference = -cameraForwardVector;
            }
            else
            {
                difference *= (float)(1.0 / Math.Sqrt(lengthSq));
            }

            crossed = Vector3.Cross(cameraUpVector, difference);
            crossed = Vector3.Normalize(crossed);
            final = Vector3.Cross(difference, crossed);

            Matrix4x4 result = new();
            result.M11 = crossed.X;
            result.M12 = crossed.Y;
            result.M13 = crossed.Z;
            result.M14 = 0.0f;
            result.M21 = final.X;
            result.M22 = final.Y;
            result.M23 = final.Z;
            result.M24 = 0.0f;
            result.M31 = difference.X;
            result.M32 = difference.Y;
            result.M33 = difference.Z;
            result.M34 = 0.0f;
            result.M41 = objectPosition.X;
            result.M42 = objectPosition.Y;
            result.M43 = objectPosition.Z;
            result.M44 = 1.0f;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 RotationQuaternion(Quaternion rotation)
        {
            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            Matrix4x4 result = Matrix4x4.Identity;
            result.M11 = 1.0f - 2.0f * (yy + zz);
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - 2.0f * (zz + xx);
            result.M23 = 2.0f * (yz + xw);
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - 2.0f * (yy + xx);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zaxis = Vector3.Normalize(Vector3.Subtract(target, eye));
            Vector3 xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

            Matrix4x4 result = Matrix4x4.Identity;
            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            result.M41 = Vector3.Dot(xaxis, eye);
            result.M42 = Vector3.Dot(yaxis, eye);
            result.M43 = Vector3.Dot(zaxis, eye);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 PerspectiveFovLH(float fov, float aspect, float znear, float zfar)
        {
            float yScale = (float)(1.0f / Math.Tan(fov * 0.5f));
            float q = zfar / (zfar - znear);

            Matrix4x4 result = new();
            result.M11 = yScale / aspect;
            result.M22 = yScale;
            result.M33 = q;
            result.M34 = 1.0f;
            result.M43 = -q * znear;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 OrthoLH(float width, float height, float znear, float zfar)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            return OrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, znear, zfar);
        }

        /// <summary>
        /// Creates a left-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="znear">Minimum z-value of the viewing volume.</param>
        /// <param name="zfar">Maximum z-value of the viewing volume.</param>
        /// <param name="result">When the method completes, contains the created projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            float zRange = 1.0f / (zfar - znear);

            Matrix4x4 result = Matrix4x4.Identity;
            result.M11 = 2.0f / (right - left);
            result.M22 = 2.0f / (top - bottom);
            result.M33 = zRange;
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = -znear * zRange;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizeEulerAngleDegrees(this Vector3 angle)
        {
            float normalizedX = angle.X % 360;
            float normalizedY = angle.Y % 360;
            float normalizedZ = angle.Z % 360;
            if (normalizedX < 0)
            {
                normalizedX += 360;
            }

            if (normalizedY < 0)
            {
                normalizedY += 360;
            }

            if (normalizedZ < 0)
            {
                normalizedZ += 360;
            }

            return new(normalizedX, normalizedY, normalizedZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float x, float y, float s)
        {
            return x * (1 - s) + y * s;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Pack(this Vector4 color)
        {
            return Pack((uint)(color.W * 255), (uint)(color.X * 255), (uint)(color.Y * 255), (uint)(color.Z * 255));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Pack(uint a, uint r, uint g, uint b)
        {
            return (a << 24) + (r << 16) + (g << 8) + b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AnglesFromNormal(Vector3 normal, out float yaw, out float pitch, out float roll)
        {
            yaw = MathF.Atan2(normal.X, normal.Z);
            pitch = MathF.Asin(-normal.Y);
            roll = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                throw new($"The minimum was greater than the maximum");
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float value)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value > 1)
            {
                return 1;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp01(Vector2 v)
        {
            return new Vector2(Clamp01(v.X), Clamp01(v.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp01(Vector3 v)
        {
            return new Vector3(Clamp01(v.X), Clamp01(v.Y), Clamp01(v.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Clamp01(Vector4 v)
        {
            return new Vector4(Clamp01(v.X), Clamp01(v.Y), Clamp01(v.Z), Clamp01(v.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Floor(this Vector2 vector)
        {
            return new(MathF.Floor(vector.X), MathF.Floor(vector.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(this Vector3 vector)
        {
            return new(MathF.Floor(vector.X), MathF.Floor(vector.Y), MathF.Floor(vector.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Floor(this Vector4 vector)
        {
            return new(MathF.Floor(vector.X), MathF.Floor(vector.Y), MathF.Floor(vector.Z), MathF.Floor(vector.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Ceiling(this Vector2 value)
        {
            return new Vector2(MathF.Ceiling(value.X), MathF.Ceiling(value.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Ceiling(this Vector3 value)
        {
            return new Vector3(MathF.Ceiling(value.X), MathF.Ceiling(value.Y), MathF.Ceiling(value.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Ceiling(this Vector4 value)
        {
            return new Vector4(MathF.Ceiling(value.X), MathF.Ceiling(value.Y), MathF.Ceiling(value.Z), MathF.Ceiling(value.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Pow(Vector2 a, float b)
        {
            return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Pow(Vector3 a, float b)
        {
            return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b), MathF.Pow(a.Z, b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Pow(Vector4 a, float b)
        {
            return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b), MathF.Pow(a.Z, b), MathF.Pow(a.W, b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Pow(Vector2 a, Vector2 b)
        {
            return new(MathF.Pow(a.X, b.X), MathF.Pow(a.Y, b.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Pow(Vector3 a, Vector3 b)
        {
            return new(MathF.Pow(a.X, b.X), MathF.Pow(a.Y, b.Y), MathF.Pow(a.Z, b.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Pow(Vector4 a, Vector4 b)
        {
            return new(MathF.Pow(a.X, b.X), MathF.Pow(a.Y, b.Y), MathF.Pow(a.Z, b.Z), MathF.Pow(a.W, b.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Exp(Vector2 a)
        {
            return new(MathF.Exp(a.X), MathF.Exp(a.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Exp(Vector3 a)
        {
            return new(MathF.Exp(a.X), MathF.Exp(a.Y), MathF.Exp(a.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Exp(Vector4 a)
        {
            return new(MathF.Exp(a.X), MathF.Exp(a.Y), MathF.Exp(a.Z), MathF.Exp(a.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp2(float x)
        {
            return BitConverter.Int32BitsToSingle(((int)(x * 0x00800000 + 126.0f)) << 23);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Exp2(Vector2 a)
        {
            return new(Exp2(a.X), Exp2(a.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Exp2(Vector3 a)
        {
            return new(Exp2(a.X), Exp2(a.Y), Exp2(a.Z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Exp2(Vector4 a)
        {
            return new(Exp2(a.X), Exp2(a.Y), Exp2(a.Z), Exp2(a.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map01ToN1P1(this float value)
        {
            return 2 * value - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearEqual(float v1, float v2, float epsilon)
        {
            float delta = MathF.Abs(v1 - v2);
            return delta <= epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearEqual(Vector2 v1, Vector2 v2, Vector2 epsilon)
        {
            if (Sse.IsSupported)
            {
                Vector128<float> vDelta = Sse.Subtract(v1.AsVector128(), v2.AsVector128());
                Vector128<float> vTemp = Vector128<float>.Zero;
                vTemp = Sse.Subtract(vTemp, vDelta);
                vTemp = Sse.Max(vTemp, vDelta);
                vTemp = Sse.CompareLessThanOrEqual(vTemp, epsilon.AsVector128());
                int mask = Sse.MoveMask(vTemp);
                return (mask & 3) == 0x3;
            }

            float dx = MathF.Abs(v1.X - v2.X);
            float dy = MathF.Abs(v1.Y - v2.Y);
            return (dx <= epsilon.X) && (dy <= epsilon.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearEqual(Vector3 v1, Vector3 v2, Vector3 epsilon)
        {
            if (Sse.IsSupported)
            {
                Vector128<float> vDelta = Sse.Subtract(v1.AsVector128(), v2.AsVector128());
                Vector128<float> vTemp = Vector128<float>.Zero;
                vTemp = Sse.Subtract(vTemp, vDelta);
                vTemp = Sse.Max(vTemp, vDelta);
                vTemp = Sse.CompareLessThanOrEqual(vTemp, epsilon.AsVector128());
                int mask = Sse.MoveMask(vTemp);
                return (mask & 7) == 0x7;
            }

            float dx = MathF.Abs(v1.X - v2.X);
            float dy = MathF.Abs(v1.Y - v2.Y);
            float dz = MathF.Abs(v1.Z - v2.Z);
            return (dx <= epsilon.X) && (dy <= epsilon.Y) && (dz <= epsilon.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearEqual(Vector4 v1, Vector4 v2, Vector4 epsilon)
        {
            if (Sse.IsSupported)
            {
                Vector128<float> vDelta = Sse.Subtract(v1.AsVector128(), v2.AsVector128());
                Vector128<float> vTemp = Vector128<float>.Zero;
                vTemp = Sse.Subtract(vTemp, vDelta);
                vTemp = Sse.Max(vTemp, vDelta);
                vTemp = Sse.CompareLessThanOrEqual(vTemp, epsilon.AsVector128());
                int mask = Sse.MoveMask(vTemp);
                return (mask & 0xf) == 0xf;
            }

            float dx = MathF.Abs(v1.X - v2.X);
            float dy = MathF.Abs(v1.Y - v2.Y);
            float dz = MathF.Abs(v1.Z - v2.Z);
            float dw = MathF.Abs(v1.W - v2.W);
            return (dx <= epsilon.X) && (dy <= epsilon.Y) && (dz <= epsilon.Z) && (dw <= epsilon.W);
        }
    }
}