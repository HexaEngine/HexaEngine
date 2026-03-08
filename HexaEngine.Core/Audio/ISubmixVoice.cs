namespace HexaEngine.Core.Audio
{
    using System;

    /// <summary>
    /// Represents a submix voice used to apply audio effects to a group of audio sources.
    /// </summary>
    public interface ISubmixVoice : IAudioInputNode, IAudioOutputNode, IAudioDeviceChild, IDisposable
    {
        /// <summary>
        /// Gets or sets the gain (volume) of the submix voice.
        /// </summary>
        float Gain { get; set; }

        /// <summary>
        /// Gets or sets the name of the submix voice.
        /// </summary>
        string Name { get; set; }
    }

    public interface IAudioOutputNode : IAudioDeviceChild
    {

    }

    public interface IAudioInputNode : IAudioDeviceChild
    {
    }

    public interface IAudioDeviceChild
    {
        public nint NativePointer { get; }
    }
}