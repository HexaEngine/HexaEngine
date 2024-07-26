#region License

/*
MIT License

Copyright(c) 2017-2018 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion

using Hexa.NET.Mathematics;
using System.Numerics;

namespace HexaEngine.Core.MeshDecimator
{
    /// <summary>
    /// A bone weight.
    /// </summary>
    public struct BoneWeight : IEquatable<BoneWeight>
    {
        #region Fields

        /// <summary>
        /// The first bone index.
        /// </summary>
        public int BoneIndex0;

        /// <summary>
        /// The second bone index.
        /// </summary>
        public int BoneIndex1;

        /// <summary>
        /// The third bone index.
        /// </summary>
        public int BoneIndex2;

        /// <summary>
        /// The fourth bone index.
        /// </summary>
        public int BoneIndex3;

        /// <summary>
        /// The first bone weight.
        /// </summary>
        public float BoneWeight0;

        /// <summary>
        /// The second bone weight.
        /// </summary>
        public float BoneWeight1;

        /// <summary>
        /// The third bone weight.
        /// </summary>
        public float BoneWeight2;

        /// <summary>
        /// The fourth bone weight.
        /// </summary>
        public float BoneWeight3;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new bone weight.
        /// </summary>
        /// <param name="boneIndex0">The first bone index.</param>
        /// <param name="boneIndex1">The second bone index.</param>
        /// <param name="boneIndex2">The third bone index.</param>
        /// <param name="boneIndex3">The fourth bone index.</param>
        /// <param name="boneWeight0">The first bone weight.</param>
        /// <param name="boneWeight1">The second bone weight.</param>
        /// <param name="boneWeight2">The third bone weight.</param>
        /// <param name="boneWeight3">The fourth bone weight.</param>
        public BoneWeight(int boneIndex0, int boneIndex1, int boneIndex2, int boneIndex3, float boneWeight0, float boneWeight1, float boneWeight2, float boneWeight3)
        {
            BoneIndex0 = boneIndex0;
            BoneIndex1 = boneIndex1;
            BoneIndex2 = boneIndex2;
            BoneIndex3 = boneIndex3;

            BoneWeight0 = boneWeight0;
            BoneWeight1 = boneWeight1;
            BoneWeight2 = boneWeight2;
            BoneWeight3 = boneWeight3;
        }

        public BoneWeight(Point4 indices, Vector4 weights)
        {
            BoneIndex0 = indices.X;
            BoneIndex1 = indices.Y;
            BoneIndex2 = indices.Z;
            BoneIndex3 = indices.W;

            BoneWeight0 = weights.X;
            BoneWeight1 = weights.Y;
            BoneWeight2 = weights.Z;
            BoneWeight3 = weights.W;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns if two bone weights equals eachother.
        /// </summary>
        /// <param name="lhs">The left hand side bone weight.</param>
        /// <param name="rhs">The right hand side bone weight.</param>
        /// <returns>If equals.</returns>
        public static bool operator ==(BoneWeight lhs, BoneWeight rhs)
        {
            return lhs.BoneIndex0 == rhs.BoneIndex0 && lhs.BoneIndex1 == rhs.BoneIndex1 && lhs.BoneIndex2 == rhs.BoneIndex2 && lhs.BoneIndex3 == rhs.BoneIndex3 &&
                new Vector4(lhs.BoneWeight0, lhs.BoneWeight1, lhs.BoneWeight2, lhs.BoneWeight3) == new Vector4(rhs.BoneWeight0, rhs.BoneWeight1, rhs.BoneWeight2, rhs.BoneWeight3);
        }

        /// <summary>
        /// Returns if two bone weights don't equal eachother.
        /// </summary>
        /// <param name="lhs">The left hand side bone weight.</param>
        /// <param name="rhs">The right hand side bone weight.</param>
        /// <returns>If not equals.</returns>
        public static bool operator !=(BoneWeight lhs, BoneWeight rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region Private Methods

        private void MergeBoneWeight(int boneIndex, float weight)
        {
            if (boneIndex == BoneIndex0)
            {
                BoneWeight0 = (BoneWeight0 + weight) * 0.5f;
            }
            else if (boneIndex == BoneIndex1)
            {
                BoneWeight1 = (BoneWeight1 + weight) * 0.5f;
            }
            else if (boneIndex == BoneIndex2)
            {
                BoneWeight2 = (BoneWeight2 + weight) * 0.5f;
            }
            else if (boneIndex == BoneIndex3)
            {
                BoneWeight3 = (BoneWeight3 + weight) * 0.5f;
            }
            else if (BoneWeight0 == 0f)
            {
                BoneIndex0 = boneIndex;
                BoneWeight0 = weight;
            }
            else if (BoneWeight1 == 0f)
            {
                BoneIndex1 = boneIndex;
                BoneWeight1 = weight;
            }
            else if (BoneWeight2 == 0f)
            {
                BoneIndex2 = boneIndex;
                BoneWeight2 = weight;
            }
            else if (BoneWeight3 == 0f)
            {
                BoneIndex3 = boneIndex;
                BoneWeight3 = weight;
            }
            Normalize();
        }

        private void Normalize()
        {
            float mag = (float)Math.Sqrt(BoneWeight0 * BoneWeight0 + BoneWeight1 * BoneWeight1 + BoneWeight2 * BoneWeight2 + BoneWeight3 * BoneWeight3);
            if (mag > float.Epsilon)
            {
                BoneWeight0 /= mag;
                BoneWeight1 /= mag;
                BoneWeight2 /= mag;
                BoneWeight3 /= mag;
            }
            else
            {
                BoneWeight0 = BoneWeight1 = BoneWeight2 = BoneWeight3 = 0f;
            }
        }

        #endregion

        #region Public Methods

        #region Object

        /// <summary>
        /// Returns a hash code for this vector.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override readonly int GetHashCode()
        {
            return BoneIndex0.GetHashCode() ^ BoneIndex1.GetHashCode() << 2 ^ BoneIndex2.GetHashCode() >> 2 ^ BoneIndex3.GetHashCode() >>
                1 ^ BoneWeight0.GetHashCode() << 5 ^ BoneWeight1.GetHashCode() << 4 ^ BoneWeight2.GetHashCode() >> 4 ^ BoneWeight3.GetHashCode() >> 3;
        }

        /// <summary>
        /// Returns if this bone weight is equal to another object.
        /// </summary>
        /// <param name="obj">The other object to compare to.</param>
        /// <returns>If equals.</returns>
        public override readonly bool Equals(object? obj)
        {
            if (obj is not BoneWeight)
            {
                return false;
            }
            BoneWeight other = (BoneWeight)obj;
            return BoneIndex0 == other.BoneIndex0 && BoneIndex1 == other.BoneIndex1 && BoneIndex2 == other.BoneIndex2 && BoneIndex3 == other.BoneIndex3 &&
                BoneWeight0 == other.BoneWeight0 && BoneWeight1 == other.BoneWeight1 && BoneWeight2 == other.BoneWeight2 && BoneWeight3 == other.BoneWeight3;
        }

        /// <summary>
        /// Returns if this bone weight is equal to another one.
        /// </summary>
        /// <param name="other">The other bone weight to compare to.</param>
        /// <returns>If equals.</returns>
        public readonly bool Equals(BoneWeight other)
        {
            return BoneIndex0 == other.BoneIndex0 && BoneIndex1 == other.BoneIndex1 && BoneIndex2 == other.BoneIndex2 && BoneIndex3 == other.BoneIndex3 &&
                BoneWeight0 == other.BoneWeight0 && BoneWeight1 == other.BoneWeight1 && BoneWeight2 == other.BoneWeight2 && BoneWeight3 == other.BoneWeight3;
        }

        /// <summary>
        /// Returns a nicely formatted string for this bone weight.
        /// </summary>
        /// <returns>The string.</returns>
        public override readonly string ToString()
        {
            return string.Format("({0}:{4:F1}, {1}:{5:F1}, {2}:{6:F1}, {3}:{7:F1})",
                BoneIndex0, BoneIndex1, BoneIndex2, BoneIndex3, BoneWeight0, BoneWeight1, BoneWeight2, BoneWeight3);
        }

        #endregion

        #region Static

        /// <summary>
        /// Merges two bone weights and stores the merged result in the first parameter.
        /// </summary>
        /// <param name="a">The first bone weight, also stores result.</param>
        /// <param name="b">The second bone weight.</param>
        public static void Merge(ref BoneWeight a, ref BoneWeight b)
        {
            if (b.BoneWeight0 > 0f)
            {
                a.MergeBoneWeight(b.BoneIndex0, b.BoneWeight0);
            }

            if (b.BoneWeight1 > 0f)
            {
                a.MergeBoneWeight(b.BoneIndex1, b.BoneWeight1);
            }

            if (b.BoneWeight2 > 0f)
            {
                a.MergeBoneWeight(b.BoneIndex2, b.BoneWeight2);
            }

            if (b.BoneWeight3 > 0f)
            {
                a.MergeBoneWeight(b.BoneIndex3, b.BoneWeight3);
            }
        }

        #endregion

        #endregion
    }
}