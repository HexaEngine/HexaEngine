﻿namespace HexaEngine.Audio
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Serialization;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Audio.Listener")]
    [EditorCategory("Audio")]
    [EditorComponent<ListenerComponent>("Listener")]
    public class ListenerComponent : IAudioComponent
    {
        private bool isActive;
#pragma warning disable CS8618 // Non-nullable field 'listener' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.
        private IListener listener;
#pragma warning restore CS8618 // Non-nullable field 'listener' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.

        /// <summary>
        /// The GUID of the <see cref="ListenerComponent"/>.
        /// </summary>
        /// <remarks>DO NOT CHANGE UNLESS YOU KNOW WHAT YOU ARE DOING. (THIS CAN BREAK REFERENCES)</remarks>
        public Guid Guid { get; set; } = Guid.NewGuid();

        [JsonIgnore]
        public bool IsSerializable { get; } = true;

        [EditorProperty("Is Active")]
        public bool IsActive
        { get => isActive; set { listener.IsActive = value; isActive = value; } }

        [JsonIgnore]
        public GameObject GameObject { get; set; } = null!;

        public void Awake()
        {
            listener = AudioManager.CreateListener();
            listener.IsActive = isActive;
        }

        public void Update()
        {
            listener.Position = GameObject.Transform.Position;
            listener.Orientation = new(GameObject.Transform.Forward, GameObject.Transform.Up);
        }

        public void Destroy()
        {
            listener.Dispose();
        }
    }
}