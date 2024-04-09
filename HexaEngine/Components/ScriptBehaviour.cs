namespace HexaEngine.Components
{
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Editor.Properties;
    using HexaEngine.Scenes;
    using HexaEngine.Scripts;
    using System.Reflection;

    [EditorComponent<ScriptBehaviour>("Script Behaviour")]
    public class ScriptBehaviour : IScriptComponent
    {
        private GameObject gameObject;
        private ScriptFlags flags;
        private IScriptBehaviour? instance;
        private PropertyInfo[] properties;
        private AssetRef scriptRef;
        private string? scriptTypeName;
        private Type? scriptType;
        private Dictionary<string, object?> propertyValues = new();

        [EditorProperty("Script", AssetType.Script)]
        public AssetRef ScriptRef
        {
            get => scriptRef;
            set
            {
                scriptRef = value;
                scriptTypeName = value.GetMetadata()?.Name;

                DestroyInstance();
                CreateInstance();
            }
        }

        [JsonIgnore]
        public Dictionary<string, object?> PropertyValues
        {
            get
            {
                if (properties != null && instance != null)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var prop = properties[i];
                        if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null)
                        {
                            if (prop.CanWrite && prop.CanWrite)
                            {
                                propertyValues[prop.Name] = prop.GetValue(instance);
                            }
                        }
                    }
                }

                return propertyValues;
            }
            set
            {
                if (properties != null && instance != null)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var prop = properties[i];
                        if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null)
                        {
                            if (prop.CanWrite && prop.CanWrite)
                            {
                                prop.SetValue(instance, propertyValues[prop.Name]);
                            }
                        }
                    }
                }

                propertyValues = value;
            }
        }

        private static readonly JsonSerializerSettings serializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        };

        public string PropertyValuesString
        {
            get
            {
                return JsonConvert.SerializeObject(PropertyValues, Formatting.None, serializerSettings);
            }
            set
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, object?>>(value, serializerSettings);
                PropertyValues = values ?? new();
            }
        }

        [JsonIgnore]
        public IScriptBehaviour? Instance { get => instance; set => instance = value; }

        [JsonIgnore]
        public ScriptFlags Flags => flags;

        [JsonIgnore]
        public GameObject GameObject
        {
            get => gameObject;
            set
            {
                gameObject = value;
                if (instance != null)
                {
                    instance.GameObject = value;
                }
            }
        }

        [JsonIgnore]
        public Type? ScriptType => scriptType;

        public event Action<IHasFlags<ScriptFlags>, ScriptFlags>? FlagsChanged;

        public void Awake()
        {
            AssemblyManager.AssembliesUnloaded += AssembliesUnloaded;
            AssemblyManager.AssemblyLoaded += AssemblyLoaded;
            CreateInstance();
            if (Application.InEditMode)
                return;
            if (instance != null)
            {
                try
                {
                    instance.GameObject = GameObject;
                    instance.Awake();
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
            }
        }

        private void AssembliesUnloaded(object? sender, EventArgs? e)
        {
            if (scriptType == null)
            {
                return;
            }

            ObjectEditorFactory.DestroyEditor(scriptType);
            scriptType = null;
            DestroyInstance();
        }

        private void AssemblyLoaded(object? sender, Assembly e)
        {
            CreateInstance();
        }

        public void Update()
        {
            if (Application.InEditMode || instance == null)
            {
                return;
            }

            try
            {
                instance.Update();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public void FixedUpdate()
        {
            if (Application.InEditMode || instance == null)
            {
                return;
            }

            try
            {
                instance.FixedUpdate();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public void Destroy()
        {
            AssemblyManager.AssembliesUnloaded -= AssembliesUnloaded;
            AssemblyManager.AssemblyLoaded -= AssemblyLoaded;
            if (Application.InEditMode || instance == null)
            {
                instance = null;
                return;
            }

            try
            {
                instance.Destroy();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }

            instance = null;
        }

        public void CreateInstance()
        {
            if (scriptTypeName == null)
            {
                return;
            }

            scriptType = AssemblyManager.GetType(scriptTypeName);
            if (scriptType == null)
            {
                Logger.Error($"Couldn't load script: {scriptTypeName}");
                return;
            }

            try
            {
                var methods = scriptType.GetMethods();
                flags = ScriptFlags.None;
                for (int i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    switch (method.Name)
                    {
                        case "Awake":
                            flags |= ScriptFlags.Awake;
                            break;

                        case "Update":
                            flags |= ScriptFlags.Update;
                            break;

                        case "FixedUpdate":
                            flags |= ScriptFlags.FixedUpdate;
                            break;

                        case "Destroy":
                            flags |= ScriptFlags.Destroy;
                            break;

                        default:
                            continue;
                    }
                }

                FlagsChanged?.Invoke(this, flags);

                instance = Activator.CreateInstance(scriptType) as IScriptBehaviour;
                properties = scriptType.GetProperties();

                List<string> toRemove = new(propertyValues.Keys);

                for (int i = 0; i < properties.Length; i++)
                {
                    var prop = properties[i];
                    if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null && prop.CanWrite && prop.CanWrite)
                    {
                        if (propertyValues.TryGetValue(prop.Name, out object? value))
                        {
                            if (prop.PropertyType.IsInstanceOfType(value))
                            {
                                value = propertyValues[prop.Name] = value;
                            }
                            else if (prop.PropertyType == typeof(float))
                            {
                                value = propertyValues[prop.Name] = (float)(double)value;
                            }
                            else if (prop.PropertyType == typeof(uint))
                            {
                                value = propertyValues[prop.Name] = (uint)(long)value;
                            }
                            else if (prop.PropertyType == typeof(int))
                            {
                                value = propertyValues[prop.Name] = (int)(long)value;
                            }
                            else if (prop.PropertyType.IsEnum)
                            {
                                value = propertyValues[prop.Name] = Enum.ToObject(prop.PropertyType, (int)(long)value);
                            }
                            else if (!prop.PropertyType.IsInstanceOfType(value))
                            {
                                value = propertyValues[prop.Name] = prop.GetValue(value);
                            }

                            prop.SetValue(instance, value);
                            toRemove.Remove(prop.Name);
                        }
                        else
                        {
                            propertyValues.Add(prop.Name, prop.GetValue(instance));
                        }
                    }
                }

                for (int i = 0; i < toRemove.Count; i++)
                {
                    propertyValues.Remove(toRemove[i]);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public void DestroyInstance()
        {
            instance = null;
        }
    }
}