namespace HexaEngine.Animations
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public class MutableTuple<T1, T2, T3> : IStructuralComparable, IStructuralEquatable, IComparable, ITuple
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        object ITuple.this[int index]
        {
            get
            {
                return index switch
                {
                    0 => Item1,
                    1 => Item1,
                    2 => Item1,
                    _ => throw new IndexOutOfRangeException(),
                };
            }
        }

        int ITuple.Length
        {
            get
            {
                return 3;
            }
        }

        public MutableTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is MutableTuple<T1, T2, T3> other)
            {
                return Item1.Equals(other.Item1) && Item2.Equals(other.Item2) && Item3.Equals(other.Item3);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode();
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            throw null;
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object other, IEqualityComparer comparer)
        {
            throw null;
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            throw null;
        }

        int IComparable.CompareTo(object obj)
        {
            throw null;
        }

        public override string ToString()
        {
            return $"{Item1}, {Item2}, {Item3}";
        }
    }
}