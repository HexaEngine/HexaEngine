namespace TestApp
{
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public static unsafe class StringExtensions
    {
        public static void ReverseString(this string s)
        {
            fixed (char* pStr = s)
            {
                char* pChar = pStr;
                char* pStrEnd = pStr + s.Length - 1;
                while (pChar < pStrEnd)
                {
                    (*pStrEnd, *pChar) = (*pChar, *pStrEnd);

                    pChar++;
                    pStrEnd--;
                }
            }
        }
    }

    public static unsafe partial class Program
    {
        public static void Main()
        {
            Quaternion quaternion = new(-0.022436896f, 0.7067439f, -0.7067629f, -0.022269966f);

            {
                var x = Vector3.Transform(Vector3.UnitX, quaternion);
                var y = Vector3.Transform(Vector3.UnitY, quaternion);
                var z = Vector3.Cross(x, y);
                var xx = Math.Acos(Vector3.Dot(x, Vector3.UnitX));
                var yy = Math.Acos(Vector3.Dot(y, Vector3.UnitY));
                var zz = Math.Acos(Vector3.Dot(z, Vector3.UnitZ));
            }

            Quaternion r = Quaternion.Normalize(quaternion);

            float yaw = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
            float pitch = MathF.Asin((float)Math.Clamp(2.0f * (r.X * r.W - r.Y * r.Z), -1, 1));
            float roll = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
            Vector3 euler = new Vector3(yaw, pitch, roll);
        }
    }

    public unsafe class UniformScale
    {
        private Vector4[] vectors = new Vector4[WorkSize];

        public const int WorkSize = 1024;

        public UniformScale()
        {
        }

        private struct Particle
        {
            public float X, Y, Z, W;
        }

        public void Baseline()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new() { X = i, Y = i, Z = i, W = i };
                Particle* p = &particle;
                Vector4* myVec = (Vector4*)p;
                vectors[i] = *myVec;
            }
        }

        public void BitwiseMask()
        {
            for (int i = 0; i < WorkSize; i++)
            {
                Particle particle = new() { X = i, Y = i, Z = i, W = i };
                Particle* p = &particle;
                float* f = (float*)p;
                Vector4 myVec = new(f[0], f[1], f[2], f[3]);
                vectors[i] = myVec;
            }
        }
    }
}