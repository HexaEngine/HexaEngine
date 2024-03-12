namespace HexaEngine.Volumes
{
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Scenes;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public class SettingsContainer<T>
    {
        private static readonly JsonSerializerSettings settings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            CheckAdditionalContent = true,
            ConstructorHandling = ConstructorHandling.Default,
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        };

        public SettingsContainer(T value)
        {
            Value = value;
        }

        public Type TargetType => typeof(T);

        public T Value { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, settings);
        }

        public void Deserialize(string json)
        {
            JsonConvert.PopulateObject(json, this);
        }
    }

    /// <summary>
    /// Represents a proxy for an IPostFx object, allowing dynamic access to its properties.
    /// </summary>
    public class PostFxProxy
    {
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        private readonly Type type;

        private readonly PropertyInfo[] properties;
        private readonly Dictionary<string, object?> propertyData = [];

        public PostFxProxy(IPostFx target)
        {
            type = target.GetType();
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var value = property.GetValue(target);
                propertyData.Add(property.Name, value);
            }
        }

        public PostFxProxy(string typeName, Dictionary<string, object?> data)
        {
            type = Type.GetType(typeName);
        }

        [JsonIgnore]
        public Type Type => type;

        public string TypeName => type.FullName ?? throw new("Generics not supported");

        [JsonIgnore]
        public PropertyInfo[] Properties => properties;

        public Dictionary<string, object?> Data => propertyData;

        public void ApplySettings(IPostFx target)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var value = propertyData[property.Name];
                property.SetValue(target, value);
            }
        }
    }

    /// <summary>
    /// Represents a Volume in 3D space and controls post-processing and weather effects.
    /// </summary>
    public class Volume : GameObject
    {
        /// <summary>
        /// Gets or sets the Volume mode.
        /// </summary>
        public VolumeMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the bounding box of the Volume.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the bounding sphere of the Volume.
        /// </summary>
        public BoundingSphere BoundingSphere { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            PostProcessingManager postManager = PostProcessingManager.Current ?? throw new();
        }
    }
}