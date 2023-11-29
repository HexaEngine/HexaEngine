namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;

    public struct LazyInitDesc<T> : IEquatable<LazyInitDesc<T>>
    {
        public T Desc;
        public ResourceRef Ref;
        public Func<IGraphicsDevice, T, IDisposable> Constructor;

        public LazyInitDesc(T desc, ResourceRef @ref, Func<IGraphicsDevice, T, IDisposable> constructor)
        {
            Desc = desc;
            Ref = @ref;
            Constructor = constructor;
        }

        public readonly void Construct<TType>(IGraphicsDevice device, List<TType> group) where TType : class, IDisposable
        {
            var resource = (TType)Constructor(device, Desc);
            Ref.Value = resource;
            lock (group)
            {
                group.Add(resource);
            }
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is LazyInitDesc<T> desc && Equals(desc);
        }

        public readonly bool Equals(LazyInitDesc<T> other)
        {
            return EqualityComparer<ResourceRef>.Default.Equals(Ref, other.Ref);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Ref);
        }

        public static bool operator ==(LazyInitDesc<T> left, LazyInitDesc<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LazyInitDesc<T> left, LazyInitDesc<T> right)
        {
            return !(left == right);
        }
    }
}