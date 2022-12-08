namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

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
            float zMult = 10.0f;
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

        public static float[] GetCascades(CameraTransform camera)
        {
            float far = camera.Far;

            float[] result = new float[16];
            result[0] = far / 25;
            result[1] = far / 10;
            result[2] = far / 5;
            result[3] = far / 2;
            return result;
        }

        public static void GetCascades(CameraTransform camera, float[] result)
        {
            float far = camera.Far;

            result[0] = far * 0.1f;
            result[1] = far * 0.2f;
            result[2] = far * 0.4f;
            result[3] = far * 0.7f;
            //result[4] = far * 0.8f;
            //result[5] = far * 1.0f;
        }

        public static unsafe void GetCascades(CameraTransform camera, float* result)
        {
            float far = camera.Far;

            result[0] = far * 0.1f;
            result[1] = far * 0.2f;
            result[2] = far * 0.4f;
            result[3] = far * 0.7f;
            //result[4] = far * 0.8f;
            //result[5] = far * 1.0f;
        }

        public static Matrix4x4[] GetLightSpaceMatrices(CameraTransform camera, Transform light, int cascadesCount = 3)
        {
            float fov = camera.Fov;
            float aspect = camera.AspectRatio;
            float far = camera.Far;
            float near = camera.Near;
            float[] cascades = GetCascades(camera);
            Matrix4x4[] ret = new Matrix4x4[16];
            for (int i = 0; i < cascadesCount; i++)
            {
                if (i == 0)
                {
                    ret[i] = GetLightSpaceMatrix(camera.View * MathUtil.PerspectiveFovLH(fov.ToRad(), aspect, near, cascades[i]), light);
                }
                else if (i < cascadesCount)
                {
                    ret[i] = GetLightSpaceMatrix(camera.View * MathUtil.PerspectiveFovLH(fov.ToRad(), aspect, cascades[i - 1], cascades[i]), light);
                }
                else
                {
                    ret[i] = GetLightSpaceMatrix(camera.View * MathUtil.PerspectiveFovLH(fov.ToRad(), aspect, cascades[i - 1], far), light);
                }
            }
            return ret;
        }

        public static Matrix4x4[] GetLightSpaceMatrices(CameraTransform camera, Transform light, Matrix4x4[] ret, float[] cascades, int cascadesCount = 3)
        {
            float fov = camera.Fov.ToRad();
            float aspect = camera.AspectRatio;
            float far = camera.Far;
            float near = camera.Near;
            GetCascades(camera, cascades);
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

        public static unsafe Matrix4x4* GetLightSpaceMatrices(CameraTransform camera, Transform light, Matrix4x4* ret, float* cascades, int cascadesCount = 3)
        {
            float fov = camera.Fov.ToRad();
            float aspect = camera.AspectRatio;
            float far = camera.Far;
            float near = camera.Near;
            GetCascades(camera, cascades);
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