namespace HexaEngine.Physics
{
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
    }
}