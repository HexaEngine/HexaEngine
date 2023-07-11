﻿using HexaEngine.Core;

namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Collections;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Scenes;
    using HexaEngine.Scripts;

    public class ScriptManager : ISystem
    {
        private readonly FlaggedList<ScriptFlags, IScriptComponent> scripts = new();
        private bool awaked;

        public string Name => "Scripts";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Update | SystemFlags.FixedUpdate | SystemFlags.Destory;

        public IReadOnlyList<IScriptComponent> Scripts => (IReadOnlyList<IScriptComponent>)scripts;

        public void Register(GameObject gameObject)
        {
            scripts.AddComponentIfIs<IScriptComponent>(gameObject, awaked);
        }

        public void Unregister(GameObject gameObject)
        {
            scripts.RemoveComponentIfIs<IScriptComponent>(gameObject, awaked);
        }

        public void Update(float delta)
        {
            if (Application.InDesignMode)
            {
                return;
            }

            var scriptList = scripts[ScriptFlags.Update];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Update();
            }
        }

        public void FixedUpdate()
        {
            if (Application.InDesignMode)
            {
                return;
            }

            var scriptList = scripts[ScriptFlags.FixedUpdate];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Update();
            }
        }

        public void Awake()
        {
            if (Application.InDesignMode)
            {
                return;
            }

            awaked = true;

            var scriptList = scripts[ScriptFlags.Awake];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Awake();
            }
        }

        public void Destroy()
        {
            if (Application.InDesignMode)
            {
                return;
            }

            awaked = false;

            var scriptList = scripts[ScriptFlags.Destroy];
            for (int i = 0; i < scriptList.Count; i++)
            {
                scriptList[i].Destory();
            }
        }
    }
}