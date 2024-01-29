namespace HexaEngine.Physics
{
    using HexaEngine.Mathematics;
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
    }
}