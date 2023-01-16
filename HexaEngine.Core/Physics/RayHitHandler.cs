namespace HexaEngine.Core.Physics
{
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using BepuPhysics.Trees;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public class RayHitHandler : IRayHitHandler
    {
        private readonly Simulation simulation;
        public CollidableReference Collidable;
        public float T;
        public Vector3 Normal;
        public bool Hit;
        public int ChildIndex;
        public CollidableMobility MobilityFilter;

        public RayHitHandler(Simulation simulation, CollidableMobility mobilityFilter)
        {
            this.simulation = simulation;
            MobilityFilter = mobilityFilter;
        }

        public RaycastResult Raycast(in Vector3 origin, in Vector3 direction, in float maxT)
        {
            return Raycast(origin, direction, maxT, this);
        }

        public static RaycastResult Raycast(in Vector3 origin, in Vector3 direction, in float maxT, RayHitHandler hitHandler)
        {
            hitHandler.Collidable = default;
            hitHandler.T = float.MaxValue;
            hitHandler.Normal = default;
            hitHandler.Hit = false;
            hitHandler.ChildIndex = 0;
            hitHandler.simulation.RayCast(origin, direction, maxT, ref hitHandler);
            return new RaycastResult()
            {
                ChildIndex = hitHandler.ChildIndex,
                Collidable = hitHandler.Collidable,
                Hit = hitHandler.Hit,
                T = hitHandler.T,
                Normal = hitHandler.Normal,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable)
        {
            return collidable.Mobility == MobilityFilter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            return AllowTest(collidable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
        {
            if (t < maximumT)
                maximumT = t;
            if (t < T)
            {
                Collidable = collidable;
                T = t;
                Normal = normal;
                Hit = true;
                ChildIndex = childIndex;
            }
        }
    }

    public class RayHitHandler2 : IRayHitHandler
    {
        private readonly Simulation simulation;
        public CollidableReference Self;
        public CollidableReference Collidable;
        public float T;
        public Vector3 Normal;
        public bool Hit;
        public int ChildIndex;

        public RayHitHandler2(Simulation simulation, CollidableReference self)
        {
            this.simulation = simulation;
            Self = self;
        }

        public RaycastResult Raycast(in Vector3 origin, in Vector3 direction, in float maxT)
        {
            return Raycast(origin, direction, maxT, this);
        }

        public static RaycastResult Raycast(in Vector3 origin, in Vector3 direction, in float maxT, RayHitHandler2 hitHandler)
        {
            hitHandler.Collidable = default;
            hitHandler.T = float.MaxValue;
            hitHandler.Normal = default;
            hitHandler.Hit = false;
            hitHandler.ChildIndex = 0;
            hitHandler.simulation.RayCast(origin, direction, maxT, ref hitHandler);
            return new RaycastResult()
            {
                ChildIndex = hitHandler.ChildIndex,
                Collidable = hitHandler.Collidable,
                Hit = hitHandler.Hit,
                T = hitHandler.T,
                Normal = hitHandler.Normal,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable)
        {
            return collidable != Self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            return AllowTest(collidable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
        {
            if (t < maximumT)
                maximumT = t;
            if (t < T)
            {
                Collidable = collidable;
                T = t;
                Normal = normal;
                Hit = true;
                ChildIndex = childIndex;
            }
        }
    }
}