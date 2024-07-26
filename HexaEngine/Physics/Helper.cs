namespace HexaEngine.Physics
{
    using Hexa.NET.Mathematics;
    using MagicPhysX;
    using System.Numerics;

    public static class Helper
    {
        public static PxTransform Convert(Vector3 position, Quaternion quaternion)
        {
            PxQuat q = new() { x = quaternion.X, y = quaternion.Y, z = quaternion.Z, w = quaternion.W };
            PxVec3 p = new() { x = position.X, y = position.Y, z = position.Z };
            PxTransform transform = new() { q = q, p = p };
            return transform;
        }

        public static PxTransform Convert((Vector3 position, Quaternion quaternion) t)
        {
            PxQuat q = new() { x = t.quaternion.X, y = t.quaternion.Y, z = t.quaternion.Z, w = t.quaternion.W };
            PxVec3 p = new() { x = t.position.X, y = t.position.Y, z = t.position.Z };
            PxTransform transform = new() { q = q, p = p };
            return transform;
        }

        public static (Vector3 position, Quaternion quaternion) Convert(PxTransform transform)
        {
            PxQuat q = transform.q;
            PxVec3 p = transform.p;
            return (new(p.x, p.y, p.z), new(q.x, q.y, q.z, q.w)); ;
        }

        public static Vector3 Convert(PxVec3 vec)
        {
            return new(vec.x, vec.y, vec.z);
        }

        public static PxVec3 Convert(Vector3 vec)
        {
            return new() { x = vec.X, y = vec.Y, z = vec.Z };
        }

        public static Quaternion Convert(PxQuat q)
        {
            return new(q.x, q.y, q.z, q.w);
        }

        public static PxQuat Convert(Quaternion quaternion)
        {
            return new() { x = quaternion.X, y = quaternion.Y, z = quaternion.Z, w = quaternion.W };
        }

        public static PxCapsuleClimbingMode Convert(CapsuleClimbingMode mode)
        {
            return mode switch
            {
                CapsuleClimbingMode.Easy => PxCapsuleClimbingMode.Easy,
                CapsuleClimbingMode.Constrained => PxCapsuleClimbingMode.Constrained,
                CapsuleClimbingMode.Last => PxCapsuleClimbingMode.Last,
                _ => throw new NotSupportedException(),
            };
        }

        public static PxControllerNonWalkableMode Convert(ControllerNonWalkableMode mode)
        {
            return mode switch
            {
                ControllerNonWalkableMode.PreventClimbing => PxControllerNonWalkableMode.PreventClimbing,
                ControllerNonWalkableMode.PreventClimbingAndForceSliding => PxControllerNonWalkableMode.PreventClimbingAndForceSliding,
                _ => throw new NotSupportedException()
            };
        }

        public static PxForceMode Convert(ForceMode mode)
        {
            return mode switch
            {
                ForceMode.Force => PxForceMode.Force,
                ForceMode.Impulse => PxForceMode.Impulse,
                ForceMode.VelocityChange => PxForceMode.VelocityChange,
                ForceMode.Acceleration => PxForceMode.Acceleration,
                _ => throw new NotSupportedException()
            };
        }

        public static PxTransform Convert(Matrix4x4 transform, out Vector3 scale)
        {
            Matrix4x4.Decompose(transform, out scale, out var rotation, out var translation);
            return new() { p = translation, q = rotation };
        }

        public static PxActorFlags Convert(ActorFlags flags)
        {
            PxActorFlags result = 0;

            if ((flags & ActorFlags.Visualization) != 0)
            {
                result |= PxActorFlags.Visualization;
            }
            if ((flags & ActorFlags.DisableGravity) != 0)
            {
                result |= PxActorFlags.DisableGravity;
            }
            if ((flags & ActorFlags.SendSleepNotifies) != 0)
            {
                result |= PxActorFlags.SendSleepNotifies;
            }
            if ((flags & ActorFlags.DisableSimulation) != 0)
            {
                result |= PxActorFlags.DisableSimulation;
            }

            return result;
        }

        public static PxRigidDynamicLockFlags Convert(TransformFlags flags)
        {
            PxRigidDynamicLockFlags result = 0;

            if ((flags & TransformFlags.LockPositionX) != 0)
            {
                result |= PxRigidDynamicLockFlags.LockLinearX;
            }
            if ((flags & TransformFlags.LockPositionY) != 0)
            {
                result |= PxRigidDynamicLockFlags.LockLinearY;
            }
            if ((flags & TransformFlags.LockPositionZ) != 0)
            {
                result |= PxRigidDynamicLockFlags.LockLinearZ;
            }

            if ((flags & TransformFlags.LockRotationX) != 0)
            {
                result |= PxRigidDynamicLockFlags.LockAngularX;
            }
            if ((flags & TransformFlags.LockRotationY) != 0)
            {
                result |= PxRigidDynamicLockFlags.LockAngularY;
            }
            if ((flags & TransformFlags.LockRotationZ) != 0)
            {
                result |= PxRigidDynamicLockFlags.LockAngularZ;
            }

            return result;
        }

        public static PxTransform Convert(Matrix4x4 matrix)
        {
            Vector3 position = matrix.Translation;
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(matrix);

            return new() { p = position, q = rotation };
        }

        public static PxD6Motion Convert(D6Motion value)
        {
            return value switch
            {
                D6Motion.Locked => PxD6Motion.Locked,
                D6Motion.Limited => PxD6Motion.Limited,
                D6Motion.Free => PxD6Motion.Free,
                _ => throw new NotSupportedException()
            };
        }

        public static PxQueryFlags Convert(QueryFlags value)
        {
            PxQueryFlags result = 0;
            if ((value & QueryFlags.Static) != 0)
            {
                result |= PxQueryFlags.Static;
            }
            if ((value & QueryFlags.Dynamic) != 0)
            {
                result |= PxQueryFlags.Dynamic;
            }
            if ((value & QueryFlags.Prefilter) != 0)
            {
                result |= PxQueryFlags.Prefilter;
            }
            if ((value & QueryFlags.Postfilter) != 0)
            {
                result |= PxQueryFlags.Postfilter;
            }
            if ((value & QueryFlags.AnyHit) != 0)
            {
                result |= PxQueryFlags.AnyHit;
            }
            if ((value & QueryFlags.NoBlock) != 0)
            {
                result |= PxQueryFlags.NoBlock;
            }
            if ((value & QueryFlags.DisableHardcodedFilter) != 0)
            {
                result |= PxQueryFlags.DisableHardcodedFilter;
            }
            if ((value & QueryFlags.Reserved) != 0)
            {
                result |= PxQueryFlags.Reserved;
            }
            return result;
        }

        public static QueryFlags Convert(PxQueryFlags value)
        {
            QueryFlags result = 0;
            if ((value & PxQueryFlags.Static) != 0)
            {
                result |= QueryFlags.Static;
            }
            if ((value & PxQueryFlags.Dynamic) != 0)
            {
                result |= QueryFlags.Dynamic;
            }
            if ((value & PxQueryFlags.Prefilter) != 0)
            {
                result |= QueryFlags.Prefilter;
            }
            if ((value & PxQueryFlags.Postfilter) != 0)
            {
                result |= QueryFlags.Postfilter;
            }
            if ((value & PxQueryFlags.AnyHit) != 0)
            {
                result |= QueryFlags.AnyHit;
            }
            if ((value & PxQueryFlags.NoBlock) != 0)
            {
                result |= QueryFlags.NoBlock;
            }
            if ((value & PxQueryFlags.DisableHardcodedFilter) != 0)
            {
                result |= QueryFlags.DisableHardcodedFilter;
            }
            if ((value & PxQueryFlags.Reserved) != 0)
            {
                result |= QueryFlags.Reserved;
            }
            return result;
        }

        public static FilterFlags Convert(PxFilterFlags filterFlags)
        {
            FilterFlags result = 0;
            if ((filterFlags & PxFilterFlags.Kill) != 0)
            {
                result |= FilterFlags.Kill;
            }
            if ((filterFlags & PxFilterFlags.Suppress) != 0)
            {
                result |= FilterFlags.Suppress;
            }
            if ((filterFlags & PxFilterFlags.Notify) != 0)
            {
                result |= FilterFlags.Notify;
            }
            if ((filterFlags & PxFilterFlags.Callback) != 0)
            {
                result |= FilterFlags.Callback;
            }

            return result;
        }

        public static PxFilterFlags Convert(FilterFlags filterFlags)
        {
            PxFilterFlags result = 0;
            if ((filterFlags & FilterFlags.Kill) != 0)
            {
                result |= PxFilterFlags.Kill;
            }
            if ((filterFlags & FilterFlags.Suppress) != 0)
            {
                result |= PxFilterFlags.Suppress;
            }
            if ((filterFlags & FilterFlags.Notify) != 0)
            {
                result |= PxFilterFlags.Notify;
            }
            if ((filterFlags & FilterFlags.Callback) != 0)
            {
                result |= PxFilterFlags.Callback;
            }

            return result;
        }

        public static ControllerCollisionFlags Convert(PxControllerCollisionFlags flags)
        {
            ControllerCollisionFlags result = 0;
            if ((flags & PxControllerCollisionFlags.CollisionSides) != 0)
            {
                result |= ControllerCollisionFlags.CollisionSides;
            }
            if ((flags & PxControllerCollisionFlags.CollisionUp) != 0)
            {
                result |= ControllerCollisionFlags.CollisionUp;
            }
            if ((flags & PxControllerCollisionFlags.CollisionDown) != 0)
            {
                result |= ControllerCollisionFlags.CollisionDown;
            }
            return result;
        }

        public static HitFlags Convert(PxHitFlags flags)
        {
            HitFlags result = 0;
            if ((flags & PxHitFlags.Position) != 0)
            {
                result |= HitFlags.Position;
            }
            if ((flags & PxHitFlags.Normal) != 0)
            {
                result |= HitFlags.Normal;
            }
            if ((flags & PxHitFlags.Uv) != 0)
            {
                result |= HitFlags.Uv;
            }
            if ((flags & PxHitFlags.AssumeNoInitialOverlap) != 0)
            {
                result |= HitFlags.AssumeNoInitialOverlap;
            }
            if ((flags & PxHitFlags.AnyHit) != 0)
            {
                result |= HitFlags.AnyHit;
            }
            if ((flags & PxHitFlags.MeshMultiple) != 0)
            {
                result |= HitFlags.MeshMultiple;
            }
            if ((flags & PxHitFlags.MeshBothSides) != 0)
            {
                result |= HitFlags.MeshBothSides;
            }
            if ((flags & PxHitFlags.PreciseSweep) != 0)
            {
                result |= HitFlags.PreciseSweep;
            }
            if ((flags & PxHitFlags.Mtd) != 0)
            {
                result |= HitFlags.Mtd;
            }
            if ((flags & PxHitFlags.FaceIndex) != 0)
            {
                result |= HitFlags.FaceIndex;
            }
            if ((flags & PxHitFlags.Default) != 0)
            {
                result |= HitFlags.Default;
            }
            if ((flags & PxHitFlags.ModifiableFlags) != 0)
            {
                result |= HitFlags.ModifiableFlags;
            }
            return result;
        }

        public static PxQueryHitType Convert(QueryHitType type)
        {
            return type switch
            {
                QueryHitType.None => PxQueryHitType.None,
                QueryHitType.Touch => PxQueryHitType.Touch,
                QueryHitType.Block => PxQueryHitType.Block,
                _ => throw new NotSupportedException(),
            };
        }

        internal static PxHitFlags Convert(HitFlags flags)
        {
            PxHitFlags result = 0;
            if ((flags & HitFlags.Position) != 0)
            {
                result |= PxHitFlags.Position;
            }
            if ((flags & HitFlags.Normal) != 0)
            {
                result |= PxHitFlags.Normal;
            }
            if ((flags & HitFlags.Uv) != 0)
            {
                result |= PxHitFlags.Uv;
            }
            if ((flags & HitFlags.AssumeNoInitialOverlap) != 0)
            {
                result |= PxHitFlags.AssumeNoInitialOverlap;
            }
            if ((flags & HitFlags.AnyHit) != 0)
            {
                result |= PxHitFlags.AnyHit;
            }
            if ((flags & HitFlags.MeshMultiple) != 0)
            {
                result |= PxHitFlags.MeshMultiple;
            }
            if ((flags & HitFlags.MeshBothSides) != 0)
            {
                result |= PxHitFlags.MeshBothSides;
            }
            if ((flags & HitFlags.PreciseSweep) != 0)
            {
                result |= PxHitFlags.PreciseSweep;
            }
            if ((flags & HitFlags.Mtd) != 0)
            {
                result |= PxHitFlags.Mtd;
            }
            if ((flags & HitFlags.FaceIndex) != 0)
            {
                result |= PxHitFlags.FaceIndex;
            }
            if ((flags & HitFlags.Default) != 0)
            {
                result |= PxHitFlags.Default;
            }
            if ((flags & HitFlags.ModifiableFlags) != 0)
            {
                result |= PxHitFlags.ModifiableFlags;
            }
            return result;
        }
    }
}