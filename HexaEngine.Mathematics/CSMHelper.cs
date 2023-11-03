namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

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
        public static Vector4[] GetFrustumCornersWorldSpace(Matrix4x4 projview)
        {
            Matrix4x4.Invert(projview, out var inv);

            Vector4[] frustumCorners = new Vector4[8];
            int i = 0;
            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        Vector4 pt = Vector4.Transform(new Vector4(2.0f * x - 1.0f, 2.0f * y - 1.0f, 2.0f * z - 1.0f, 1.0f), inv);
                        frustumCorners[i] = pt / pt.W;
                        i++;
                    }
                }
            }

            return frustumCorners;
        }

        /// <summary>
        /// Calculates the light space matrix for shadow mapping using cascades.
        /// </summary>
        /// <param name="viewProj">The combined view-projection matrix.</param>
        /// <param name="transform">The transformation of the camera.</param>
        /// <param name="frustum">The bounding frustum for this cascade.</param>
        /// <returns>The light space matrix for the cascade.</returns>
        public static Matrix4x4 GetLightSpaceMatrix(Matrix4x4 viewProj, Transform transform, ref BoundingFrustum frustum)
        {
            Vector4[] corners = GetFrustumCornersWorldSpace(viewProj);

            Vector3 center = new(0, 0, 0);
            for (int i = 0; i < corners.Length; i++)
            {
                center += new Vector3(corners[i].X, corners[i].Y, corners[i].Z);
            }
            center /= corners.Length;

            var lightView = MathUtil.LookAtLH(center, center + transform.Forward, transform.Up);

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                var trf = Vector4.Transform(corners[i], lightView);
                minX = MathF.Min(minX, trf.X);
                maxX = MathF.Max(maxX, trf.X);
                minY = MathF.Min(minY, trf.Y);
                maxY = MathF.Max(maxY, trf.Y);
                minZ = MathF.Min(minZ, trf.Z);
                maxZ = MathF.Max(maxZ, trf.Z);
            }

            // Tune this parameter according to the scene
            float zMult = 5;
            if (minZ < 0)
            {
                minZ *= zMult;
            }
            else
            {
                minZ /= zMult;
            }
            if (maxZ < 0)
            {
                maxZ /= zMult;
            }
            else
            {
                maxZ *= zMult;
            }

            Matrix4x4 lightProjection = MathUtil.OrthoOffCenterLH(minX, maxX, minY, maxY, minZ, maxZ);
            Matrix4x4 viewProjOut = lightView * lightProjection;
            frustum.Initialize(viewProjOut);

            return Matrix4x4.Transpose(viewProjOut);
        }

        /// <summary>
        /// Calculates cascade split distances based on the camera's near and far planes.
        /// </summary>
        /// <param name="camera">The camera's transform information.</param>
        /// <param name="result">An array to store the cascade split distances.</param>
        /// <param name="count">The number of cascades to calculate.</param>
        public static unsafe void GetCascades(CameraTransform camera, float* result, int count)
        {
            float farClip = camera.Far;
            float nearClip = camera.Near;

            float clipRange = farClip - nearClip;
            float minZ = nearClip;
            float maxZ = nearClip + clipRange;
            float range = maxZ - minZ;
            float ratio = maxZ / minZ;

            float cascadeSplitLambda = 0.85f;

            for (uint i = 0; i < count; i++)
            {
                float p = (i + 1) / (float)count;
                float log = minZ * MathF.Pow(ratio, p);
                float uniform = minZ + range * p;
                float d = cascadeSplitLambda * (log - uniform) + uniform;
                //result[i] = (d - nearClip) / clipRange;
                float splitDist = (d - nearClip) / clipRange;
                result[i] = nearClip + splitDist * clipRange;
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
        /// <param name="cascadesCount">The number of cascades to calculate.</param>
        /// <returns>An array of light space matrices for each cascade.</returns>
        public static unsafe Matrix4x4* GetLightSpaceMatrices(CameraTransform camera, Transform light, Matrix4x4* ret, float* cascades, BoundingFrustum[] frustra, int cascadesCount = 4)
        {
            float fov = camera.Fov.ToRad();
            float aspect = camera.AspectRatio;
            float far = camera.Far;
            float near = camera.Near;

            Matrix4x4 view = camera.View; //GetCameraView(camera);
            GetCascades(camera, cascades, cascadesCount);
            for (int i = 0; i < cascadesCount; i++)
            {
                if (i == 0)
                {
                    ret[i] = GetLightSpaceMatrix(view * MathUtil.PerspectiveFovLH(fov, aspect, near, cascades[i]), light, ref frustra[i]);
                }
                else if (i < cascadesCount)
                {
                    ret[i] = GetLightSpaceMatrix(view * MathUtil.PerspectiveFovLH(fov, aspect, cascades[i - 1], cascades[i]), light, ref frustra[i]);
                }
                else
                {
                    ret[i] = GetLightSpaceMatrix(view * MathUtil.PerspectiveFovLH(fov, aspect, cascades[i - 1], far), light, ref frustra[i]);
                }
            }

            return ret;
        }
    }
}