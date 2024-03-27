namespace HexaEngine.Graphics.Graph
{
    using HexaEngine.Core.Graphics;

    public class ResourceDescriptor<T> : IResourceDescriptor<T>, IResourceDescriptor where T : struct
    {
        private T desc;
        private ResourceRef @ref;
        private GraphResourceContainer? container;
        private Func<IGraphicsDevice, T, IDisposable> constructor;
        private ResourceCreationFlags flags;
        private PrivateFlags privateFlags;
        private ResourceDescriptor<T>? shareSource;

        private enum PrivateFlags
        {
            None = 0,
            Created = 1,
            Shared = 2,
        }

        public ResourceDescriptor(T desc, ResourceRef @ref, GraphResourceContainer? container, Func<IGraphicsDevice, T, IDisposable> constructor, ResourceCreationFlags flags)
        {
            this.desc = desc;
            this.container = container;
            this.@ref = @ref;
            this.constructor = constructor;
            this.flags = flags;
        }

        public virtual bool IsCreated
        {
            get => (privateFlags & PrivateFlags.Created) != 0;
            protected internal set
            {
                if (value)
                {
                    privateFlags |= PrivateFlags.Created;
                }
                else
                {
                    privateFlags &= ~PrivateFlags.Created;
                }
            }
        }

        public virtual bool IsShared
        {
            get => (privateFlags & PrivateFlags.Shared) != 0;
            protected internal set
            {
                if (value)
                {
                    privateFlags |= PrivateFlags.Shared;
                }
                else
                {
                    privateFlags &= ~PrivateFlags.Shared;
                }
                Ref.Shared = value;
            }
        }

        public virtual ResourceDescriptor<T>? ShareSource
        {
            get => shareSource;
            protected internal set
            {
                if (value == null)
                {
                    return;
                }

                shareSource = value;

                value.IsShared = true;
                IsShared = true;

                Ref.ShareSource = value.Ref;
                Container?.AddSharedResource(value.Ref);
            }
        }

        public ResourceRef Ref { get => @ref; set => @ref = value; }

        public GraphResourceContainer? Container { get => container; set => container = value; }

        public T Desc { get => desc; set => desc = value; }

        public ResourceCreationFlags Flags { get => flags; set => flags = value; }

        public Func<IGraphicsDevice, T, IDisposable> Constructor { get => constructor; set => constructor = value; }

        public TType Construct<TType>(IGraphicsDevice device, List<TType> group) where TType : class, IDisposable
        {
            var resource = (TType)Constructor(device, Desc);
            Ref.Value = resource;
            lock (group)
            {
                group.Add(resource);
            }
            IsCreated = true;
            return resource;
        }

        public override string ToString()
        {
            return Ref.Name;
        }
    }
}