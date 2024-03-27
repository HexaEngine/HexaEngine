namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;

    public interface IResourceDescriptor
    {
        GraphResourceContainer? Container { get; set; }

        ResourceCreationFlags Flags { get; set; }

        bool IsCreated { get; }

        bool IsShared { get; }

        ResourceRef Ref { get; set; }
    }

    public interface IResourceDescriptor<T> : IResourceDescriptor where T : struct
    {
        Func<IGraphicsDevice, T, IDisposable> Constructor { get; set; }

        T Desc { get; set; }

        ResourceDescriptor<T>? ShareSource { get; }

        TType Construct<TType>(IGraphicsDevice device, List<TType> group) where TType : class, IDisposable;
    }
}