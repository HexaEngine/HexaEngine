namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;

    public interface ITransform
    {
        Vector3 Backward { get; }
        Vector3 Down { get; }
        Vector3 Forward { get; }
        Matrix4x4 Global { get; }
        Matrix4x4 GlobalInverse { get; }
        Quaternion GlobalOrientation { get; set; }
        Vector3 GlobalPosition { get; set; }
        Vector3 GlobalScale { get; set; }
        Vector3 Left { get; }
        Matrix4x4 Local { get; set; }
        Matrix4x4 LocalInverse { get; }
        Quaternion Orientation { get; set; }
        Transform? Parent { get; set; }
        Vector3 Position { get; set; }
        (Vector3, Quaternion) PositionRotation { get; set; }
        (Vector3, Quaternion, Vector3) PositionRotationScale { get; set; }
        Vector3 Right { get; }
        Vector3 Rotation { get; set; }
        Vector3 Scale { get; set; }
        Vector3 Up { get; }
        Vector3 Velocity { get; }
        Matrix4x4 View { get; }
        Matrix4x4 ViewInv { get; }

        event EventHandler? Updated;

        Transform Clone();
        TransformSnapshot CreateSnapshot();
        bool Recalculate();
        void RestoreState();
        void SaveState();
    }
}