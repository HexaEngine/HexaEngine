namespace HexaEngine.Core.IO.Binary.Meshes.Processing
{
    using HexaEngine.Core.Unsafes;
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// Provides spatial sorting functionality for efficiently finding vertices close to a given position.
    /// </summary>
    public class SpatialSort
    {
        /// <summary>
        /// Represents the normal vector of the plane used for spatial sorting.
        /// </summary>
        protected Vector3 mPlaneNormal;

        /// <summary>
        /// Represents the centroid used for spatial sorting.
        /// </summary>
        protected Vector3 mCentroid;

        /// <summary>
        /// Represents the list of positions used for spatial sorting.
        /// </summary>
        protected List<Entry> mPositions;

        /// <summary>
        /// Indicates whether the spatial sorting has been finalized.
        /// </summary>
        protected bool mFinalized;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpatialSort"/> class.
        /// </summary>
        /// <param name="positions">The array of positions to be spatially sorted.</param>
        /// <param name="elementOffset">The offset between elements in the positions array.</param>
        public SpatialSort(Vector3[] positions, int elementOffset)
        {
            mPositions = new();
            mPlaneNormal = PlaneInit;
            mPlaneNormal = Vector3.Normalize(mPlaneNormal);
            Fill(positions, elementOffset);
        }

        /// <summary>
        /// Represents the initial plane normal for spatial sorting.
        /// </summary>
        public static readonly Vector3 PlaneInit = new(0.8523f, 0.34321f, 0.5736f);

        /// <summary>
        /// Represents an entry in the spatial sort, containing information about the index, position, and distance.
        /// </summary>
        protected struct Entry : IComparable<Entry>
        {
            /// <summary>
            /// The index associated with the entry.
            /// </summary>
            public uint Index;

            /// <summary>
            /// The position associated with the entry.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// The distance associated with the entry.
            /// </summary>
            public float Distance;

            /// <summary>
            /// Initializes a new instance of the <see cref="Entry"/> struct with default values.
            /// </summary>
            public Entry()
            {
                Index = uint.MaxValue;
                Position = default;
                Distance = float.MaxValue;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Entry"/> struct with specified index and position.
            /// </summary>
            /// <param name="index">The index to associate with the entry.</param>
            /// <param name="position">The position to associate with the entry.</param>
            public Entry(uint index, Vector3 position)
            {
                Index = index;
                Position = position;
                Distance = float.MaxValue;
            }

            /// <summary>
            /// Compares this instance to another <see cref="Entry"/> and returns an indication of their relative values.
            /// </summary>
            /// <param name="other">An <see cref="Entry"/> to compare with this instance.</param>
            /// <returns>
            /// A value that indicates the relative order of the objects being compared.
            /// </returns>
            public int CompareTo(Entry other)
            {
                if (this > other)
                {
                    return 1;
                }

                if (this < other)
                {
                    return -1;
                }

                return 0;
            }

            /// <summary>
            /// Determines whether one <see cref="Entry"/> is less than another <see cref="Entry"/>.
            /// </summary>
            public static bool operator <(Entry a, Entry b) => a.Distance < b.Distance;

            /// <summary>
            /// Determines whether one <see cref="Entry"/> is greater than another <see cref="Entry"/>.
            /// </summary>
            public static bool operator >(Entry a, Entry b) => a.Distance > b.Distance;
        }


        /// <summary>
        /// Fills the spatial sort data with the given positions.
        /// </summary>
        /// <param name="positions">The array of positions to be added.</param>
        /// <param name="elementOffset">The offset between elements in the positions array.</param>
        /// <param name="finish">Specifies whether to finish the spatial sort after adding positions.</param>
        public void Fill(Vector3[] positions, int elementOffset, bool finish = true)
        {
            mPositions.Clear();
            mFinalized = false;
            Append(positions, elementOffset, finish);
            mFinalized = finish;
        }

        /// <summary>
        /// Calculates the signed distance of a given position to the reference plane.
        /// </summary>
        /// <param name="pPosition">The position for which to calculate the distance.</param>
        /// <returns>The signed distance of the position to the reference plane.</returns>
        protected float CalculateDistance(Vector3 pPosition)
        {
            return Vector3.Dot(pPosition - mCentroid, mPlaneNormal);
        }

        /// <summary>
        /// Finishes the spatial sort by calculating centroids and sorting positions by distance.
        /// </summary>
        public void Finish()
        {
            float scale = 1.0f / mPositions.Count;
            for (uint i = 0; i < mPositions.Count; i++)
            {
                mCentroid += scale * mPositions[(int)i].Position;
            }
            for (uint i = 0; i < mPositions.Count; i++)
            {
                var entry = mPositions[(int)i];
                entry.Distance = CalculateDistance(mPositions[(int)i].Position);
                mPositions[(int)i] = entry;
            }

            mPositions.Sort();
            mFinalized = true;
        }

        /// <summary>
        /// Appends positions to the spatial sort.
        /// </summary>
        /// <param name="positions">The array of positions to be appended.</param>
        /// <param name="elementOffset">The offset between elements in the positions array.</param>
        /// <param name="finish">Specifies whether to finish the spatial sort after appending positions.</param>
        public unsafe void Append(Vector3[] positions, int elementOffset, bool finish = true)
        {
            int pNumPositions = positions.Length;
            Trace.Assert(!mFinalized, "You cannot add positions to the SpatialSort object after it has been finalized.");
            // store references to all given positions along with their distance to the reference plane
            int initial = mPositions.Count;
            mPositions.EnsureCapacity(initial + pNumPositions);
            fixed (Vector3* pPositions = positions)
            {
                for (uint a = 0; a < pNumPositions; a++)
                {
                    byte* tempPointer = (byte*)pPositions;
                    Vector3* vec = (Vector3*)(tempPointer + a * elementOffset);
                    mPositions.Add(new((uint)(a + initial), *vec));
                }
            }

            if (finish)
            {
                // now sort the array ascending by distance.
                Finish();
            }
        }

        /// <summary>
        /// Finds positions close to a given position within a specified radius.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="radius">The radius to search for positions within.</param>
        /// <param name="results">The list to store the indices of found positions.</param>
        public void FindPositions(Vector3 position, float radius, List<uint> results)
        {
            Trace.Assert(mFinalized, "The SpatialSort object must be finalized before FindPositions can be called.");
            float dist = CalculateDistance(position);
            float minDist = dist - radius, maxDist = dist + radius;

            // clear the array
            results.Clear();

            // quick check for positions outside the range
            if (mPositions.Count == 0)
            {
                return;
            }

            if (maxDist < mPositions.First().Distance)
            {
                return;
            }

            if (minDist > mPositions.Last().Distance)
            {
                return;
            }

            // do a binary search for the minimal distance to start the iteration there
            uint index = (uint)mPositions.Count / 2;
            uint binaryStepSize = (uint)mPositions.Count / 4;
            while (binaryStepSize > 1)
            {
                if (mPositions[(int)index].Distance < minDist)
                {
                    index += binaryStepSize;
                }
                else
                {
                    index -= binaryStepSize;
                }

                binaryStepSize /= 2;
            }

            // depending on the direction of the last step we need to single step a bit back or forth
            // to find the actual beginning element of the range
            while (index > 0 && mPositions[(int)index].Distance > minDist)
            {
                index--;
            }

            while (index < mPositions.Count - 1 && mPositions[(int)index].Distance < minDist)
            {
                index++;
            }

            // Mow start iterating from there until the first position lays outside of the distance range.
            // Add all positions inside the distance range within the given radius to the result array

            Iterator<Entry> it = new Iterator<Entry>(mPositions) + index;
            float pSquared = radius * radius;

            while (it.Current.Distance < maxDist)
            {
                if ((it.Current.Position - position).LengthSquared() < pSquared)
                {
                    results.Add(it.Current.Index);
                }

                ++it;
                if (it.End)
                {
                    break;
                }
            }

            // that's it
        }

        private unsafe int ToBinary(float pValue)
        {
            // If this assertion fails, signed int is not big enough to store a float on your platform.
            //  Please correct the declaration of BinFloat a few lines above - but do it in a portable,
            //  #ifdef'd manner!
            Trace.Assert(sizeof(int) >= sizeof(float), "sizeof(BinFloat) >= sizeof(ai_real)");

            // If this assertion fails, Visual C++ has finally moved to ILP64. This means that this
            //  code has just become legacy code! Find out the current _value of _MSC_VER and modify
            //  the #if above so it evaluates false on the current and all upcoming VC versions (or
            //  on the current platform, if LP64 or LLP64 are still used on other platforms).
            Trace.Assert(sizeof(int) == sizeof(float), "sizeof(BinFloat) == sizeof(ai_real)");

            // This works best on Visual C++, but other compilers have their problems with it.
            int binValue = *(int*)&pValue;
            //::memcpy(&binValue, &pValue, sizeof(pValue));
            //return binValue;

            // floating-point numbers are of sign-magnitude format, so find out what signed number
            //  representation we must convert negative values to.
            // See http://en.wikipedia.org/wiki/Signed_number_representations.
            int mask = 1 << 8 * sizeof(int) - 1;

            // Two's complement?
            bool DefaultValue = -42 == ~42 + 1 && (binValue & mask) != 0;
            bool OneComplement = -42 == ~42 && (binValue & mask) != 0;

            if (DefaultValue)
            {
                return mask - binValue;
            }
            // One's complement?
            else if (OneComplement)
            {
                return -0 - binValue;
            }
            // Sign-magnitude? -0 = 1000... binary
            return binValue;
        }

        /// <summary>
        /// Finds positions with identical coordinates to a given position within a specified tolerance.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <param name="results">The list to store the indices of positions with identical coordinates.</param>
        public void FindIdenticalPositions(Vector3 position, List<uint> results)
        {
            Trace.Assert(mFinalized, "The SpatialSort object must be finalized before FindIdenticalPositions can be called.");
            // Epsilons have a huge disadvantage: they are of constant precision, while floating-point
            //  values are of log2 precision. If you apply e=0.01 to 100, the epsilon is rather small, but
            //  if you apply it to 0.001, it is enormous.

            // The best way to overcome this is the unit in the last place (ULP). A precision of 2 ULPs
            //  tells us that a float does not differ more than 2 bits from the "real" _value. ULPs are of
            //  logarithmic precision - around 1, they are 1*(2^24) and around 10000, they are 0.00125.

            // For standard C math, we can assume a precision of 0.5 ULPs according to IEEE 754. The
            //  incoming vertex positions might have already been transformed, probably using rather
            //  inaccurate SSE instructions, so we assume a tolerance of 4 ULPs to safely identify
            //  identical vertex positions.
            const int toleranceInULPs = 4;
            // An interesting point is that the inaccuracy grows linear with the number of operations:
            //  multiplying to numbers, each inaccurate to four ULPs, results in an inaccuracy of four ULPs
            //  plus 0.5 ULPs for the multiplication.
            // To compute the distance to the plane, a dot product is needed - that is a multiplication and
            //  an addition on each number.
            const int distanceToleranceInULPs = toleranceInULPs + 1;
            // The squared distance between two 3D vectors is computed the same way, but with an additional
            //  subtraction.
            const int distance3DToleranceInULPs = distanceToleranceInULPs + 1;

            // Convert the plane distance to its signed integer representation so the ULPs tolerance can be
            //  applied. For some reason, VC won't optimize two calls of the bit pattern conversion.
            int minDistBinary = ToBinary(CalculateDistance(position)) - distanceToleranceInULPs;
            int maxDistBinary = minDistBinary + 2 * distanceToleranceInULPs;

            // clear the array in this strange fashion because a simple clear() would also deallocate
            // the array which we want to avoid
            results.Capacity = 0;

            // do a binary search for the minimal distance to start the iteration there
            uint index = (uint)mPositions.Count / 2;
            uint binaryStepSize = (uint)mPositions.Count / 4;
            while (binaryStepSize > 1)
            {
                // Ugly, but conditional jumps are faster with integers than with floats
                if (minDistBinary > ToBinary(mPositions[(int)index].Distance))
                {
                    index += binaryStepSize;
                }
                else
                {
                    index -= binaryStepSize;
                }

                binaryStepSize /= 2;
            }

            // depending on the direction of the last step we need to single step a bit back or forth
            // to find the actual beginning element of the range
            while (index > 0 && minDistBinary < ToBinary(mPositions[(int)index].Distance))
            {
                index--;
            }

            while (index < mPositions.Count - 1 && minDistBinary > ToBinary(mPositions[(int)index].Distance))
            {
                index++;
            }

            // Now start iterating from there until the first position lays outside of the distance range.
            // Add all positions inside the distance range within the tolerance to the result array
            Iterator<Entry> it = new Iterator<Entry>(mPositions) + index;
            while (ToBinary(it.Current.Distance) < maxDistBinary)
            {
                if (distance3DToleranceInULPs >= ToBinary((it.Current.Position - position).LengthSquared()))
                {
                    results.Add(it.Current.Index);
                }

                ++it;
                if (it.End)
                {
                    break;
                }
            }

            // that's it
        }

        /// <summary>
        /// Generates a mapping table based on the spatial sort, assigning unique indices to positions within a radius.
        /// </summary>
        /// <param name="fill">The list to store the generated mapping table.</param>
        /// <param name="radius">The radius for generating the mapping table.</param>
        /// <returns>The number of unique indices generated.</returns>
        public uint GenerateMappingTable(List<uint> fill, float radius)
        {
            Trace.Assert(mFinalized, "The SpatialSort object must be finalized before GenerateMappingTable can be called.");
            fill.Clear();
            for (int i = 0; i < mPositions.Count; i++)
            {
                fill.Add(uint.MaxValue);
            }

            float dist, maxDist;

            uint t = 0;
            float pSquared = radius * radius;
            for (int i = 0; i < mPositions.Count;)
            {
                dist = Vector3.Dot(mPositions[i].Position - mCentroid, mPlaneNormal);
                maxDist = dist + radius;

                fill[(int)mPositions[i].Index] = t;
                Vector3 oldpos = mPositions[i].Position;
                for (++i; i < fill.Count && mPositions[i].Distance < maxDist && (mPositions[i].Position - oldpos).LengthSquared() < pSquared; ++i)
                {
                    fill[(int)mPositions[i].Index] = t;
                }
                ++t;
            }

#if ASSIMP_BUILD_DEBUG

            // debug invariant: mPositions[i].mIndex values must range from 0 to mPositions.size()-1
            for (int i = 0; i < fill.Count; ++i)
            {
                Trace.Asset(fill[i] < mPositions.Count);
            }

#endif

            return t;
        }
    }
}