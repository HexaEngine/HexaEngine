namespace HexaEngine.Rendering.Graph
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graph;

    public struct LazyInitDesc<T>
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
            group.Add(resource);
        }
    }
}