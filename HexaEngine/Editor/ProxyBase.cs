namespace HexaEngine.Editor
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public class ProxyBase : IProxy
    {
        private readonly List<PropertyInfo> properties = [];
        private readonly Dictionary<string, object?> propertyData = [];

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        private Type? targetType;

        public ProxyBase(object target)
        {
            targetType = target.GetType();
            TypeName = targetType.FullName ?? throw new NotSupportedException("Generics not supported");
            properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty).ToList();
            foreach (var property in properties)
            {
                var value = property.GetValue(target);
                propertyData.Add(property.Name, value);
            }
        }

        [JsonConstructor]
        public ProxyBase(Dictionary<string, object?> data, string typeName)
        {
            propertyData = data;
            TypeName = typeName;
        }

        public Dictionary<string, object?> Data => propertyData;

        [JsonIgnore]
        public IReadOnlyList<PropertyInfo> Properties => properties;

        [JsonIgnore]
        public Type? TargetType => targetType;

        public string TypeName { get; }

        public virtual void Apply(object target)
        {
            foreach (var property in properties)
            {
                if (propertyData.TryGetValue(property.Name, out var value) && value != null)
                {
                    if (property.CanWrite)
                    {
                        try
                        {
                            property.SetValue(target, value);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        public virtual void UpdateType(object target)
        {
            Type type = target.GetType();
            if (TypeName != type.FullName)
            {
                return;
            }

            targetType = type;

            properties.Clear();
            properties.AddRange(targetType.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty));
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyInfo property = properties[i];

                if (propertyData.TryGetValue(property.Name, out var value))
                {
                    if (property.PropertyType == typeof(float))
                    {
                        propertyData[property.Name] = (float)(double)value;
                    }
                    else if (property.PropertyType == typeof(uint))
                    {
                        propertyData[property.Name] = (uint)(long)value;
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        propertyData[property.Name] = (int)(long)value;
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        propertyData[property.Name] = Enum.ToObject(property.PropertyType, (int)(long)value);
                    }
                    else if (!property.PropertyType.IsInstanceOfType(value))
                    {
                        propertyData[property.Name] = property.GetValue(target);
                    }

                    continue;
                }

                propertyData.Add(property.Name, property.GetValue(target));
            }
        }
    }
}