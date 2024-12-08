#define MoveAndReplace

namespace HexaEngine.PerformanceTests
{
    using Hexa.NET.Mathematics;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public struct OctreeNode<T>
    {
        public int ParentIndex;
        public OctreeNodeChildren Children = new();
        public BoundingBox Bounds;
        public List<OctreeObject<T>>? Objects;

        public readonly int ObjectsCount => Objects?.Count ?? 0;

        public OctreeNode()
        {
        }

        public void AddObject(T value, BoundingSphere sphere)
        {
            Objects ??= ListPool<OctreeObject<T>>.Shared.Rent();
            Objects.Add(new OctreeObject<T>(value, sphere));
        }

        public void RemoveObject(T value)
        {
            if (Objects == null) return;
            Objects.Remove(new(value, default));
            if (Objects.Count == 0)
            {
                ListPool<OctreeObject<T>>.Shared.Return(Objects, false);
                Objects = null;
            }
        }

        public void RemoveAt(int index)
        {
            if (Objects == null) return;
            Objects.RemoveAt(index);
            if (Objects.Count == 0)
            {
                ListPool<OctreeObject<T>>.Shared.Return(Objects, false);
                Objects = null;
            }
        }

        public void Clear()
        {
            if (Objects != null)
            {
                ListPool<OctreeObject<T>>.Shared.Return(Objects, true);
                Objects = null;
            }
        }

        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        public unsafe struct OctreeNodeChildren
        {
            public int Child0;
            public int Child1;
            public int Child2;
            public int Child3;
            public int Child4;
            public int Child5;
            public int Child6;
            public int Child7;

            public OctreeNodeChildren()
            {
                AsSpan().Fill(-1);
            }

            public int this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    fixed (OctreeNodeChildren* @this = &this)
                    {
                        return ((int*)@this)[index];
                    }
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    fixed (OctreeNodeChildren* @this = &this)
                    {
                        ((int*)@this)[index] = value;
                    }
                }
            }

            public Span<int> AsSpan()
            {
                return MemoryMarshal.CreateSpan(ref Child0, 8);
            }

            public ReadOnlySpan<int> AsReadOnlySpan()
            {
                return MemoryMarshal.CreateSpan(ref Child0, 8);
            }

            private string GetDebuggerDisplay()
            {
                var span = AsReadOnlySpan();
                return $"{span[0]}, {span[1]}, {span[2]}, {span[3]}, {span[4]}, {span[5]}, {span[6]}, {span[7]}";
            }
        }
    }
}