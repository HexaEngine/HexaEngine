namespace HexaEngine.MiniAudio
{
    using Hexa.NET.MiniAudio;
    using HexaEngine.Core.Audio;

    public unsafe struct TaskSyncObject
    {
        public MaFence* Fence;
        public TaskCompletionSource<ISound> Task;
        public MiniAudioSound Result;

        public TaskSyncObject(MaFence* fence, TaskCompletionSource<ISound> task, MiniAudioSound result)
        {
            Fence = fence;
            Task = task;
            Result = result;
        }

        public bool IsSignaled()
        {
            return Volatile.Read(ref Fence->Counter) == 0;
        }
    }
}