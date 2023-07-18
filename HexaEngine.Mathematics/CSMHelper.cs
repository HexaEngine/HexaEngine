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

        public static Matrix4x4 GetCameraView(CameraTransform camera)
        {
            Quaternion r = camera.GlobalOrientation;

            float halfYaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y)) * 0.5f;
            float sy = MathF.Sin(halfYaw);

            float y2 = sy + sy;

            float wy2 = MathF.Cos(halfYaw) * y2;

            float zaxisZ = 1 - sy * y2;

            Vector3 pos = camera.GlobalPosition;

            float yaxisY = zaxisZ * zaxisZ - wy2 * -wy2;

            Matrix4x4 result = Matrix4x4.Identity;

            result.M11 = zaxisZ;
            result.M21 = 0;
            result.M31 = -wy2;

            result.M12 = 0;
            result.M22 = yaxisY;
            result.M32 = 0;

            result.M13 = wy2;
            result.M23 = 0;
            result.M33 = zaxisZ;

            result.M41 = zaxisZ * pos.X - wy2 * pos.Z;
            result.M42 = yaxisY * pos.Y;
            result.M43 = wy2 * pos.X + zaxisZ * pos.Z;

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
            return result;
        }

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

        public static unsafe void TransformInvView(CameraTransform camera, Matrix4x4* ret, int cascadesCount = 4)
        {
            for (int i = 0; i < cascadesCount; i++)
            {
                ret[i] = Matrix4x4.Transpose(camera.ViewInv * Matrix4x4.Transpose(ret[i]));
            }
        }
    }
}