namespace HexaEngine.UI
{
    public interface IDependencyElement
    {
        public string? Name { get; set; }

        public DependencyObject? Parent { get; set; }

        public T? ResolveObject<T>() where T : DependencyObject
        {
            if (Parent is null)
            {
                return null;
            }

            if (Parent is T t)
            {
                return t;
            }

            return Parent.ResolveObject<T>();
        }

        public DependencyObject? ResolveObject(string name)
        {
            if (Parent is null)
            {
                return null;
            }

            if (Parent.Name == name)
            {
                return Parent;
            }

            return Parent.ResolveObject(name);
        }
    }
}