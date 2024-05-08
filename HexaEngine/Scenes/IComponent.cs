﻿namespace HexaEngine.Scenes
{
    using HexaEngine.Collections;
    using HexaEngine.Scripts;

    public interface IComponent
    {
        /// <summary>
        /// The GUID of the <see cref="IComponent"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        Guid Guid { get; set; }

        [JsonIgnore]
        GameObject GameObject { get; set; }

        bool IsSerializable { get; }

        void Awake();

        void Destroy();
    }

    public interface IAudioComponent : IComponent
    {
        void Update();
    }

    public interface IScriptComponent : IComponent, INotifyFlagsChanged<ScriptFlags>
    {
        int ExecutionOrderIndex { get; set; }
        Type? ScriptType { get; }

        void ScriptCreate();

        void ScriptLoad();

        void ScriptAwake();

        void FixedUpdate();

        void Update();
    }
}