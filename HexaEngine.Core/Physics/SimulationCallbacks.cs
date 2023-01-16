namespace HexaEngine.Physics
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuPhysics.CollisionDetection;
    using BepuPhysics.Constraints;
    using BepuUtilities;
    using HexaEngine.Physics.Characters;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    internal struct NarrowphaseCallbacks : INarrowPhaseCallbacks
    {
        public CharacterControllers Characters;
        public ContactEvents Events;

        public NarrowphaseCallbacks(CharacterControllers characters, ContactEvents events)
        {
            Characters = characters;
            Events = events;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial = new PairMaterialProperties { FrictionCoefficient = 1, MaximumRecoveryVelocity = 2, SpringSettings = new SpringSettings(30, 1) };
            //Characters.TryReportContacts(pair, ref manifold, workerIndex, ref pairMaterial); // TODO: FIX
            Events.HandleManifold(workerIndex, pair, ref manifold);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
            Characters.Dispose();
            Events.Dispose();
        }

        public void Initialize(Simulation simulation)
        {
            //Characters.Initialize(simulation); // TODO: FIX
            Events.Initialize(simulation);
        }
    }

    //Note that the engine does not require any particular form of gravity- it, like all the contact callbacks, is managed by a callback.
    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;

        //Note that velocity integration uses "wide" types. These are array-of-struct-of-arrays types that use SIMD accelerated types underneath.
        //Rather than handling a single body at a time, the callback handles up to Vector<float>.Count bodies simultaneously.
        private Vector3Wide gravityWideDt;

        /// <summary>
        /// Performs any required initialization logic after the Simulation instance has been constructed.
        /// </summary>
        /// <param dbgName="simulation">Simulation that owns these callbacks.</param>
        public void Initialize(Simulation simulation)
        {
            //In this demo, we don't need to initialize anything.
            //If you had a simulation with per body gravity stored in a CollidableProperty<T> or something similar, having the simulation provided in a callback can be helpful.
        }

        /// <summary>
        /// Gets how the pose integrator should handle angular velocity integration.
        /// </summary>
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving; //Don't care about fidelity in this demo!

        public bool AllowSubstepsForUnconstrainedBodies { get; }
        public bool IntegrateVelocityForKinematics { get; }

        public PoseIntegratorCallbacks(Vector3 gravity) : this()
        {
            Gravity = gravity;
        }

        /// <summary>
        /// Called prior to integrating the simulation's active bodies. When used with a substepping timestepper, this could be called multiple times per frame with different time step values.
        /// </summary>
        /// <param dbgName="dt">Current time step duration.</param>
        public void PrepareForIntegration(float dt)
        {
            //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
            gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        /// <summary>
        /// Callback called for each active body within the simulation during body integration.
        /// </summary>
        /// <param dbgName="bodyIndex">Index of the body being visited.</param>
        /// <param dbgName="pose">Body's current pose.</param>
        /// <param dbgName="localInertia">Body's current local inertia.</param>
        /// <param dbgName="workerIndex">Index of the worker thread processing this body.</param>
        /// <param dbgName="velocity">Reference to the body's current velocity to integrate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation, BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt, ref BodyVelocityWide velocity)
        {
            //This also is a handy spot to implement things like position dependent gravity or per-body damping.
            //We don't have to check for kinematics; IntegrateVelocityForKinematics returns false in this type, so we'll never see them in this callback.
            //Note that these are SIMD operations and "Wide" types. There are Vector<float>.Count lanes of execution being evaluated simultaneously.
            //The types are laid out in array-of-structures-of-arrays (AOSOA) format. That's because this function is frequently called from vectorized contexts within the solver.
            //Transforming to "array of structures" (AOS) format for the callback and then back to AOSOA would involve a lot of overhead, so instead the callback works on the AOSOA representation directly.
            velocity.Linear += gravityWideDt;
        }
    }
}