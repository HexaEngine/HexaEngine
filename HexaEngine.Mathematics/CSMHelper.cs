namespace HexaEngine.Mathematics
{
    using System;
    using System.Diagnostics.Metrics;
    using System.Numerics;
    using System.Text;

    public static class CSMHelper
    {
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

        public static Matrix4x4 GetLightSpaceMatrix(Matrix4x4 viewProj, Transform transform)
        {
            Vector4[] corners = GetFrustumCornersWorldSpace(viewProj);

            Vector3 center = new(0, 0, 0);
            for (int i = 0; i < corners.Length; i++)
            {
                center += new Vector3(corners[i].X, corners[i].Y, corners[i].Z);
            }
            center /= corners.Length;

            var lightView = MathUtil.LookAtLH(center - transform.Forward, center, transform.Up);

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            for (int i = 0; i < corners.Length; i++)
            {
                var trf = Vector4.Transform(corners[i], lightView);
                minX = Math.Min(minX, trf.X);
                maxX = Math.Max(maxX, trf.X);
                minY = Math.Min(minY, trf.Y);
                maxY = Math.Max(maxY, trf.Y);
                minZ = Math.Min(minZ, trf.Z);
                maxZ = Math.Max(maxZ, trf.Z);
            }

            // Tune this parameter according to the scene
            float zMult = 1.0f;
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

            return Matrix4x4.Transpose(lightView * lightProjection);
        }

        public static float Uniform(int i, float n, float m, float far) => (far - n) * (i / m);

        public static float Logarithmic(int i, float n, float far) => 1 / MathF.Log(far / n) * MathF.Log(i / n);

        public static unsafe void GetCascades(CameraTransform camera, float* result, int count)
        {
            float farClip = camera.Far;
            float nearClip = camera.Near;

            float clipRange = farClip - nearClip;
            float minZ = nearClip;
            float maxZ = nearClip + clipRange;
            float range = maxZ - minZ;
            float ratio = maxZ / minZ;

            float cascadeSplitLambda = 0.95f;

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

            Span<float> span = new Span<float>(result, count);
        }

        public static unsafe Matrix4x4* GetLightSpaceMatrices(CameraTransform camera, Transform light, Matrix4x4* ret, float* cascades, int cascadesCount = 4)
        {
            float fov = camera.Fov.ToRad();
            float aspect = camera.AspectRatio;
            float far = camera.Far;
            float near = camera.Near;
            GetCascades(camera, cascades, cascadesCount);
            for (int i = 0; i < cascadesCount; i++)
            {
                if (i == 0)
                {
                    ret[i] = GetLightSpaceMatrix(camera.View * MathUtil.PerspectiveFovLH(fov, aspect, near, cascades[i]), light);
                }
                else if (i < cascadesCount)
                {
                    ret[i] = GetLightSpaceMatrix(camera.View * MathUtil.PerspectiveFovLH(fov, aspect, cascades[i - 1], cascades[i]), light);
                }
                else
                {
                    ret[i] = GetLightSpaceMatrix(camera.View * MathUtil.PerspectiveFovLH(fov, aspect, cascades[i - 1], far), light);
                }
            }
            return ret;
        }
    }
}