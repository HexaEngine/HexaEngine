namespace HexaEngine.Rendering.Batching
{
    using HexaEngine.Core.Scenes;

    public struct BatchRendererPair<T> : IEquatable<BatchRendererPair<T>> where T : IBatchRenderer
    {
        public GameObject GameObject;
        public T Renderer;

        public BatchRendererPair(GameObject gameObject, T renderer)
        {
            GameObject = gameObject;
            Renderer = renderer;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is BatchRendererPair<T> pair && Equals(pair);
        }

        public readonly bool Equals(BatchRendererPair<T> other)
        {
            return EqualityComparer<GameObject>.Default.Equals(GameObject, other.GameObject) &&
                   EqualityComparer<T>.Default.Equals(Renderer, other.Renderer);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(GameObject, Renderer);
        }

        public static bool operator ==(BatchRendererPair<T> left, BatchRendererPair<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BatchRendererPair<T> left, BatchRendererPair<T> right)
        {
            return !(left == right);
        }

        public static implicit operator BatchRendererPair(BatchRendererPair<T> pair)
        {
            return new(pair.GameObject, pair.Renderer);
        }
    }

    public struct BatchRendererPair : IEquatable<BatchRendererPair>
    {
        public GameObject GameObject;
        public IBatchRenderer Renderer;

        public BatchRendererPair(GameObject gameObject, IBatchRenderer renderer)
        {
            GameObject = gameObject;
            Renderer = renderer;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is BatchRendererPair<IBatchRenderer> pair && Equals(pair);
        }

        public readonly bool Equals(BatchRendererPair other)
        {
            return EqualityComparer<GameObject>.Default.Equals(GameObject, other.GameObject) &&
                   EqualityComparer<IBatchRenderer>.Default.Equals(Renderer, other.Renderer);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(GameObject, Renderer);
        }

        public static bool operator ==(BatchRendererPair left, BatchRendererPair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BatchRendererPair left, BatchRendererPair right)
        {
            return !(left == right);
        }
    }
}