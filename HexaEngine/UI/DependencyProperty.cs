namespace HexaEngine.UI
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public delegate bool ValidateValueCallback(object? value);

    public class DependencyProperty
    {
        private readonly HashSet<Type> owners = [];
        private readonly List<(Type owner, PropertyMetadata metadata)> meta = [];

        internal DependencyProperty(PropertyMetadata defaultMetadata, int globalIndex, string name, Type ownerType, Type propertyType, bool readOnly, ValidateValueCallback? validateValueCallback)
        {
            DefaultMetadata = defaultMetadata;
            GlobalIndex = globalIndex;
            Name = name;
            OwnerType = ownerType;
            PropertyType = propertyType;
            ReadOnly = readOnly;
            ValidateValueCallback = validateValueCallback;
            meta.Add((ownerType, defaultMetadata));
            owners.Add(ownerType);
        }

        public PropertyMetadata DefaultMetadata { get; }

        public int GlobalIndex { get; }

        public string Name { get; }

        public Type OwnerType { get; }

        public Type PropertyType { get; }

        public bool ReadOnly { get; }

        public ValidateValueCallback? ValidateValueCallback { get; }

        public void AddOwner<T>()
        {
            AddOwner(typeof(T), DefaultMetadata);
        }

        public void AddOwner<T>(PropertyMetadata metadata)
        {
            AddOwner(typeof(T), metadata);
        }

        public void AddOwner(Type type)
        {
            AddOwner(type, DefaultMetadata);
        }

        public void AddOwner(Type ownerType, PropertyMetadata metadata)
        {
            owners.Add(ownerType);
            meta.Add((ownerType, metadata));
        }

        public PropertyMetadata? GetMetadata(DependencyObject dependencyObject)
        {
            return GetMetadata(dependencyObject.DependencyObjectType);
        }

        public PropertyMetadata? GetMetadata(Type type)
        {
            for (int i = 0; i < meta.Count; i++)
            {
                var (owner, metadata) = meta[i];
                if (type.IsAssignableTo(owner))
                {
                    return metadata;
                }
            }

            return null;
        }

        public virtual bool IsValidType(object value)
        {
            return owners.Contains(value.GetType());
        }

        public virtual bool IsValidValue(object value)
        {
            return value.GetType().IsAssignableTo(PropertyType);
        }

        public static readonly object? UnsetValue = null;

        private static int gid;
        private static readonly List<DependencyProperty> properties = [];

        public static DependencyProperty<TType> Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOwner, TType>(string name, bool readOnly)
        {
            Type type = typeof(TOwner);
            PropertyInfo property = type.GetProperty(name) ?? throw new ArgumentException($"Couldn't find property named \"{name}\" in \"{type}\"");
            PropertyMetadata metadata = new(default(TType), null);
            DependencyProperty<TType> dp = new(metadata, Interlocked.Increment(ref gid), name, type, readOnly, null);
            lock (properties)
            {
                properties.Add(dp);
            }
            return dp;
        }

        public static DependencyProperty<TType> Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOwner, TType>(string name, bool readOnly, PropertyMetadata metadata)
        {
            Type type = typeof(TOwner);
            PropertyInfo property = type.GetProperty(name) ?? throw new ArgumentException($"Couldn't find property named \"{name}\" in \"{type}\"");
            DependencyProperty<TType> dp = new(metadata, Interlocked.Increment(ref gid), name, type, readOnly, null);
            lock (properties)
            {
                properties.Add(dp);
            }
            return dp;
        }

        public static DependencyProperty<TType> Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOwner, TType>(string name, bool readOnly, PropertyMetadata metadata, ValidateValueCallback validateValueCallback)
        {
            Type type = typeof(TOwner);
            PropertyInfo property = type.GetProperty(name) ?? throw new ArgumentException($"Couldn't find property named \"{name}\" in \"{type}\"");
            DependencyProperty<TType> dp = new(metadata, Interlocked.Increment(ref gid), name, type, readOnly, validateValueCallback);
            lock (properties)
            {
                properties.Add(dp);
            }
            return dp;
        }

        public static DependencyProperty<TType> RegisterAttached<TOwner, TType>(string name, bool readOnly)
        {
            Type type = typeof(TOwner);
            PropertyMetadata metadata = new(default(TType), null);
            DependencyProperty<TType> dp = new(metadata, Interlocked.Increment(ref gid), name, type, readOnly, null);
            lock (properties)
            {
                properties.Add(dp);
            }
            return dp;
        }

        public static DependencyProperty<TType> RegisterAttached<TOwner, TType>(string name, bool readOnly, PropertyMetadata metadata)
        {
            Type type = typeof(TOwner);
            DependencyProperty<TType> dp = new(metadata, Interlocked.Increment(ref gid), name, type, readOnly, null);
            lock (properties)
            {
                properties.Add(dp);
            }
            return dp;
        }

        public static DependencyProperty<TType> RegisterAttached<TOwner, TType>(string name, bool readOnly, PropertyMetadata metadata, ValidateValueCallback validateValueCallback)
        {
            Type type = typeof(TOwner);
            DependencyProperty<TType> dp = new(metadata, Interlocked.Increment(ref gid), name, type, readOnly, validateValueCallback);
            lock (properties)
            {
                properties.Add(dp);
            }
            return dp;
        }
    }

    public class DependencyProperty<TType> : DependencyProperty
    {
        internal DependencyProperty(PropertyMetadata defaultMetadata, int globalIndex, string name, Type ownerType, bool readOnly, ValidateValueCallback? validateValueCallback) : base(defaultMetadata, globalIndex, name, ownerType, typeof(TType), readOnly, validateValueCallback)
        {
        }

        public override sealed bool IsValidValue(object value)
        {
            // this is generally faster than GetType().IsAssignableTo
            return value is TType;
        }
    }
}