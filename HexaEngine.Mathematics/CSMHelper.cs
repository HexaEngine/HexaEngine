namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Represents an enumeration of modes for splitting cascades in a shadow-casting scene.
    /// </summary>
    public enum CascadesSplitMode
    {
        /// <summary>
        /// Fixed mode uses predefined depth splits, offering complete control but may require manual adjustments.
        /// </summary>
        Fixed,

        /// <summary>
        /// Linear mode evenly distributes the entire depth range between near and far clipping planes into cascades.
        /// </summary>
        Linear,

        /// <summary>
        /// Log mode uses a logarithmic function to calculate cascade splits, providing better depth perception.
        /// </summary>
        Log
    }

    /// <summary>
    /// Helper class for working with Cascaded Shadow Mapping (CSM).
    /// </summary>
    public static class CSMHelper
    {
        /// <summary>
        /// Gets the world space coordinates of the frustum corners using the given projection-view matrix.
        /// </summary>
        /// <param name="projview">The projection-view matrix.</param>
        /// <returns>An array of world space coordinates for the frustum corners.</returns>
        public static Vector3[] GetFrustumCornersWorldSpace(Matrix4x4 projview)
        {
            Matrix4x4.Invert(projview, out var inv);

            Vector3[] frustumCorners = new Vector3[8];
            int i = 0;
            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        Vector4 pt = Vector4.Transform(new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f), inv);
                        pt /= pt.W;
                        frustumCorners[i] = new(pt.X, pt.Y, pt.Z);
                        i++;
                    }
                }
            }

            return frustumCorners;
        }

        /// <summary>
        /// Gets the world space coordinates of the frustum corners using the given projection-view matrix.
        /// </summary>
        /// <param name="projview">The projection-view matrix.</param>
        /// <param name="frustumCorners">A pointer to an array to store the world space coordinates of the frustum corners. Must be [<see cref="BoundingFrustum.CornerCount"/>] in size.</param>
        public static unsafe void GetFrustumCornersWorldSpace(Matrix4x4 projview, Vector3* frustumCorners)
        {
            Matrix4x4.Invert(projview, out var inv);

            int i = 0;
            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        Vector4 pt = Vector4.Transform(new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f), inv);
                        pt /= pt.W;
                        frustumCorners[i] = new(pt.X, pt.Y, pt.Z);
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the light space matrix for shadow mapping.
        /// </summary>
        /// <param name="cameraViewProjection">The combined view-projection matrix of the camera.</param>
        /// <param name="light">The transform of the light casting shadows.</param>
        /// <param name="lightFrustrum">The bounding frustum of the shadow camera.</param>
        /// <param name="smSize">The size of the shadow map.</param>
        /// <param name="stabilize">Whether to stabilize the shadow map.</param>
        /// <returns>The transposed light space matrix.</returns>
        public static unsafe Matrix4x4 GetLightSpaceMatrix(Matrix4x4 cameraViewProjection, Transform light, ref BoundingFrustum lightFrustrum, float smSize, bool stabilize = true)
        {
            Vector3* corners = stackalloc Vector3[BoundingFrustum.CornerCount];

            // Get the world space corners of the camera frustum
            GetFrustumCornersWorldSpace(cameraViewProjection, corners);

            // Calculate the center of the frustum
            Vector3 center = new(0, 0, 0);
            for (uint i = 0; i < BoundingFrustum.CornerCount; i++)
            {
                center += corners[i];
            }
            center /= BoundingFrustum.CornerCount;

            if (stabilize)
            {
                const float farFactor = 1.5f;
                const float lightDistanceFactor = 4.0f;
                // Compute radius of bounding sphere.
                float radius = 0;
                for (uint i = 0; i < BoundingFrustum.CornerCount; i++)
                {
                    float distance = (corners[i] - center).Length();
                    radius = MathF.Max(radius, distance);
                }
                radius = MathF.Ceiling(radius * 8) / 8;

                // Compute AABB from the bounding sphere.
                Vector3 maxExtents = new(radius);
                Vector3 minExtents = -maxExtents;

                // Compute look-at matrix.
                var lightView = MathUtil.LookAtLH(center, center + light.Forward * lightDistanceFactor * radius, Vector3.UnitY);

                float l = minExtents.X;
                float b = minExtents.Y;
                float n = minExtents.Z - farFactor * radius;
                float r = maxExtents.X;
                float t = maxExtents.Y;
                float f = maxExtents.Z * farFactor;

                // Compute ortho projection matrix.
                Matrix4x4 lightProjection = MathUtil.OrthoOffCenterLH(l, r, b, t, n, f);

                // Calculate center of the shadow volume and convert it to texel coords.
                Matrix4x4 shadowMatrix = lightView * lightProjection;
                Vector4 shadowOrigin = Vector4.Transform(new Vector4(0, 0, 0, 1), shadowMatrix);
                shadowOrigin *= smSize / 2.0f;

                // round texel coords for texel snapping and convert back to world space coords.
                Vector4 roundedOrigin = MathUtil.Round(shadowOrigin);
                Vector4 roundOffset = roundedOrigin - shadowOrigin;
                roundOffset *= 2.0f / smSize;

                // offset the projection matrix for texel snapping.
                Matrix4x4 shadowProjection = lightProjection;
                shadowProjection.M41 += roundOffset.X;
                shadowProjection.M42 += roundOffset.Y;
                lightProjection = shadowProjection;

                // compute the new shadow matrix and update the frustum.
                Matrix4x4 viewProjOut = lightView * lightProjection;
                lightFrustrum.Update(viewProjOut);

                // transpose for convenience, because usually that is the final step before GPU transfer.
                return Matrix4x4.Transpose(viewProjOut);
            }
            else
            {
                // compute look-at matrix.
                var lightView = MathUtil.LookAtLH(center, center + light.Forward, Vector3.UnitY);

                // compute AABB of the shadow volume.
                Vector3 min = new(float.MaxValue);
                Vector3 max = new(float.MinValue);
                for (uint i = 0; i < BoundingFrustum.CornerCount; i++)
                {
                    var trf = Vector3.Transform(corners[i], lightView);
                    min = Vector3.Min(min, trf);
                    max = Vector3.Max(max, trf);
                }

                // compute ortho projection matrix.
                Matrix4x4 lightProjection = MathUtil.OrthoOffCenterLH(min.X, max.X, min.Y, max.Y, min.Z, max.Z);

                // compute the new shadow matrix and update the frustum.
                Matrix4x4 viewProjOut = lightView * lightProjection;
                lightFrustrum.Update(viewProjOut);

                // transpose for convenience, because usually that is the final step before GPU transfer.
                return Matrix4x4.Transpose(viewProjOut);
            }
        }

        /// <summary>
        /// Gets cascades with logarithmic depth splits based on the provided camera transform and user-defined parameters.
        /// </summary>
        /// <param name="camera">The camera transform containing near and far clipping distances.</param>
        /// <param name="result">An array to store the resulting cascade splits.</param>
        /// <param name="count">The number of cascades to generate.</param>
        /// <param name="lambda">A parameter controlling the cascade split distribution (default is 0.85).</param>
        public static unsafe void GetCascadesLog(CameraTransform camera, float* result, int count, float lambda = 0.85f)
        {
            float farClip = camera.Far;
            float nearClip = camera.Near;

            float clipRange = farClip - nearClip;
            float minZ = nearClip;
            float maxZ = nearClip + clipRange;
            float range = maxZ - minZ;
            float ratio = maxZ / minZ;

            float cascadeSplitLambda = lambda;

            for (uint i = 0; i < count; i++)
            {
                float p = (i + 1) / (float)count;
                float log = minZ * MathF.Pow(ratio, p);
                float uniform = minZ + range * p;
                float d = cascadeSplitLambda * (log - uniform) + uniform;
                float splitDist = (d - nearClip) / clipRange;
                result[i] = nearClip + splitDist * clipRange;
            }
        }

        /// <summary>
        /// Gets cascades with linear depth splits based on the provided camera transform.
        /// </summary>
        /// <param name="camera">The camera transform containing near and far clipping distances.</param>
        /// <param name="result">An array to store the resulting cascade splits.</param>
        /// <param name="count">The number of cascades to generate.</param>
        public static unsafe void GetCascadesLinear(CameraTransform camera, float* result, int count)
        {
            float farClip = camera.Far;
            float nearClip = camera.Near;
            float clipRange = farClip - nearClip;
            float minZ = nearClip;
            float maxZ = nearClip + clipRange;
            float range = maxZ - minZ;
            float step = range / count;

            for (int i = 0; i < count; i++)
            {
                result[i] = nearClip + i * step;
            }
        }

        /// <summary>
        /// Gets cascades with fixed depth splits based on the provided camera transform and predefined splits.
        /// </summary>
        /// <param name="camera">The camera transform containing near and far clipping distances.</param>
        /// <param name="result">An array to store the resulting cascade splits.</param>
        /// <param name="count">The number of cascades to generate.</param>
        /// <param name="fixedCascades">An array of predefined cascade splits.</param>
        public static unsafe void GetCascadesFixed(CameraTransform camera, float* result, int count, float* fixedCascades)
        {
            float nearClip = camera.Near;

            for (int i = 0; i < count; i++)
            {
                result[i] = nearClip + fixedCascades[i];
            }
        }

        /// <summary>
        /// Calculates light space matrices for shadow mapping using cascades.
        /// </summary>
        /// <param name="camera">The camera's transform information.</param>
        /// <param name="light">The light's transform information.</param>
        /// <param name="ret">An array to store the light space matrices.</param>
        /// <param name="cascades">An array of cascade split distances.</param>
        /// <param name="frustra">An array of bounding frustums for cascades.</param>
        /// <param name="smSize">The size of the shadow map.</param>
        /// <param name="cascadesCount">The number of cascades to calculate.</param>
        /// <returns>An array of light space matrices for each cascade.</returns>
        public static unsafe Matrix4x4* GetLightSpaceMatrices(CameraTransform camera, Transform light, Matrix4x4* ret, float* cascades, BoundingFrustum[] frustra, float smSize, int cascadesCount = 4)
        {
            float fov = camera.Fov.ToRad();
            float aspect = camera.AspectRatio;
            float far = camera.Far;
            float near = camera.Near;

            Matrix4x4 view = camera.View;
            GetCascadesLog(camera, cascades, cascadesCount);
            for (int i = 0; i < cascadesCount; i++)
            {
                if (i == 0)
                {
                    ret[i] = GetLightSpaceMatrix(view * MathUtil.PerspectiveFovLH(fov, aspect, near, cascades[i]), light, ref frustra[i], smSize);
                }
                else if (i < cascadesCount)
                {
                    ret[i] = GetLightSpaceMatrix(view * MathUtil.PerspectiveFovLH(fov, aspect, cascades[i - 1], cascades[i]), light, ref frustra[i], smSize);
                }
                else
                {
                    ret[i] = GetLightSpaceMatrix(view * MathUtil.PerspectiveFovLH(fov, aspect, cascades[i - 1], far), light, ref frustra[i], smSize);
                }
            }

            return ret;
        }
    }
}