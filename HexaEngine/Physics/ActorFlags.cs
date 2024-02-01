namespace HexaEngine.Physics
{
    [Flags]
    public enum ActorFlags
    {
        Visualization = 1,
        DisableGravity = 2,
        SendSleepNotifies = 4,
        DisableSimulation = 8
    }
}