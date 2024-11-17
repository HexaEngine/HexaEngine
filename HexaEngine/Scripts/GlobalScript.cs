namespace HexaEngine.Scripts
{
    using Hexa.NET.Logging;
    using HexaEngine.Collections;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using System;

    [Flags]
    public enum GlobalScriptFlags
    {
        None = 0,
        OnLoad = 1 << 0,
        OnUnload = 1 << 1,
        OnSceneLoad = 1 << 2,
        OnSceneUnload = 1 << 3,
        Update = 1 << 4,
        FixedUpdate = 1 << 5,
    }

    public delegate void OnLoadDelegate();

    public delegate void OnUnloadDelegate();

    public delegate void OnSceneLoadDelegate(Scene scene);

    public delegate void OnSceneUnloadDelegate(Scene scene);

    public class GlobalScript : INotifyFlagsChanged<GlobalScriptFlags>
    {
        private AssetRef scriptRef;
        private string? scriptTypeName;

        private Type? scriptType;

        private object? instance;
        private GlobalScriptFlags flags;

        private OnLoadDelegate? loadDelegate;
        private OnUnloadDelegate? unloadDelegate;
        private UpdateDelegate? updateDelegate;
        private FixedUpdateDelegate? fixedUpdateDelegate;
        private OnSceneUnloadDelegate? unloadSceneDelegate;
        private OnSceneLoadDelegate? loadSceneDelegate;
        private bool loaded;

        public GlobalScript()
        {
        }

        [EditorProperty("Script", AssetType.Script)]
        public AssetRef ScriptRef
        {
            get => scriptRef;
            set
            {
                scriptRef = value;
                scriptType = null;
                scriptTypeName = value.GetMetadata()?.Name;

                DestroyInstance();
                CreateInstance();
            }
        }

        [JsonIgnore]
        public object? Instance { get => instance; protected internal set => instance = value; }

        [JsonIgnore]
        public Type? ScriptType => scriptType;

        [JsonIgnore]
        public GlobalScriptFlags Flags => flags;

        public event Action<IHasFlags<GlobalScriptFlags>, GlobalScriptFlags>? FlagsChanged;

        public void CreateInstance()
        {
            lock (this)
            {
                scriptType = ScriptAssemblyManager.GetType(scriptTypeName!);

                if (scriptType == null)
                {
                    LoggerFactory.General.Error($"Couldn't load global script: {scriptTypeName}");
                    return;
                }

                try
                {
                    var methods = scriptType.GetMethods();
                    flags = GlobalScriptFlags.None;
                    for (int i = 0; i < methods.Length; i++)
                    {
                        var method = methods[i];
                        switch (method.Name)
                        {
                            case "OnLoad":
                                flags |= GlobalScriptFlags.OnLoad;
                                loadDelegate = method.CreateDelegate<OnLoadDelegate>(instance);
                                break;

                            case "OnUnload":
                                flags |= GlobalScriptFlags.OnUnload;
                                unloadDelegate = method.CreateDelegate<OnUnloadDelegate>(instance);
                                break;

                            case "OnSceneLoad":
                                flags |= GlobalScriptFlags.OnSceneLoad;
                                loadSceneDelegate = method.CreateDelegate<OnSceneLoadDelegate>(instance);
                                break;

                            case "OnSceneUnload":
                                flags |= GlobalScriptFlags.OnSceneUnload;
                                unloadSceneDelegate = method.CreateDelegate<OnSceneUnloadDelegate>(instance);
                                break;

                            case "Update":
                                flags |= GlobalScriptFlags.Update;
                                updateDelegate = method.CreateDelegate<UpdateDelegate>(instance);
                                break;

                            case "FixedUpdate":
                                flags |= GlobalScriptFlags.FixedUpdate;
                                fixedUpdateDelegate = method.CreateDelegate<FixedUpdateDelegate>(instance);
                                break;

                            default:
                                continue;
                        }
                    }

                    FlagsChanged?.Invoke(this, flags);

                    instance = Activator.CreateInstance(scriptType) ?? throw new Exception($"Couldn't create instance of global script '{scriptType}'");
                }
                catch (Exception e)
                {
                    LoggerFactory.General.Log(e);
                }
            }
        }

        public void DestroyInstance()
        {
            lock (this)
            {
                instance = null;
                loadDelegate = null;
                unloadDelegate = null;
                loadSceneDelegate = null;
                unloadSceneDelegate = null;
                updateDelegate = null;
                fixedUpdateDelegate = null;
            }
        }

        public void LoadScript()
        {
        }

        public void OnLoadScript()
        {
            if (loaded) return;
            if (Application.InEditMode)
            {
                return;
            }

            loaded = true;

            if (loadDelegate != null)
            {
                try
                {
                    loadDelegate();
                }
                catch (Exception e)
                {
                    LoggerFactory.General.Log(e);
                }
            }
        }

        public void OnUnloadScript()
        {
            if (!loaded) return;
            if (Application.InEditMode)
            {
                return;
            }

            loaded = false;

            if (unloadDelegate != null)
            {
                try
                {
                    unloadDelegate();
                }
                catch (Exception e)
                {
                    LoggerFactory.General.Log(e);
                }
            }

            DestroyInstance();
        }

        public void OnLoadScene(Scene scene)
        {
            if (!loaded) return;
            if (Application.InEditMode || loadSceneDelegate == null)
            {
                return;
            }

            try
            {
                loadSceneDelegate(scene);
            }
            catch (Exception e)
            {
                LoggerFactory.General.Log(e);
            }
        }

        public void OnUnloadScene(Scene scene)
        {
            if (!loaded) return;
            if (Application.InEditMode || unloadSceneDelegate == null)
            {
                return;
            }

            try
            {
                unloadSceneDelegate(scene);
            }
            catch (Exception e)
            {
                LoggerFactory.General.Log(e);
            }
        }

        public void Update()
        {
            if (!loaded) return;
            if (Application.InEditMode || updateDelegate == null)
            {
                return;
            }

            try
            {
                updateDelegate();
            }
            catch (Exception e)
            {
                LoggerFactory.General.Log(e);
            }
        }

        public void FixedUpdate()
        {
            if (!loaded) return;
            if (Application.InEditMode || fixedUpdateDelegate == null)
            {
                return;
            }

            try
            {
                fixedUpdateDelegate();
            }
            catch (Exception e)
            {
                LoggerFactory.General.Log(e);
            }
        }
    }
}