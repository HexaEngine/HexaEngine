namespace HexaEngine.Scenes
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public static class Instantiator
    {
        private static readonly ConcurrentDictionary<Type, CacheEntry> cache = new();

        public enum ReferenceMode
        {
            /// <summary>
            /// Ignores all reference types.
            /// </summary>
            Ignore,

            /// <summary>
            /// Copies all references from the source to the cloned object, but doesn't duplicate the instance.
            /// </summary>
            KeepRef,

            /// <summary>
            /// Duplicates the reference object.
            /// </summary>
            DeepCopy
        }

        private struct CacheEntry
        {
            public FieldInfo[] Fields;
            public PropertyInfo[] Properties;

            public CacheEntry([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
            {
                Fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField).Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() == null).ToArray();
                Properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty).Where(x => x.GetCustomAttribute<JsonIgnoreAttribute>() == null && x.CanRead).ToArray();
            }

            public CacheEntry(FieldInfo[] fields, PropertyInfo[] properties)
            {
                Fields = fields;
                Properties = properties;
            }
        }

        public static void ClearCache()
        {
            cache.Clear();
        }

        public static void RemoveFromCache(Type type)
        {
            cache.TryRemove(type, out _);
        }

        private static CacheEntry GetEntry([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
        {
#pragma warning disable IL2067 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
            return cache.GetOrAdd(type, x => new CacheEntry(x));
#pragma warning restore IL2067 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The parameter of method does not have matching annotations.
        }

        public static object? Instantiate(object? source, ReferenceMode mode)
        {
            if (source == null)
            {
                return null;
            }

            // eg for strings and other objects that support ICloneable which makes the process way easier than type testing.
            if (source is ICloneable cloneable)
            {
                return cloneable.Clone();
            }

            Type type = source.GetType();
            var entry = GetEntry(type);
            object clone = Activator.CreateInstance(type) ?? throw new Exception();

            if (source is IList sourceList && clone is IList list)
            {
                for (int i = 0; i < sourceList.Count; i++)
                {
                    list.Add(Instantiate(sourceList[i], mode));
                }
                return list;
            }

            foreach (var field in entry.Fields)
            {
                Type fieldType = field.FieldType;
                if (fieldType.IsValueType || mode == ReferenceMode.KeepRef)
                {
                    field.SetValue(clone, field.GetValue(source));
                }
                else if (mode == ReferenceMode.DeepCopy)
                {
                    field.SetValue(clone, Instantiate(field.GetValue(source), mode));
                }
            }

            foreach (var prop in entry.Properties)
            {
                Type propType = prop.PropertyType;
                if (!prop.CanWrite)
                {
                    var value = prop.GetValue(source);

                    if (value is IList sourceList1)
                    {
                        IList? destList = (IList?)prop.GetValue(clone);
                        if (destList == null)
                            continue;
                        for (int i = 0; i < sourceList1.Count; i++)
                        {
                            destList.Add(Instantiate(sourceList1[i], mode));
                        }
                    }

                    continue;
                }
                if (propType.IsValueType || mode == ReferenceMode.KeepRef)
                {
                    prop.SetValue(clone, prop.GetValue(source));
                }
                else if (mode == ReferenceMode.DeepCopy)
                {
                    prop.SetValue(clone, Instantiate(prop.GetValue(source), mode));
                }
            }

            return clone;
        }

        public static object? Instantiate(object? source)
        {
            return Instantiate(source, ReferenceMode.DeepCopy);
        }

        public static T? Instantiate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T source, ReferenceMode mode) where T : class, new()
        {
            return (T?)Instantiate((object)source, mode);
        }

        public static T? Instantiate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicConstructors)] T>(T source) where T : class, new()
        {
            return Instantiate(source, ReferenceMode.DeepCopy);
        }
    }
}