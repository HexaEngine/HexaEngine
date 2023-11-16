namespace HexaEngine.Components
{
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scripts;
    using System.Reflection;

    [EditorComponent<ScriptBehaviour>("Script Behaviour")]
    public class ScriptBehaviour : IScriptComponent
    {
        private ScriptFlags flags;
        private IScriptBehaviour? instance;
        private PropertyInfo[] properties;
        private string? scriptType;
        private Dictionary<string, object?> propertyValues = new();

        public string? ScriptType
        {
            get => scriptType;
            set
            {
                scriptType = value;
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

        private static JsonSerializerSettings serializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
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
            get => instance?.GameObject;
            set
            {
                if (instance != null)
                {
                    instance.GameObject = value;
                }
            }
        }

        public event Action<IHasFlags<ScriptFlags>, ScriptFlags>? FlagsChanged;

        public void Awake()
        {
            CreateInstance();
            if (Application.InDesignMode)
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

        public void Update()
        {
            if (Application.InDesignMode || instance == null)
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
            if (Application.InDesignMode || instance == null)
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
            if (Application.InDesignMode || instance == null)
            {
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
            if (ScriptType == null)
            {
                return;
            }

            Type? type = AssemblyManager.GetType(ScriptType);
            if (type == null)
            {
                Logger.Error($"Couldn't load script: {ScriptType}");
                return;
            }

            try
            {
                var methods = type.GetMethods();
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

                instance = Activator.CreateInstance(type) as IScriptBehaviour;
                properties = type.GetProperties();

                for (int i = 0; i < properties.Length; i++)
                {
                    var prop = properties[i];
                    if (prop.GetCustomAttribute<EditorPropertyAttribute>() != null && prop.CanWrite && prop.CanWrite)
                    {
                        if (propertyValues.ContainsKey(prop.Name))
                        {
                            prop.SetValue(instance, propertyValues[prop.Name]);
                        }
                        else
                        {
                            propertyValues.Add(prop.Name, prop.GetValue(instance));
                        }
                    }
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