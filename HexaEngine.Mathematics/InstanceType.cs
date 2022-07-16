namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct InstanceData
    {
        public Matrix4x4 Transform;

        public override bool Equals(object? obj)
        {
            if (obj is InstanceData instance)
            {
                return instance.Transform == Transform;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Transform.GetHashCode();
        }

        public static bool operator ==(InstanceData left, InstanceData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InstanceData left, InstanceData right)
        {
            return !(left == right);
        }
    }

    public struct InstanceSlot
    {
        public int ID;
        public InstanceData Data;
    }
}