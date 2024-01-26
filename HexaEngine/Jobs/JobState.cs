namespace HexaEngine.Jobs
{
    public enum JobState
    {
        NotCreated,
        Created,
        Running,
        CompletedSuccessfully,
        CompletedSynchronously,
        Faulted,
    }
}