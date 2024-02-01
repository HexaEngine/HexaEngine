namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;

    /// <summary>
    /// A utility class containing various mathematical functions and constants.
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// The factor to convert degrees to radians.
        /// </summary>
        public const double DegToRadFactor = Math.PI / 180;

        /// <summary>
        /// The factor to convert radians to degrees.
        /// </summary>
        public const double RadToDefFactor = 180 / Math.PI;

        /// <summary>
        /// The mathematical constant PI.
        /// </summary>
        public const float PI = MathF.PI;

        /// <summary>
        /// Two times the mathematical constant PI.
        /// </summary>
        public const float PI2 = 2 * MathF.PI;

        /// <summary>
        /// Half of the mathematical constant PI.
        /// </summary>
        public const float PIDIV2 = MathF.PI / 2;

        /// <summary>
        /// The square root of 2.
        /// </summary>
        public const float SQRT2 = 1.41421356237309504880f;

        /// <summary>
        /// The square root of 3.
        /// </summary>
        public const float SQRT3 = 1.73205080756887729352f;

        /// <summary>
        /// The square root of 6.
        /// </summary>
        public const float SQRT6 = 2.44948974278317809820f;

        /// <summary>
        /// A Vector4 containing a small epsilon value.
        /// </summary>
        public static readonly Vector4 SplatEpsilon = new(BitConverter.UInt32BitsToSingle(0x34000000));

        /// <summary>
        /// Represents a constant Vector4 with all components set to positive infinity.
        /// </summary>
        public static readonly Vector4 Infinity = new(BitConverter.UInt32BitsToSingle(0x7F800000));

        /// <summary>
        /// Represents a quiet NaN (Not a Number) in single-precision floating-point format.
        /// </summary>
        public static readonly Vector4 QNaN = new(BitConverter.UInt32BitsToSingle(0x7FC00000));

        /// <summary>
        /// Rounds the given float to the nearest integer.
        /// </summary>
        /// <param name="x">The input float value.</param>
        /// <returns>The rounded integer value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Round(this float x)
        {
            return (int)MathF.Floor(x);
        }

        /// <summary>
        /// Creates a rotation matrix from yaw, pitch, and roll angles.
        /// </summary>
        /// <param name="yaw">The yaw angle (in radians).</param>
        /// <param name="pitch">The pitch angle (in radians).</param>
        /// <param name="roll">The roll angle (in radians).</param>
        /// <returns>The rotation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            return RotationQuaternion(quaternion);
        }

        /// <summary>
        /// Creates a transformation matrix from position, rotation, and scale.
        /// </summary>
        /// <param name="pos">The position vector.</param>
        /// <param name="rotation">The rotation vector (in radians).</param>
        /// <param name="scale">The scale vector.</param>
        /// <returns>The transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, Vector3 rotation, Vector3 scale)
        {
            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) * Matrix4x4.CreateTranslation(pos);
        }

        /// <summary>
        /// Creates a transformation matrix from position, rotation, and uniform scale.
        /// </summary>
        /// <param name="pos">The position vector.</param>
        /// <param name="rotation">The rotation vector (in radians).</param>
        /// <param name="scale">The uniform scale factor.</param>
        /// <returns>The transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, Vector3 rotation, float scale)
        {
            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) * Matrix4x4.CreateTranslation(pos);
        }

        /// <summary>
        /// Creates a 4x4 transformation matrix from a position, rotation, and uniform scale.
        /// </summary>
        /// <param name="position">The translation vector representing the position.</param>
        /// <param name="rotation">The quaternion representing the rotation.</param>
        /// <param name="scale">The uniform scale factor.</param>
        /// <returns>The 4x4 transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 position, Quaternion rotation, float scale)
        {
            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }

        /// <summary>
        /// Creates a 4x4 transformation matrix from a position, rotation, and non-uniform scale.
        /// </summary>
        /// <param name="position">The translation vector representing the position.</param>
        /// <param name="rotation">The quaternion representing the rotation.</param>
        /// <param name="scale">The vector representing non-uniform scaling factors along the x, y, and z axes.</param>
        /// <returns>The 4x4 transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }

        /// <summary>
        /// Creates a transformation matrix from position and uniform scale.
        /// </summary>
        /// <param name="pos">The position vector.</param>
        /// <param name="scale">The uniform scale factor.</param>
        /// <returns>The transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, float scale)
        {
            return Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale);
        }

        /// <summary>
        /// Creates a transformation matrix from position and non-uniform scale.
        /// </summary>
        /// <param name="pos">The position vector.</param>
        /// <param name="scale">The non-uniform scale vector.</param>
        /// <returns>The transformation matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 CreateTransform(Vector3 pos, Vector3 scale)
        {
            return Matrix4x4.CreateTranslation(pos) * Matrix4x4.CreateScale(scale);
        }

        /// <summary>
        /// Calculates yaw, pitch, and roll angles from a <see cref="Quaternion"/> and stores them in the provided out parameters.
        /// </summary>
        /// <param name="r">The input <see cref="Quaternion"/>.</param>
        /// <param name="yaw">The calculated yaw angle (in radians).</param>
        /// <param name="pitch">The calculated pitch angle (in radians).</param>
        /// <param name="roll">The calculated roll angle (in radians).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetYawPitchRoll(this Quaternion r, out float yaw, out float pitch, out float roll)
        {
            yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            pitch = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
            roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
        }

        /// <summary>
        /// Calculates yaw, pitch, and roll angles from a <see cref="Quaternion"/> and returns them as a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="r">The input <see cref="Quaternion"/>.</param>
        /// <returns>A <see cref="Vector3"/> containing yaw, pitch, and roll angles (in radians).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToYawPitchRoll(this Quaternion r)
        {
            float yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            float pitch = MathF.Asin(2.0f * (r.X * r.W - r.Y * r.Z));
            float roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
            return new Vector3(yaw, pitch, roll);
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> from radians to degrees.
        /// </summary>
        /// <param name="v">The input vector in radians.</param>
        /// <returns>A <see cref="Vector3"/> with values converted to degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToDeg(this Vector3 v)
        {
            return new Vector3((float)(v.X * RadToDefFactor), (float)(v.Y * RadToDefFactor), (float)(v.Z * RadToDefFactor));
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> from degrees to radians.
        /// </summary>
        /// <param name="v">The input vector in degrees.</param>
        /// <returns>A <see cref="Vector3"/> with values converted to radians.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToRad(this Vector3 v)
        {
            return new Vector3((float)(v.X * DegToRadFactor), (float)(v.Y * DegToRadFactor), (float)(v.Z * DegToRadFactor));
        }

        /// <summary>
        /// Converts an angle from radians to degrees.
        /// </summary>
        /// <param name="v">The input angle in radians.</param>
        /// <returns>The angle value converted to degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToDeg(this float v)
        {
            return v == 0 ? 0.0f : (float)(v * RadToDefFactor);
        }

        /// <summary>
        /// Converts an angle from degrees to radians.
        /// </summary>
        /// <param name="v">The input angle in degrees.</param>
        /// <returns>The angle value converted to radians.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToRad(this float v)
        {
            return (float)(v * DegToRadFactor);
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> representing yaw, pitch, and roll angles to a quaternion.
        /// </summary>
        /// <param name="vector">The input <see cref="Vector3"/> with yaw, pitch, and roll angles.</param>
        /// <returns>The corresponding <see cref="Quaternion"/> representing the rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ToQuaternion(this Vector3 vector)
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
        /// <returns>When the method completes, contains the created billboard matrix.</returns>

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

        /// <summary>
        /// Creates a rotation matrix from the specified quaternion.
        /// </summary>
        /// <param name="rotation">The input quaternion representing the rotation.</param>
        /// <returns>The rotation matrix based on the provided quaternion.</returns>
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

        /// <summary>
        /// Creates a left-handed view matrix based on the specified camera position, target point, and up direction.
        /// </summary>
        /// <param name="eye">The position of the camera.</param>
        /// <param name="target">The target point that the camera is looking at.</param>
        /// <param name="up">The up direction of the camera.</param>
        /// <returns>The left-handed view matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 zAxis = Vector3.Normalize(Vector3.Subtract(target, eye));
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

            Matrix4x4 result = Matrix4x4.Identity;
            result.M11 = xAxis.X; result.M21 = xAxis.Y; result.M31 = xAxis.Z;
            result.M12 = yAxis.X; result.M22 = yAxis.Y; result.M32 = yAxis.Z;
            result.M13 = zAxis.X; result.M23 = zAxis.Y; result.M33 = zAxis.Z;

            result.M41 = Vector3.Dot(xAxis, eye);
            result.M42 = Vector3.Dot(yAxis, eye);
            result.M43 = Vector3.Dot(zAxis, eye);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
            return result;
        }

        /// <summary>
        /// Creates a left-handed perspective projection matrix based on the field of view, aspect ratio, and depth range.
        /// </summary>
        /// <param name="fov">The field of view in radians.</param>
        /// <param name="aspect">The aspect ratio of the view.</param>
        /// <param name="zNear">The minimum depth of the view frustum.</param>
        /// <param name="zFar">The maximum depth of the view frustum.</param>
        /// <returns>The perspective projection matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 PerspectiveFovLH(float fov, float aspect, float zNear, float zFar)
        {
            float yScale = (float)(1.0f / Math.Tan(fov * 0.5f));
            float q = zFar / (zFar - zNear);

            Matrix4x4 result = new();
            result.M11 = yScale / aspect;
            result.M22 = yScale;
            result.M33 = q;
            result.M34 = 1.0f;
            result.M43 = -q * zNear;
            return result;
        }

        /// <summary>
        /// Creates a left-handed orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the view volume.</param>
        /// <param name="height">The height of the view volume.</param>
        /// <param name="zNear">The minimum depth of the view volume.</param>
        /// <param name="zFar">The maximum depth of the view volume.</param>
        /// <returns>The orthographic projection matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 OrthoLH(float width, float height, float zNear, float zFar)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            return OrthoOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, zNear, zFar);
        }

        /// <summary>
        /// Creates a left-handed, customized orthographic projection matrix.
        /// </summary>
        /// <param name="left">Minimum x-value of the viewing volume.</param>
        /// <param name="right">Maximum x-value of the viewing volume.</param>
        /// <param name="bottom">Minimum y-value of the viewing volume.</param>
        /// <param name="top">Maximum y-value of the viewing volume.</param>
        /// <param name="zNear">Minimum z-value of the viewing volume.</param>
        /// <param name="zFar">Maximum z-value of the viewing volume.</param>
        /// <returns>When the method completes, contains the created projection matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 OrthoOffCenterLH(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            float zRange = 1.0f / (zFar - zNear);

            Matrix4x4 result = Matrix4x4.Identity;
            result.M11 = 2.0f / (right - left);
            result.M22 = 2.0f / (top - bottom);
            result.M33 = zRange;
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = -zNear * zRange;
            return result;
        }

        /// <summary>
        /// Normalizes Euler angles to the [0, 360] degree range.
        /// </summary>
        /// <param name="angle">The input Euler angles.</param>
        /// <returns>The normalized Euler angles.</returns>
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

        /// <summary>
        /// Computes the dot product of two 2D vectors. Uses SIMD instructions if supported;
        /// otherwise, falls back to the standard non-SIMD Dot product method.
        /// </summary>
        /// <param name="a">The first 2D vector.</param>
        /// <param name="b">The second 2D vector.</param>
        /// <returns>The dot product of the two 2D vectors.</returns>
        public static float Dot(Vector2 a, Vector2 b)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vecA = a.AsVector128();
                Vector128<float> vecB = b.AsVector128();
                Vector128<float> result = Sse41.DotProduct(vecA, vecB, 0x3f);
                return result.ToScalar();
            }

            return Vector2.Dot(a, b);
        }

        /// <summary>
        /// Computes the dot product of two 3D vectors. Uses SIMD instructions if supported;
        /// otherwise, falls back to the standard non-SIMD Dot product method.
        /// </summary>
        /// <param name="a">The first 3D vector.</param>
        /// <param name="b">The second 3D vector.</param>
        /// <returns>The dot product of the two 3D vectors.</returns>
        public static float Dot(Vector3 a, Vector3 b)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vecA = a.AsVector128();
                Vector128<float> vecB = b.AsVector128();
                Vector128<float> result = Sse41.DotProduct(vecA, vecB, 0x7f);
                return result.ToScalar();
            }

            return Vector3.Dot(a, b);
        }

        /// <summary>
        /// Computes the dot product of two 4D vectors. Uses SIMD instructions if supported;
        /// otherwise, falls back to the standard non-SIMD Dot product method.
        /// </summary>
        /// <param name="a">The first 4D vector.</param>
        /// <param name="b">The second 4D vector.</param>
        /// <returns>The dot product of the two 4D vectors.</returns>
        public static float Dot(Vector4 a, Vector4 b)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vecA = a.AsVector128();
                Vector128<float> vecB = b.AsVector128();
                Vector128<float> result = Sse41.DotProduct(vecA, vecB, 0xff);
                return result.ToScalar();
            }

            return Vector4.Dot(a, b);
        }

        /// <summary>
        /// Estimates the normalized form of a 2D vector. This method provides a fast
        /// approximation using SIMD instructions if supported; otherwise, it falls back
        /// to the standard non-SIMD normalization method.
        /// </summary>
        /// <param name="vector">The 2D vector to normalize.</param>
        /// <returns>The estimated normalized 2D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizeEst(Vector2 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                Vector128<float> vTemp = Sse41.DotProduct(vec, vec, 0x3f);
                Vector128<float> vResult = Sse.ReciprocalSqrt(vTemp);
                Vector128<float> result = Sse.Multiply(vResult, vec);
                return result.AsVector2();
            }

            return Vector2.Normalize(vector);
        }

        /// <summary>
        /// Estimates the normalized form of a 3D vector. This method provides a fast
        /// approximation using SIMD instructions if supported; otherwise, it falls back
        /// to the standard non-SIMD normalization method.
        /// </summary>
        /// <param name="vector">The 3D vector to normalize.</param>
        /// <returns>The estimated normalized 3D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizeEst(Vector3 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                Vector128<float> vTemp = Sse41.DotProduct(vec, vec, 0x7f);
                Vector128<float> vResult = Sse.ReciprocalSqrt(vTemp);
                Vector128<float> result = Sse.Multiply(vResult, vec);
                return result.AsVector3();
            }

            return Vector3.Normalize(vector);
        }

        /// <summary>
        /// Estimates the normalized form of a 4D vector. This method provides a fast
        /// approximation using SIMD instructions if supported; otherwise, it falls back
        /// to the standard non-SIMD normalization method.
        /// </summary>
        /// <param name="vector">The 4D vector to normalize.</param>
        /// <returns>The estimated normalized 4D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 NormalizeEst(Vector4 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                Vector128<float> vTemp = Sse41.DotProduct(vec, vec, 0xFF);
                Vector128<float> vResult = Sse.ReciprocalSqrt(vTemp);
                Vector128<float> result = Sse.Multiply(vResult, vec);
                return result.AsVector4();
            }

            return Vector4.Normalize(vector);
        }

        /// <summary>
        /// Normalizes a 2D vector. This method uses SIMD instructions if supported;
        /// otherwise, it falls back to the standard non-SIMD normalization method.
        /// </summary>
        /// <param name="vector">The 2D vector to normalize.</param>
        /// <returns>The normalized 2D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalize(Vector2 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                Vector128<float> vLengthSq = Sse41.DotProduct(vec, vec, 0x3f);
                // Prepare for the division
                Vector128<float> vResult = Sse.Sqrt(vLengthSq);
                // Create zero with a single instruction
                Vector128<float> vZeroMask = Vector128<float>.Zero;
                // Test for a divide by zero (Must be FP to detect -0.0)
                vZeroMask = Sse.CompareNotEqual(vZeroMask, vResult);
                // Failsafe on zero (Or epsilon) length planes
                // If the length is infinity, set the elements to zero
                vLengthSq = Sse.CompareNotEqual(vLengthSq, Infinity.AsVector128());
                // Divide to perform the normalization
                vResult = Sse.Divide(vec, vResult);
                // Any that are infinity, set to zero
                vResult = Sse.And(vResult, vZeroMask);
                // Select qnan or result based on infinite length
                Vector128<float> vTemp1 = Sse.AndNot(vLengthSq, QNaN.AsVector128());
                Vector128<float> vTemp2 = Sse.And(vResult, vLengthSq);
                vResult = Sse.Or(vTemp1, vTemp2);
                return vResult.AsVector2();
            }

            return Vector2.Normalize(vector);
        }

        /// <summary>
        /// Normalizes a 3D vector. This method uses SIMD instructions if supported;
        /// otherwise, it falls back to the standard non-SIMD normalization method.
        /// </summary>
        /// <param name="vector">The 3D vector to normalize.</param>
        /// <returns>The normalized 3D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                Vector128<float> vLengthSq = Sse41.DotProduct(vec, vec, 0x7f);
                // Prepare for the division
                Vector128<float> vResult = Sse.Sqrt(vLengthSq);
                // Create zero with a single instruction
                Vector128<float> vZeroMask = Vector128<float>.Zero;
                // Test for a divide by zero (Must be FP to detect -0.0)
                vZeroMask = Sse.CompareNotEqual(vZeroMask, vResult);
                // Failsafe on zero (Or epsilon) length planes
                // If the length is infinity, set the elements to zero
                vLengthSq = Sse.CompareNotEqual(vLengthSq, Infinity.AsVector128());
                // Divide to perform the normalization
                vResult = Sse.Divide(vec, vResult);
                // Any that are infinity, set to zero
                vResult = Sse.And(vResult, vZeroMask);
                // Select qnan or result based on infinite length
                Vector128<float> vTemp1 = Sse.AndNot(vLengthSq, QNaN.AsVector128());
                Vector128<float> vTemp2 = Sse.And(vResult, vLengthSq);
                vResult = Sse.Or(vTemp1, vTemp2);
                return vResult.AsVector3();
            }

            return Vector3.Normalize(vector);
        }

        /// <summary>
        /// Normalizes a 4D vector. This method uses SIMD instructions if supported;
        /// otherwise, it falls back to the standard non-SIMD normalization method.
        /// </summary>
        /// <param name="vector">The 4D vector to normalize.</param>
        /// <returns>The normalized 4D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Normalize(Vector4 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                Vector128<float> vLengthSq = Sse41.DotProduct(vec, vec, 0xFF);
                // Prepare for the division
                Vector128<float> vResult = Sse.Sqrt(vLengthSq);
                // Create zero with a single instruction
                Vector128<float> vZeroMask = Vector128<float>.Zero;
                // Test for a divide by zero (Must be FP to detect -0.0)
                vZeroMask = Sse.CompareNotEqual(vZeroMask, vResult);
                // Failsafe on zero (Or epsilon) length planes
                // If the length is infinity, set the elements to zero
                vLengthSq = Sse.CompareNotEqual(vLengthSq, Infinity.AsVector128());
                // Divide to perform the normalization
                vResult = Sse.Divide(vec, vResult);
                // Any that are infinity, set to zero
                vResult = Sse.And(vResult, vZeroMask);
                // Select qnan or result based on infinite length
                Vector128<float> vTemp1 = Sse.AndNot(vLengthSq, QNaN.AsVector128());
                Vector128<float> vTemp2 = Sse.And(vResult, vLengthSq);
                vResult = Sse.Or(vTemp1, vTemp2);
                return vResult.AsVector4();
            }

            return Vector4.Normalize(vector);
        }

        /// <summary>
        /// Calculates the squared length of a 2D vector. This method uses SIMD instructions
        /// if supported; otherwise, it falls back to the standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 2D vector.</param>
        /// <returns>The squared length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LengthSquared(Vector2 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0x3f);
                return vResult.ToScalar();
            }

            return vector.LengthSquared();
        }

        /// <summary>
        /// Calculates the squared length of a 3D vector. This method uses SIMD instructions
        /// if supported; otherwise, it falls back to the standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 3D vector.</param>
        /// <returns>The squared length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LengthSquared(Vector3 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0x7f);
                return vResult.ToScalar();
            }

            return vector.LengthSquared();
        }

        /// <summary>
        /// Calculates the squared length of a 4D vector. This method uses SIMD instructions
        /// if supported; otherwise, it falls back to the standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 4D vector.</param>
        /// <returns>The squared length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LengthSquared(Vector4 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0xff);
                return vResult.ToScalar();
            }

            return vector.LengthSquared();
        }

        /// <summary>
        /// Calculates the length of a 2D vector. This method uses SIMD instructions if
        /// supported; otherwise, it falls back to the standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 2D vector.</param>
        /// <returns>The length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Length(Vector2 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0x3f);
                vResult = Sse.Sqrt(vResult);
                return vResult.ToScalar();
            }

            return vector.Length();
        }

        /// <summary>
        /// Calculates the length of a 3D vector. This method uses SIMD instructions if
        /// supported; otherwise, it falls back to the standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 3D vector.</param>
        /// <returns>The length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Length(Vector3 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0x7f);
                vResult = Sse.Sqrt(vResult);
                return vResult.ToScalar();
            }

            return vector.LengthSquared();
        }

        /// <summary>
        /// Calculates the length of a 4D vector. This method uses SIMD instructions if
        /// supported; otherwise, it falls back to the standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 4D vector.</param>
        /// <returns>The length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Length(Vector4 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0xff);
                vResult = Sse.Sqrt(vResult);
                return vResult.ToScalar();
            }

            return vector.LengthSquared();
        }

        /// <summary>
        /// Estimates the length of a 2D vector using a fast reciprocal square root approximation.
        /// This method utilizes SIMD instructions if supported; otherwise, it falls back to the
        /// standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 2D vector.</param>
        /// <returns>The estimated length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LengthEst(Vector2 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0x3f);
                vResult = Sse.ReciprocalSqrt(vResult);
                return vResult.ToScalar();
            }

            return vector.Length();
        }

        /// <summary>
        /// Estimates the length of a 3D vector using a fast reciprocal square root approximation.
        /// This method utilizes SIMD instructions if supported; otherwise, it falls back to the
        /// standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 3D vector.</param>
        /// <returns>The estimated length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LengthEst(Vector3 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0x7f);
                vResult = Sse.ReciprocalSqrt(vResult);
                return vResult.ToScalar();
            }

            return vector.LengthSquared();
        }

        /// <summary>
        /// Estimates the length of a 4D vector using a fast reciprocal square root approximation.
        /// This method utilizes SIMD instructions if supported; otherwise, it falls back to the
        /// standard non-SIMD method.
        /// </summary>
        /// <param name="vector">The 4D vector.</param>
        /// <returns>The estimated length of the vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LengthEst(Vector4 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                var vResult = Sse41.DotProduct(vec, vec, 0xff);
                vResult = Sse.ReciprocalSqrt(vResult);
                return vResult.ToScalar();
            }

            return vector.LengthSquared();
        }

        /// <summary>
        /// Performs linear interpolation between two values.
        /// </summary>
        /// <param name="x">The first value.</param>
        /// <param name="y">The second value.</param>
        /// <param name="s">The interpolation factor (0 to 1).</param>
        /// <returns>The interpolated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float x, float y, float s)
        {
            return x * (1 - s) + y * s;
        }

        /// <summary>
        /// Linearly interpolates between two 2D vectors.
        /// </summary>
        /// <param name="x">The starting vector.</param>
        /// <param name="y">The ending vector.</param>
        /// <param name="s">The interpolation factor. Should be in the range [0, 1].</param>
        /// <returns>The interpolated 2D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Lerp(Vector2 x, Vector2 y, float s)
        {
            return Vector2.Lerp(x, y, s);
        }

        /// <summary>
        /// Linearly interpolates between two 3D vectors.
        /// </summary>
        /// <param name="x">The starting vector.</param>
        /// <param name="y">The ending vector.</param>
        /// <param name="s">The interpolation factor. Should be in the range [0, 1].</param>
        /// <returns>The interpolated 3D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(Vector3 x, Vector3 y, float s)
        {
            return Vector3.Lerp(x, y, s);
        }

        /// <summary>
        /// Linearly interpolates between two 4D vectors.
        /// </summary>
        /// <param name="x">The starting vector.</param>
        /// <param name="y">The ending vector.</param>
        /// <param name="s">The interpolation factor. Should be in the range [0, 1].</param>
        /// <returns>The interpolated 4D vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Lerp(Vector4 x, Vector4 y, float s)
        {
            return Vector4.Lerp(x, y, s);
        }

        /// <summary>
        /// Linearly interpolates between two quaternions.
        /// </summary>
        /// <param name="x">The starting quaternion.</param>
        /// <param name="y">The ending quaternion.</param>
        /// <param name="s">The interpolation factor. Should be in the range [0, 1].</param>
        /// <returns>The interpolated quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Lerp(Quaternion x, Quaternion y, float s)
        {
            return Quaternion.Lerp(x, y, s);
        }

        /// <summary>
        /// Spherically interpolates between two quaternions.
        /// </summary>
        /// <param name="x">The starting quaternion.</param>
        /// <param name="y">The ending quaternion.</param>
        /// <param name="s">The interpolation factor. Should be in the range [0, 1].</param>
        /// <returns>The spherical interpolated quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Slerp(Quaternion x, Quaternion y, float s)
        {
            return Quaternion.Slerp(x, y, s);
        }

        /// <summary>
        /// Packs an ARGB color into a single 32-bit unsigned integer.
        /// </summary>
        /// <param name="color">The color as a Vector4 (values in the range [0, 1]).</param>
        /// <returns>The packed ARGB color as a 32-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackARGB(this Vector4 color)
        {
            return PackARGB((uint)(color.W * 255), (uint)(color.X * 255), (uint)(color.Y * 255), (uint)(color.Z * 255));
        }

        /// <summary>
        /// Packs individual ARGB color components into a single 32-bit unsigned integer.
        /// </summary>
        /// <param name="a">The alpha component (0 to 255).</param>
        /// <param name="r">The red component (0 to 255).</param>
        /// <param name="g">The green component (0 to 255).</param>
        /// <param name="b">The blue component (0 to 255).</param>
        /// <returns>The packed ARGB color as a 32-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackARGB(uint a, uint r, uint g, uint b)
        {
            return (a << 24) + (r << 16) + (g << 8) + b;
        }

        /// <summary>
        /// Computes yaw, pitch, and roll angles from a normal vector.
        /// </summary>
        /// <param name="normal">The input normal vector.</param>
        /// <param name="yaw">The resulting yaw angle (in radians).</param>
        /// <param name="pitch">The resulting pitch angle (in radians).</param>
        /// <param name="roll">The resulting roll angle (in radians).</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AnglesFromNormal(Vector3 normal, out float yaw, out float pitch, out float roll)
        {
            yaw = MathF.Atan2(normal.X, normal.Z);
            pitch = MathF.Asin(-normal.Y);
            roll = 0;
        }

        /// <summary>
        /// Decomposes a given double-precision floating-point value into a normalized fraction (mantissa) and modifies an integral power of two (exponent).
        /// </summary>
        /// <param name="value">The input double-precision floating-point number.</param>
        /// <param name="exponent">An integer representing the integral power of two, modified by the function.</param>
        /// <returns>
        /// The normalized fraction (mantissa) of the input value.
        /// </returns>
        public static double Frexp(double value, ref int exponent)
        {
            if (value == 0)
            {
                exponent = 0;
                return 0;
            }

            long bits = BitConverter.DoubleToInt64Bits(value);
            exponent = (int)((bits >> 52) & 0x7FF) - 1022;
            long mantissa = (bits & 0xFFFFFFFFFFFFF) | 0x10000000000000; // Set the hidden bit

            double resultMantissa = BitConverter.Int64BitsToDouble(mantissa);
            return resultMantissa;
        }

        /// <summary>
        /// Decomposes a given floating-point value into a normalized fraction (mantissa) and modifies an integral power of two (exponent).
        /// </summary>
        /// <param name="value">The input floating-point number.</param>
        /// <param name="exponent">An integer representing the integral power of two, modified by the function.</param>
        /// <returns>
        /// The normalized fraction (mantissa) of the input value.
        /// </returns>
        public static float Frexp(float value, ref int exponent)
        {
            if (value == 0)
            {
                exponent = 0;
                return 0;
            }

            int bits = BitConverter.SingleToInt32Bits(value);
            exponent = ((bits >> 23) & 0xFF) - 126;
            int mantissa = (bits & 0x7FFFFF) | 0x800000; // Set the hidden bit

            float resultMantissa = BitConverter.Int32BitsToSingle(mantissa);
            return resultMantissa;
        }

        /// <summary>
        /// Builds a floating-point number from a normalized fraction (mantissa) and an integral power of two (exponent).
        /// </summary>
        /// <param name="x">The normalized fraction (mantissa).</param>
        /// <param name="exp">The integral power of two (exponent).</param>
        /// <returns>
        /// The floating-point number built from the given mantissa and exponent.
        /// </returns>
        public static double Ldexp(double x, int exp)
        {
            // Special case for zero input
            if (x == 0)
            {
                return 0;
            }

            // Extract sign and exponent from the IEEE 754 representation
            long bits = BitConverter.DoubleToInt64Bits(x);
            int sign = (int)((bits >> 63) & 1);
            int oldExp = (int)((bits >> 52) & 0x7FF) - 1022;

            // Calculate new exponent and set it in the IEEE 754 representation
            int newExp = oldExp + exp;
            long resultBits = ((long)sign << 63) | ((long)(newExp + 1022) << 52) | (bits & 0xFFFFFFFFFFFFF);

            // Reconstruct the double with the modified exponent
            return BitConverter.Int64BitsToDouble(resultBits);
        }

        /// <summary>
        /// Builds a floating-point number from a normalized fraction (mantissa) and an integral power of two (exponent).
        /// </summary>
        /// <param name="x">The normalized fraction (mantissa).</param>
        /// <param name="exp">The integral power of two (exponent).</param>
        /// <returns>
        /// The floating-point number built from the given mantissa and exponent.
        /// </returns>
        public static float Ldexp(float x, int exp)
        {
            // Special case for zero input
            if (x == 0)
            {
                return 0;
            }

            // Extract sign and exponent from the IEEE 754 representation
            int bits = BitConverter.SingleToInt32Bits(x);
            int sign = (bits >> 31) & 1;
            int oldExp = ((bits >> 23) & 0xFF) - 127;

            // Calculate new exponent and set it in the IEEE 754 representation
            int newExp = oldExp + exp;
            int resultBits = ((sign & 1) << 31) | ((newExp + 127) << 23) | (bits & 0x7FFFFF);

            // Reconstruct the float with the modified exponent
            return BitConverter.Int32BitsToSingle(resultBits);
        }

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Clamp(sbyte value, sbyte min, sbyte max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(byte value, byte min, byte max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Clamp(ushort value, ushort min, ushort max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Clamp(short value, short min, short max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(uint value, uint min, uint max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Clamp(ulong value, ulong min, ulong max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long value, long min, long max)
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

        /// <summary>
        /// Clamps a value to a specified range.
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value within the specified range.</returns>
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

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Clamp01(sbyte value)
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

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp01(byte value)
        {
            if (value > 1)
            {
                return 1;
            }

            return value;
        }

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Clamp01(short value)
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

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Clamp01(ushort value)
        {
            if (value > 1)
            {
                return 1;
            }

            return value;
        }

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp01(int value)
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

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp01(uint value)
        {
            if (value > 1)
            {
                return 1;
            }

            return value;
        }

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp01(long value)
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

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Clamp01(ulong value)
        {
            if (value > 1)
            {
                return 1;
            }

            return value;
        }

        /// <summary>
        /// Clamps a value to the range [0, 1].
        /// </summary>
        /// <param name="value">The input value to be clamped.</param>
        /// <returns>The clamped value within the range [0, 1].</returns>
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

        /// <summary>
        /// Clamps each component of a <see cref="Vector2"/> to a specified range.
        /// </summary>
        /// <param name="v">The input <see cref="Vector2"/> to be clamped.</param>
        /// <param name="min">The minimum <see cref="Vector2"/> of the range.</param>
        /// <param name="max">The maximum <see cref="Vector2"/> of the range.</param>
        /// <returns>The <see cref="Vector2"/> with components clamped within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max)
        {
            return Vector2.Clamp(v, min, max);
        }

        /// <summary>
        /// Clamps each component of a <see cref="Vector3"/> to a specified range.
        /// </summary>
        /// <param name="v">The input <see cref="Vector3"/> to be clamped.</param>
        /// <param name="min">The minimum <see cref="Vector3"/> of the range.</param>
        /// <param name="max">The maximum <see cref="Vector3"/> of the range.</param>
        /// <returns>The <see cref="Vector3"/> with components clamped within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(Vector3 v, Vector3 min, Vector3 max)
        {
            return Vector3.Clamp(v, min, max);
        }

        /// <summary>
        /// Clamps each component of a <see cref="Vector4"/> to a specified range.
        /// </summary>
        /// <param name="v">The input <see cref="Vector4"/> to be clamped.</param>
        /// <param name="min">The minimum <see cref="Vector4"/> of the range.</param>
        /// <param name="max">The maximum <see cref="Vector4"/> of the range.</param>
        /// <returns>The <see cref="Vector4"/> with components clamped within the specified range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Clamp(Vector4 v, Vector4 min, Vector4 max)
        {
            return Vector4.Clamp(v, min, max);
        }

        /// <summary>
        /// Clamps each component of a <see cref="Vector2"/> to the range [0, 1].
        /// </summary>
        /// <param name="v">The input <see cref="Vector2"/> to be clamped.</param>
        /// <returns>The <see cref="Vector2"/> with components clamped within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp01(Vector2 v)
        {
            return Vector2.Clamp(v, Vector2.Zero, Vector2.One);
        }

        /// <summary>
        /// Clamps each component of a <see cref="Vector3"/> to the range [0, 1].
        /// </summary>
        /// <param name="v">The input <see cref="Vector3"/> to be clamped.</param>
        /// <returns>The <see cref="Vector3"/> with components clamped within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp01(Vector3 v)
        {
            return Vector3.Clamp(v, Vector3.Zero, Vector3.One);
        }

        /// <summary>
        /// Clamps each component of a <see cref="Vector4"/> to the range [0, 1].
        /// </summary>
        /// <param name="v">The input <see cref="Vector4"/> to be clamped.</param>
        /// <returns>The <see cref="Vector4"/> with components clamped within the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Clamp01(Vector4 v)
        {
            return Vector4.Clamp(v, Vector4.Zero, Vector4.One);
        }

        /// <summary>
        /// Floors each component of a <see cref="Vector2"/> to the nearest lower integer.
        /// </summary>
        /// <param name="vector">The input <see cref="Vector2"/>.</param>
        /// <returns>The <see cref="Vector2"/> with components floored to the nearest lower integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Floor(this Vector2 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                vec = Sse41.Floor(vec);
                return vec.AsVector2();
            }

            return new(MathF.Floor(vector.X), MathF.Floor(vector.Y));
        }

        /// <summary>
        /// Floors each component of a <see cref="Vector3"/> to the nearest lower integer.
        /// </summary>
        /// <param name="vector">The input <see cref="Vector3"/>.</param>
        /// <returns>The <see cref="Vector3"/> with components floored to the nearest lower integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(this Vector3 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                vec = Sse41.Floor(vec);
                return vec.AsVector3();
            }

            return new(MathF.Floor(vector.X), MathF.Floor(vector.Y), MathF.Floor(vector.Z));
        }

        /// <summary>
        /// Floors each component of a <see cref="Vector4"/> to the nearest lower integer.
        /// </summary>
        /// <param name="vector">The input <see cref="Vector4"/>.</param>
        /// <returns>The <see cref="Vector4"/> with components floored to the nearest lower integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Floor(this Vector4 vector)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = vector.AsVector128();
                vec = Sse41.Floor(vec);
                return vec.AsVector4();
            }

            return new(MathF.Floor(vector.X), MathF.Floor(vector.Y), MathF.Floor(vector.Z), MathF.Floor(vector.W));
        }

        /// <summary>
        /// Rounds each component of a <see cref="Vector2"/> to the nearest higher integer.
        /// </summary>
        /// <param name="value">The input <see cref="Vector2"/>.</param>
        /// <returns>The <see cref="Vector2"/> with components rounded to the nearest higher integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Ceiling(this Vector2 value)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = value.AsVector128();
                vec = Sse41.Ceiling(vec);
                return vec.AsVector2();
            }

            return new Vector2(MathF.Ceiling(value.X), MathF.Ceiling(value.Y));
        }

        /// <summary>
        /// Rounds each component of a <see cref="Vector3"/> to the nearest higher integer.
        /// </summary>
        /// <param name="value">The input <see cref="Vector3"/>.</param>
        /// <returns>The <see cref="Vector3"/> with components rounded to the nearest higher integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Ceiling(this Vector3 value)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = value.AsVector128();
                vec = Sse41.Ceiling(vec);
                return vec.AsVector3();
            }

            return new Vector3(MathF.Ceiling(value.X), MathF.Ceiling(value.Y), MathF.Ceiling(value.Z));
        }

        /// <summary>
        /// Rounds each component of a <see cref="Vector4"/> to the nearest higher integer.
        /// </summary>
        /// <param name="value">The input <see cref="Vector4"/>.</param>
        /// <returns>The <see cref="Vector4"/> with components rounded to the nearest higher integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Ceiling(this Vector4 value)
        {
            if (Sse41.IsSupported)
            {
                Vector128<float> vec = value.AsVector128();
                vec = Sse41.Ceiling(vec);
                return vec.AsVector4();
            }

            return new Vector4(MathF.Ceiling(value.X), MathF.Ceiling(value.Y), MathF.Ceiling(value.Z), MathF.Ceiling(value.W));
        }

        /// <summary>
        /// Calculates the element-wise power of a <see cref="Vector2"/> to the given exponent.
        /// </summary>
        /// <param name="a">The input <see cref="Vector2"/>.</param>
        /// <param name="b">The exponent to which the components are raised.</param>
        /// <returns>The result of raising each component of 'a' to the power of 'b'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Pow(Vector2 a, float b)
        {
            return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b));
        }

        /// <summary>
        /// Calculates the element-wise power of a <see cref="Vector3"/> to the given exponent.
        /// </summary>
        /// <param name="a">The input <see cref="Vector3"/>.</param>
        /// <param name="b">The exponent to which the components are raised.</param>
        /// <returns>The result of raising each component of 'a' to the power of 'b'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Pow(Vector3 a, float b)
        {
            return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b), MathF.Pow(a.Z, b));
        }

        /// <summary>
        /// Calculates the element-wise power of a <see cref="Vector4"/> to the given exponent.
        /// </summary>
        /// <param name="a">The input <see cref="Vector4"/>.</param>
        /// <param name="b">The exponent to which the components are raised.</param>
        /// <returns>The result of raising each component of 'a' to the power of 'b'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Pow(Vector4 a, float b)
        {
            return new(MathF.Pow(a.X, b), MathF.Pow(a.Y, b), MathF.Pow(a.Z, b), MathF.Pow(a.W, b));
        }

        /// <summary>
        /// Calculates the element-wise power of a <see cref="Vector2"/> to the given exponent.
        /// </summary>
        /// <param name="a">The input <see cref="Vector2"/>.</param>
        /// <param name="b">The <see cref="Vector2"/> of exponents for each component.</param>
        /// <returns>The result of raising each component of 'a' to the corresponding component in 'b'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Pow(Vector2 a, Vector2 b)
        {
            return new(MathF.Pow(a.X, b.X), MathF.Pow(a.Y, b.Y));
        }

        /// <summary>
        /// Calculates the element-wise power of a <see cref="Vector3"/> to the given exponent.
        /// </summary>
        /// <param name="a">The input <see cref="Vector3"/>.</param>
        /// <param name="b">The <see cref="Vector3"/> of exponents for each component.</param>
        /// <returns>The result of raising each component of 'a' to the corresponding component in 'b'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Pow(Vector3 a, Vector3 b)
        {
            return new(MathF.Pow(a.X, b.X), MathF.Pow(a.Y, b.Y), MathF.Pow(a.Z, b.Z));
        }

        /// <summary>
        /// Calculates the element-wise power of a <see cref="Vector4"/> to the given exponent.
        /// </summary>
        /// <param name="a">The input <see cref="Vector4"/>.</param>
        /// <param name="b">The <see cref="Vector4"/> of exponents for each component.</param>
        /// <returns>The result of raising each component of 'a' to the corresponding component in 'b'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Pow(Vector4 a, Vector4 b)
        {
            return new(MathF.Pow(a.X, b.X), MathF.Pow(a.Y, b.Y), MathF.Pow(a.Z, b.Z), MathF.Pow(a.W, b.W));
        }

        /// <summary>
        /// Calculates the exponential function (e^x) for a <see cref="float"/>.
        /// </summary>
        /// <param name="value">The input <see cref="float"/>.</param>
        /// <returns>The result of the exponential function for 'value'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp(float value)
        {
            return MathF.Exp(value);
        }

        /// <summary>
        /// Calculates the element-wise exponential function (e^x) for a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="a">The input <see cref="Vector2"/>.</param>
        /// <returns>The result of the exponential function for each component of 'a'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Exp(Vector2 a)
        {
            return new(MathF.Exp(a.X), MathF.Exp(a.Y));
        }

        /// <summary>
        /// Calculates the element-wise exponential function (e^x) for a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="a">The input <see cref="Vector3"/>.</param>
        /// <returns>The result of the exponential function for each component of 'a'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Exp(Vector3 a)
        {
            return new(MathF.Exp(a.X), MathF.Exp(a.Y), MathF.Exp(a.Z));
        }

        /// <summary>
        /// Calculates the element-wise exponential function (e^x) for a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="a">The input <see cref="Vector4"/>.</param>
        /// <returns>The result of the exponential function for each component of 'a'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Exp(Vector4 a)
        {
            return new(MathF.Exp(a.X), MathF.Exp(a.Y), MathF.Exp(a.Z), MathF.Exp(a.W));
        }

        /// <summary>
        /// Calculates 2^x for a float value.
        /// </summary>
        /// <param name="x">The input float value.</param>
        /// <returns>The result of 2^x.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp2(float x)
        {
            return BitConverter.Int32BitsToSingle(((int)(x * 0x00800000 + 126.0f)) << 23);
        }

        /// <summary>
        /// Calculates 2^x for each component of a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="a">The input <see cref="Vector2"/>.</param>
        /// <returns>The result of 2^x for each component of 'a'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Exp2(Vector2 a)
        {
            return new(Exp2(a.X), Exp2(a.Y));
        }

        /// <summary>
        /// Calculates 2^x for each component of a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="a">The input <see cref="Vector3"/>.</param>
        /// <returns>The result of 2^x for each component of 'a'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Exp2(Vector3 a)
        {
            return new(Exp2(a.X), Exp2(a.Y), Exp2(a.Z));
        }

        /// <summary>
        /// Calculates 2^x for each component of a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="a">The input <see cref="Vector4"/>.</param>
        /// <returns>The result of 2^x for each component of 'a'.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Exp2(Vector4 a)
        {
            return new(Exp2(a.X), Exp2(a.Y), Exp2(a.Z), Exp2(a.W));
        }

        /// <summary>
        /// Maps a value in the range [0, 1] to the range [-1, 1] by scaling and shifting.
        /// </summary>
        /// <param name="value">The input value in the range [0, 1].</param>
        /// <returns>The mapped value in the range [-1, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map01ToN1P1(this float value)
        {
            return 2 * value - 1;
        }

        /// <summary>
        /// Maps a value in the range [-1, 1] to the range [0, 1] by scaling and shifting.
        /// </summary>
        /// <param name="value">The input value in the range [-1, 1].</param>
        /// <returns>The mapped value in the range [0, 1].</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MapN1P1To01(this float value)
        {
            return value * 0.5f + 0.5f;
        }

        /// <summary>
        /// Determines if two single-precision floating-point values are approximately equal within a specified tolerance.
        /// </summary>
        /// <param name="v1">The first value to compare.</param>
        /// <param name="v2">The second value to compare.</param>
        /// <param name="epsilon">The tolerance within which the values are considered equal.</param>
        /// <returns><c>true</c> if the values are approximately equal within the specified tolerance; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NearEqual(float v1, float v2, float epsilon)
        {
            float delta = MathF.Abs(v1 - v2);
            return delta <= epsilon;
        }

        /// <summary>
        /// Determines if two <see cref="Vector2"/> instances are approximately equal component-wise within a specified tolerance.
        /// </summary>
        /// <param name="v1">The first <see cref="Vector2"/> to compare.</param>
        /// <param name="v2">The second <see cref="Vector2"/> to compare.</param>
        /// <param name="epsilon">The tolerance within which the components are considered equal.</param>
        /// <returns><c>true</c> if the components of the <see cref="Vector2"/> instances are approximately equal within the specified tolerance; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Determines if two <see cref="Vector3"/> instances are approximately equal component-wise within a specified tolerance.
        /// </summary>
        /// <param name="v1">The first <see cref="Vector3"/> to compare.</param>
        /// <param name="v2">The second <see cref="Vector3"/> to compare.</param>
        /// <param name="epsilon">The tolerance within which the components are considered equal.</param>
        /// <returns><c>true</c> if the components of the <see cref="Vector3"/> instances are approximately equal within the specified tolerance; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Determines if two <see cref="Vector4"/> instances are approximately equal component-wise within a specified tolerance.
        /// </summary>
        /// <param name="v1">The first <see cref="Vector4"/> to compare.</param>
        /// <param name="v2">The second <see cref="Vector4"/> to compare.</param>
        /// <param name="epsilon">The tolerance within which the components are considered equal.</param>
        /// <returns><c>true</c> if the components of the <see cref="Vector4"/> instances are approximately equal within the specified tolerance; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Rounds the components of a <see cref="Vector2"/> to the nearest integer values.
        /// </summary>
        /// <param name="v">The vector to round.</param>
        /// <returns>A new <see cref="Vector2"/> with rounded components.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Round(Vector2 v)
        {
            return new Vector2(MathF.Round(v.X), MathF.Round(v.Y));
        }

        /// <summary>
        /// Rounds the components of a <see cref="Vector3"/> to the nearest integer values.
        /// </summary>
        /// <param name="v">The vector to round.</param>
        /// <returns>A new <see cref="Vector3"/> with rounded components.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Round(Vector3 v)
        {
            return new Vector3(MathF.Round(v.X), MathF.Round(v.Y), MathF.Round(v.Z));
        }

        /// <summary>
        /// Rounds the components of a <see cref="Vector4"/> to the nearest integer values.
        /// </summary>
        /// <param name="v">The vector to round.</param>
        /// <returns>A new <see cref="Vector4"/> with rounded components.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Round(Vector4 v)
        {
            return new Vector4(MathF.Round(v.X), MathF.Round(v.Y), MathF.Round(v.Z), MathF.Round(v.W));
        }

        /// <summary>
        /// Calculates x^2 or x * x for the <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>x * x</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqr(float value)
        {
            return value * value;
        }

        /// <summary>
        /// Calculates x^2 or x * x for each components of a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>x * x</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Sqr(Vector2 value)
        {
            return value * value;
        }

        /// <summary>
        /// Calculates x^2 or x * x for each components of a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>x * x</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sqr(Vector3 value)
        {
            return value * value;
        }

        /// <summary>
        /// Calculates x^2 or x * x for each components of a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>x * x</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Sqr(Vector4 value)
        {
            return value * value;
        }

        /// <summary>
        /// Converts a Vector4 to a Vector2.
        /// </summary>
        /// <param name="vector">The Vector4 to convert.</param>
        /// <returns>A new Vector2 with X and Y components from the input Vector4.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVec2(this Vector4 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <summary>
        /// Converts a Vector4 to a Vector3.
        /// </summary>
        /// <param name="vector">The Vector4 to convert.</param>
        /// <returns>A new Vector3 with X, Y, and Z components from the input Vector4.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVec3(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Converts a Vector3 to a Vector2.
        /// </summary>
        /// <param name="vector">The Vector3 to convert.</param>
        /// <returns>A new Vector2 with X and Y components from the input Vector3.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVec2(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        /// <summary>
        /// Determines whether the specified signed byte is a prime number.
        /// </summary>
        /// <param name="number">The signed byte to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(sbyte number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Determines whether the specified byte is a prime number.
        /// </summary>
        /// <param name="number">The byte to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(byte number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Determines whether the specified short integer is a prime number.
        /// </summary>
        /// <param name="number">The short integer to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(short number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Determines whether the specified unsigned short integer is a prime number.
        /// </summary>
        /// <param name="number">The unsigned short integer to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(ushort number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Determines whether the specified integer is a prime number.
        /// </summary>
        /// <param name="number">The integer to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(int number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Determines whether the specified unsigned integer is a prime number.
        /// </summary>
        /// <param name="number">The unsigned integer to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(uint number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Determines whether the specified long integer is a prime number.
        /// </summary>
        /// <param name="number">The long integer to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(long number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Determines whether the specified unsigned long integer is a prime number.
        /// </summary>
        /// <param name="number">The unsigned long integer to check for primality.</param>
        /// <returns><c>true</c> if the specified number is prime; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(ulong number)
        {
            return PrimeNumberCache.IsPrime(number);
        }

        /// <summary>
        /// Calculates the greatest common divisor (GCD) of two signed integers.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns>The GCD of the two integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Calculates the greatest common divisor (GCD) of two unsigned integers.
        /// </summary>
        /// <param name="a">The first unsigned integer.</param>
        /// <param name="b">The second unsigned integer.</param>
        /// <returns>The GCD of the two unsigned integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GCD(uint a, uint b)
        {
            while (b != 0)
            {
                uint temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Calculates the greatest common divisor (GCD) of two long integers.
        /// </summary>
        /// <param name="a">The first long integer.</param>
        /// <param name="b">The second long integer.</param>
        /// <returns>The GCD of the two long integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GCD(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Calculates the greatest common divisor (GCD) of two unsigned long integers.
        /// </summary>
        /// <param name="a">The first unsigned long integer.</param>
        /// <param name="b">The second unsigned long integer.</param>
        /// <returns>The GCD of the two unsigned long integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GCD(ulong a, ulong b)
        {
            while (b != 0)
            {
                ulong temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Calculates the lowest common divisor (LDC) of two signed integers.
        /// </summary>
        /// <param name="a">The first integer.</param>
        /// <param name="b">The second integer.</param>
        /// <returns>The LDC of the two integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LDC(int a, int b)
        {
            int gcd = GCD(a, b);
            return gcd != 0 ? Math.Abs(a * b) / gcd : 0;
        }

        /// <summary>
        /// Calculates the lowest common divisor (LDC) of two unsigned integers.
        /// </summary>
        /// <param name="a">The first unsigned integer.</param>
        /// <param name="b">The second unsigned integer.</param>
        /// <returns>The LDC of the two unsigned integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LDC(uint a, uint b)
        {
            uint gcd = GCD(a, b);
            return gcd != 0 ? a * b / gcd : 0;
        }

        /// <summary>
        /// Calculates the lowest common divisor (LDC) of two long integers.
        /// </summary>
        /// <param name="a">The first long integer.</param>
        /// <param name="b">The second long integer.</param>
        /// <returns>The LDC of the two long integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long LDC(long a, long b)
        {
            long gcd = GCD(a, b);
            return gcd != 0 ? Math.Abs(a * b) / gcd : 0;
        }

        /// <summary>
        /// Calculates the lowest common divisor (LDC) of two unsigned long integers.
        /// </summary>
        /// <param name="a">The first unsigned long integer.</param>
        /// <param name="b">The second unsigned long integer.</param>
        /// <returns>The LDC of the two unsigned long integers.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong LDC(ulong a, ulong b)
        {
            ulong gcd = GCD(a, b);
            return gcd != 0 ? a * b / gcd : 0;
        }

        /// <summary>
        /// Checks if all components are equal.
        /// </summary>
        /// <param name="vec">The <see cref="Vector2"/>.</param>
        /// <returns>returns <c>true</c> if all components of the <see cref="Vector2"/> are equals, otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ComponentsEquals(this Vector2 vec)
        {
            return vec.X == vec.Y;
        }

        /// <summary>
        /// Checks if all components are equal.
        /// </summary>
        /// <param name="vec">The <see cref="Vector3"/>.</param>
        /// <returns>returns <c>true</c> if all components of the <see cref="Vector3"/> are equals, otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ComponentsEquals(this Vector3 vec)
        {
            return vec.X == vec.Y && vec.X == vec.Z;
        }

        /// <summary>
        /// Checks if all components are equal.
        /// </summary>
        /// <param name="vec">The <see cref="Vector4"/>.</param>
        /// <returns>returns <c>true</c> if all components of the <see cref="Vector4"/> are equals, otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ComponentsEquals(this Vector4 vec)
        {
            return vec.X == vec.Y && vec.X == vec.Z && vec.X == vec.W;
        }
    }
}