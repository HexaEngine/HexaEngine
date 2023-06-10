namespace HexaEngine.Core.IO.Meshes.Processing
{
    using System.Diagnostics;
    using System.Linq;
    using System.Numerics;
    using HexaEngine.Core.Unsafes;

    public class SpatialSort
    {
        protected Vector3 mPlaneNormal;
        protected Vector3 mCentroid;
        protected List<Entry> mPositions;
        protected bool mFinalized;

        public SpatialSort(Vector3[] positions, int pElementOffset)
        {
            mPositions = new();
            mPlaneNormal = PlaneInit;
            mPlaneNormal = Vector3.Normalize(mPlaneNormal);
            Fill(positions, pElementOffset);
        }

        public static readonly Vector3 PlaneInit = new(0.8523f, 0.34321f, 0.5736f);

        protected struct Entry : IComparable<Entry>
        {
            public uint Index;
            public Vector3 Position;
            public float Distance;

            public Entry()
            {
                Index = uint.MaxValue;
                Position = default;
                Distance = float.MaxValue;
            }

            public Entry(uint index, Vector3 position)
            {
                Index = index;
                Position = position;
                Distance = float.MaxValue;
            }

            public int CompareTo(Entry other)
            {
                if (this > other)
                    return 1;
                if (this < other)
                    return -1;
                return 0;
            }

            public static bool operator <(Entry a, Entry b) => a.Distance < b.Distance;

            public static bool operator >(Entry a, Entry b) => a.Distance > b.Distance;
        }

        public void Fill(Vector3[] positions, int pElementOffset, bool finalize = true)
        {
            mPositions.Clear();
            mFinalized = false;
            Append(positions, pElementOffset, finalize);
            mFinalized = finalize;
        }

        protected float CalculateDistance(Vector3 pPosition)
        {
            return Vector3.Dot(pPosition - mCentroid, mPlaneNormal);
        }

        public void Finalize()
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

        public unsafe void Append(Vector3[] positions, int pElementOffset, bool finalize = true)
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
                    Vector3* vec = (Vector3*)(tempPointer + a * pElementOffset);
                    mPositions.Add(new((uint)(a + initial), *vec));
                }
            }

            if (finalize)
            {
                // now sort the array ascending by distance.
                Finalize();
            }
        }

        public void FindPositions(Vector3 pPosition, float pRadius, List<uint> poResults)
        {
            Trace.Assert(mFinalized, "The SpatialSort object must be finalized before FindPositions can be called.");
            float dist = CalculateDistance(pPosition);
            float minDist = dist - pRadius, maxDist = dist + pRadius;

            // clear the array
            poResults.Clear();

            // quick check for positions outside the range
            if (mPositions.Count == 0)
                return;
            if (maxDist < mPositions.First().Distance)
                return;
            if (minDist > mPositions.Last().Distance)
                return;

            // do a binary search for the minimal distance to start the iteration there
            uint index = (uint)mPositions.Count / 2;
            uint binaryStepSize = (uint)mPositions.Count / 4;
            while (binaryStepSize > 1)
            {
                if (mPositions[(int)index].Distance < minDist)
                    index += binaryStepSize;
                else
                    index -= binaryStepSize;

                binaryStepSize /= 2;
            }

            // depending on the direction of the last step we need to single step a bit back or forth
            // to find the actual beginning element of the range
            while (index > 0 && mPositions[(int)index].Distance > minDist)
                index--;
            while (index < mPositions.Count - 1 && mPositions[(int)index].Distance < minDist)
                index++;

            // Mow start iterating from there until the first position lays outside of the distance range.
            // Add all positions inside the distance range within the given radius to the result array

            Iterator<Entry> it = new Iterator<Entry>(mPositions) + index;
            float pSquared = pRadius * pRadius;

            while (it.Current.Distance < maxDist)
            {
                if ((it.Current.Position - pPosition).LengthSquared() < pSquared)
                    poResults.Add(it.Current.Index);
                ++it;
                if (it.End)
                    break;
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
            //  code has just become legacy code! Find out the current value of _MSC_VER and modify
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
                return mask - binValue;
            // One's complement?
            else if (OneComplement)
                return -0 - binValue;
            // Sign-magnitude? -0 = 1000... binary
            return binValue;
        }

        public void FindIdenticalPositions(Vector3 pPosition, List<uint> poResults)
        {
            Trace.Assert(mFinalized, "The SpatialSort object must be finalized before FindIdenticalPositions can be called.");
            // Epsilons have a huge disadvantage: they are of constant precision, while floating-point
            //  values are of log2 precision. If you apply e=0.01 to 100, the epsilon is rather small, but
            //  if you apply it to 0.001, it is enormous.

            // The best way to overcome this is the unit in the last place (ULP). A precision of 2 ULPs
            //  tells us that a float does not differ more than 2 bits from the "real" value. ULPs are of
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
            int minDistBinary = ToBinary(CalculateDistance(pPosition)) - distanceToleranceInULPs;
            int maxDistBinary = minDistBinary + 2 * distanceToleranceInULPs;

            // clear the array in this strange fashion because a simple clear() would also deallocate
            // the array which we want to avoid
            poResults.Capacity = 0;

            // do a binary search for the minimal distance to start the iteration there
            uint index = (uint)mPositions.Count / 2;
            uint binaryStepSize = (uint)mPositions.Count / 4;
            while (binaryStepSize > 1)
            {
                // Ugly, but conditional jumps are faster with integers than with floats
                if (minDistBinary > ToBinary(mPositions[(int)index].Distance))
                    index += binaryStepSize;
                else
                    index -= binaryStepSize;

                binaryStepSize /= 2;
            }

            // depending on the direction of the last step we need to single step a bit back or forth
            // to find the actual beginning element of the range
            while (index > 0 && minDistBinary < ToBinary(mPositions[(int)index].Distance))
                index--;
            while (index < mPositions.Count - 1 && minDistBinary > ToBinary(mPositions[(int)index].Distance))
                index++;

            // Now start iterating from there until the first position lays outside of the distance range.
            // Add all positions inside the distance range within the tolerance to the result array
            Iterator<Entry> it = new Iterator<Entry>(mPositions) + index;
            while (ToBinary(it.Current.Distance) < maxDistBinary)
            {
                if (distance3DToleranceInULPs >= ToBinary((it.Current.Position - pPosition).LengthSquared()))
                    poResults.Add(it.Current.Index);
                ++it;
                if (it.End)
                    break;
            }

            // that's it
        }

        public uint GenerateMappingTable(List<uint> fill, float pRadius)
        {
            Trace.Assert(mFinalized, "The SpatialSort object must be finalized before GenerateMappingTable can be called.");
            fill.Clear();
            for (int i = 0; i < mPositions.Count; i++)
            {
                fill.Add(uint.MaxValue);
            }

            float dist, maxDist;

            uint t = 0;
            float pSquared = pRadius * pRadius;
            for (int i = 0; i < mPositions.Count;)
            {
                dist = Vector3.Dot(mPositions[i].Position - mCentroid, mPlaneNormal);
                maxDist = dist + pRadius;

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