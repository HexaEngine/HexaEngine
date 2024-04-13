namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.Intrinsics;

    public static class VectorExtensions
    {
        public static Vector2 Normalize(this Vector2 v)
        {
            return MathUtil.Normalize(v);
        }

        public static Vector3 Normalize(this Vector3 v)
        {
            return MathUtil.Normalize(v);
        }

        public static Vector4 Normalize(this Vector4 v)
        {
            return MathUtil.Normalize(v);
        }

        public static Vector2D Normalize(this Vector2D v)
        {
            return MathUtil.Normalize(v);
        }

        public static Vector3D Normalize(this Vector3D v)
        {
            return MathUtil.Normalize(v);
        }

        public static Vector4D Normalize(this Vector4D v)
        {
            return MathUtil.Normalize(v);
        }

        public static Vector256<double> AsVector256(this Vector2D vector)
        {
            double x = vector.X;
            double y = vector.Y;
            double z = 0;
            double w = 0;

            return Vector256.Create(x, y, z, w);
        }

        public static Vector2D AsVector2D(this Vector256<double> vector)
        {
            double x = vector.GetElement(0);
            double y = vector.GetElement(1);

            return new(x, y);
        }

        public static Vector256<double> AsVector256(this Vector3D vector)
        {
            double x = vector.X;
            double y = vector.Y;
            double z = vector.Z;
            double w = 0;

            return Vector256.Create(x, y, z, w);
        }

        public static Vector3D AsVector3D(this Vector256<double> vector)
        {
            double x = vector.GetElement(0);
            double y = vector.GetElement(1);
            double z = vector.GetElement(2);

            return new(x, y, z);
        }

        public static Vector256<double> AsVector256(this Vector4D vector)
        {
            double x = vector.X;
            double y = vector.Y;
            double z = vector.Z;
            double w = vector.W;

            return Vector256.Create(x, y, z, w);
        }

        public static Vector4D AsVector4D(this Vector256<double> vector)
        {
            double x = vector.GetElement(0);
            double y = vector.GetElement(1);
            double z = vector.GetElement(2);
            double w = vector.GetElement(3);

            return new(x, y, z, w);
        }
    }
}