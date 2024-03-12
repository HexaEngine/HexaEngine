namespace HexaEngine.UI
{
    public interface IDependencyElement
    {
        public string? Name { get; set; }

        public DependencyElement? Parent { get; set; }

        public T? ResolveObject<T>() where T : DependencyElement
        {
            if (Parent is null) return null;
            if (Parent is T t)
            {
                return t;
            }

            return Parent.ResolveObject<T>();
        }

        public DependencyElement? ResolveObject(string name)
        {
            if (Parent is null) return null;
            if (Parent.Name == name)
            {
                return Parent;
            }

            return Parent.ResolveObject(name);
        }
    }

    public interface IChildContainer : IDependencyElement
    {
        UIElementCollection Children { get; }
    }

    public interface ICompositionTarget : IDependencyElement
    {
        public void Invalidate()
        {
            if (Parent is ICompositionTarget target)
            {
                target.Invalidate();
            }
        }

        protected void OnComposeTarget();
    }

    public partial class DependencyElement : IDependencyElement
    {
        public string? Name { get; set; }

        public DependencyElement? Parent { get; set; }

        public virtual void Invalidate()
        {
            Parent?.Invalidate();
        }

        public T? ResolveObject<T>() where T : DependencyElement
        {
            if (Parent is null) return null;
            if (Parent is T t)
            {
                return t;
            }

            return Parent.ResolveObject<T>();
        }

        public DependencyElement? ResolveObject(string name)
        {
            if (Parent is null) return null;
            if (Parent.Name == name)
            {
                return Parent;
            }

            return Parent.ResolveObject(name);
        }
    }
}