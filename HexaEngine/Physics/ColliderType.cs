namespace HexaEngine.Physics
{
    public enum ColliderType
    {
        Static,
        Dynamic,
        Kinematic
    }

    public enum CharacterControllerShape
    {
        Capsule,
        Box
    }

    public enum CapsuleClimbingMode
    {
        Easy,
        Constrained,
        Last
    }

    public enum ControllerNonWalkableMode
    {
        PreventClimbing,
        PreventClimbingAndForceSliding
    }

    public enum ForceMode
    {
        Force,
        Impulse,
        VelocityChange,
        Acceleration
    }
}