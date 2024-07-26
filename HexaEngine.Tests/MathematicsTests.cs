namespace HexaEngine.Tests
{
    using Hexa.NET.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.Intrinsics.X86;
    using System.Runtime.Intrinsics;

    public class MathematicsTests
    {
        [Test]
        public void RemapVec4()
        {
            Vector4 plane = new(0, 1, 0.25f, 0.75f);
            Vector4 planeANormalized = MathUtil.Remap(plane, new Vector4(0), new Vector4(1), new Vector4(-1), new Vector4(1));
            Vector4 planeBNormalized = Remap(plane, new Vector4(0), new Vector4(1), new Vector4(-1), new Vector4(1));

            Assert.That(planeANormalized, Is.EqualTo(planeBNormalized));
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        private static Vector4 Remap(Vector4 value, Vector4 low1, Vector4 high1, Vector4 low2, Vector4 high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        [Test]
        public void RemapVec3()
        {
            Vector3 plane = new(0, 1, 0.75f);
            Vector3 planeANormalized = MathUtil.Remap(plane, new Vector3(0), new Vector3(1), new Vector3(-1), new Vector3(1));
            Vector3 planeBNormalized = Remap(plane, new Vector3(0), new Vector3(1), new Vector3(-1), new Vector3(1));

            Assert.That(planeANormalized, Is.EqualTo(planeBNormalized));
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        private static Vector3 Remap(Vector3 value, Vector3 low1, Vector3 high1, Vector3 low2, Vector3 high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        [Test]
        public void RemapVec2()
        {
            Vector2 plane = new(0, 1);
            Vector2 planeANormalized = MathUtil.Remap(plane, new Vector2(0), new Vector2(1), new Vector2(-1), new Vector2(1));
            Vector2 planeBNormalized = Remap(plane, new Vector2(0), new Vector2(1), new Vector2(-1), new Vector2(1));

            Assert.That(planeANormalized, Is.EqualTo(planeBNormalized));
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        private static Vector2 Remap(Vector2 value, Vector2 low1, Vector2 high1, Vector2 low2, Vector2 high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        [Test]
        public void RemapFloat()
        {
            float plane = 1;
            float planeANormalized = MathUtil.Remap(plane, 0, 1, -1, 1);
            float planeBNormalized = Remap(plane, 0, 1, -1, 1);

            Assert.That(planeANormalized, Is.EqualTo(planeBNormalized));
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        private static float Remap(float value, float low1, float high1, float low2, float high2)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        [Test]
        public void NormalizePlane()
        {
            Vector4 plane = new(1, 3, 4, 8);
            Vector4 planeANormalized = MathUtil.NormalizePlane(plane);
            Vector4 planeBNormalized = NormalizePlane(plane);

            Assert.That(planeANormalized, Is.EqualTo(planeBNormalized));
        }

        private static Vector4 NormalizePlane(Vector4 plane)
        {
            float length = (float)Math.Sqrt(plane.X * plane.X + plane.Y * plane.Y + plane.Z * plane.Z);
            return new Vector4(plane.X / length, plane.Y / length, plane.Z / length, plane.W / length);
        }

        [Test]
        public void ProjectVec2()
        {
            Vector2 vector = new(1, 2);
            Vector2 onto = new(10, 5);
            Vector2 projectedVectorA = MathUtil.Project(vector, onto);
            Vector2 projectedVectorB = Project(vector, onto);

            Assert.That(projectedVectorA, Is.EqualTo(projectedVectorB));
        }

        [Test]
        public void ProjectVec2Zero()
        {
            Vector2 vector = new(1, 2);
            Vector2 onto = new(0, 0);
            Vector2 normalizedVector = MathUtil.Project(vector, onto);
            Vector2 normalizedVec = Project(vector, onto);

            Assert.That(normalizedVector, Is.EqualTo(normalizedVec));
        }

        private static Vector2 Project(Vector2 vector, Vector2 onto)
        {
            float dot = Vector2.Dot(vector, onto);
            float ontoLengthSquared = onto.LengthSquared();

            // Handle edge case when onto vector is zero
            if (ontoLengthSquared < float.Epsilon)
            {
                return Vector2.Zero;
            }

            return onto * (dot / ontoLengthSquared);
        }

        [Test]
        public void NormalizeVec4()
        {
            Vector4 vector = new(1, 2, 3, 4);
            Vector4 normalizedVector = MathUtil.Normalize(vector);
            Vector4 normalizedVec = Vector4.Normalize(vector);

            Assert.That(normalizedVector, Is.EqualTo(normalizedVec));
        }

        [Test]
        public void NormalizeVec3()
        {
            Vector3 vector = new(1, 2, 3);
            Vector3 normalizedVector = MathUtil.Normalize(vector);
            Vector3 normalizedVec = Vector3.Normalize(vector);

            Assert.That(normalizedVector, Is.EqualTo(normalizedVec));
        }

        [Test]
        public void NormalizeVec2()
        {
            Vector2 vector = new(1, 2);
            Vector2 normalizedVector = MathUtil.Normalize(vector);
            Vector2 normalizedVec = Vector2.Normalize(vector);

            Assert.That(normalizedVector, Is.EqualTo(normalizedVec));
        }

        [Test]
        public void NormalizeVec4D()
        {
            Vector4D vector = new(1, 2, 3, 4);
            Vector4D normalizedVector = MathUtil.Normalize(vector);
            Vector4D normalizedVec = Vector4D.Normalize(vector);

            Assert.That(normalizedVector, Is.EqualTo(normalizedVec));
        }

        [Test]
        public void NormalizeVec3D()
        {
            Vector3D vector = new(1, 2, 3);
            Vector3D normalizedVector = MathUtil.Normalize(vector);
            Vector3D normalizedVec = Vector3D.Normalize(vector);

            Assert.That(normalizedVector, Is.EqualTo(normalizedVec));
        }

        [Test]
        public void NormalizeVec2D()
        {
            Vector2D vector = new(1, 2);
            Vector2D normalizedVector = MathUtil.Normalize(vector);
            Vector2D normalizedVec = Vector2D.Normalize(vector);

            Assert.That(normalizedVector, Is.EqualTo(normalizedVec));
        }

        [Test]
        public void DotVec4()
        {
            Vector4 vector = new(1, 2, 3, 4);
            double dot0 = MathUtil.Dot(vector, vector);
            double dot1 = Vector4.Dot(vector, vector);

            Assert.That(dot0, Is.EqualTo(dot1));
        }

        [Test]
        public void DotVec3()
        {
            Vector3 vector = new(1, 2, 3);
            double dot0 = MathUtil.Dot(vector, vector);
            double dot1 = Vector3.Dot(vector, vector);

            Assert.That(dot0, Is.EqualTo(dot1));
        }

        [Test]
        public void DotVec2()
        {
            Vector2 vector = new(1, 2);
            double dot0 = MathUtil.Dot(vector, vector);
            double dot1 = Vector2.Dot(vector, vector);

            Assert.That(dot0, Is.EqualTo(dot1));
        }

        [Test]
        public void DotVec4D()
        {
            Vector4D vector = new(1, 2, 3, 4);
            double dot0 = MathUtil.Dot(vector, vector);
            double dot1 = Vector4D.Dot(vector, vector);

            Assert.That(dot0, Is.EqualTo(dot1));
        }

        [Test]
        public void DotVec3D()
        {
            Vector3D vector = new(1, 2, 3);
            double dot0 = MathUtil.Dot(vector, vector);
            double dot1 = Vector3D.Dot(vector, vector);

            Assert.That(dot0, Is.EqualTo(dot1));
        }

        [Test]
        public void DotVec2D()
        {
            Vector2D vector = new(1, 2);
            double dot0 = MathUtil.Dot(vector, vector);
            double dot1 = Vector2D.Dot(vector, vector);

            Assert.That(dot0, Is.EqualTo(dot1));
        }

        [TestCase(0, ExpectedResult = 0f)]
        [TestCase(0.5f, ExpectedResult = 0.5f)]
        [TestCase(1, ExpectedResult = 1f)]
        [Test]
        public float Smoothstep(float s)
        {
            float edge0 = 0;
            float edge1 = 1;

            float result = MathUtil.SmoothStep(edge0, edge1, s);

            return result;
        }

        [TestCase(0, ExpectedResult = -2f)]
        [TestCase(0.5f, ExpectedResult = 0f)]
        [TestCase(1, ExpectedResult = 2f)]
        [Test]
        public float Smoothstep2(float s)
        {
            float edge0 = -2;
            float edge1 = 2;

            float result = MathUtil.SmoothStep(edge0, edge1, s);

            return result;
        }

        [Test]
        public void SmoothstepVec2()
        {
            Vector2 edge0 = new(0);
            Vector2 edge1 = new(1);
            float s = 0.5f;

            Vector2 resultSSE = MathUtil.SmoothStep(edge0, edge1, s);
            Vector2 resultNonSSE = SmoothStepNonSSE(edge0, edge1, s);

            Assert.That(resultSSE, Is.EqualTo(resultNonSSE));
        }

        [Test]
        public void SmoothstepVec3()
        {
            Vector3 edge0 = new(0);
            Vector3 edge1 = new(1);
            float s = 0.5f;

            Vector3 resultSSE = MathUtil.SmoothStep(edge0, edge1, s);
            Vector3 resultNonSSE = SmoothStepNonSSE(edge0, edge1, s);

            Assert.That(resultSSE, Is.EqualTo(resultNonSSE));
        }

        [Test]
        public void SmoothstepVec4()
        {
            Vector4 edge0 = new(0);
            Vector4 edge1 = new(1);
            float s = 0.5f;

            Vector4 resultSSE = MathUtil.SmoothStep(edge0, edge1, s);
            Vector4 resultNonSSE = SmoothStepNonSSE(edge0, edge1, s);

            Assert.That(resultSSE, Is.EqualTo(resultNonSSE));
        }

        private static float Clamp01(float s)
        {
            if (s < 0.0f)
                return 0.0f;
            if (s > 1.0f)
                return 1.0f;
            return s;
        }

        private static Vector2 SmoothStepNonSSE(Vector2 edge0, Vector2 edge1, float s)
        {
            s = Clamp01(s);
            s = s * s * (3f - 2f * s);
            return edge0 + (edge1 - edge0) * s;
        }

        private static Vector3 SmoothStepNonSSE(Vector3 edge0, Vector3 edge1, float s)
        {
            s = Clamp01(s);
            s = s * s * (3f - 2f * s);
            return edge0 + (edge1 - edge0) * s;
        }

        private static Vector4 SmoothStepNonSSE(Vector4 edge0, Vector4 edge1, float s)
        {
            s = Clamp01(s);
            s = s * s * (3f - 2f * s);
            return edge0 + (edge1 - edge0) * s;
        }
    }
}