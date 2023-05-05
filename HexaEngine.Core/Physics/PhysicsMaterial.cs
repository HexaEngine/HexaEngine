namespace HexaEngine.Core.Physics
{
    using BepuPhysics.Constraints;

    public struct PhysicsMaterial
    {
        public SpringSettings SpringSettings;
        public float FrictionCoefficient;
        public float MaximumRecoveryVelocity;

        public PhysicsMaterial(SpringSettings springSettings, float frictionCoefficient = 1, float maximumRecoveryVelocity = 2)
        {
            SpringSettings = springSettings;
            FrictionCoefficient = frictionCoefficient;
            MaximumRecoveryVelocity = maximumRecoveryVelocity;
        }

        public PhysicsMaterial(float frictionCoefficient = 1, float maximumRecoveryVelocity = 2)
        {
            SpringSettings = new SpringSettings(30, 1);
            FrictionCoefficient = frictionCoefficient;
            MaximumRecoveryVelocity = maximumRecoveryVelocity;
        }
    }
}