namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Lights;
    using Hexa.NET.Mathematics;
    using HexaEngine.Scenes;
    using System.Numerics;

    public unsafe class IBLBaker
    {
        private BoundingFrustum[] frusta = new BoundingFrustum[6];
        private Matrix4x4[] matrices = new Matrix4x4[6];

        private IComputePipeline irradiance;

        private static Matrix4x4 GetProjectionMatrix(float far)
        {
            return MathUtil.PerspectiveFovLH(90f.ToRad(), 1, 0.001f, far);
        }

        private static void GetProbeSpaceMatrices(Transform probe, float far, Matrix4x4[] matrices, BoundingFrustum[] frusta)
        {
            var proj = GetProjectionMatrix(far);

            var pos = probe.GlobalPosition;
            var right = probe.Right;
            var left = probe.Left;
            var up = probe.Up;
            var down = probe.Down;
            var forward = probe.Forward;
            var backward = probe.Backward;

            matrices[0] = MathUtil.LookAtLH(pos, pos + right, up) * proj;  // X+ 0
            matrices[1] = MathUtil.LookAtLH(pos, pos + left, up) * proj;  // X- 1
            matrices[2] = MathUtil.LookAtLH(pos, pos + up, backward) * proj; // Y+ 2
            matrices[3] = MathUtil.LookAtLH(pos, pos + down, up) * proj;  // Y- 3
            matrices[4] = MathUtil.LookAtLH(pos, pos + forward, up) * proj;  // Z+ 4
            matrices[5] = MathUtil.LookAtLH(pos, pos + backward, up) * proj;  // Z- 5

            for (int i = 0; i < matrices.Length; i++)
            {
                var mat = matrices[i];
                frusta[i].Update(mat);
                matrices[i] = Matrix4x4.Transpose(mat);
            }
        }

        public IBLBaker(IGraphicsDevice device)
        {
        }

        public void BakeStatic(Scene scene, Probe probe)
        {
            GetProbeSpaceMatrices(probe.Transform, probe.Range, matrices, frusta);
        }
    }
}